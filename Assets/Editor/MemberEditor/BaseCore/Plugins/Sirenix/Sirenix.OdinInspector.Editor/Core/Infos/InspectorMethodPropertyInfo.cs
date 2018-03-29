#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorMethodPropertyInfo.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Contains meta-data information about a method-backed property in the inspector.
    /// </summary>
    public class InspectorMethodPropertyInfo<TOwner> : InspectorPropertyInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InspectorMethodPropertyInfo{TOwner}"/> class.
        /// </summary>
        /// <param name="methodInfo">The method to represent.</param>
        public InspectorMethodPropertyInfo(MethodInfo methodInfo)
            : base(methodInfo, PropertyType.Method, SerializationBackend.None, false)
        {
        }

        /// <summary>
        /// Gets the value of this property from the given owner. This method will throw an exception.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Cannot get the value of a property of type PropertyType.Method.</exception>
        public override object GetValue(object owner)
        {
            throw new InvalidOperationException("Cannot get the value of a property of type PropertyType.Method.");
        }

        /// <summary>
        /// Sets the value of this property on the given owner. This method will throw an exception.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.InvalidOperationException">Cannot set the value of a property of type PropertyType.Method.</exception>
        public override void SetValue(object owner, object value)
        {
            throw new InvalidOperationException("Cannot set the value of a property of type PropertyType.Method.");
        }

        /// <summary>
        /// Returns false and a null getter setter.
        /// </summary>
        /// <typeparam name="TOwner2">The type of the owner2.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="getterSetter">The getter setter.</param>
        /// <returns></returns>
        public override bool TryConvertToGetterSetter<TOwner2, TValue>(out IValueGetterSetter<TOwner2, TValue> getterSetter)
        {
            getterSetter = null;
            return false;
        }
    }
}
#endif