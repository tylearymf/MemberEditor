#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UnityOnlyPropertyInfo.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;

    /// <summary>
    /// Contains meta-data information about a Unity-only "ghost" property in the inspector, which is only represented by a Unity <see cref="UnityEditor.SerializedProperty"/>, and has no managed member representation at all.
    /// </summary>
    public class UnityOnlyPropertyInfo : InspectorPropertyInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnityOnlyPropertyInfo"/> class.
        /// </summary>
        /// <param name="unityPropertyName">Name of the unity property.</param>
        /// <param name="typeOfOwner">The type of owner.</param>
        /// <param name="typeOfValue">The type of value.</param>
        /// <param name="isEditable">Whether the property is editable.</param>
        public UnityOnlyPropertyInfo(string unityPropertyName, Type typeOfOwner, Type typeOfValue, bool isEditable)
            : base(unityPropertyName, typeOfOwner, typeOfValue, isEditable)
        {
        }

        /// <summary>
        /// Whether this property only exists as a Unity <see cref="SerializedProperty" />, and has no associated managed member to represent it.
        /// </summary>
        public override bool IsUnityPropertyOnly { get { return true; } }

        /// <summary>
        /// Gets the value of this property from the given owner. This method will throw an exception.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public override object GetValue(object owner)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the value of this property on the given owner. This method will throw an exception.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public override void SetValue(object owner, object value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns false and a null getter setter.
        /// </summary>
        /// <typeparam name="TOwner1">The type of the owner2.</typeparam>
        /// <typeparam name="TValue1">The type of the value.</typeparam>
        /// <param name="getterSetter">The getter setter.</param>
        /// <returns></returns>
        public override bool TryConvertToGetterSetter<TOwner1, TValue1>(out IValueGetterSetter<TOwner1, TValue1> getterSetter)
        {
            getterSetter = null;
            return false;
        }
    }
}
#endif