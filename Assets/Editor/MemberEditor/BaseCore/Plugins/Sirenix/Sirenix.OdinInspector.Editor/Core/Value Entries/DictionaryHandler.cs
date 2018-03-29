#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Editor
{
    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// <para>A class that provides various utilities for modifying and querying dictionary values represented by a <see cref="PropertyValueEntry"/>.</para>
    /// <para>It is also responsible for translating and ordering dictionary keys into persistent indices.</para>
    /// </summary>
    public class DictionaryHandler<TDictionary, TKey, TValue> : IDictionaryHandler<TKey> where TDictionary : IDictionary<TKey, TValue>
    {
        private int lastUpdateID = -1;
        private IPropertyValueEntry<TDictionary> valueEntry;
        private Dictionary<TDictionary, int> dictIndexMap = new Dictionary<TDictionary, int>();
        private List<TKey>[] keys;
        private List<TKey>[] oldKeys;
        private Queue<DictionaryChange> queuedChanges = new Queue<DictionaryChange>();
        private bool firstChange = true;

        private bool supportsPrefabModifications = false;

        private enum DictionaryChangeType
        {
            SetValue,
            RemoveKey
        }

        private class DictionaryChange
        {
            public TKey Key { get; private set; }
            public TValue Value { get; private set; }
            public DictionaryChangeType ChangeType { get; private set; }

            private DictionaryChange()
            {
            }

            public static DictionaryChange Remove(TKey key)
            {
                var result = new DictionaryChange();

                result.Key = key;
                result.Value = default(TValue);
                result.ChangeType = DictionaryChangeType.RemoveKey;

                return result;
            }

            public static DictionaryChange SetValue(TKey key, TValue value)
            {
                var result = new DictionaryChange();

                result.Key = key;
                result.Value = value;
                result.ChangeType = DictionaryChangeType.SetValue;

                return result;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryHandler{TDictionary, TKey, TValue}"/> class.
        /// </summary>
        /// <param name="valueEntry">The value entry to represent.</param>
        public DictionaryHandler(IPropertyValueEntry<TDictionary> valueEntry)
        {
            this.supportsPrefabModifications = DictionaryKeyUtility.KeyTypeSupportsPersistentPaths(typeof(TKey));

            this.valueEntry = valueEntry;
            this.keys = new List<TKey>[this.valueEntry.Property.Tree.WeakTargets.Count];
            this.oldKeys = new List<TKey>[this.valueEntry.Property.Tree.WeakTargets.Count];

            for (int i = 0; i < this.keys.Length; i++)
            {
                this.keys[i] = new List<TKey>();
                this.oldKeys[i] = new List<TKey>();
            }
        }

        /// <summary>
        /// Whether the dictionary represented by this handler supports prefab modifications.
        /// </summary>
        public bool SupportsPrefabModifications { get { return this.supportsPrefabModifications; } }

        /// <summary>
        /// Gets the key value at the given selection and dictionary index.
        /// </summary>
        public TKey GetKey(int selectionIndex, int childIndex)
        {
            this.EnsureUpdated();

            var list = this.keys[selectionIndex];
            return list[childIndex];
        }

        /// <summary>
        /// Gets the key value at the given index from the given dictionary.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">dictionary is null</exception>
        /// <exception cref="System.ArgumentException">
        /// Given dictionary object cannot be assigned to type TDictionary.
        /// or
        /// The given dictionary to get key from was not registered in the handler.
        /// </exception>
        public TKey GetKey(object dictionary, int childIndex)
        {
            if (object.ReferenceEquals(dictionary, null))
            {
                throw new ArgumentNullException("dictionary");
            }

            if (!(dictionary is TDictionary))
            {
                throw new ArgumentException("Given dictionary object of type " + dictionary.GetType().GetNiceName() + " cannot be assigned to " + typeof(TDictionary).GetNiceName());
            }

            this.EnsureUpdated();

            TDictionary dict = (TDictionary)dictionary;

            int selectionIndex;

            if (!this.dictIndexMap.TryGetValue(dict, out selectionIndex))
            {
                this.ForceUpdate();
            }

            if (this.dictIndexMap.TryGetValue(dict, out selectionIndex))
            {
                var list = this.keys[selectionIndex];
                return list[childIndex];
            }

            throw new ArgumentException("The given dictionary to get key from was not registered in the handler.");
        }

        object IDictionaryHandler.GetKey(int selectionIndex, int childIndex)
        {
            return this.GetKey(selectionIndex, childIndex);
        }

        object IDictionaryHandler.GetKey(object dictionary, int childIndex)
        {
            return this.GetKey(dictionary, childIndex);
        }

        /// <summary>
        /// Queues a set value modification for a given key. Modifications are applied in Repaint.
        /// </summary>
        public void SetValue(TKey key, object value)
        {
            this.queuedChanges.Enqueue(DictionaryChange.SetValue(key, (TValue)value));
        }

        void IDictionaryHandler.SetValue(object key, object value)
        {
            this.SetValue((TKey)key, value);
        }

        /// <summary>
        /// Queues a remove modification for a given key. Modifications are applied in Repaint.
        /// </summary>
        public void Remove(TKey key)
        {
            this.queuedChanges.Enqueue(DictionaryChange.Remove(key));
        }

        void IDictionaryHandler.Remove(object key)
        {
            this.Remove((TKey)key);
        }

        /// <summary>
        /// Apply all queued changes, and apply prefab modifications if applicable. This method only does something during Repaint.
        /// </summary>
        /// <returns>
        /// true if any changes were made, otherwise false
        /// </returns>
        public bool ApplyChanges()
        {
            if (Event.current != null && Event.current.type != EventType.Repaint)
            {
                return false;
            }

            bool changed = this.queuedChanges.Count > 0;

            while (this.queuedChanges.Count > 0)
            {
                var change = this.queuedChanges.Dequeue();

                switch (change.ChangeType)
                {
                    case DictionaryChangeType.SetValue:
                        for (int i = 0; i < this.valueEntry.ValueCount; i++)
                        {
                            var dict = this.valueEntry.Values[i];

                            if (dict != null)
                            {
                                dict[change.Key] = change.Value;
                            }

                            if (this.supportsPrefabModifications && this.valueEntry.SerializationBackend == SerializationBackend.Odin && this.valueEntry.Property.Tree.HasPrefabs)
                            {
                                this.valueEntry.Property.Tree.RegisterPrefabDictionaryAddKeyModification(this.valueEntry.Property, i, change.Key);

                                var child = this.valueEntry.Property.Children[DictionaryKeyUtility.GetDictionaryKeyString(change.Key)];

                                if (child != null)
                                {
                                    // We also need to register a value modification immediately so as not to lose the old value
                                    child = child.Children["Value"];
                                    this.valueEntry.Property.Tree.RegisterPrefabValueModification(child, i, forceImmediate: true);
                                }
                            }
                        }

                        break;

                    case DictionaryChangeType.RemoveKey:
                        for (int i = 0; i < this.valueEntry.ValueCount; i++)
                        {
                            var dict = this.valueEntry.Values[i];

                            if (dict != null)
                            {
                                dict.Remove(change.Key);
                            }

                            if (this.supportsPrefabModifications && this.valueEntry.SerializationBackend == SerializationBackend.Odin && this.valueEntry.Property.Tree.HasPrefabs)
                            {
                                this.valueEntry.Property.Tree.RegisterPrefabDictionaryRemoveKeyModification(this.valueEntry.Property, i, change.Key);
                            }
                        }

                        break;

                    default:
                        throw new NotImplementedException(change.ChangeType.ToString());
                }
            }

            if (changed)
            {
                //
                // Changing just one key may have changed the entire ordering of the dictionary.
                // Keep everything valid by refreshing the names and paths of every single child
                // property of the dictionary.
                //

                this.ForceUpdate();

                var children = this.valueEntry.Property.Children;
                children.ClearPathCache();
                children.Update();

                for (int i = 0; i < children.Count; i++)
                {
                    children[i].ForceUpdatePropertyNameAndPath(i);
                }
            }

            return changed;
        }

        private void EnsureUpdated()
        {
            if (this.valueEntry.Property.Tree.UpdateID != this.lastUpdateID)
            {
                this.dictIndexMap.Clear();
                this.lastUpdateID = this.valueEntry.Property.Tree.UpdateID;

                for (int i = 0; i < this.keys.Length; i++)
                {
                    // Swap lists and keep the old one for a change comparison
                    var oldKeyList = this.keys[i];
                    var keyList = this.oldKeys[i];

                    this.oldKeys[i] = oldKeyList;
                    this.keys[i] = keyList;

                    keyList.Clear();

                    var dict = this.valueEntry.Values[i];

                    if (object.ReferenceEquals(dict, null)) continue;

                    this.dictIndexMap[dict] = i;

                    var castDict = dict as Dictionary<TKey, TValue>;

                    if (castDict != null)
                    {
                        // Reduce garbage allocation
                        foreach (var pair in castDict.GFIterator())
                        {
                            keyList.Add(pair.Key);
                        }
                    }
                    else
                    {
                        foreach (var key in dict.Keys)
                        {
                            keyList.Add(key);
                        }
                    }

                    if (keyList.Count > 1)
                    {
                        var comparer = DictionaryKeyUtility.KeyComparer<TKey>.Default;

                        var a = keyList[0];
                        for (int j = 1; j < keyList.Count; j++)
                        {
                            var b = keyList[j];
                            if (comparer.Compare(a, b) > 0)
                            {
                                keyList.Sort(comparer);
                                break;
                            }
                            a = b;
                        }
                    }

                    if (keyList.Count != oldKeyList.Count)
                    {
                        this.OnChangedUpdatePaths();
                    }
                    else
                    {
                        for (int j = 0; j < keyList.Count; j++)
                        {
                            if (!PropertyValueEntry<TKey>.EqualityComparer(keyList[j], oldKeyList[j]))
                            {
                                this.OnChangedUpdatePaths();
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void OnChangedUpdatePaths()
        {
            if (this.firstChange)
            {
                this.firstChange = false;
                return;
            }

            this.valueEntry.Property.Children.Update();
            this.valueEntry.Property.ForceUpdatePropertyNameAndPath(null);
        }

        /// <summary>
        /// Force the dictionary handler to update its internal dictionary index mappings.
        /// </summary>
        public void ForceUpdate()
        {
            this.lastUpdateID -= 1000;
            this.EnsureUpdated();
        }
    }
}
#endif