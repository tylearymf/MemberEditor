#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="CodeGeneratorExtensions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor.CodeGeneration
{
    using System;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public static class CodeGeneratorExtensions
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static string Stringify(this AccessModifier accessModifier)
        {
            switch (accessModifier)
            {
                case AccessModifier.Private:
                    return "private";

                case AccessModifier.Protected:
                    return "protected";

                case AccessModifier.Public:
                    return "public";

                case AccessModifier.Internal:
                    return "internal";

                case AccessModifier.ProtectedInternal:
                    return "protected internal";

                default:
                    throw new ArgumentException("Unknown access modifier");
            }
        }
    }
}
#endif