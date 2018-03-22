#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GroupPropertyChildren.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;

    /// <summary>
    /// Represents the members of a property group as children.
    /// </summary>
    public sealed class GroupPropertyChildren : PropertyChildren
    {
        private InspectorPropertyGroupInfo info;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupPropertyChildren"/> class.
        /// </summary>
        /// <param name="property">The property to handle children for.</param>
        /// <exception cref="System.ArgumentException">Given property info must be of type group.</exception>
        public GroupPropertyChildren(InspectorProperty property) : base(property)
        {
            if (property.Info.PropertyType != PropertyType.Group)
            {
                throw new ArgumentException("Given property info must be of type group.");
            }

            this.info = (InspectorPropertyGroupInfo)property.Info;
        }

        /// <summary>
        /// The actual number of children; this is different from <see cref="Count" />, in that <see cref="Count" /> will be 0 if <see cref="GetAllowChildren" /> is false.
        /// </summary>
        protected override int ActualCount { get { return this.info.GroupInfos.Length; } }

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
            return true;
        }

        /// <summary>
        /// The implementaton that calculates a path for a given index.
        /// </summary>
        /// <param name="index">The index to calculate a path for.</param>
        /// <returns>
        /// The calculated path.
        /// </returns>
        protected override string GetPathImplementation(int index)
        {
            return this.Property.Path + "." + this.info.GroupInfos[index].PropertyName;
        }

        /// <summary>
        /// Creates a child property for the given index.
        /// </summary>
        /// <param name="index">The index to create a child for.</param>
        /// <returns>
        /// The created child.
        /// </returns>
        protected override InspectorProperty CreateChild(int index)
        {
            return InspectorProperty.Create(this.Property.Tree, this.Property, this.info.GroupInfos[index], index);
        }
    }
}
#endif