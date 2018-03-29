#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="CustomValueDrawerAttributeDrawer.cs" company="Sirenix IVS">
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
    using System.Collections;
    using System.Reflection.Emit;

    /// <summary>
    /// Draws properties marked with <see cref="ValidateInputAttribute"/>.
    /// </summary>
    /// <seealso cref="ValidateInputAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0, 0, double.MaxValue)]
    public class CustomValueDrawerAttributeDrawer<T> : OdinAttributeDrawer<CustomValueDrawerAttribute, T>
    {
        private delegate T InstanceDelegateWithLabel(ref object owner, T value, GUIContent label);

        private class Context
        {
            public string ErrorMessage;
            public Func<T, GUIContent, T> CustomValueDrawerStaticWithLabel;
            public InstanceDelegateWithLabel CustomValueDrawerInstanceWithLabel;
            internal InspectorProperty MemberProperty;
        }

        /// <summary>
        /// Excludes functionality for lists and instead works on the list elements.
        /// </summary>
        public override bool CanDrawTypeFilter(Type type)
        {
            return !typeof(IList).IsAssignableFrom(type);
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, CustomValueDrawerAttribute attribute, GUIContent label)
        {
            var context = entry.Context.Get(this, "context", (Context)null);

            if (context.Value == null)
            {
                context.Value = new Context();
                context.Value.MemberProperty = entry.Property.FindParent(PropertyValueCategory.Member, true);

                var parentType = context.Value.MemberProperty.ParentType;
                var methodInfo = parentType
                    .FindMember()
                    .IsNamed(attribute.MethodName)
                    .IsMethod()
                    .HasReturnType<T>()
                    .HasParameters<T, GUIContent>()
                    .GetMember<MethodInfo>(out context.Value.ErrorMessage);

                if (context.Value.ErrorMessage == null)
                {
                    if (methodInfo.IsStatic())
                    {
                        context.Value.CustomValueDrawerStaticWithLabel = (Func<T, GUIContent, T>)Delegate.CreateDelegate(typeof(Func<T, GUIContent, T>), methodInfo);
                    }
                    else
                    {
                        DynamicMethod emittedMethod;

                        emittedMethod = new DynamicMethod("CustomValueDrawerAttributeDrawer." + typeof(T).GetNiceFullName() + entry.Property.Path, typeof(T), new Type[] { typeof(object).MakeByRefType(), typeof(T), typeof(GUIContent) }, true);

                        var il = emittedMethod.GetILGenerator();

                        if (parentType.IsValueType)
                        {
                            il.DeclareLocal(typeof(T));

                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldind_Ref);
                            il.Emit(OpCodes.Unbox_Any, parentType);
                            il.Emit(OpCodes.Stloc_0);
                            il.Emit(OpCodes.Ldloca_S, 0);
                            il.Emit(OpCodes.Ldarg_1);

                            il.Emit(OpCodes.Ldarg_2);

                            il.Emit(OpCodes.Call, methodInfo);
                            il.Emit(OpCodes.Stloc_1);
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldloc_0);
                            il.Emit(OpCodes.Box, parentType);
                            il.Emit(OpCodes.Stind_Ref);
                            il.Emit(OpCodes.Ldloc_1);
                            il.Emit(OpCodes.Ret);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldind_Ref);
                            il.Emit(OpCodes.Castclass, parentType);
                            il.Emit(OpCodes.Ldarg_1);

                            il.Emit(OpCodes.Ldarg_2);

                            il.Emit(OpCodes.Callvirt, methodInfo);
                            il.Emit(OpCodes.Ret);
                        }
                        context.Value.CustomValueDrawerInstanceWithLabel = (InstanceDelegateWithLabel)emittedMethod.CreateDelegate(typeof(InstanceDelegateWithLabel));
                    }
                }
            }

            if (context.Value.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(context.Value.ErrorMessage);
                this.CallNextDrawer(entry.Property, label);
            }
            else
            {
                if (context.Value.CustomValueDrawerStaticWithLabel != null)
                {
                    entry.SmartValue = context.Value.CustomValueDrawerStaticWithLabel(entry.SmartValue, label);
                }
                else
                {
                    var val = context.Value.MemberProperty.ParentValues[0];
                    entry.SmartValue = context.Value.CustomValueDrawerInstanceWithLabel(ref val, entry.SmartValue, label);
                }
            }
        }
    }
}
#endif