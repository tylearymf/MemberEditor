#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OnValueChangedAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using System.Reflection;
    using Utilities.Editor;
    using UnityEngine;
    using Utilities;
    using System;

    /// <summary>
    /// Draws properties marked with <see cref="OnValueChangedAttribute"/>.
    /// </summary>
	/// <seealso cref="OnValueChangedAttribute"/>
	/// <seealso cref="OnInspectorGUIAttribute"/>
	/// <seealso cref="ValidateInputAttribute"/>
	/// <seealso cref="InfoBoxAttribute"/>
    [OdinDrawer]
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public sealed class OnValueChangedAttributeDrawer<T> : OdinAttributeDrawer<OnValueChangedAttribute, T>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, OnValueChangedAttribute attribute, GUIContent label)
        {
            // TODO: (Optimization) Create config container class for single dictionary lookup
            var methodInfo = entry.Property.Context.Get(this, "method_info_" + attribute.MethodName, (MethodInfo)null);
            var errorMessage = entry.Property.Context.Get(this, "error_message_" + attribute.MethodName, (string)null);

            if (methodInfo.Value == null && errorMessage.Value == null)
            {
                methodInfo.Value =
                    entry.Property.ParentType.FindMember().IsMethod().IsNamed(attribute.MethodName).HasParameters<T>().GetMember<MethodInfo>(out errorMessage.Value)
                   ?? entry.Property.ParentType.FindMember().IsMethod().IsNamed(attribute.MethodName).GetMember<MethodInfo>(out errorMessage.Value);
            }

            if (methodInfo.Value == null)
            {
                SirenixEditorGUI.ErrorMessageBox(errorMessage.Value);
            }
            else
            {
                var subscribed = entry.Property.Context.Get(this, "onvaluechanged_subscribed_" + attribute.MethodName, false);

                // TODO: (Optimization) Use EmitUtilities
                if (subscribed.Value == false)
                {
                    MethodInfo method = methodInfo.Value;
                    var parameters = method.GetParameters();

                    Action<int> action;

                    if (method.IsStatic)
                    {
                        if (parameters.Length == 0)
                        {
                            action = (int index) =>
                            {
                                method.Invoke(null, null);
                            };
                        }
                        else
                        {
                            action = (int index) =>
                            {
                                method.Invoke(null, new object[] { entry.WeakValues[index] });
                            };
                        }
                    }
                    else
                    {
                        if (parameters.Length == 0)
                        {
                            action = (int index) =>
                            {
                                object inst = entry.Property.ParentValues[index];
                                method.Invoke(inst, null);

                                if (entry.ParentType.IsValueType && entry.Property.ParentValueProperty != null)
                                {
                                    entry.Property.ParentValueProperty.ValueEntry.WeakValues[index] = inst;
                                    GUIHelper.RequestRepaint();
                                }
                            };
                        }
                        else
                        {
                            action = (int index) =>
                            {
                                object inst = entry.Property.ParentValues[index];
                                method.Invoke(entry.Property.ParentValues[index], new object[] { entry.WeakValues[index] });

                                if (entry.ParentType.IsValueType && entry.Property.ParentValueProperty != null)
                                {
                                    entry.Property.ParentValueProperty.ValueEntry.WeakValues[index] = inst;
                                    GUIHelper.RequestRepaint();
                                }
                            };
                        }
                    }

                    entry.OnValueChanged += action;

                    if (attribute.IncludeChildren || typeof(T).IsValueType)
                    {
                        entry.OnChildValueChanged += action;
                    }

                    subscribed.Value = true;
                }
            }

            this.CallNextDrawer(entry, label);
        }
    }
}
#endif