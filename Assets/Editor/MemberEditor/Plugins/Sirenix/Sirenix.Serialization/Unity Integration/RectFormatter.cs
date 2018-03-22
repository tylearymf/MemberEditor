//-----------------------------------------------------------------------
// <copyright file="RectFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Serialization
{
    using UnityEngine;

    /// <summary>
    /// Custom formatter for the <see cref="Rect"/> type.
    /// </summary>
    /// <seealso cref="Sirenix.Serialization.MinimalBaseFormatter{UnityEngine.Rect}" />
    [CustomFormatter]
    public class RectFormatter : MinimalBaseFormatter<Rect>
    {
        private static readonly Serializer<float> Serializer = Serialization.Serializer.Get<float>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref Rect value, IDataReader reader)
        {
            value.x = RectFormatter.Serializer.ReadValue(reader);
            value.y = RectFormatter.Serializer.ReadValue(reader);
            value.width = RectFormatter.Serializer.ReadValue(reader);
            value.height = RectFormatter.Serializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref Rect value, IDataWriter writer)
        {
            RectFormatter.Serializer.WriteValue(value.x, writer);
            RectFormatter.Serializer.WriteValue(value.y, writer);
            RectFormatter.Serializer.WriteValue(value.width, writer);
            RectFormatter.Serializer.WriteValue(value.height, writer);
        }
    }
}