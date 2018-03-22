//-----------------------------------------------------------------------
// <copyright file="EnumSerializer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Globalization;

namespace Sirenix.Serialization
{
    using System;

    /// <summary>
    /// Serializer for all enums.
    /// </summary>
    /// <typeparam name="T">The type of the enum to serialize and deserialize.</typeparam>
    /// <seealso cref="Sirenix.Serialization.Serializer{T}" />
    public sealed class EnumSerializer<T> : Serializer<T>
    {
        static EnumSerializer()
        {
            if (typeof(T).IsEnum == false)
            {
                throw new Exception("Type " + typeof(T).Name + " is not an enum.");
            }
        }

        /// <summary>
        /// Reads an enum value of type <see cref="T" />.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <returns>
        /// The value which has been read.
        /// </returns>
        public override T ReadValue(IDataReader reader)
        {
            string name;
            var entry = reader.PeekEntry(out name);

            if (entry == EntryType.Integer)
            {
                ulong value;
                if (reader.ReadUInt64(out value) == false)
                {
                    reader.Context.Config.DebugContext.LogWarning("Failed to read entry '" + name + "' of type " + entry.ToString());
                }
                return (T)Enum.ToObject(typeof(T), value);
            }
            else
            {
                reader.Context.Config.DebugContext.LogWarning("Expected entry of type " + EntryType.Integer.ToString() + ", but got entry '" + name + "' of type " + entry.ToString());
                reader.SkipEntry();
                return default(T);
            }
        }

        /// <summary>
        /// Writes an enum value of type <see cref="T" />.
        /// </summary>
        /// <param name="name">The name of the value to write.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="writer">The writer to use.</param>
        public override void WriteValue(string name, T value, IDataWriter writer)
        {
            ulong ul;

            FireOnSerializedType();

            try
            {
                ul = Convert.ToUInt64(value as Enum, CultureInfo.InvariantCulture);
            }
            catch (OverflowException)
            {
                unchecked
                {
                    ul = (ulong)Convert.ToInt64(value as Enum, CultureInfo.InvariantCulture);
                }
            }

            writer.WriteUInt64(name, ul);
        }
    }
}