#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="IEditableListPropertyChildren.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    /// <summary>
    /// Represents an instance of <see cref="PropertyChildren"/> that can be edited after creation - such as children for lists and arrays.
    /// </summary>
    internal interface IEditableListPropertyChildren
    {
        /// <summary>
        /// Insert space for a child at the given index.
        /// </summary>
        /// <param name="index">The index to insert a space at.</param>
        void InsertSpaceAt(int index);

        /// <summary>
        /// Remove the child at the given index.
        /// </summary>
        /// <param name="index">The index to remove a child at.</param>
        void RemoveChildAt(int index);
    }
}
#endif