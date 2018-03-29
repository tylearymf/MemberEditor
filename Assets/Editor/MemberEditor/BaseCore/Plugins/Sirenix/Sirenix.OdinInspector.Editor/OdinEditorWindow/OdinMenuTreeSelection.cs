#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinMenuTreeSelection.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Collections;

    /// <summary>
    /// Handles the selection of a Odin Menu Tree with support for multi selection.
    /// </summary>
    /// <seealso cref="OdinMenuTree" />
    /// <seealso cref="OdinMenuItem" />
    /// <seealso cref="OdinMenuStyle" />
    /// <seealso cref="OdinMenuTreeExtensions" />
    /// <seealso cref="OdinMenuEditorWindow" />
    public class OdinMenuTreeSelection : IList<OdinMenuItem>
    {
        private readonly List<OdinMenuItem> selection;

        private bool supportsMultiSelect;

        /// <summary>
        /// Initializes a new instance of the <see cref="OdinMenuTreeSelection"/> class.
        /// </summary>
        /// <param name="supportsMultiSelect">if set to <c>true</c> [supports multi select].</param>
        public OdinMenuTreeSelection(bool supportsMultiSelect)
        {
            this.supportsMultiSelect = supportsMultiSelect;
            this.selection = new List<OdinMenuItem>();
        }

        /// <summary>
        /// Occurs whenever the selection has changed.
        /// </summary>
        public event Action OnSelectionChanged;

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count { get { return this.selection.Count; } }

        /// <summary>
        /// Gets or sets a value indicating whether multi selection is supported.
        /// </summary>
        public bool SupportsMultiSelect
        {
            get { return this.supportsMultiSelect; }
            set { this.supportsMultiSelect = value; }
        }

        /// <summary>
        /// Gets the <see cref="OdinMenuItem"/> at the specified index.
        /// </summary>
        public OdinMenuItem this[int index]
        {
            get { return this.selection[index]; }
        }

        /// <summary>
        /// Adds a menu item to the selection. If the menu item is already selected, then the item is pushed to the bottom of the selection list.
        /// If multi selection is off, then the previous selected menu item is removed first.
        /// Adding a item to the selection triggers <see cref="OnSelectionChanged"/>.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(OdinMenuItem item)
        {
            if (this.supportsMultiSelect == false)
            {
                this.selection.Clear();
            }

            // Removing it first puts the newly added item in the bottom 
            // of the selection list - which a lot of selection logic depend upon.
            this.Remove(item);
            this.selection.Add(item);
            this.ApplyChanges();
        }

        /// <summary>
        /// Clears the selection and triggers <see cref="OnSelectionChanged"/>.
        /// </summary>
        public void Clear()
        {
            this.selection.Clear();
            this.ApplyChanges();
        }

        /// <summary>
        /// Determines whether an OdinMenuItem is selected.
        /// </summary>
        public bool Contains(OdinMenuItem item)
        {
            return this.selection.Contains(item);
        }

        /// <summary>
        /// Copies all the elements of the current array to the specified array starting at the specified destination array index.
        /// </summary>
        public void CopyTo(OdinMenuItem[] array, int arrayIndex)
        {
            this.selection.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        public IEnumerator<OdinMenuItem> GetEnumerator()
        {
            return this.selection.GetEnumerator();
        }

        /// <summary>
        /// Searches for the specified menu item and returns the index location.
        /// </summary>
        public int IndexOf(OdinMenuItem item)
        {
            return this.selection.IndexOf(item);
        }

        /// <summary>
        /// Removes the specified menu item and triggers <see cref="OnSelectionChanged"/>.
        /// </summary>
        public bool Remove(OdinMenuItem item)
        {
            var result = this.selection.Remove(item);

            if (result)
            {
                this.ApplyChanges();
            }

            return result;
        }

        /// <summary>
        /// Removes the menu item at the specified index and triggers <see cref="OnSelectionChanged"/>.
        /// </summary>
        public void RemoveAt(int index)
        {
            this.selection.RemoveAt(index);
            this.ApplyChanges();
        }

        private void ApplyChanges()
        {
            try
            {
                if (this.OnSelectionChanged != null)
                {
                    OnSelectionChanged();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        bool ICollection<OdinMenuItem>.IsReadOnly { get { return false; } }

        void IList<OdinMenuItem>.Insert(int index, OdinMenuItem item)
        {
            throw new NotSupportedException();
        }

        OdinMenuItem IList<OdinMenuItem>.this[int index]
        {
            get { return this.selection[index]; }
            set { this.Add(value); }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.selection.GetEnumerator();
        }
    }
}
#endif