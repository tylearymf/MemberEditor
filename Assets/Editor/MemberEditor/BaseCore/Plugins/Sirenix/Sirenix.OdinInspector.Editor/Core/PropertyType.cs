#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyType.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    /// <summary>
    /// Enumeration describing the different types of properties that exist.
    /// </summary>
    public enum PropertyType
    {
        /// <summary>
        /// Property represents a reference type value.
        /// </summary>
        ReferenceType,

        /// <summary>
        /// Property represents a value type value.
        /// </summary>
        ValueType,

        /// <summary>
        /// Property represents a method.
        /// </summary>
        Method,

        /// <summary>
        /// Property represents a named group of properties.
        /// </summary>
        Group
    }
}
#endif