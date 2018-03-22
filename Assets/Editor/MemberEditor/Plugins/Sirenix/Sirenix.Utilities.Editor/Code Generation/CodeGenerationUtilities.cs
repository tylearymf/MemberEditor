#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="CodeGenerationUtilities.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor.CodeGeneration
{
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public static class CodeGenerationUtilities
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static readonly Dictionary<string, UserAssemblyType?> UserScriptAssemblies = new Dictionary<string, UserAssemblyType?>()
        {
            { "Assembly-CSharp-firstpass",          UserAssemblyType.Plugin },
            { "Assembly-CSharp-Editor-firstpass",   UserAssemblyType.PluginEditor },
            { "Assembly-CSharp",                    UserAssemblyType.Standard },
            { "Assembly-CSharp-Editor",             UserAssemblyType.StandardEditor },
        };

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static bool TypeIsFromUnityAssembly(Type type)
        {
            return type.Assembly.GetName().Name.StartsWith("Unity", StringComparison.InvariantCulture);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static bool TypeIsFromUserScriptAssembly(Type type)
        {
            return UserScriptAssemblies.ContainsKey(type.Assembly.GetName().Name);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static bool TypeIsFromUserScriptAssembly(Type type, out UserAssemblyType? assemblyType)
        {
            return UserScriptAssemblies.TryGetValue(type.Assembly.GetName().Name, out assemblyType);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static string PrintAsParameters(Type[] types, string[] names, bool includeParentheses = false, bool isExtension = false)
        {
            if (types == null || names == null)
            {
                throw new ArgumentNullException();
            }

            if (types.Length != names.Length)
            {
                throw new ArgumentException("Type and name array aren't of the same length.");
            }

            StringBuilder result = new StringBuilder();

            if (includeParentheses)
            {
                result.Append("(");
            }

            for (int i = 0; i < names.Length; i++)
            {
                if (i != 0)
                {
                    result.Append(", ");
                }
                else if (isExtension)
                {
                    result.Append("this ");
                }

                result.Append(types[i].GetNiceName());
                result.Append(" ");
                result.Append(names[i]);
            }

            if (includeParentheses)
            {
                result.Append(")");
            }

            return result.ToString();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static string PrintAsParameters(ParameterInfo[] parameters, bool includeBraces = false, bool isExtension = false)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException();
            }

            StringBuilder result = new StringBuilder();

            if (includeBraces)
            {
                result.Append("(");
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                if (i != 0)
                {
                    result.Append(", ");
                }
                else if (isExtension)
                {
                    result.Append("this ");
                }

                var parameter = parameters[i];

                result.Append(parameter.ParameterType.GetNiceName());
                result.Append(" ");
                result.Append(parameter.Name);

                if (parameter.IsOptional)
                {
                    var value = parameter.DefaultValue;
                    string valueStr;

                    if (value == null || value is Missing)
                    {
                        valueStr = (parameter.ParameterType.IsClass || parameter.ParameterType.IsInterface) ? "null" : ("default(" + parameter.ParameterType.Name + ")");
                    }
                    else
                    {
                        var type = value.GetType();

                        if (type == typeof(float))
                        {
                            valueStr = value.ToString() + "f";
                        }
                        else if (type == typeof(bool))
                        {
                            valueStr = value.ToString().ToLower(CultureInfo.InvariantCulture);
                        }
                        else if (type == typeof(string))
                        {
                            valueStr = "\"" + value.ToString() + "\"";
                        }
                        else if (type == typeof(char))
                        {
                            valueStr = "'" + value.ToString() + "'";
                        }
                        else if (type.IsEnum)
                        {
                            valueStr = type.Name + "." + value.ToString();
                        }
                        else
                        {
                            valueStr = value.ToString();
                        }
                    }

                    result.Append(" = ");
                    result.Append(valueStr);
                }
            }

            if (includeBraces)
            {
                result.Append(")");
            }

            return result.ToString();
        }
    }
}
#endif