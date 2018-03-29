#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="EnabledIfAttributeDrawer.cs" company="Sirenix IVS">
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

    internal static class IfAttributesHelper
    {
        private class IfAttributesContext
        {
            public Func<bool> StaticMemberGetter;
            public Func<object, bool> InstanceMemberGetter;
            public Func<object> StaticObjectMemberGetter;
            public Func<object, object> InstanceObjectMemberGetter;
            public string ErrorMessage;
            public bool Result;
        }

        public static void HandleIfAttributesCondition(OdinDrawer drawer, InspectorProperty property, string memberName, object value, out bool result, out string errorMessage)
        {
            var context = property.Context.Get(drawer, "IfAttributeContext", (IfAttributesContext)null);

            if (context.Value == null)
            {
                context.Value = new IfAttributesContext();
                MemberInfo memberInfo = property.ParentType
                    .FindMember()
                    .IsNamed(memberName)
                    .HasNoParameters()
                    .GetMember(out context.Value.ErrorMessage);

                if (memberInfo != null)
                {
                    string name = (memberInfo is MethodInfo) ? memberInfo.Name + "()" : memberInfo.Name;

                    if (memberInfo.GetReturnType() == typeof(bool))
                    {
                        if (memberInfo.IsStatic())
                        {
                            context.Value.StaticMemberGetter = DeepReflection.CreateValueGetter<bool>(property.ParentType, name);
                        }
                        else
                        {
                            context.Value.InstanceMemberGetter = DeepReflection.CreateWeakInstanceValueGetter<bool>(property.ParentType, name);
                        }
                    }
                    else
                    {
                        if (value == null)
                        {
                            context.Value.ErrorMessage = "An member with a non-bool value was referenced, but no value was specified in the EnabledIf attribute.";
                        }
                        else
                        {
                            if (memberInfo.IsStatic())
                            {
                                context.Value.StaticObjectMemberGetter = DeepReflection.CreateValueGetter<object>(property.ParentType, name);
                            }
                            else
                            {
                                context.Value.InstanceObjectMemberGetter = DeepReflection.CreateWeakInstanceValueGetter<object>(property.ParentType, name);
                            }
                        }
                    }
                }
            }
            errorMessage = context.Value.ErrorMessage;

            if (Event.current.type != EventType.Layout)
            {
                result = context.Value.Result;
                return;
            }

            context.Value.Result = false;

            if (context.Value.ErrorMessage == null)
            {
                if (context.Value.InstanceMemberGetter != null)
                {
                    for (int i = 0; i < property.ParentValues.Count; i++)
                    {
                        if (context.Value.InstanceMemberGetter(property.ParentValues[i]))
                        {
                            context.Value.Result = true;
                            break;
                        }
                    }
                }
                else if (context.Value.InstanceObjectMemberGetter != null)
                {
                    for (int i = 0; i < property.ParentValues.Count; i++)
                    {
                        var val = context.Value.InstanceObjectMemberGetter(property.ParentValues[i]);
                        if (Equals(val, value))
                        {
                            context.Value.Result = true;
                            break;
                        }
                    }
                }
                else if (context.Value.StaticObjectMemberGetter != null)
                {
                    var val = context.Value.StaticObjectMemberGetter();
                    if (Equals(val, value))
                    {
                        context.Value.Result = true;
                    }
                }
                else if (context.Value.StaticMemberGetter != null)
                {
                    if (context.Value.StaticMemberGetter())
                    {
                        context.Value.Result = true;
                    }
                }
            }

            result = context.Value.Result;
        }
    }


    /// <summary>
    /// Draws properties marked with <see cref="EnableIfAttribute"/>.
    /// </summary>
    /// <seealso cref="EnableIfAttribute"/>
    /// <seealso cref="DisableIfAttribute"/>
    /// <seealso cref="DisableInEditorModeAttribute"/>
    /// <seealso cref="DisableInPlayModeAttribute"/>
    /// <seealso cref="ReadOnlyAttribute"/>
    /// <seealso cref="ShowIfAttribute"/>
    /// <seealso cref="HideIfAttribute"/>
    /// <seealso cref="HideInInspector"/>
    [OdinDrawer]
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public sealed class EnableIfAttributeDrawer : OdinAttributeDrawer<EnableIfAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, EnableIfAttribute attribute, GUIContent label)
        {
            if (GUI.enabled == false)
            {
                this.CallNextDrawer(property, label);
                return;
            }

            bool result;
            string errorMessage;

            IfAttributesHelper.HandleIfAttributesCondition(this, property, attribute.MemberName, attribute.Value, out result, out errorMessage);

            if (errorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(errorMessage);
                this.CallNextDrawer(property, label);
            }
            else if (!result)
            {
                GUIHelper.PushGUIEnabled(false);
                this.CallNextDrawer(property, label);
                GUIHelper.PopGUIEnabled();

            }
            else
            {
                this.CallNextDrawer(property, label);
            }
        }
    }
}
#endif