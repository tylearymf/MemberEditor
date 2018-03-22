//-----------------------------------------------------------------------
// <copyright file="DictionaryFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
    using Sirenix.Utilities;
    using System;

    /// <summary>
    /// Custom Odin serialization formatter for <see cref="IndexedDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [CustomGenericFormatter(typeof(IndexedDictionary<,>))]
    public sealed class IndexedDictionaryFormatter<TKey, TValue> : BaseFormatter<IndexedDictionary<TKey, TValue>>
    {
        private static readonly Serializer<TKey> KeyReaderWriter = Serializer.Get<TKey>();
        private static readonly Serializer<TValue> ValueReaderWriter = Serializer.Get<TValue>();

        static IndexedDictionaryFormatter()
        {
            // This exists solely to prevent IL2CPP code stripping from removing the generic type's instance constructor
            // which it otherwise seems prone to do, regardless of what might be defined in any link.xml file.

            new DictionaryFormatter<int, string>();
        }

        /// <summary>
        /// Creates a new instance of <see cref="IndexedDictionaryFormatter{TKey, TValue}"/>.
        /// </summary>
        public IndexedDictionaryFormatter()
        { }

        /// <summary>
        /// Returns null.
        /// </summary>
        protected override IndexedDictionary<TKey, TValue> GetUninitializedObject()
        {
            return null;
        }

        /// <summary>
        /// Deserialization implementation.
        /// </summary>
        /// <param name="value">The value to deserialize.</param>
        /// <param name="reader">The reader to read from.</param>
        protected override void DeserializeImplementation(ref IndexedDictionary<TKey, TValue> value, IDataReader reader)
        {
            string name;
            var entry = reader.PeekEntry(out name);

            if (entry == EntryType.StartOfArray)
            {
                try
                {
                    long length;
                    reader.EnterArray(out length);
                    Type type;
                    value = new IndexedDictionary<TKey, TValue>((int)length);

                    // We must remember to register the dictionary reference ourselves, since we return null in GetUninitializedObject
                    this.RegisterReferenceID(value, reader);

                    for (int i = 0; i < length; i++)
                    {
                        if (reader.PeekEntry(out name) == EntryType.EndOfArray)
                        {
                            reader.Context.Config.DebugContext.LogError("Reached end of array after " + i + " elements, when " + length + " elements were expected.");
                            break;
                        }

                        bool exitNode = true;

                        try
                        {
                            reader.EnterNode(out type);
                            TKey key = KeyReaderWriter.ReadValue(reader);
                            TValue val = ValueReaderWriter.ReadValue(reader);

                            value.Add(key, val);
                            //value[key] = val;
                        }
                        catch (SerializationAbortException ex)
                        {
                            exitNode = false;
                            throw ex;
                        }
                        catch (Exception ex)
                        {
                            reader.Context.Config.DebugContext.LogException(ex);
                        }
                        finally
                        {
                            if (exitNode)
                            {
                                reader.ExitNode();
                            }
                        }

                        if (reader.IsInArrayNode == false)
                        {
                            reader.Context.Config.DebugContext.LogError("Reading array went wrong at position " + reader.Stream.Position + ".");
                            break;
                        }
                    }
                }
                finally
                {
                    reader.ExitArray();
                }
            }
            else
            {
                reader.SkipEntry();
            }
        }

        /// <summary>
        /// Serialization implementation.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="writer">The writer to write to.</param>
        protected override void SerializeImplementation(ref IndexedDictionary<TKey, TValue> value, IDataWriter writer)
        {
            try
            {
                writer.BeginArrayNode(value.Count);

                bool endNode = true;

                for (int i = 0; i < value.Count; i++)
                {
                    var entry = value.Get(i);

                    try
                    {
                        writer.BeginStructNode(null, null);
                        KeyReaderWriter.WriteValue(entry.Key, writer);
                        ValueReaderWriter.WriteValue(entry.Value, writer);
                    }
                    catch (SerializationAbortException ex)
                    {
                        endNode = false;
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        writer.Context.Config.DebugContext.LogException(ex);
                    }
                    finally
                    {
                        if (endNode)
                        {
                            writer.EndNode(null);
                        }
                    }
                }
            }
            finally
            {
                writer.EndArrayNode();
            }
        }
    }
}