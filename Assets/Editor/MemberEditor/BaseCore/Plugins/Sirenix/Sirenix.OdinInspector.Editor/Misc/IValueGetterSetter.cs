#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="IValueGetterSetter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    /// <summary>
    /// Used by all InspectorProperty to tell Odin how to set or get a value on any given property.
    /// </summary>
    public interface IValueGetterSetter
    {
        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="value">The value.</param>
        void SetValue(object owner, object value);

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <returns></returns>
        object GetValue(object owner);
    }

    /// <summary>
    /// Used by all <see cref="AliasGetterSetter{TOwner, TValue, TPropertyOwner, TPropertyValue}"/> to tell Odin how to set or get a value on any given property.
    /// </summary>
    public interface IValueGetterSetter<TOwner, TValue> : IValueGetterSetter
    {
        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="value">The value.</param>
        void SetValue(ref TOwner owner, TValue value);

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <returns></returns>
        TValue GetValue(ref TOwner owner);
    }
}
#endif