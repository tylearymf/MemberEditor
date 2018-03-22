//-----------------------------------------------------------------------
// <copyright file="EmptyTypeFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Serialization
{
    /// <summary>
    /// A formatter for empty types. It writes no data, and skips all data that is to be read, deserializing a "default" value.
    /// </summary>
    public class EmptyTypeFormatter<T> : EasyBaseFormatter<T>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        /// <param name="value">Not yet documented.</param>
        /// <param name="entryName">Not yet documented.</param>
        /// <param name="entryType">Not yet documented.</param>
        /// <param name="reader">Not yet documented.</param>
        protected override void ReadDataEntry(ref T value, string entryName, EntryType entryType, IDataReader reader)
        {
            // Just skip
            reader.SkipEntry();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        /// <param name="value">Not yet documented.</param>
        /// <param name="writer">Not yet documented.</param>
        protected override void WriteDataEntries(ref T value, IDataWriter writer)
        {
            // Do nothing
        }
    }
}