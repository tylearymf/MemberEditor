#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="WeakListPropertyChildren.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;
    using System.Collections;

    /// <summary>
    /// Represents the children of a weakly typed list (<see cref="IList"/>) property.
    /// </summary>
    public sealed class WeakListPropertyChildren : ListPropertyChildren
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WeakListPropertyChildren"/> class.
        /// </summary>
        /// <param name="property">The property to handle children for.</param>
        public WeakListPropertyChildren(InspectorProperty property) : base(property)
        {
        }

        /// <summary>
        /// Updates the child count of the property.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// Property with list children has no value entry.
        /// or
        /// Property with list children has a value entry with non-list values in it.
        /// </exception>
        protected override void UpdateCount()
        {
            var valueEntry = this.Property.ValueEntry;

            if (valueEntry == null)
            {
                throw new InvalidOperationException("Property with list children has no value entry.");
            }

            if (typeof(IList).IsAssignableFrom(valueEntry.TypeOfValue) == false)
            {
                throw new InvalidOperationException("Property with list children has a value entry with non-list values in it.");
            }

            this.count = int.MaxValue;

            for (int i = 0; i < valueEntry.WeakValues.Count; i++)
            {
                IList list = valueEntry.WeakValues[i] as IList;

                if (list == null)
                {
                    this.count = 0;
                }
                else if (list.Count < this.count)
                {
                    this.count = list.Count;
                }
            }
        }
    }
}
#endif