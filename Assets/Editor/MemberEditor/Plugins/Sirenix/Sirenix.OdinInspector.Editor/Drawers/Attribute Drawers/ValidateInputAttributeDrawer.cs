#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ValidateInputAttributeDrawer.cs" company="Sirenix IVS">
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
    /// Draws properties marked with <see cref="ValidateInputAttribute"/>.
    /// </summary>
    /// <seealso cref="ValidateInputAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0, 10000, 0)]
    public sealed class ValidateInputAttributeDrawer<T> : OdinAttributeDrawer<ValidateInputAttribute, T>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, ValidateInputAttribute attribute, GUIContent label)
        {
            PropertyContext<InnerDrawer> context;

            if (entry.Context.Get(this, "inner_drawer", out context))
            {
                context.Value = (InnerDrawer)Activator.CreateInstance(typeof(InnerDrawer<>).MakeGenericType(typeof(T), entry.ParentType), this);
            }

            context.Value.DrawPropertyLayout(entry, attribute, label);
        }

        private abstract class InnerDrawer
        {
            public abstract void DrawPropertyLayout(IPropertyValueEntry<T> entry, ValidateInputAttribute attribute, GUIContent label);
        }

        private class InnerDrawer<TParent> : InnerDrawer
        {
            private ValidateInputAttributeDrawer<T> drawer;

            public InnerDrawer(ValidateInputAttributeDrawer<T> drawer)
            {
                this.drawer = drawer;
            }

            private class ValidateContext
            {
                public string MemberErrorMessage;
                public string ValidationMessage;
                public Func<TParent, T, bool> InstanceValidationMethodCaller;
                public Func<T, bool> StaticValidationMethodCaller;
                public ValidateDelegateInstance1 InstanceValidationMethodCallerWithMessage;
                public ValidateDelegateStatic1 StaticValidationMethodCallerWithMessage;
                public ValidateDelegateInstance2 InstanceValidationMethodCallerWithMessageAndType;
                public ValidateDelegateStatic2 StaticValidationMethodCallerWithMessageAndType;
                public StringMemberHelper ValidationMessageHelper;
                public InfoMessageType MessageType;
                public object ShakeGroupKey;
            }

            private delegate bool ValidateDelegateInstance1(TParent target, T value, ref string message);

            private delegate bool ValidateDelegateStatic1(T value, ref string message);

            private delegate bool ValidateDelegateInstance2(TParent target, T value, ref string message, ref InfoMessageType? messageType);

            private delegate bool ValidateDelegateStatic2(T value, ref string message, ref InfoMessageType? messageType);

            /// <summary>
            /// Draws the property.
            /// </summary>
            public override void DrawPropertyLayout(IPropertyValueEntry<T> entry, ValidateInputAttribute attribute, GUIContent label)
            {
                var context = entry.Property.Context.Get(this.drawer, "ValidateInputAttributeDrawer", (ValidateContext)null);

                if (context.Value == null)
                {
                    context.Value = new ValidateContext();

                    context.Value.ShakeGroupKey = UniqueDrawerKey.Create(entry.Property, this.drawer);

                    MethodInfo methodInfo = entry.ParentType.FindMember()
                        .IsMethod()
                        .HasReturnType<bool>()
                        .HasParameters(entry.BaseValueType)
                        .IsNamed(attribute.MemberName)
                        .GetMember<MethodInfo>(out context.Value.MemberErrorMessage);

                    if (context.Value.MemberErrorMessage == null)
                    {
                        if (methodInfo.IsStatic())
                        {
                            context.Value.StaticValidationMethodCaller = (Func<T, bool>)Delegate.CreateDelegate(typeof(Func<T, bool>), methodInfo);
                        }
                        else
                        {
                            context.Value.InstanceValidationMethodCaller = (Func<TParent, T, bool>)Delegate.CreateDelegate(typeof(Func<TParent, T, bool>), methodInfo);
                        }
                    }
                    else
                    {
                        string errorMsg;

                        methodInfo = entry.ParentType.FindMember()
                            .IsMethod()
                            .HasReturnType<bool>()
                            .HasParameters(entry.BaseValueType, typeof(string).MakeByRefType())
                            .IsNamed(attribute.MemberName)
                            .GetMember<MethodInfo>(out errorMsg);

                        if (errorMsg == null)
                        {
                            context.Value.MemberErrorMessage = null;

                            if (methodInfo.IsStatic())
                            {
                                context.Value.StaticValidationMethodCallerWithMessage = (ValidateDelegateStatic1)Delegate.CreateDelegate(typeof(ValidateDelegateStatic1), methodInfo);
                            }
                            else
                            {
                                context.Value.InstanceValidationMethodCallerWithMessage = (ValidateDelegateInstance1)Delegate.CreateDelegate(typeof(ValidateDelegateInstance1), methodInfo);
                            }
                        }
                        else
                        {
                            context.Value.MemberErrorMessage += "\nor\n" + errorMsg;

                            methodInfo = entry.ParentType.FindMember()
                                .IsMethod()
                                .HasReturnType<bool>()
                                .HasParameters(entry.BaseValueType, typeof(string).MakeByRefType(), typeof(Nullable<>).MakeGenericType(typeof(InfoMessageType)).MakeByRefType())
                                .IsNamed(attribute.MemberName)
                                .GetMember<MethodInfo>(out errorMsg);

                            if (errorMsg == null)
                            {
                                context.Value.MemberErrorMessage = null;

                                if (methodInfo.IsStatic())
                                {
                                    context.Value.StaticValidationMethodCallerWithMessageAndType = (ValidateDelegateStatic2)Delegate.CreateDelegate(typeof(ValidateDelegateStatic2), methodInfo);
                                }
                                else
                                {
                                    context.Value.InstanceValidationMethodCallerWithMessageAndType = (ValidateDelegateInstance2)Delegate.CreateDelegate(typeof(ValidateDelegateInstance2), methodInfo);
                                }
                            }
                            else
                            {
                                context.Value.MemberErrorMessage += "\nor\n" + errorMsg;
                            }
                        }
                    }

                    if (attribute.DefaultMessage != null)
                    {
                        context.Value.ValidationMessageHelper = new StringMemberHelper(entry.ParentType, attribute.DefaultMessage);

                        if (context.Value.ValidationMessageHelper.ErrorMessage != null)
                        {
                            if (context.Value.MemberErrorMessage != null)
                            {
                                context.Value.MemberErrorMessage += "\n\n" + context.Value.ValidationMessageHelper.ErrorMessage;
                            }
                            else
                            {
                                context.Value.MemberErrorMessage = context.Value.ValidationMessageHelper.ErrorMessage;
                            }

                            context.Value.ValidationMessageHelper = null;
                        }
                    }

                    if (context.Value.MemberErrorMessage == null)
                    {
                        Action<int> action = (int index) =>
                        {
                            if (!RunValidation(entry, attribute, context))
                            {
                                SirenixEditorGUI.StartShakingGroup(context.Value.ShakeGroupKey);
                            }
                        };

                        entry.OnValueChanged += action;

                        if (attribute.IncludeChildren)
                        {
                            entry.OnChildValueChanged += action;
                        }

                        entry.Property.Tree.OnUndoRedoPerformed += () => action(0);

                        RunValidation(entry, attribute, context);
                    }
                }
                else
                {
                    context.Value.ShakeGroupKey = UniqueDrawerKey.Create(entry.Property, this.drawer);
                }

                if (context.Value.MemberErrorMessage != null)
                {
                    SirenixEditorGUI.ErrorMessageBox(context.Value.MemberErrorMessage);
                }

                SirenixEditorGUI.BeginShakeableGroup(context.Value.ShakeGroupKey);

                if (context.Value.ValidationMessage != null)
                {
                    if (context.Value.MessageType == InfoMessageType.Error)
                    {
                        SirenixEditorGUI.ErrorMessageBox(context.Value.ValidationMessage);
                    }
                    else if (context.Value.MessageType == InfoMessageType.Warning)
                    {
                        SirenixEditorGUI.WarningMessageBox(context.Value.ValidationMessage);
                    }
                    else if (context.Value.MessageType == InfoMessageType.Info)
                    {
                        SirenixEditorGUI.InfoMessageBox(context.Value.ValidationMessage);
                    }
                    else
                    {
                        SirenixEditorGUI.MessageBox(context.Value.ValidationMessage);
                    }
                }

                GUIUtility.GetControlID(12938712, FocusType.Passive);
                this.drawer.CallNextDrawer(entry.Property, label);
                SirenixEditorGUI.EndShakeableGroup(context.Value.ShakeGroupKey);
            }

            private static bool RunValidation(IPropertyValueEntry<T> entry, ValidateInputAttribute attribute, PropertyContext<ValidateContext> context)
            {
                if (context.Value.MemberErrorMessage != null)
                {
                    return true;
                }

                bool hasError = false;
                string message = null;
                InfoMessageType? messageType = null;

                for (int i = 0; i < entry.Property.ParentValues.Count; i++)
                {
                    try
                    {
                        if (context.Value.StaticValidationMethodCaller != null)
                        {
                            hasError = context.Value.StaticValidationMethodCaller(entry.Values[i]) == false;
                        }
                        else if (context.Value.StaticValidationMethodCallerWithMessage != null)
                        {
                            hasError = context.Value.StaticValidationMethodCallerWithMessage(entry.Values[i], ref message) == false;
                        }
                        else if (context.Value.StaticValidationMethodCallerWithMessageAndType != null)
                        {
                            hasError = context.Value.StaticValidationMethodCallerWithMessageAndType(entry.Values[i], ref message, ref messageType) == false;
                        }
                        else if (context.Value.InstanceValidationMethodCaller != null)
                        {
                            hasError = context.Value.InstanceValidationMethodCaller((TParent)entry.Property.ParentValues[i], entry.Values[i]) == false;
                        }
                        else if (context.Value.InstanceValidationMethodCallerWithMessage != null)
                        {
                            hasError = context.Value.InstanceValidationMethodCallerWithMessage((TParent)entry.Property.ParentValues[i], entry.Values[i], ref message) == false;
                        }
                        else
                        {
                            hasError = context.Value.InstanceValidationMethodCallerWithMessageAndType((TParent)entry.Property.ParentValues[i], entry.Values[i], ref message, ref messageType) == false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }

                    context.Value.MessageType = messageType ?? attribute.MessageType;

                    if (!hasError)
                    {
                        context.Value.ValidationMessage = null;
                    }
                    else if (message != null)
                    {
                        context.Value.ValidationMessage = message;
                    }
                    else if (context.Value.ValidationMessageHelper != null)
                    {
                        context.Value.ValidationMessage = context.Value.ValidationMessageHelper.ForceGetString(entry.Property.ParentValues[i]);
                    }
                    else
                    {
                        context.Value.ValidationMessage = "Value for property '" + entry.Property.NiceName + "' is invalid";
                    }

                    if (hasError)
                    {
                        break;
                    }
                }

                return !hasError;
            }
        }
    }
}
#endif