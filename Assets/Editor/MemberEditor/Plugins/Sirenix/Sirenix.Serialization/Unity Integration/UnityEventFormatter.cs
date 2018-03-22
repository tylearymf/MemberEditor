//-----------------------------------------------------------------------
// <copyright file="UnityEventFormatter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Serialization
{
    using Sirenix.Utilities;
    using System;
    using UnityEngine.Events;

    /// <summary>
    /// Custom formatter for the <see cref="UnityEvent"/> type.
    /// </summary>
    /// <seealso cref="Sirenix.Serialization.ReflectionFormatter{UnityEngine.Events.UnityEvent}" />
    [CustomFormatter]
    public class UnityEventFormatter : ReflectionFormatter<UnityEvent>
    {
        /// <summary>
        /// Gets a new UnityEvent instance.
        /// </summary>
        /// <returns>
        /// A new UnityEvent instance.
        /// </returns>
        protected override UnityEvent GetUninitializedObject()
        {
            return new UnityEvent();
        }
    }

    /// <summary>
    /// Custom generic formatter for the <see cref="UnityEvent{T0}"/>, <see cref="UnityEvent{T0, T1}"/>, <see cref="UnityEvent{T0, T1, T2}"/> and <see cref="UnityEvent{T0, T1, T2, T3}"/> types.
    /// </summary>
    /// <typeparam name="T">The type of UnityEvent that this formatter can serialize and deserialize.</typeparam>
    /// <seealso cref="Sirenix.Serialization.ReflectionFormatter{UnityEngine.Events.UnityEvent}" />
    [CustomFormatter]
    public class UnityEventFormatter<T> : ReflectionFormatter<T> where T : class, new()
    {
        static UnityEventFormatter()
        {
            Type type = typeof(T);

            if (!(type != typeof(UnityEvent)
                && type.ImplementsOrInherits(typeof(UnityEventBase))
                && (type.ImplementsOrInherits(typeof(UnityEvent))
                || type.ImplementsOpenGenericClass(typeof(UnityEvent<>))
                || type.ImplementsOpenGenericClass(typeof(UnityEvent<,>))
                || type.ImplementsOpenGenericClass(typeof(UnityEvent<,,>))
                || type.ImplementsOpenGenericClass(typeof(UnityEvent<,,,>)))))
            {
                throw new ArgumentException("Cannot create a UnityEventFormatter for type " + typeof(T).Name);
            }
        }

        /// <summary>
        /// Get an uninitialized object of type <see cref="T" />.
        /// </summary>
        /// <returns>
        /// An uninitialized object of type <see cref="T" />.
        /// </returns>
        protected override T GetUninitializedObject()
        {
            return new T();
        }
    }
}