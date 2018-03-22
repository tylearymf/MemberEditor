#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyStrongListElementValueEntry.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using Sirenix.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the values of an <see cref="InspectorProperty"/> for a dictionary key value pair element, and contains utilities for querying the values' type and getting and setting them.
    /// </summary>
    public sealed class PropertyDictionaryElementValueEntry<TDictionary, TKey, TValue> : PropertyValueEntry<TDictionary, EditableKeyValuePair<TKey, TValue>> where TDictionary : IDictionary<TKey, TValue>
    {
        private PropertyDictionaryElementValueEntry()
        {
        }

        private bool hasTempInvalidKey;
        private TKey tempInvalidKey;

        /// <summary>
        /// Whether this entry curreny has a temporary invalid key while the user is editing in the inspector.
        /// </summary>
        public bool HasTempInvalidKey { get { return this.hasTempInvalidKey; } }

        /// <summary>
        /// The value category of this value entry.
        /// </summary>
        public override PropertyValueCategory ValueCategory { get { return PropertyValueCategory.DictionaryElement; } }

        /// <summary>
        /// Gets the actual boxed value of the tree target.
        /// </summary>
        protected override object GetActualBoxedValue(TDictionary parent)
        {
            return this.GetActualValue(parent);
        }

        /// <summary>
        /// Gets the actual value of the tree target.
        /// </summary>
        protected override EditableKeyValuePair<TKey, TValue> GetActualValue(TDictionary parent)
        {
            var handler = (IDictionaryHandler<TKey>)this.Property.Parent.ValueEntry.GetDictionaryHandler();

            TKey key = handler.GetKey(parent, this.Property.Index);
            TValue value;
            parent.TryGetValue(key, out value);

            return new EditableKeyValuePair<TKey, TValue>(this.hasTempInvalidKey ? this.tempInvalidKey : key, value);
        }

        /// <summary>
        /// Sets the actual target tree value.
        /// </summary>
        protected override void SetActualBoxedValueImplementation(int index, object value)
        {
            this.SetActualValueImplementation(index, (EditableKeyValuePair<TKey, TValue>)value);
        }

        /// <summary>
        /// Sets the actual target tree value.
        /// </summary>
        protected override void SetActualValueImplementation(int index, EditableKeyValuePair<TKey, TValue> value)
        {
            var parentProperty = this.Property.Parent;
            var parentEntry = (IPropertyValueEntry<TDictionary>)parentProperty.ValueEntry;
            var handler = (IDictionaryHandler<TKey>)parentEntry.GetDictionaryHandler();
            var dict = parentEntry.Values[index];

            TKey oldKey = handler.GetKey(index, this.Property.Index);
            TValue oldValue;

            dict.TryGetValue(oldKey, out oldValue);

            TKey newKey = value.Key;
            TValue newValue = value.Value;

            if (!PropertyValueEntry<TKey>.EqualityComparer(oldKey, newKey))
            {
                // Key has changed; ignore if new key already exists in dictionary
                if (dict.ContainsKey(newKey))
                {
                    this.hasTempInvalidKey = true;
                    this.tempInvalidKey = newKey;
                }
                else
                {
                    bool isPrefab = this.SerializationBackend == SerializationBackend.Odin && this.Property.Tree.HasPrefabs;

                    this.hasTempInvalidKey = false;
                    this.tempInvalidKey = default(TKey);

                    dict.Remove(oldKey);
                    dict.Add(newKey, newValue);

                    if (isPrefab && handler.SupportsPrefabModifications)
                    {
                        this.Property.Tree.RegisterPrefabDictionaryRemoveKeyModification(parentProperty, index, oldKey);
                        this.Property.Tree.RegisterPrefabDictionaryAddKeyModification(parentProperty, index, newKey);
                    }

                    //
                    // Changing just one key may have changed the entire ordering of the dictionary.
                    // Keep everything valid by refreshing the names and paths of every single child
                    // property of the dictionary.
                    //

                    handler.ForceUpdate();
                    parentProperty.Children.ClearPathCache();
                    parentProperty.Children.Update();

                    for (int i = 0; i < parentProperty.Children.Count; i++)
                    {
                        parentProperty.Children[i].ForceUpdatePropertyNameAndPath(i);
                    }

                    //
                    // Get the value entry which now represents the new key, and register a value
                    // modification for it immediately, so as not to lose the old value.
                    //

                    if (isPrefab)
                    {
                        string childName = DictionaryKeyUtility.GetDictionaryKeyString(newKey);

                        var child = parentProperty.Children[childName];
                        child.ValueEntry.Update();
                        child = child.Children["Value"];
                        child.ValueEntry.Update();

                        if (handler.SupportsPrefabModifications)
                        {
                            this.Property.Tree.RegisterPrefabValueModification(child, index, forceImmediate: true);
                        }
                    }
                }
            }
            else if (!PropertyValueEntry<TValue>.EqualityComparer(oldValue, newValue))
            {
                // Only value has changed, this is much simpler
                dict[newKey] = newValue;
            }
        }
    }
}
#endif