#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyStrongListValueEntryChanger.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Provides functionality for applying list modifications to value entries representing strong lists.
    /// </summary>
    public sealed class PropertyStrongListValueEntryChanger<TList, TElement> : PropertyListValueEntryChanger where TList : IList<TElement>
    {
        private static readonly bool ValueIsArray = typeof(TList).IsArray;

        private IPropertyValueEntry<TList> entry;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyStrongListValueEntryChanger{TList, TElement}"/> class.
        /// </summary>
        /// <param name="entry">The entry to represent.</param>
        /// <exception cref="System.ArgumentNullException">entry is null</exception>
        public PropertyStrongListValueEntryChanger(IPropertyValueEntry<TList> entry)
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
        public override Type ElementType { get { return typeof(TElement); } }

        /// <summary>
        /// The value entry that this changer is associated with.
        /// </summary>
        protected override IPropertyValueEntry Entry { get { return this.entry; } }

        /// <summary>
        /// Implementation of the add list element change.
        /// </summary>
        protected override void AddListElementImplementation(object[] values)
        {
            if (ValueIsArray)
            {
                for (int i = 0; i < this.ValueCount; i++)
                {
                    TElement[] oldArray = (TElement[])this.Entry.WeakValues[i];
                    TElement[] newArray = ArrayUtilities.CreateNewArrayWithAddedElement(oldArray, (TElement)values[i]);

                    this.SetAllTreeActualValueReferences(i, oldArray, newArray);
                }
            }
            else
            {
                for (int i = 0; i < this.ValueCount; i++)
                {
                    TList list = (TList)this.Entry.WeakValues[i];
                    try
                    {
                        list.Add((TElement)values[i]);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Implementation of the clear list change.
        /// </summary>
        protected override void ClearListImplementation()
        {
            if (ValueIsArray)
            {
                for (int i = 0; i < this.ValueCount; i++)
                {
                    TElement[] oldArray = (TElement[])this.Entry.WeakValues[i];
                    TElement[] newArray = new TElement[0];

                    this.SetAllTreeActualValueReferences(i, oldArray, newArray);
                }
            }
            else
            {
                for (int i = 0; i < this.ValueCount; i++)
                {
                    TList list = (TList)this.Entry.WeakValues[i];

                    try
                    {
                        list.Clear();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Implementation of the insert list element change.
        /// </summary>
        protected override void InsertListElementAtImplementation(int index, object[] values)
        {
            if (ValueIsArray)
            {
                for (int i = 0; i < this.ValueCount; i++)
                {
                    TElement[] oldArray = (TElement[])this.Entry.WeakValues[i];

                    if (index > oldArray.Length)
                    {
                        index = oldArray.Length;
                    }

                    TElement[] newArray = ArrayUtilities.CreateNewArrayWithInsertedElement(oldArray, index, (TElement)values[i]);

                    this.SetAllTreeActualValueReferences(i, oldArray, newArray);
                }
            }
            else
            {
                for (int i = 0; i < this.ValueCount; i++)
                {
                    TList list = (TList)this.Entry.WeakValues[i];

                    try
                    {
                        list.Insert(index, (TElement)values[i]);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                }
            }

            ((IEditableListPropertyChildren)this.Entry.Property.Children).InsertSpaceAt(index);
        }

        /// <summary>
        /// Implementation of the remove list element change.
        /// </summary>
        protected override void RemoveListElementAtImplementation(int index)
        {
            if (ValueIsArray)
            {
                for (int i = 0; i < this.ValueCount; i++)
                {
                    TElement[] oldArray = (TElement[])this.Entry.WeakValues[i];
                    TElement[] newArray = ArrayUtilities.CreateNewArrayWithRemovedElement(oldArray, index);

                    this.SetAllTreeActualValueReferences(i, oldArray, newArray);
                }
            }
            else
            {
                for (int i = 0; i < this.ValueCount; i++)
                {
                    TList list = (TList)this.Entry.WeakValues[i];

                    try
                    {
                        list.RemoveAt(index);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }
                }
            }

            ((IEditableListPropertyChildren)this.Entry.Property.Children).RemoveChildAt(index);
        }
    }
}
#endif