//-----------------------------------------------------------------------
// <copyright file="SerializationPolicies.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
    using Utilities;
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    /// <summary>
    /// Contains a set of default implementations of the <see cref="ISerializationPolicy"/> interface.
    /// <para />
    /// NOTE: Policies are not necessarily compatible with each other in intuitive ways.
    /// Data serialized with the <see cref="SerializationPolicies.Everything"/> policy
    /// will for example fail to deserialize auto-properties with <see cref="SerializationPolicies.Strict"/>,
    /// even if only strict data is needed.
    /// It is best to ensure that you always use the same policy for serialization and deserialization.
    /// <para />
    /// This class and all of its policies are thread-safe.
    /// </summary>
    public static class SerializationPolicies
    {
        private static readonly object LOCK = new object();

        private static volatile ISerializationPolicy everythingPolicy;
        private static volatile ISerializationPolicy unityPolicy;
        private static volatile ISerializationPolicy strictPolicy;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static bool TryGetByID(string name, out ISerializationPolicy policy)
        {
            switch (name)
            {
                case "Sirenix.SerializationPolicies.Everything":
                    policy = SerializationPolicies.Everything;
                    break;

                case "Sirenix.SerializationPolicies.Unity":
                    policy = SerializationPolicies.Unity;
                    break;

                case "Sirenix.SerializationPolicies.Strict":
                    policy = SerializationPolicies.Strict;
                    break;

                default:
                    policy = null;
                    break;
            }

            return policy != null;
        }

        /// <summary>
        /// All fields not marked with <see cref="NonSerializedAttribute"/> are serialized. If a field is marked with both <see cref="NonSerializedAttribute"/> and <see cref="OdinSerializeAttribute"/>, then the field will be serialized.
        /// </summary>
        public static ISerializationPolicy Everything
        {
            get
            {
                if (everythingPolicy == null)
                {
                    lock (LOCK)
                    {
                        if (everythingPolicy == null)
                        {
                            everythingPolicy = new CustomSerializationPolicy("Sirenix.SerializationPolicies.Everything", true, (member) =>
                            {
                                if (!(member is FieldInfo))
                                {
                                    return false;
                                }

                                if (member.IsDefined<OdinSerializeAttribute>(true))
                                {
                                    return true;
                                }

                                return !member.IsDefined<NonSerializedAttribute>(true);
                            });
                        }
                    }
                }

                return everythingPolicy;
            }
        }

        /// <summary>
        /// Public fields and fields or auto-properties marked with <see cref="SerializeField"/> or <see cref="OdinSerializeAttribute"/> and not marked with <see cref="NonSerializedAttribute"/> are serialized.
        /// <para />
        /// There are two exceptions: all fields in tuples, as well as in private nested types marked as compiler generated (e.g. lambda capture classes) are also serialized.
        /// </summary>
        public static ISerializationPolicy Unity
        {
            get
            {
                if (unityPolicy == null)
                {
                    lock (LOCK)
                    {
                        if (unityPolicy == null)
                        {
                            // In Unity 2017.1's .NET 4.6 profile, Tuples implement System.ITuple. In Unity 2017.2 and up, tuples implement System.ITupleInternal instead for some reason.
                            Type tupleInterface = typeof(string).Assembly.GetType("System.ITuple") ?? typeof(string).Assembly.GetType("System.ITupleInternal");

                            unityPolicy = new CustomSerializationPolicy("Sirenix.SerializationPolicies.Unity", true, (member) =>
                            {
                                // If SerializeAttribute is defined, NonSerializedAttribute is ignored.
                                // This enables users to ignore Unity's infinite serialization depth warnings.
                                if (member.IsDefined<NonSerializedAttribute>(true) && !member.IsDefined<OdinSerializeAttribute>())
                                {
                                    return false;
                                }

                                if (member is FieldInfo && ((member as FieldInfo).IsPublic || (member.DeclaringType.IsNestedPrivate && member.DeclaringType.IsDefined<CompilerGeneratedAttribute>()) || (tupleInterface != null && tupleInterface.IsAssignableFrom(member.DeclaringType))))
                                {
                                    return true;
                                }

                                return member.IsDefined<SerializeField>(true) || member.IsDefined<OdinSerializeAttribute>(true);
                            });
                        }
                    }
                }

                return unityPolicy;
            }
        }

        /// <summary>
        /// Only fields and auto-properties marked with <see cref="SerializeField"/> or <see cref="OdinSerializeAttribute"/> and not marked with <see cref="NonSerializedAttribute"/> are serialized.
        /// <para />
        /// There is one exception: all fields in private nested types marked as compiler generated (e.g. lambda capture classes) are also serialized.
        /// </summary>
        public static ISerializationPolicy Strict
        {
            get
            {
                if (strictPolicy == null)
                {
                    lock (LOCK)
                    {
                        if (strictPolicy == null)
                        {
                            strictPolicy = new CustomSerializationPolicy("Sirenix.SerializationPolicies.Strict", true, (member) =>
                            {
                                if (member.IsDefined<NonSerializedAttribute>())
                                {
                                    return false;
                                }

                                if (member is FieldInfo && member.DeclaringType.IsNestedPrivate && member.DeclaringType.IsDefined<CompilerGeneratedAttribute>())
                                {
                                    return true;
                                }

                                return member.IsDefined<SerializeField>(true) || member.IsDefined<OdinSerializeAttribute>(true);
                            });
                        }
                    }
                }

                return strictPolicy;
            }
        }
    }
}