//-----------------------------------------------------------------------
// <copyright file="LayerMaskFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Serialization
{
    using UnityEngine;

    /// <summary>
    /// Custom formatter for the <see cref="LayerMask"/> type.
    /// </summary>
    /// <seealso cref="Sirenix.Serialization.MinimalBaseFormatter{UnityEngine.LayerMask}" />
    [CustomFormatter]
    public class LayerMaskFormatter : MinimalBaseFormatter<LayerMask>
    {
        private static readonly Serializer<int> Serializer = Serialization.Serializer.Get<int>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref LayerMask value, IDataReader reader)
        {
            value.value = LayerMaskFormatter.Serializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref LayerMask value, IDataWriter writer)
        {
            LayerMaskFormatter.Serializer.WriteValue(value.value, writer);
        }
    }
}