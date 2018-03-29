#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AliasGetterSetter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;

    /// <summary>
    /// A polymorphic alias for getting and setting the values of an <see cref="InspectorValuePropertyInfo{TPropertyOwner, TPropertyValue}" />.
    /// </summary>
    /// <typeparam name="TOwner">The type of the owner.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TPropertyOwner">The type of the property owner.</typeparam>
    /// <typeparam name="TPropertyValue">The type of the property value.</typeparam>
    public class AliasGetterSetter<TOwner, TValue, TPropertyOwner, TPropertyValue> : IValueGetterSetter<TOwner, TValue>
    {
        private InspectorValuePropertyInfo<TPropertyOwner, TPropertyValue> info;

        /// <summary>
        /// Initializes a new instance of the <see cref="AliasGetterSetter{TOwner, TValue, TPropertyOwner, TPropertyValue}"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <exception cref="System.ArgumentNullException">info</exception>
        public AliasGetterSetter(InspectorValuePropertyInfo<TPropertyOwner, TPropertyValue> info)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            this.info = info;
        }

        /// <summary>
        /// Gets the value from a given weakly typed owner.
        /// </summary>
        /// <param name="owner">The weakly typed owner.</param>
        /// <returns>The found value.</returns>
        public object GetValue(object owner)
        {
            TOwner castOwner = (TOwner)owner;
            return this.GetValue(ref castOwner);
        }

        /// <summary>
        /// Gets the value from a given owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The found value.</returns>
        /// <exception cref="System.ArgumentNullException">owner is null</exception>
        public TValue GetValue(ref TOwner owner)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            TPropertyOwner castOwner = (TPropertyOwner)(object)owner;
            return (TValue)(object)this.info.GetValue(ref castOwner);
        }

        /// <summary>
        /// Sets the weakly typed value on a given weakly typed owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="value">The value.</param>
        public void SetValue(object owner, object value)
        {
            TOwner castOwner = (TOwner)owner;
            this.SetValue(ref castOwner, (TValue)value);
        }

        /// <summary>
        /// Sets the value on a given owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="value">The value.</param>
        public void SetValue(ref TOwner owner, TValue value)
        {
            TPropertyOwner castOwner = (TPropertyOwner)(object)owner;
            this.info.SetValue(ref castOwner, (TPropertyValue)(object)value);
            owner = (TOwner)(object)castOwner;
        }
    }
}
#endif