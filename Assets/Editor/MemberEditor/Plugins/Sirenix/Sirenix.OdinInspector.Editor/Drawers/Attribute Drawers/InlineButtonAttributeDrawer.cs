#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InlineButtonAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Reflection;
    using UnityEngine;
    using UnityEditor;

    /// <summary>
    /// Draws properties marked with <see cref="InlineButtonAttribute"/>
    /// </summary>
    [OdinDrawer]
    [DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
    public sealed class InlineButtonAttributeDrawer<T> : OdinAttributeDrawer<InlineButtonAttribute, T>
    {
        private class ButtonContext
        {
            public string ErrorMessage;
            public StringMemberHelper LabelHelper;
            public Action StaticMethodCaller;

            //public Action<T> StaticParameterMethodCaller;
            public Action<object> InstanceMethodCaller;

            public Action<object, T> InstanceParameterMethodCaller;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, InlineButtonAttribute attribute, GUIContent label)
        {
            var context = entry.Property.Context.Get<ButtonContext>(this, "ButtonContext", (ButtonContext)null);
            if (context.Value == null)
            {
                context.Value = new ButtonContext();
                context.Value.LabelHelper = new StringMemberHelper(entry.ParentType, attribute.Label ?? attribute.MemberMethod.SplitPascalCase(), ref context.Value.ErrorMessage);

                if (context.Value.ErrorMessage == null)
                {
                    MethodInfo method;

                    if (MemberFinder.Start(entry.ParentType)
                        .IsMethod()
                        .IsNamed(attribute.MemberMethod)
                        .HasNoParameters()
                        .TryGetMember<MethodInfo>(out method, out context.Value.ErrorMessage))
                    {
                        if (method.IsStatic())
                        {
                            context.Value.StaticMethodCaller = EmitUtilities.CreateStaticMethodCaller(method);
                        }
                        else
                        {
                            context.Value.InstanceMethodCaller = EmitUtilities.CreateWeakInstanceMethodCaller(method);
                        }
                    }
                    else if (MemberFinder.Start(entry.ParentType)
                        .IsMethod()
                        .IsNamed(attribute.MemberMethod)
                        .HasParameters<T>()
                        .TryGetMember<MethodInfo>(out method, out context.Value.ErrorMessage))
                    {
                        if (method.IsStatic())
                        {
                            context.Value.ErrorMessage = "Static parameterized method is currently not supported.";
                        }
                        else
                        {
                            context.Value.InstanceParameterMethodCaller = EmitUtilities.CreateWeakInstanceMethodCaller<T>(method);
                        }
                    }
                }
            }

            if (context.Value.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(context.Value.ErrorMessage);
                this.CallNextDrawer(entry, label);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical();
                this.CallNextDrawer(entry, label);
                EditorGUILayout.EndVertical();

                if (GUILayout.Button(context.Value.LabelHelper.GetString(entry), EditorStyles.miniButton, GUILayoutOptions.ExpandWidth(false).MinWidth(20)))
                {
                    // Invoke method.
                    if (context.Value.StaticMethodCaller != null)
                    {
                        context.Value.StaticMethodCaller();
                    }
                    //else if(context.Value.StaticParameterMethodCaller != null)
                    //{
                    //	context.Value.StaticParameterMethodCaller(entry.SmartValue);
                    //}
                    else if (context.Value.InstanceMethodCaller != null)
                    {
                        context.Value.InstanceMethodCaller(entry.Property.ParentValues[0]);
                    }
                    else if (context.Value.InstanceParameterMethodCaller != null)
                    {
                        context.Value.InstanceParameterMethodCaller(entry.Property.ParentValues[0], entry.SmartValue);
                    }
                    else
                    {
                        Debug.LogError("No method found.");
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
#endif