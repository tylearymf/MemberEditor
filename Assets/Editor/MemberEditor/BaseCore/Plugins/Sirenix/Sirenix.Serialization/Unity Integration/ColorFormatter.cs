//-----------------------------------------------------------------------
// <copyright file="ColorFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Serialization
{
    using UnityEngine;

    /// <summary>
    /// Custom formatter for the <see cref="Color"/> type.
    /// </summary>
    /// <seealso cref="Sirenix.Serialization.MinimalBaseFormatter{UnityEngine.Color}" />
    [CustomFormatter]
    public class ColorFormatter : MinimalBaseFormatter<Color>
    {
        private static readonly Serializer<float> Serializer = Serialization.Serializer.Get<float>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref Color value, IDataReader reader)
        {
            value.r = ColorFormatter.Serializer.ReadValue(reader);
            value.g = ColorFormatter.Serializer.ReadValue(reader);
            value.b = ColorFormatter.Serializer.ReadValue(reader);
            value.a = ColorFormatter.Serializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref Color value, IDataWriter writer)
        {
            ColorFormatter.Serializer.WriteValue(value.r, writer);
            ColorFormatter.Serializer.WriteValue(value.g, writer);
            ColorFormatter.Serializer.WriteValue(value.b, writer);
            ColorFormatter.Serializer.WriteValue(value.a, writer);
        }
    }
}