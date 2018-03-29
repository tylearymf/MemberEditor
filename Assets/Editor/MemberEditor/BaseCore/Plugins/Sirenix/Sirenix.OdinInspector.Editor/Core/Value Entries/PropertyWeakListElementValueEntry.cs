#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyWeakListElementValueEntry.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;
    using System.Collections;

    /// <summary>
    /// Represents the values of an <see cref="InspectorProperty"/> for a weak list element, and contains utilities for querying the values' type and getting and setting them.
    /// </summary>
    public sealed class PropertyWeakListElementValueEntry<TParent, TValue> : PropertyValueEntry<TParent, TValue> where TParent : IList
    {
        private PropertyWeakListElementValueEntry()
        {
        }

        /// <summary>
        /// The value category of this value entry.
        /// </summary>
        public override PropertyValueCategory ValueCategory { get { return PropertyValueCategory.WeakListElement; } }

        /// <summary>
        /// Gets the actual boxed value of the tree target.
        /// </summary>
        protected override object GetActualBoxedValue(TParent parent)
        {
            IList castList = parent;
            return castList[this.Property.Index];
        }

        /// <summary>
        /// Gets the actual value of the tree target.
        /// </summary>
        protected override TValue GetActualValue(TParent parent)
        {
            return (TValue)parent[this.Property.Index];
        }

        /// <summary>
        /// Sets the actual target tree value.
        /// </summary>
        protected override void SetActualBoxedValueImplementation(int index, object value)
        {
            IList castList = this.GetParent(index);

            castList[this.Property.Index] = value;
        }

        /// <summary>
        /// Sets the actual target tree value.
        /// </summary>
        protected override void SetActualValueImplementation(int index, TValue value)
        {
            TParent parent = this.GetParent(index);

            parent[this.Property.Index] = value;
        }
    }
}
#endif