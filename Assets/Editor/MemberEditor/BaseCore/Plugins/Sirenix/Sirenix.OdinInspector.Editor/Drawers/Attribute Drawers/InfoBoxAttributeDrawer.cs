#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InfoBoxAttributeDrawer.cs" company="Sirenix IVS">
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
    /// Draws properties marked with <see cref="InfoBoxAttribute"/>.
    /// Draws an info box above the property. Error and warning info boxes can be tracked by Odin Scene Validator.
    /// </summary>
    /// <seealso cref="InfoBoxAttribute"/>
    /// <seealso cref="DetailedInfoBoxAttribute"/>
    /// <seealso cref="RequiredAttribute"/>
    /// <seealso cref="ValidateInputAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0, 10001, 0)]
    public sealed class InfoBoxAttributeDrawer : OdinAttributeDrawer<InfoBoxAttribute>
    {
        private class InfoBoxContext
        {
            public bool DrawMessageBox;
            public string ErrorMessage;
            public MethodInfo InstanceValidationParameterMethod;
            public MethodInfo StaticValidationParameterMethod;
            public Func<object, bool> InstanceValidationMethodCaller;
            public WeakValueGetter InstanceValueGetter;
            public Func<bool> StaticValidationCaller;
            public StringMemberHelper MessageHelper;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, InfoBoxAttribute attribute, GUIContent label)
        {
            PropertyContext<InfoBoxContext> context = null;

            context = property.Context.Get(this, "Config_" + this.GetHashCode(), (InfoBoxContext)null);
            if (context.Value == null)
            {
                context.Value = new InfoBoxContext()
                {
                    MessageHelper = new StringMemberHelper(property.ParentType, attribute.Message)
                };

                context.Value.ErrorMessage = context.Value.MessageHelper.ErrorMessage;

                MemberInfo memberInfo;

                if (attribute.VisibleIf != null)
                {
                    // Parameter functions
                    if (property.ValueEntry != null &&
                        property.ParentType.FindMember()
                        .IsMethod()
                        .HasReturnType<bool>()
                        .HasParameters(property.ValueEntry.BaseValueType)
                        .IsNamed(attribute.VisibleIf)
                        .TryGetMember(out memberInfo, out context.Value.ErrorMessage))
                    {
                        if (context.Value.ErrorMessage == null)
                        {
                            if (memberInfo is MethodInfo)
                            {
                                if (memberInfo.IsStatic())
                                {
                                    context.Value.StaticValidationParameterMethod = memberInfo as MethodInfo;
                                }
                                else
                                {
                                    context.Value.InstanceValidationParameterMethod = memberInfo as MethodInfo;
                                }
                            }
                            else
                            {
                                context.Value.ErrorMessage = "Invalid member type!";
                            }
                        }
                    }
                    // Fields, properties, and no-parameter functions.
                    else if (property.ParentType.FindMember()
                        .HasReturnType<bool>()
                        .HasNoParameters()
                        .IsNamed(attribute.VisibleIf)
                        .TryGetMember(out memberInfo, out context.Value.ErrorMessage))
                    {
                        if (context.Value.ErrorMessage == null)
                        {
                            if (memberInfo is FieldInfo)
                            {
                                if (memberInfo.IsStatic())
                                {
                                    context.Value.StaticValidationCaller = EmitUtilities.CreateStaticFieldGetter<bool>(memberInfo as FieldInfo);
                                }
                                else
                                {
                                    context.Value.InstanceValueGetter = EmitUtilities.CreateWeakInstanceFieldGetter(property.ParentType, memberInfo as FieldInfo);
                                }
                            }
                            else if (memberInfo is PropertyInfo)
                            {
                                if (memberInfo.IsStatic())
                                {
                                    context.Value.StaticValidationCaller = EmitUtilities.CreateStaticPropertyGetter<bool>(memberInfo as PropertyInfo);
                                }
                                else
                                {
                                    context.Value.InstanceValueGetter = EmitUtilities.CreateWeakInstancePropertyGetter(property.ParentType, memberInfo as PropertyInfo);
                                }
                            }
                            else if (memberInfo is MethodInfo)
                            {
                                if (memberInfo.IsStatic())
                                {
                                    context.Value.StaticValidationCaller = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), memberInfo as MethodInfo);
                                }
                                else
                                {
                                    context.Value.InstanceValidationMethodCaller = EmitUtilities.CreateWeakInstanceMethodCallerFunc<bool>(memberInfo as MethodInfo);
                                }
                            }
                            else
                            {
                                context.Value.ErrorMessage = "Invalid member type!";
                            }
                        }
                    }
                }
            }

            if (context.Value.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(context.Value.ErrorMessage);
            }
            else
            {
                if (Event.current.type == EventType.Layout)
                {
                    var parentValue = property.ParentValues[0];

                    try
                    {
                        context.Value.DrawMessageBox =
                            attribute.VisibleIf == null ||
                            (context.Value.StaticValidationParameterMethod != null && (bool)context.Value.StaticValidationParameterMethod.Invoke(null, new object[] { property.ValueEntry.WeakSmartValue })) ||
                            (context.Value.InstanceValidationParameterMethod != null && (bool)context.Value.InstanceValidationParameterMethod.Invoke(null, new object[] { property.ParentValues[0], property.ValueEntry.WeakSmartValue })) ||
                            (context.Value.InstanceValidationMethodCaller != null && context.Value.InstanceValidationMethodCaller(property.ParentValues[0])) ||
                            (context.Value.InstanceValueGetter != null && (bool)context.Value.InstanceValueGetter(ref parentValue)) ||
                            (context.Value.StaticValidationCaller != null && context.Value.StaticValidationCaller());
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                if (context.Value.DrawMessageBox)
                {
                    switch (attribute.InfoMessageType)
                    {
                        case InfoMessageType.None:
                            SirenixEditorGUI.MessageBox(context.Value.MessageHelper.GetString(property));
                            break;

                        case InfoMessageType.Info:
                            SirenixEditorGUI.InfoMessageBox(context.Value.MessageHelper.GetString(property));
                            break;

                        case InfoMessageType.Warning:
                            SirenixEditorGUI.WarningMessageBox(context.Value.MessageHelper.GetString(property));
                            break;

                        case InfoMessageType.Error:
                            SirenixEditorGUI.ErrorMessageBox(context.Value.MessageHelper.GetString(property));
                            break;

                        default:
                            SirenixEditorGUI.ErrorMessageBox("Unknown InfoBoxType: " + attribute.InfoMessageType.ToString());
                            break;
                    }
                }
            }

            this.CallNextDrawer(property, label);
        }
    }
}
#endif