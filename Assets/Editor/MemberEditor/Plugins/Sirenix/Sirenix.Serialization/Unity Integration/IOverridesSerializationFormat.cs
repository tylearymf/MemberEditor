//-----------------------------------------------------------------------
// <copyright file="IOverridesSerializationFormat.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
    /// <summary>
    /// Not yet documented.
    /// </summary>
    public interface IOverridesSerializationFormat
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        DataFormat GetFormatToSerializeAs(bool isPlayer);
    }
}