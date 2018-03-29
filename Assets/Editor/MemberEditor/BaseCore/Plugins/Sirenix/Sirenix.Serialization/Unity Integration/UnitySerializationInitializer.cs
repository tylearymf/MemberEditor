//-----------------------------------------------------------------------
// <copyright file="UnitySerializationInitializer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Serialization
{
    using Sirenix.Utilities;
    using System;
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// Utility class which initializes the Sirenix serialization system to be compatible with Unity.
    /// </summary>
    public static class UnitySerializationInitializer
    {
        private static readonly object LOCK = new object();
        private static bool initialized = false;

        /// <summary>
        /// Initializes the Sirenix serialization system to be compatible with Unity.
        /// </summary>
        private static void Initialize()
        {
            if (!initialized)
            {
                lock (LOCK)
                {
                    if (!initialized)
                    {
                        // Ensure that the config instance is loaded before deserialization of anything occurs.
                        // If we try to load it during deserialization, Unity will throw exceptions, as a lot of
                        // the Unity API is disallowed during serialization and deserialization.
                        GlobalSerializationConfig.LoadInstanceIfAssetExists();

                        // Custom UnityEvent formatter resolution
                        FormatterLocator.FormatterResolve += (type) =>
                        {
                            if (type != typeof(UnityEvent)
                                && type.ImplementsOrInherits(typeof(UnityEventBase))
                                && (type.ImplementsOrInherits(typeof(UnityEvent))
                                || type.ImplementsOpenGenericClass(typeof(UnityEvent<>))
                                || type.ImplementsOpenGenericClass(typeof(UnityEvent<,>))
                                || type.ImplementsOpenGenericClass(typeof(UnityEvent<,,>))
                                || type.ImplementsOpenGenericClass(typeof(UnityEvent<,,,>))))
                            {
                                return (IFormatter)Activator.CreateInstance(typeof(UnityEventFormatter<>).MakeGenericType(type));
                            }

                            return null;
                        };

                        initialized = true;
                    }
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeRuntime()
        {
            Initialize();
        }

#if UNITY_EDITOR

        [UnityEditor.InitializeOnLoadMethod]
#endif
        private static void InitializeEditor()
        {
            Initialize();
        }
    }
}