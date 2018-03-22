//-----------------------------------------------------------------------
// <copyright file="SelfFormatterFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;

namespace Sirenix.Serialization
{
    /// <summary>
    /// Formatter for types that implement the <see cref="ISelfFormatter"/> interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Sirenix.Serialization.BaseFormatter{T}" />
    public sealed class SelfFormatterFormatter<T> : BaseFormatter<T> where T : ISelfFormatter
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DeserializeImplementation(ref T value, IDataReader reader)
        {
            value.Deserialize(reader);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void SerializeImplementation(ref T value, IDataWriter writer)
        {
            value.Serialize(writer);
        }
    }
}