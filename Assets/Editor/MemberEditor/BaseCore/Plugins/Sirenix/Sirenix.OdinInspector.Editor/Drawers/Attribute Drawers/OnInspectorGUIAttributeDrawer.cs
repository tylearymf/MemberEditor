#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OnInspectorGUIAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using System;
    using System.Reflection;
    using Utilities.Editor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Draws properties marked with <see cref="OnInspectorGUIAttribute"/>.
	/// Calls the method, the attribute is either attached to, or the method that has been specified in the attribute, to allow for custom GUI drawing.
    /// </summary>
	/// <seealso cref="OnInspectorGUIAttribute"/>
	/// <seealso cref="OnValueChangedAttribute"/>
	/// <seealso cref="ValidateInputAttribute"/>
	/// <seealso cref="DrawWithUnityAttribute"/>
	///	<seealso cref="InlineEditorAttribute"/>
    [OdinDrawer]
    [DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
    public sealed class OnInspectorGUIAttributeDrawer : OdinAttributeDrawer<OnInspectorGUIAttribute>
    {
        private class MethodContext
        {
            public Action StaticMethod;
            public Action<object> InstanceMethod;
            public Delegate StaticMethodWithParam;
            public string ErrorMessage;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, OnInspectorGUIAttribute attribute, GUIContent label)
        {
            if (property.Info.PropertyType == PropertyType.Method)
            {
                var methodConfig = property.Context.Get(this, "Config", (MethodContext)null);

                if (methodConfig.Value == null)
                {
                    methodConfig.Value = new MethodContext();

                    MethodInfo methodInfo = property.Info.MemberInfo as MethodInfo;

                    if (methodInfo.ReturnType != typeof(void))
                    {
                        methodConfig.Value.ErrorMessage = "The method '" + methodInfo.Name + "' must have a return type of type void.";
                    }
                    else if (methodInfo.GetParameters().Length > 0)
                    {
                        methodConfig.Value.ErrorMessage = "The method '" + methodInfo.Name + "' cannot take any parameters.";
                    }
                    else
                    {
                        methodConfig.Value.InstanceMethod = EmitUtilities.CreateWeakInstanceMethodCaller(methodInfo);
                    }
                }

                if (methodConfig.Value.ErrorMessage != null)
                {
                    SirenixEditorGUI.ErrorMessageBox(methodConfig.Value.ErrorMessage);
                }
                else
                {
                    if (methodConfig.Value.InstanceMethod != null)
                    {
                        methodConfig.Value.InstanceMethod(property.ParentValues[0]);
                    }
                }
            }
            else
            {
                if (attribute.PrependMethodName != null)
                {
                    this.DoInspectorGUI(property, "Prepend", attribute.PrependMethodName);
                }

                this.CallNextDrawer(property, label);

                if (attribute.AppendMethodName != null)
                {
                    this.DoInspectorGUI(property, "Append", attribute.AppendMethodName);
                }
            }
        }

        private void DoInspectorGUI(InspectorProperty property, string config, string methodName)
        {
            var methodConfig = property.Context.Get(this, config, (MethodContext)null);

            if (methodConfig.Value == null)
            {
                methodConfig.Value = new MethodContext();

                MethodInfo methodInfo = property.ParentType
                    .FindMember()
                    .IsMethod()
                    .IsNamed(methodName)
                    .HasNoParameters()
                    .ReturnsVoid()
                    .GetMember<MethodInfo>(out methodConfig.Value.ErrorMessage);

                if (methodConfig.Value.ErrorMessage == null && methodInfo != null)
                {
                    if (methodInfo.IsStatic)
                    {
                        methodConfig.Value.StaticMethod = EmitUtilities.CreateStaticMethodCaller(methodInfo);
                    }
                    else
                    {
                        methodConfig.Value.InstanceMethod = EmitUtilities.CreateWeakInstanceMethodCaller(methodInfo);
                    }
                }
                else
                {
                    string otherErrorMessage;
                    methodInfo = property.ParentType
                        .FindMember()
                        .IsMethod()
                        .IsNamed(methodName)
                        .HasParameters(property.ValueEntry.BaseValueType)
                        .IsStatic()
                        .ReturnsVoid()
                        .GetMember<MethodInfo>(out otherErrorMessage);

                    if (otherErrorMessage == null)
                    {
                        methodConfig.Value.ErrorMessage = null;
                        var delegateType = typeof(Action<>).MakeGenericType(property.ValueEntry.TypeOfValue);
                        methodConfig.Value.StaticMethodWithParam = Delegate.CreateDelegate(delegateType, methodInfo);
                    }
                    else
                    {
                        methodConfig.Value.ErrorMessage += "\nor\n" + otherErrorMessage;
                    }
                }
            }

            if (methodConfig.Value.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(methodConfig.Value.ErrorMessage);
            }
            else
            {
                if (methodConfig.Value.InstanceMethod != null)
                {
                    methodConfig.Value.InstanceMethod(property.ParentValues[0]);
                }
                else if (methodConfig.Value.StaticMethodWithParam != null)
                {
                    methodConfig.Value.StaticMethodWithParam.DynamicInvoke(property.ValueEntry.WeakSmartValue);
                }
                else
                {
                    methodConfig.Value.StaticMethod();
                }
            }
        }
    }
}
#endif