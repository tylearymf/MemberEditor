#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ListPropertyChildren.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    /// <summary>
    /// Represents the children of a list property.
    /// </summary>
    public abstract class ListPropertyChildren : PropertyChildren, IEditableListPropertyChildren
    {
        /// <summary>
        /// The count of children.
        /// </summary>
        protected int count;

        /// <summary>
        /// The largest count of a list in this property (for multi-selection).
        /// </summary>
        protected int maxListChildCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListPropertyChildren"/> class.
        /// </summary>
        /// <param name="property">The property to handle children for.</param>
        protected ListPropertyChildren(InspectorProperty property) : base(property)
        {
        }

        /// <summary>
        /// Whether this <see cref="PropertyChildren" /> instance represents the elements of a collection.
        /// </summary>
        public override bool IsCollection { get { return true; } }

        /// <summary>
        /// The actual number of children; this is different from <see cref="Count" />, in that <see cref="Count" /> will be 0 if <see cref="GetAllowChildren" /> is false.
        /// </summary>
        protected override int ActualCount { get { return this.count; } }

        /// <summary>
        /// Gets the maximum list child count.
        /// </summary>
        /// <value>
        /// The maximum list child count.
        /// </value>
        public int MaxListChildCount { get { return this.maxListChildCount; } }

        /// <summary>
        /// Determines whether to allow children on the property or not.
        /// </summary>
        /// <returns>
        /// Whether to allow children on the property or not.
        /// </returns>
        protected override bool GetAllowChildren()
        {
            var overrideType = this.Property.ValueEntry.ValueState;
            return overrideType == PropertyValueState.None || overrideType == PropertyValueState.CollectionLengthConflict;
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
            return this.Property.Path + ".$" + index;
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
            return InspectorProperty.Create(this.Property.Tree, this.Property, null, index);
        }

        void IEditableListPropertyChildren.InsertSpaceAt(int index)
        {
            this.count++;
            this.InsertSpaceAt(index);
        }

        void IEditableListPropertyChildren.RemoveChildAt(int index)
        {
            this.count--;
            this.RemoveChildAt(index);
        }
    }
}
#endif