//-----------------------------------------------------------------------
// <copyright file="Vector4Formatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Serialization
{
    using UnityEngine;

    /// <summary>
    /// Custom formatter for the <see cref="Vector4"/> type.
    /// </summary>
    /// <seealso cref="Sirenix.Serialization.MinimalBaseFormatter{UnityEngine.Vector4}" />
    [CustomFormatter]
    public class Vector4Formatter : MinimalBaseFormatter<Vector4>
    {
        private static readonly Serializer<float> Serializer = Serialization.Serializer.Get<float>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref Vector4 value, IDataReader reader)
        {
            value.x = Vector4Formatter.Serializer.ReadValue(reader);
            value.y = Vector4Formatter.Serializer.ReadValue(reader);
            value.z = Vector4Formatter.Serializer.ReadValue(reader);
            value.w = Vector4Formatter.Serializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref Vector4 value, IDataWriter writer)
        {
            Vector4Formatter.Serializer.WriteValue(value.x, writer);
            Vector4Formatter.Serializer.WriteValue(value.y, writer);
            Vector4Formatter.Serializer.WriteValue(value.z, writer);
            Vector4Formatter.Serializer.WriteValue(value.w, writer);
        }
    }
}