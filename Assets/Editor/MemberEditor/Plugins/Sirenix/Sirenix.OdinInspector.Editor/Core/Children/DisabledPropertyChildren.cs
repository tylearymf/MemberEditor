#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DisabledPropertyChildren.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;

    /// <summary>
    /// Represents children for a property where children are disabled. This will never contain any children.
    /// </summary>
    public sealed class DisabledPropertyChildren : PropertyChildren
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisabledPropertyChildren"/> class.
        /// </summary>
        /// <param name="property">The property to handle children for.</param>
        public DisabledPropertyChildren(InspectorProperty property) : base(property)
        {
        }

        /// <summary>
        /// The actual number of children; this is different from <see cref="Count" />, in that <see cref="Count" /> will be 0 if <see cref="GetAllowChildren" /> is false.
        /// </summary>
        protected override int ActualCount { get { return 0; } }

        /// <summary>
        /// Whether this <see cref="PropertyChildren" /> instance represents the elements of a collection.
        /// </summary>
        public override bool IsCollection { get { return false; } }

        /// <summary>
        /// Determines whether to allow children on the property or not.
        /// </summary>
        /// <returns>
        /// Whether to allow children on the property or not.
        /// </returns>
        protected override bool GetAllowChildren()
        {
            return false;
        }

        /// <summary>
        /// The implementaton that calculates a path for a given index.
        /// </summary>
        /// <param name="index">The index to calculate a path for.</param>
        /// <returns>
        /// The calculated path.
        /// </returns>
        /// <exception cref="System.NotSupportedException">Cannot get child paths for properties that cannot have children.</exception>
        protected override string GetPathImplementation(int index)
        {
            throw new NotSupportedException("Cannot get child paths for properties that cannot have children.");
        }

        /// <summary>
        /// Creates a child property for the given index.
        /// </summary>
        /// <param name="index">The index to create a child for.</param>
        /// <returns>
        /// The created child.
        /// </returns>
        /// <exception cref="System.NotSupportedException">Cannot create children for properties that cannot have children.</exception>
        protected override InspectorProperty CreateChild(int index)
        {
            throw new NotSupportedException("Cannot create children for properties that cannot have children.");
        }
    }
}
#endif