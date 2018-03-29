#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyWeakListValueEntryChanger.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;
    using System.Collections;

    /// <summary>
    /// Provides functionality for applying list modifications to value entries representing weak lists.
    /// </summary>
    public sealed class PropertyWeakListValueEntryChanger<TList> : PropertyListValueEntryChanger where TList : IList
    {
        private IPropertyValueEntry<TList> entry;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyWeakListValueEntryChanger{TList}"/> class.
        /// </summary>
        /// <param name="entry">The entry to represent.</param>
        /// <exception cref="System.ArgumentNullException">entry is null</exception>
        public PropertyWeakListValueEntryChanger(IPropertyValueEntry<TList> entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            this.entry = entry;
        }

        /// <summary>
        /// The element type of the lists that this changer represents.
        /// </summary>
        public override Type ElementType { get { return typeof(object); } }

        /// <summary>
        /// The value entry that this changer is associated with.
        /// </summary>
        protected override IPropertyValueEntry Entry { get { return this.entry; } }

        /// <summary>
        /// Implementation of the add list element change.
        /// </summary>
        protected override void AddListElementImplementation(object[] values)
        {
            for (int i = 0; i < this.ValueCount; i++)
            {
                TList list = (TList)this.Entry.WeakValues[i];
                list.Add(values[i]);
            }
        }

        /// <summary>
        /// Implementation of the clear list change.
        /// </summary>
        protected override void ClearListImplementation()
        {
            for (int i = 0; i < this.ValueCount; i++)
            {
                TList list = (TList)this.Entry.WeakValues[i];
                list.Clear();
            }
        }

        /// <summary>
        /// Implementation of the insert list element change.
        /// </summary>
        protected override void InsertListElementAtImplementation(int index, object[] values)
        {
            for (int i = 0; i < this.ValueCount; i++)
            {
                TList list = (TList)this.Entry.WeakValues[i];
                list.Insert(index, values[i]);
            }

            ((IEditableListPropertyChildren)this.Entry.Property.Children).InsertSpaceAt(index);
        }

        /// <summary>
        /// Implementation of the remove list element change.
        /// </summary>
        /// <param name="index"></param>
        protected override void RemoveListElementAtImplementation(int index)
        {
            for (int i = 0; i < this.ValueCount; i++)
            {
                TList list = (TList)this.Entry.WeakValues[i];
                list.RemoveAt(index);
            }

            ((IEditableListPropertyChildren)this.Entry.Property.Children).RemoveChildAt(index);
        }
    }
}
#endif