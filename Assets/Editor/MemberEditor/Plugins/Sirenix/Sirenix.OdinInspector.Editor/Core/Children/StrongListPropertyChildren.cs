#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="StrongListPropertyChildren.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the children of a strongly typed list (<see cref="IList{T}"/>) property.
    /// </summary>
    public sealed class StrongListPropertyChildren<TList, TElement> : ListPropertyChildren where TList : IList<TElement>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        public StrongListPropertyChildren(InspectorProperty property) : base(property)
        {
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void UpdateCount()
        {
            var valueEntry = this.Property.ValueEntry;

            if (valueEntry == null)
            {
                throw new InvalidOperationException("Property with list children has no value entry.");
            }

            if (typeof(TList).IsAssignableFrom(valueEntry.TypeOfValue) == false)
            {
                throw new InvalidOperationException("Property with list children has a value entry with non-list values in it.");
            }

            this.count = int.MaxValue;
            this.maxListChildCount = 0;

            for (int i = 0; i < valueEntry.WeakValues.Count; i++)
            {
                TList list = (TList)valueEntry.WeakValues[i];

                if (list == null)
                {
                    this.count = 0;
                }
                else
                {
                    if (list.Count < this.count)
                    {
                        this.count = list.Count;
                    }

                    if (list.Count > this.maxListChildCount)
                    {
                        this.maxListChildCount = list.Count;
                    }
                }
            }
        }
    }
}
#endif