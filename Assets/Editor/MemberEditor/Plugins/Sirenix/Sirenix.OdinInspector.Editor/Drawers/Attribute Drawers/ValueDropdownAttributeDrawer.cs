#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ValueDropdownAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Utilities.Editor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Draws properties marked with <see cref="ValueDropdownAttribute"/>.
    /// </summary>
    /// <seealso cref="ValueDropdownAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0, 0, 2001)]
    public sealed class ValueDropdownAttributeDrawer<T> : OdinAttributeDrawer<ValueDropdownAttribute, T>
    {
        private static readonly bool IsStrongList = typeof(T).ImplementsOpenGenericInterface(typeof(IList<>));
        private static readonly Type StrongListElementType = IsStrongList ? typeof(T).GetArgumentsOfInheritedOpenGenericInterface(typeof(IList<>))[0] : null;
        private static List<int> selectedValuesBuffer = new List<int>();

        private class PropertyConfig<TBase>
        {
            public bool CallNextDrawer;
            public bool IsValueDropdown;
            public string ErrorMessage;
            public Func<object, IList<TBase>> InstanceValueDropdownGetter;
            public Func<IList<TBase>> StaticValueDropdownGetter;
            public Func<object, IList<ValueDropdownItem<TBase>>> ValueDropdownInstanceValueDropdownGetter;
            public Func<IList<ValueDropdownItem<TBase>>> ValueDropdownStaticValueDropdownGetter;
            public InspectorProperty GetParentValuesFromProperty;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, ValueDropdownAttribute attribute, GUIContent label)
        {
            // We invoke a strongly typed method of the base value type, so polymorphism doesn't break
            PropertyContext<Action<IPropertyValueEntry, ValueDropdownAttribute, GUIContent>> invoker;

            if (entry.Context.Get(this, "invoker", out invoker))
            {
                MethodInfo method = this.GetType().GetMethod("DoProperty", Flags.InstanceAnyVisibility);
                method = method.MakeGenericMethod(entry.BaseValueType);
                invoker.Value = (Action<IPropertyValueEntry, ValueDropdownAttribute, GUIContent>)Delegate.CreateDelegate(typeof(Action<IPropertyValueEntry, ValueDropdownAttribute, GUIContent>), this, method);
            }

            invoker.Value(entry.Property.BaseValueEntry, attribute, label);
        }

        private void DoProperty<TBase>(IPropertyValueEntry weakEntry, ValueDropdownAttribute attribute, GUIContent label)
        {
            var entry = (IPropertyValueEntry<TBase>)weakEntry;
            var config = entry.Property.Context.Get(this, "Config", (PropertyConfig<TBase>)null);

            if (config.Value == null)
            {
                config.Value = new PropertyConfig<TBase>();

                Type parentType;

                if (entry.ValueCategory == PropertyValueCategory.Member)
                {
                    parentType = entry.ParentType;
                    config.Value.GetParentValuesFromProperty = entry.Property;
                }
                else
                {
                    parentType = entry.Property.Parent.ParentType;
                    config.Value.GetParentValuesFromProperty = entry.Property.Parent;
                }

                MemberInfo memberInfo = parentType.FindMember()
                    .HasReturnType<IList<TBase>>(true)
                    .HasNoParameters()
                    .IsNamed(attribute.MemberName)
                    .GetMember<MemberInfo>(out config.Value.ErrorMessage);

                if (config.Value.ErrorMessage == null)
                {
                    string memberName = attribute.MemberName + ((memberInfo is MethodInfo) ? "()" : "");
                    if (memberInfo.IsStatic())
                    {
                        config.Value.StaticValueDropdownGetter = DeepReflection.CreateValueGetter<IList<TBase>>(parentType, memberName);
                    }
                    else
                    {
                        config.Value.InstanceValueDropdownGetter = DeepReflection.CreateWeakInstanceValueGetter<IList<TBase>>(parentType, memberName);
                    }
                    config.Value.IsValueDropdown = false;
                }
                else
                {
                    string errorMessage;

                    memberInfo = parentType.FindMember()
                       .HasReturnType<IList<ValueDropdownItem<TBase>>>(true)
                       .HasNoParameters()
                       .IsNamed(attribute.MemberName)
                       .GetMember<MemberInfo>(out errorMessage);

                    if (errorMessage == null)
                    {
                        string memberName = attribute.MemberName + ((memberInfo is MethodInfo) ? "()" : "");
                        if (memberInfo.IsStatic())
                        {
                            config.Value.ValueDropdownStaticValueDropdownGetter = DeepReflection.CreateValueGetter<IList<ValueDropdownItem<TBase>>>(parentType, memberName);
                        }
                        else
                        {
                            config.Value.ValueDropdownInstanceValueDropdownGetter = DeepReflection.CreateWeakInstanceValueGetter<IList<ValueDropdownItem<TBase>>>(parentType, memberName);
                        }
                        config.Value.ErrorMessage = null;
                        config.Value.IsValueDropdown = true;
                    }
                    else
                    {
                        if (config.Value.ErrorMessage != errorMessage)
                        {
                            config.Value.ErrorMessage += " or\n" + errorMessage;
                        }

                        if (IsStrongList)
                        {
                            memberInfo = parentType.FindMember()
                               .HasReturnType(typeof(IList<>).MakeGenericType(StrongListElementType), true)
                               .HasNoParameters()
                               .IsNamed(attribute.MemberName)
                               .GetMember<MemberInfo>(out errorMessage);

                            if (errorMessage != null)
                            {
                                config.Value.ErrorMessage += " or\n" + errorMessage;

                                Type valueDropdown = typeof(ValueDropdownItem<>).MakeGenericType(StrongListElementType);

                                memberInfo = parentType.FindMember()
                                   .HasReturnType(typeof(IList<>).MakeGenericType(valueDropdown), true)
                                   .HasNoParameters()
                                   .IsNamed(attribute.MemberName)
                                   .GetMember<MemberInfo>(out errorMessage);

                                if (errorMessage != null)
                                {
                                    config.Value.ErrorMessage += " or\n" + errorMessage;
                                }
                                else
                                {
                                    config.Value.ErrorMessage = null;
                                    config.Value.CallNextDrawer = true;
                                }
                            }
                            else
                            {
                                config.Value.ErrorMessage = null;
                                config.Value.CallNextDrawer = true;
                            }
                        }
                    }
                }
            }

            if (config.Value.ErrorMessage != null)
            {
                if (entry.ValueCategory == PropertyValueCategory.Member)
                {
                    SirenixEditorGUI.ErrorMessageBox(config.Value.ErrorMessage);
                }

                this.CallNextDrawer(entry, label);
            }
            else if (config.Value.CallNextDrawer)
            {
                this.CallNextDrawer(entry, label);
            }
            else
            {
                if (config.Value.IsValueDropdown)
                {
                    IList<ValueDropdownItem<TBase>> selectList = config.Value.ValueDropdownStaticValueDropdownGetter != null ?
                      config.Value.ValueDropdownStaticValueDropdownGetter() :
                      config.Value.ValueDropdownInstanceValueDropdownGetter(config.Value.GetParentValuesFromProperty.ParentValues[0]);

                    selectedValuesBuffer.Clear();

                    if (selectList != null && selectList.Count > 0)
                    {
                        for (int i = 0; i < entry.Values.Count; i++)
                        {
                            var val = entry.Values[i];
                            for (int j = 0; j < selectList.Count; j++)
                            {
                                if (EqualityComparer<TBase>.Default.Equals((TBase)val, selectList[j].Value))
                                {
                                    selectedValuesBuffer.Add(j);
                                }
                            }
                        }

                        //if (Event.current.type == EventType.Repaint && selectList.Count > 0 && selectedValuesBuffer.Count == 0 && entry.Values.Count == 1)
                        //{
                        //    entry.SmartValue = selectList[0].Value;
                        //}
                    }

                    if (SirenixEditorFields.Dropdown<ValueDropdownItem<TBase>>(label, selectedValuesBuffer, selectList, false))
                    {
                        if (selectedValuesBuffer.Count > 0)
                        {
                            entry.SmartValue = selectList[selectedValuesBuffer[0]].Value;
                        }
                    }

                    //IList<ValueDropdownItem<TBase>> selectList = config.Value.ValueDropdownStaticValueDropdownGetter != null ?
                    //    config.Value.ValueDropdownStaticValueDropdownGetter() :
                    //    config.Value.ValueDropdownInstanceValueDropdownGetter(config.Value.GetParentValuesFromProperty.ParentValues[0]);

                    //ValueDropdownItem<TBase> selected = new ValueDropdownItem<TBase>(null, entry.SmartValue);
                    //entry.SmartValue = SirenixEditorFields.Dropdown(label, selected, selectList).Value;
                }
                else
                {
                    IList<TBase> selectList = config.Value.StaticValueDropdownGetter != null ?
                        config.Value.StaticValueDropdownGetter() :
                        config.Value.InstanceValueDropdownGetter(config.Value.GetParentValuesFromProperty.ParentValues[0]);

                    entry.SmartValue = SirenixEditorFields.Dropdown(label, entry.SmartValue, selectList);
                }
            }
        }
    }
}
#endif