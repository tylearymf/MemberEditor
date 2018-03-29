#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AtomHandlerLocator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
    using System;
    using System.Linq;
    using Sirenix.Utilities;
    using System.Collections.Generic;
    using UnityEngine;

    public static class AtomHandlerLocator
    {
        private static readonly Dictionary<Type, Type> AtomHandlerTypes = new Dictionary<Type, Type>();
        private static readonly Dictionary<Type, IAtomHandler> AtomHandlers = new Dictionary<Type, IAtomHandler>();

        static AtomHandlerLocator()
        {
            var atomHandlers = AppDomain.CurrentDomain
                                        .GetAssemblies()
                                        .Where(ass => ass.IsDefined(typeof(AtomContainerAttribute), true))
                                        .SelectMany(ass => ass.GetTypes())
                                        .Where(type => !type.IsAbstract && type.IsDefined(typeof(AtomHandlerAttribute), false) && type.GetConstructor(Type.EmptyTypes) != null && typeof(IAtomHandler).IsAssignableFrom(type) && type.ImplementsOpenGenericInterface(typeof(IAtomHandler<>)));

            foreach (var handler in atomHandlers)
            {
                var atomicType = handler.GetArgumentsOfInheritedOpenGenericInterface(typeof(IAtomHandler<>))[0];

                if (atomicType.IsAbstract)
                {
                    Debug.LogError("The type '" + atomicType.GetNiceName() + "' cannot be marked atomic, as it is abstract.");
                    continue;
                }

                AtomHandlerTypes.Add(atomicType, handler);
            }
        }

        public static bool IsMarkedAtomic(this Type type)
        {
            return AtomHandlerTypes.ContainsKey(type);
        }

        public static IAtomHandler GetAtomHandler(Type type)
        {
            if (!AtomHandlerTypes.ContainsKey(type))
            {
                return null;
            }

            IAtomHandler result;

            if (!AtomHandlers.TryGetValue(type, out result))
            {
                result = (IAtomHandler)Activator.CreateInstance(AtomHandlerTypes[type]);
                AtomHandlers[type] = result;
            }

            return result;
        }

        public static IAtomHandler<T> GetAtomHandler<T>()
        {
            return (IAtomHandler<T>)GetAtomHandler(typeof(T));
        }
    }
}
#endif