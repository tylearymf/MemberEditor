//-----------------------------------------------------------------------
// <copyright file="ObjectFieldAlignment.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    /// <summary>
    /// How the square object field should be aligned.
    /// </summary>
    /// <seealso cref="PreviewFieldAttribute"/>
    public enum ObjectFieldAlignment
    {
        /// <summary>
        /// Left aligned.
        /// </summary>
        Left = 0,

        /// <summary>
        /// Centered.
        /// </summary>
        Center = 1,

        /// <summary>
        /// Right aligned.
        /// </summary>
        Right = 2,
    }
}