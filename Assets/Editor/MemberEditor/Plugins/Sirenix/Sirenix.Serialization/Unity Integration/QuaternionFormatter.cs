//-----------------------------------------------------------------------
// <copyright file="QuaternionFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Serialization
{
    using UnityEngine;

    /// <summary>
    /// Custom formatter for the <see cref="Quaternion"/> type.
    /// </summary>
    /// <seealso cref="Sirenix.Serialization.MinimalBaseFormatter{UnityEngine.Quaternion}" />
    [CustomFormatter]
    public class QuaternionFormatter : MinimalBaseFormatter<Quaternion>
    {
        private static readonly Serializer<float> Serializer = Serialization.Serializer.Get<float>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref Quaternion value, IDataReader reader)
        {
            value.x = QuaternionFormatter.Serializer.ReadValue(reader);
            value.y = QuaternionFormatter.Serializer.ReadValue(reader);
            value.z = QuaternionFormatter.Serializer.ReadValue(reader);
            value.w = QuaternionFormatter.Serializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref Quaternion value, IDataWriter writer)
        {
            QuaternionFormatter.Serializer.WriteValue(value.x, writer);
            QuaternionFormatter.Serializer.WriteValue(value.y, writer);
            QuaternionFormatter.Serializer.WriteValue(value.z, writer);
            QuaternionFormatter.Serializer.WriteValue(value.w, writer);
        }
    }
}