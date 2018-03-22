#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorValuePropertyInfo.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;
    using System.Reflection;
    using Utilities;

    /// <summary>
    /// Contains meta-data information about a field or property-backed property in the inspector.
    /// </summary>
    public class InspectorValuePropertyInfo<TOwner, TValue> : InspectorPropertyInfo, IValueGetterSetter<TOwner, TValue>
    {
        private ValueGetter<TOwner, TValue> getter;
        private ValueSetter<TOwner, TValue> setter;

        /// <summary>
        /// Initializes a new instance of the <see cref="InspectorValuePropertyInfo{TOwner, TValue}"/> class.
        /// </summary>
        /// <param name="fieldInfo">The field to represent.</param>
        /// <param name="serializationBackend">The serialization backend.</param>
        /// <param name="allowEditable">Whether the property can be editable.</param>
        public InspectorValuePropertyInfo(FieldInfo fieldInfo, SerializationBackend serializationBackend, bool allowEditable)
            : base(fieldInfo, fieldInfo.FieldType.IsValueType ? PropertyType.ValueType : PropertyType.ReferenceType, serializationBackend, allowEditable)
        {
            MemberAliasFieldInfo aliasFieldInfo = fieldInfo as MemberAliasFieldInfo;

            if (aliasFieldInfo != null)
            {
                fieldInfo = aliasFieldInfo.AliasedField;
            }

            this.getter = EmitUtilities.CreateInstanceFieldGetter<TOwner, TValue>(fieldInfo);
            this.setter = EmitUtilities.CreateInstanceFieldSetter<TOwner, TValue>(fieldInfo);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InspectorValuePropertyInfo{TOwner, TValue}"/> class.
        /// </summary>
        /// <param name="propertyInfo">The property to represent.</param>
        /// <param name="serializationBackend">The serialization backend.</param>
        /// <param name="allowEditable">Whether the property can be editable.</param>
        public InspectorValuePropertyInfo(PropertyInfo propertyInfo, SerializationBackend serializationBackend, bool allowEditable)
            : base(propertyInfo, propertyInfo.PropertyType.IsValueType ? PropertyType.ValueType : PropertyType.ReferenceType, serializationBackend, allowEditable)

        {
            MemberAliasPropertyInfo aliasPropertyInfo = propertyInfo as MemberAliasPropertyInfo;

            if (aliasPropertyInfo != null)
            {
                propertyInfo = aliasPropertyInfo.AliasedProperty;
            }

            this.getter = EmitUtilities.CreateInstancePropertyGetter<TOwner, TValue>(propertyInfo);

            if (propertyInfo.CanWrite)
            {
                this.setter = EmitUtilities.CreateInstancePropertySetter<TOwner, TValue>(propertyInfo);
            }
        }

        /// <summary>
        /// Gets the value of this property from the given owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public override object GetValue(object owner)
        {
            TOwner castOwner = (TOwner)owner;
            return this.GetValue(ref castOwner);
        }

        /// <summary>
        /// Gets the value of this property from the given owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <exception cref="System.ArgumentNullException">owner is null</exception>
        public TValue GetValue(ref TOwner owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            return this.getter(ref owner);
        }

        /// <summary>
        /// Sets the value of this property on the given owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="value">The value.</param>
        public override void SetValue(object owner, object value)
        {
            if (this.setter != null)
            {
                TOwner castOwner = (TOwner)owner;
                this.setter(ref castOwner, (TValue)value);
            }
            else
            {
                UnityEngine.Debug.LogError("Someone tried to set a value to a read-only property.");
            }
        }

        /// <summary>
        /// Sets the value of this property on the given owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">owner is null</exception>
        public void SetValue(ref TOwner owner, TValue value)
        {
            if (this.setter != null)
            {
                if (owner == null)
                {
                    throw new ArgumentNullException("owner");
                }

                if (this.SerializationBackend == SerializationBackend.Unity)
                {
                    //Debug.LogWarning("You are setting the instance value directly on a property backed by Unity's serialization. This is a bad idea, and will probably break undo.");
                }

                this.setter(ref owner, value);
            }
            else
            {
                UnityEngine.Debug.LogError("Someone tried to set a value to a read-only property.");
            }
        }

        /// <summary>
        /// <para>Tries to convert this property to a strongly typed <see cref="IValueGetterSetter{TOwner, TValue}" />.</para>
        /// <para>A polymorphic alias <see cref="AliasGetterSetter{TOwner, TValue, TPropertyOwner, TPropertyValue}" /> will be created if necessary.</para>
        /// </summary>
        /// <typeparam name="TOwner2">The type of the owner.</typeparam>
        /// <typeparam name="TValue2">The type of the value.</typeparam>
        /// <param name="getterSetter">The converted getter setter.</param>
        /// <returns>True if the conversion succeeded, otherwise false.</returns>
        public override bool TryConvertToGetterSetter<TOwner2, TValue2>(out IValueGetterSetter<TOwner2, TValue2> getterSetter)
        {
            if (typeof(TOwner) == typeof(TOwner2) && typeof(TValue) == typeof(TValue2))
            {
                getterSetter = this as IValueGetterSetter<TOwner2, TValue2>;
                return true;
            }

            if (typeof(TOwner).IsAssignableFrom(typeof(TOwner2)) && typeof(TValue).IsAssignableFrom(typeof(TValue2)))
            {
                getterSetter = new AliasGetterSetter<TOwner2, TValue2, TOwner, TValue>(this);
                return true;
            }

            getterSetter = null;
            return false;
        }
    }
}
#endif