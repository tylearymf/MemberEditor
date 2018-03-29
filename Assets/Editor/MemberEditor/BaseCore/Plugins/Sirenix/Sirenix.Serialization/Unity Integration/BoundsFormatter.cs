//-----------------------------------------------------------------------
// <copyright file="BoundsFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Serialization
{
    using UnityEngine;

    /// <summary>
    /// Custom formatter for the <see cref="Bounds"/> type.
    /// </summary>
    /// <seealso cref="Sirenix.Serialization.MinimalBaseFormatter{UnityEngine.Bounds}" />
    [CustomFormatter]
    public class BoundsFormatter : MinimalBaseFormatter<Bounds>
    {
        private static readonly Serializer<Vector3> Serializer = Serialization.Serializer.Get<Vector3>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref Bounds value, IDataReader reader)
        {
            value.center = Serializer.ReadValue(reader);
            value.size = Serializer.ReadValue(reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref Bounds value, IDataWriter writer)
        {
            Serializer.WriteValue(value.center, writer);
            Serializer.WriteValue(value.size, writer);
        }
    }
}