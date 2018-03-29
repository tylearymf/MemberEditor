#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SerializationBackend.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    /// <summary>
    /// Enumeration that describes the different possible serialization backends that a property can have.
    /// </summary>
    public enum SerializationBackend
    {
        /// <summary>
        /// The property is serialized by Unity. Polymorphism, null values and types such as <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> are not supported.
        /// </summary>
        Unity,

        /// <summary>
        /// The property is serialized by Odin. Polymorphism, null values and types such as <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> are supported.
        /// </summary>
        Odin,

        /// <summary>
        /// <para>The property is not serialized by anything - possibly because it is a method, possibly because it is a field or property shown in the inspector without being serialized.</para>
        /// <para>In the case of fields or properties, polymorphism, null values and types such as <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> are supported, but will not be saved.</para>
        /// </summary>
        None
    }
}
#endif