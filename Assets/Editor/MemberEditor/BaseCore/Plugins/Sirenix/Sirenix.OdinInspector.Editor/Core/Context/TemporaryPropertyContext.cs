#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TemporaryPropertyContext.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using Sirenix.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// <para>A temporary contextual value attached to an <see cref="InspectorProperty"/>, mapped to a key, contained in a <see cref="PropertyContextContainer"/>.</para>
    /// <para>The value of a <see cref="TemporaryPropertyContext{T}"/> will be reset at the start of each GUI frame.</para>
    /// </summary>
    /// <seealso cref="PropertyContextContainer"/>
    public sealed class TemporaryPropertyContext<T> : ITemporaryContext
    {
        private static Action<object> clearMethod;
        private static bool clearMethodSearched;

        private static readonly bool IsITemporaryContext = typeof(T).ImplementsOrInherits(typeof(ITemporaryContext));
        private static readonly bool IsValueType = typeof(T).IsValueType;

        private static Action<object> ClearMethod
        {
            get
            {
                if (!clearMethodSearched)
                {
                    clearMethodSearched = true;

                    if (typeof(T).IsArray)
                    {
                        Type elementType = typeof(T).GetElementType();
                        object defaultValue = elementType.IsValueType ? Activator.CreateInstance(elementType) : null;

                        clearMethod = (obj) =>
                        {
                            Array array = obj as Array;

                            for (int i = 0; i < array.Length; i++)
                            {
                                // No boxing here in case of value types - only unboxing! No garbage generated.
                                array.SetValue(defaultValue, i);
                            }
                        };
                    }
                    else
                    {
                        if (typeof(T).ImplementsOrInherits(typeof(IList)))
                        {
                            var clearMethodInfo = typeof(IList).GetMethod("Clear", Flags.InstanceAnyVisibility);

                            if (clearMethodInfo != null)
                            {
                                clearMethod = (obj) => clearMethodInfo.Invoke(obj, null);
                            }
                        }
                        else if (typeof(T).ImplementsOpenGenericInterface(typeof(ICollection<>)))
                        {
                            Type arg = typeof(T).GetArgumentsOfInheritedOpenGenericInterface(typeof(ICollection<>))[0];
                            var clearMethodInfo = typeof(ICollection<>).MakeGenericType(arg).GetMethod("Clear", Flags.InstanceAnyVisibility);

                            if (clearMethodInfo != null)
                            {
                                clearMethod = (obj) => clearMethodInfo.Invoke(obj, null);
                            }
                        }
                    }
                }

                return clearMethod;
            }
        }

        /// <summary>
        /// The contained value.
        /// </summary>
        public T Value;

        /// <summary>
        /// Performs an explicit conversion from <see cref="TemporaryPropertyContext{T}"/> to <see cref="T"/>.
        /// </summary>
        /// <param name="context">The configuration.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator T(TemporaryPropertyContext<T> context)
        {
            if (context == null)
            {
                return default(T);
            }
            else
            {
                return context.Value;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance, of the format "<see cref="TemporaryPropertyContext{T}"/>: Value.ToString()".
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.GetType().GetNiceName() + ": " + this.Value;
        }

        /// <summary>
        /// Resets the value of the context.
        /// </summary>
        public void Reset()
        {
            if (IsValueType)
            {
                this.Value = default(T);
            }
            else if (this.Value != null)
            {
                bool reset = false;

                if (IsITemporaryContext)
                {
                    ((ITemporaryContext)this.Value).Reset();
                    reset = true;
                }
                else if (ClearMethod != null)
                {
                    ClearMethod(this.Value);
                    reset = true;
                }

                if (!reset)
                {
                    this.Value = default(T);
                }
            }
        }
    }
}
#endif