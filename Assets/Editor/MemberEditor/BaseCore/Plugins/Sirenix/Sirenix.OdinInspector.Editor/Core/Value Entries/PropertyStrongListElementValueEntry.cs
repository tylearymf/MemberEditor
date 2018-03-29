#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyStrongListElementValueEntry.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the values of an <see cref="InspectorProperty"/> for a strong list element, and contains utilities for querying the values' type and getting and setting them.
    /// </summary>
    public sealed class PropertyStrongListElementValueEntry<TParent, TElement, TValue> : PropertyValueEntry<TParent, TValue> where TParent : IList<TElement> where TValue : TElement
    {
        private PropertyStrongListElementValueEntry()
        {
        }

        /// <summary>
        /// The value category of this value entry.
        /// </summary>
        public override PropertyValueCategory ValueCategory { get { return PropertyValueCategory.StrongListElement; } }

        /// <summary>
        /// Gets the actual boxed value of the tree target.
        /// </summary>
        protected override object GetActualBoxedValue(TParent parent)
        {
            IList<object> castList = (IList<object>)parent;
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
            IList<object> castList = (IList<object>)this.GetParent(index);
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