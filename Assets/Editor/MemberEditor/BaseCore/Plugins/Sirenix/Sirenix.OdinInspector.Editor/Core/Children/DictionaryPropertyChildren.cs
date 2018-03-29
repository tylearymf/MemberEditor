#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DictionaryPropertyChildren.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using Sirenix.Serialization;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the children of a dictionary property.
    /// </summary>
    public sealed class DictionaryPropertyChildren<TDictionary, TKey, TValue> : PropertyChildren where TDictionary : IDictionary<TKey, TValue>
    {
        private int count;

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryPropertyChildren{TDictionary, TKey, TValue}"/> class.
        /// </summary>
        /// <param name="property">The property to handle children for.</param>
        public DictionaryPropertyChildren(InspectorProperty property) : base(property)
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
            var handler = this.Property.ValueEntry.GetDictionaryHandler();
            var key = handler.GetKey(0, index);
            var keyStr = DictionaryKeyUtility.GetDictionaryKeyString(key);

            return this.Property.Path + "." + keyStr + ".#entry";
        }

        /// <summary>
        /// Updates the child count of the property.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// Property with dictionary children has no value entry.
        /// or
        /// Property with dictionary children has a value entry with non-dictionary values in it.
        /// </exception>
        protected override void UpdateCount()
        {
            var valueEntry = this.Property.ValueEntry;

            if (valueEntry == null)
            {
                throw new InvalidOperationException("Property with dictionary children has no value entry.");
            }

            if (typeof(TDictionary).IsAssignableFrom(valueEntry.TypeOfValue) == false)
            {
                throw new InvalidOperationException("Property with dictionary children has a value entry with non-dictionary values in it.");
            }

            this.count = int.MaxValue;

            for (int i = 0; i < valueEntry.WeakValues.Count; i++)
            {
                TDictionary dict = (TDictionary)valueEntry.WeakValues[i];

                if (dict == null)
                {
                    this.count = 0;
                }
                else if (dict.Count < this.count)
                {
                    this.count = dict.Count;
                }
            }
        }
    }
}
#endif