//-----------------------------------------------------------------------
// <copyright file="TimeSpanFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Serialization
{
    using System;

    /// <summary>
    /// Custom formatter for the <see cref="TimeSpan"/> type.
    /// </summary>
    /// <seealso cref="Sirenix.Serialization.MinimalBaseFormatter{System.TimeSpan}" />
    [CustomFormatter]
    public sealed class TimeSpanFormatter : MinimalBaseFormatter<TimeSpan>
    {
        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref TimeSpan value, IDataReader reader)
        {
            string name;

            if (reader.PeekEntry(out name) == EntryType.Integer)
            {
                long ticks;
                reader.ReadInt64(out ticks);
                value = new TimeSpan(ticks);
            }
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref TimeSpan value, IDataWriter writer)
        {
            writer.WriteInt64(null, value.Ticks);
        }
    }
}