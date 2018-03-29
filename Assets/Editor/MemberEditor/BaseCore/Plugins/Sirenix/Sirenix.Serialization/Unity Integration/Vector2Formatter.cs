//-----------------------------------------------------------------------
// <copyright file="Vector2Formatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Serialization
{
    using UnityEngine;

    /// <summary>
    /// Custom formatter for the <see cref="Vector2"/> type.
    /// </summary>
    /// <seealso cref="Sirenix.Serialization.MinimalBaseFormatter{UnityEngine.Vector2}" />
    [CustomFormatter]
    public class Vector2Formatter : MinimalBaseFormatter<Vector2>
    {
        private static readonly Serializer<float> Serializer = Serialization.Serializer.Get<float>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref Vector2 value, IDataReader reader)
        {
            value.x = Vector2Formatter.Serializer.ReadValue(reader);
            value.y = Vector2Formatter.Serializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref Vector2 value, IDataWriter writer)
        {
            Vector2Formatter.Serializer.WriteValue(value.x, writer);
            Vector2Formatter.Serializer.WriteValue(value.y, writer);
        }
    }
}