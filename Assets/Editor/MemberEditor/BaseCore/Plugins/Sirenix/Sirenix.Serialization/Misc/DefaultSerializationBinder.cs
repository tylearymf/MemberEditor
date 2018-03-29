//-----------------------------------------------------------------------
// <copyright file="DefaultSerializationBinder.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Serialization
{
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides a default, catch-all <see cref="TwoWaySerializationBinder"/> implementation. This binder only includes assembly names, without versions and tokens, in order to increase compatibility.
    /// </summary>
    /// <seealso cref="Sirenix.Serialization.TwoWaySerializationBinder" />
    public class DefaultSerializationBinder : TwoWaySerializationBinder
    {
        private static readonly Dictionary<string, Assembly> assemblyNameLookUp = new Dictionary<string, Assembly>();

        private static readonly object TYPEMAP_LOCK = new object();
        private static readonly object NAMEMAP_LOCK = new object();
        private static readonly Dictionary<string, Type> typeMap = new Dictionary<string, Type>();
        private static readonly Dictionary<Type, string> nameMap = new Dictionary<Type, string>();

        static DefaultSerializationBinder()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = assembly.GetName().Name;

                if (!assemblyNameLookUp.ContainsKey(name))
                {
                    assemblyNameLookUp.Add(name, assembly);
                }
            }
        }

        /// <summary>
        /// Bind a type to a name.
        /// </summary>
        /// <param name="type">The type to bind.</param>
        /// <param name="debugContext">The debug context to log to.</param>
        /// <returns>
        /// The name that the type has been bound to.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">The type argument is null.</exception>
        public override string BindToName(Type type, DebugContext debugContext = null)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            string result;

            lock (NAMEMAP_LOCK)
            {
                if (nameMap.TryGetValue(type, out result) == false)
                {
                    if (type.IsGenericType)
                    {
                        // We track down all assemblies in the generic type definition
                        List<Type> toResolve = type.GetGenericArguments().ToList();
                        HashSet<Assembly> assemblies = new HashSet<Assembly>();

                        while (toResolve.Count > 0)
                        {
                            var t = toResolve[0];

                            if (t.IsGenericType)
                            {
                                toResolve.AddRange(t.GetGenericArguments());
                            }

                            assemblies.Add(t.Assembly);
                            toResolve.RemoveAt(0);
                        }

                        result = type.FullName + ", " + type.Assembly.GetName().Name;

                        foreach (var ass in assemblies)
                        {
                            result = result.Replace(ass.FullName, ass.GetName().Name);
                        }
                    }
                    else if (type.IsDefined(typeof(CompilerGeneratedAttribute), false))
                    {
                        result = type.FullName + ", " + type.Assembly.GetName().Name;
                    }
                    else
                    {
                        result = type.FullName + ", " + type.Assembly.GetName().Name;
                    }

                    nameMap.Add(type, result);
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the specified type name is mapped.
        /// </summary>
        public override bool ContainsType(string typeName)
        {
            lock (TYPEMAP_LOCK)
            {
                return typeMap.ContainsKey(typeName);
            }
        }

        /// <summary>
        /// Binds a name to type.
        /// </summary>
        /// <param name="typeName">The name of the type to bind.</param>
        /// <param name="debugContext">The debug context to log to.</param>
        /// <returns>
        /// The type that the name has been bound to, or null if the type could not be resolved.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">The typeName argument is null.</exception>
        public override Type BindToType(string typeName, DebugContext debugContext = null)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }

            Type result;

            lock (TYPEMAP_LOCK)
            {
                if (typeMap.TryGetValue(typeName, out result) == false)
                {
                    // Do all sorts of fancy stuff, maybe

                    // Final fallback to classic .NET type string format
                    if (result == null)
                    {
                        result = Type.GetType(typeName);
                    }

                    if (result == null)
                    {
                        result = AssemblyUtilities.GetTypeByCachedFullName(typeName);
                    }

                    // TODO: Type lookup error handling; use an out bool or a "Try" pattern?

                    string typeStr, assemblyStr;

                    ParseName(typeName, out typeStr, out assemblyStr);

                    if (result == null && assemblyStr != null && assemblyNameLookUp.ContainsKey(assemblyStr))
                    {
                        var assembly = assemblyNameLookUp[assemblyStr];
                        result = assembly.GetType(typeStr);
                    }

                    if (result == null)
                    {
                        result = AssemblyUtilities.GetTypeByCachedFullName(typeStr);
                    }

                    if (result == null && debugContext != null)
                    {
                        debugContext.LogWarning("Failed deserialization type lookup for type name '" + typeName + "'.");
                    }

                    // We allow null values on purpose so we don't have to keep re-performing invalid name lookups
                    typeMap.Add(typeName, result);
                }
            }

            return result;
        }

        private static void ParseName(string fullName, out string typeName, out string assemblyName)
        {
            typeName = null;
            assemblyName = null;

            int firstComma = fullName.IndexOf(',');

            if (firstComma < 0 || (firstComma + 1) == fullName.Length)
            {
                typeName = fullName.Trim(',', ' ');
                return;
            }
            else
            {
                typeName = fullName.Substring(0, firstComma);
            }

            int secondComma = fullName.IndexOf(',', firstComma + 1);

            if (secondComma < 0)
            {
                assemblyName = fullName.Substring(firstComma).Trim(',', ' ');
            }
            else
            {
                assemblyName = fullName.Substring(firstComma, secondComma - firstComma).Trim(',', ' ');
            }
        }
    }
}