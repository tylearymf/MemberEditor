#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyListValueEntryChanger.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;
    using System.Collections.Generic;
    using Utilities.Editor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Provides functionality for applying list modifications to value entries representing lists.
    /// </summary>
    public abstract class PropertyListValueEntryChanger
    {
        private List<CollectionChange> queuedListChanges = new List<CollectionChange>();

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected internal PropertyListValueEntryChanger()
        {
        }

        private enum ChangeType
        {
            Add,
            InsertAt,
            RemoveAt,
            Clear
        }

        private struct CollectionChange
        {
            public ChangeType ChangeType;
            public object[] Values;
            public int Index;
            public string Id;

            public CollectionChange(object[] values, string id)
            {
                this.ChangeType = ChangeType.Add;
                this.Values = values;
                this.Index = -1;
                this.Id = id;
            }

            public CollectionChange(object[] values, int index, string id)
            {
                this.ChangeType = ChangeType.InsertAt;
                this.Values = values;
                this.Index = index;
                this.Id = id;
            }

            public CollectionChange(int index, string id)
            {
                this.ChangeType = ChangeType.RemoveAt;
                this.Values = null;
                this.Index = index;
                this.Id = id;
            }

            public CollectionChange(string id)
            {
                this.ChangeType = ChangeType.Clear;
                this.Values = null;
                this.Index = -1;
                this.Id = id;
            }
        }

        /// <summary>
        /// The amount of list values that this changer represents. This is always equal to <see cref="PropertyValueEntry.ValueCount"/>.
        /// </summary>
        public int ValueCount { get { return this.Entry.ValueCount; } }

        /// <summary>
        /// The element type of the lists that this changer represents.
        /// </summary>
        public abstract Type ElementType { get; }

        /// <summary>
        /// The value entry that this changer is associated with.
        /// </summary>
        protected abstract IPropertyValueEntry Entry { get; }

        /// <summary>
        /// Queue a change to add an element to all lists represented. Changes are applied in Repaint.
        /// </summary>
        /// <param name="values">The values to add.</param>
        /// <param name="changeId">The change identifier. This is used to group changes together when multiple changes are made from different sources.</param>
        /// <exception cref="System.ArgumentNullException">values is null</exception>
        /// <exception cref="System.ArgumentException">Wrong number of values given.</exception>
        public void AddListElement(object[] values, string changeId)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (values.Length != this.Entry.Property.Tree.WeakTargets.Count)
            {
                throw new ArgumentException("Property Tree has " + this.Entry.Property.Tree.WeakTargets.Count + " targets, but " + values.Length + " values were given.");
            }

            if (this.Entry.ValueIsStrongList)
            {
                this.ValidateStrongListElementValues(values);
            }

            this.queuedListChanges.Add(new CollectionChange(values, changeId));
            GUIHelper.RequestRepaint();
        }

        /// <summary>
        /// Queue a change to insert an element into all lists represented. Changes are applied in Repaint.
        /// </summary>
        /// <param name="index">The index to insert at.</param>
        /// <param name="values">The values to add.</param>
        /// <param name="changeId">The change identifier. This is used to group changes together when multiple changes are made from different sources.</param>
        /// <exception cref="System.ArgumentNullException">values is null</exception>
        /// <exception cref="System.ArgumentException">Wrong number of values given.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public void InsertListElementAt(int index, object[] values, string changeId)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (values.Length != this.Entry.Property.Tree.WeakTargets.Count)
            {
                throw new ArgumentException("Property Tree has " + this.Entry.Property.Tree.WeakTargets.Count + " targets, but " + values.Length + " values were given.");
            }

            if (index < 0 || index > this.Entry.Property.Children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (this.Entry.ValueIsStrongList)
            {
                this.ValidateStrongListElementValues(values);
            }

            this.queuedListChanges.Add(new CollectionChange(values, index, changeId));

            GUIHelper.RequestRepaint();
        }

        /// <summary>
        /// Queue a change to remove an element from all lists represented. Changes are applied in Repaint.
        /// </summary>
        /// <param name="index">The index to remove at.</param>
        /// <param name="changeId">The change identifier. This is used to group changes together when multiple changes are made from different sources.</param>
        /// <exception cref="System.ArgumentNullException">values is null</exception>
        /// <exception cref="System.ArgumentException">Wrong number of values given.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public void RemoveListElementAt(int index, string changeId)
        {
            if (index < 0 || index >= this.Entry.Property.Children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            this.queuedListChanges.Add(new CollectionChange(index, changeId));
            GUIHelper.RequestRepaint();
        }

        /// <summary>
        /// Queue a change to clear all lists represented. Changes are applied in Repaint.
        /// </summary>
        /// <param name="changeId">The change identifier. This is used to group changes together when multiple changes are made from different sources.</param>
        /// <exception cref="System.ArgumentNullException">values is null</exception>
        /// <exception cref="System.ArgumentException">Wrong number of values given.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public void ClearList(string changeId)
        {
            if (this.queuedListChanges == null)
            {
                throw new NotSupportedException("Can only remove elements from values that are weak or strong lists.");
            }

            this.queuedListChanges.Add(new CollectionChange(changeId));
            GUIHelper.RequestRepaint();
        }

        /// <summary>
        /// Applies all queued changes to the represented lists. This method only does something during Repaint.
        /// </summary>
        public bool ApplyChanges()
        {
            // We want to apply the changes at the end of the frame which is always repaint or used.
            if (Event.current != null && (Event.current.type != EventType.Repaint && Event.current.type != EventType.Used))
            {
                return false;
            }

            bool changed = this.queuedListChanges.Count > 0;

            for (int i = 0; i < this.queuedListChanges.Count; i++)
            {
                var change = this.queuedListChanges[i];

                switch (change.ChangeType)
                {
                    case ChangeType.Add:
                        this.AddListElementImplementation(change.Values);
                        GUIHelper.RequestRepaint();
                        break;

                    case ChangeType.InsertAt:
                        this.InsertListElementAtImplementation(change.Index, change.Values);
                        GUIHelper.RequestRepaint();
                        break;

                    case ChangeType.RemoveAt:
                        this.RemoveListElementAtImplementation(change.Index);
                        GUIHelper.RequestRepaint();
                        break;

                    case ChangeType.Clear:
                        this.ClearListImplementation();
                        GUIHelper.RequestRepaint();
                        break;

                    default:
                        throw new NotImplementedException(change.ChangeType.ToString());
                }

                this.AdjustFutureChangesAfterChange(i, change);
            }

            this.queuedListChanges.Clear();
            return changed;
        }

        /// <summary>
        /// Implementation of the remove list element change.
        /// </summary>
        protected abstract void RemoveListElementAtImplementation(int index);

        /// <summary>
        /// Implementation of the insert list element change.
        /// </summary>
        protected abstract void InsertListElementAtImplementation(int index, object[] values);

        /// <summary>
        /// Implementation of the add list element change.
        /// </summary>
        protected abstract void AddListElementImplementation(object[] values);

        /// <summary>
        /// Implementation of the clear list change.
        /// </summary>
        protected abstract void ClearListImplementation();

        /// <summary>
        /// Sets all value references on a given selection index in the property tree to a given new value.
        /// </summary>
        protected void SetAllTreeActualValueReferences(int index, object value, object newValue)
        {
            foreach (var prop in this.Entry.Property.Tree.EnumerateTree(true))
            {
                var entry = prop.ValueEntry;

                if (prop.Info.PropertyType == PropertyType.ReferenceType && entry != null)
                {
                    if (object.ReferenceEquals(entry.WeakValues[index], value))
                    {
                        var castEntry = (IValueEntryActualValueSetter)entry; // Might not be the actual array type

                        try
                        {
                            castEntry.SetActualValue(index, newValue);
                        }
                        catch (InvalidCastException)
                        {
                        }
                    }
                }
            }
        }

        private void ValidateStrongListElementValues(object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                object value = values[i];
                bool isNull = object.ReferenceEquals(null, value);

                if (this.ElementType.IsValueType && isNull)
                {
                    throw new ArgumentException("Given value at index " + i + " was null when list element type '" + this.ElementType.GetNiceName() + "' is a value type.");
                }
                else if (isNull == false && this.ElementType.IsAssignableFrom(value.GetType()) == false)
                {
                    throw new ArgumentException("Given value at index " + i + " of type '" + value.GetType().GetNiceName() + "' was not assignable to expected list element type '" + this.ElementType.GetNiceName() + "'.");
                }
            }
        }

        private void AdjustFutureChangesAfterChange(int changedIndex, CollectionChange change)
        {
            // The purpose here is to ensure that, if any future changes depend on indices above
            // the index changed by this change, we correct their indices accordingly so they
            // still point at the same "conceptual" index from before this change.

            // Add will never need correction
            if (change.ChangeType == ChangeType.Add)
            {
                return;
            }

            for (int i = changedIndex + 1; i < this.queuedListChanges.Count; i++)
            {
                var futureChange = this.queuedListChanges[i];

                if (change.ChangeType == ChangeType.Clear)
                {
                    switch (futureChange.ChangeType)
                    {
                        case ChangeType.Add:
                        case ChangeType.Clear:
                            // Do nothing, command is valid
                            break;

                        case ChangeType.InsertAt:
                            // Can now only insert at index 0
                            futureChange.Index = 0;
                            this.queuedListChanges[i] = futureChange;
                            break;

                        case ChangeType.RemoveAt:
                            // Can't remove elements after clearing, so remove change
                            this.queuedListChanges.RemoveAt(i--);
                            break;
                    }
                }
                else if (futureChange.Id != change.Id && futureChange.ChangeType != ChangeType.Add && futureChange.Index > change.Index)
                {
                    if (change.ChangeType == ChangeType.InsertAt)
                    {
                        futureChange.Index += 1;
                    }
                    else
                    {
                        futureChange.Index -= 1;
                    }

                    this.queuedListChanges[i] = futureChange;
                }
            }
        }
    }
}
#endif