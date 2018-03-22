#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Editor
{
    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A polymorphic alias for a <see cref="PropertyValueEntry"/> instance, used to implement strongly typed polymorphism in Odin.
    /// </summary>
    public abstract class PropertyValueEntryAlias : IPropertyValueEntry, IValueEntryActualValueSetter
    {
        /// <summary>
        /// The number of parallel values this entry represents. This will always be exactly equal to the count of <see cref="PropertyTree.WeakTargets" />.
        /// </summary>
        public abstract int ValueCount { get; }

        /// <summary>
        /// Whether this value entry is editable or not.
        /// </summary>
        public abstract bool IsEditable { get; }

        /// <summary>
        /// If this value entry has the override type <see cref="PropertyValueState.Reference" />, this is the path of the property it references.
        /// </summary>
        public abstract string TargetReferencePath { get; }

        /// <summary>
        /// <para>The actual serialization backend for this value entry, possibly inherited from the serialization backend of the root property this entry is a child of.</para>
        /// <para>Note that this is *not* always equal to <see cref="InspectorPropertyInfo.SerializationBackend" />.</para>
        /// </summary>
        public abstract SerializationBackend SerializationBackend { get; }

        /// <summary>
        /// The property whose values this value entry represents.
        /// </summary>
        public abstract InspectorProperty Property { get; }

        /// <summary>
        /// Whether the type of this entry implements <see cref="System.Collections.IList" />.
        /// </summary>
        public abstract bool ValueIsWeakList { get; }

        /// <summary>
        /// Whether the type of this entry implements <see cref="System.Collections.Generic.IList{T}" />.
        /// </summary>
        public abstract bool ValueIsStrongList { get; }

        /// <summary>
        /// Whether the type of this entry implements <see cref="System.Collections.Generic.IDictionary{TKey, TValue}" />.
        /// </summary>
        public abstract bool ValueIsValidDictionary { get; }

        /// <summary>
        /// The value category of this value entry.
        /// </summary>
        public abstract PropertyValueCategory ValueCategory { get; }

        /// <summary>
        /// Provides access to the weakly typed values of this value entry.
        /// </summary>
        public abstract IPropertyValueCollection WeakValues { get; }

        /// <summary>
        /// Whether this value entry has been changed from its prefab counterpart.
        /// </summary>
        public abstract bool ValueChangedFromPrefab { get; }

        /// <summary>
        /// Whether this value entry has had its list length changed from its prefab counterpart.
        /// </summary>
        public abstract bool ListLengthChangedFromPrefab { get; }

        /// <summary>
        /// Whether this value entry has had its dictionary values changes from its prefab counterpart.
        /// </summary>
        public abstract bool DictionaryChangedFromPrefab { get; }

        /// <summary>
        /// <para>A weakly typed smart value that represents the first element of the value entry's value collection, but has "smart logic" for setting the value that detects relevant changes and applies them in parallel.</para>
        /// <para>This lets you often just use the smart value instead of having to deal with the tedium of multiple parallel values.</para>
        /// </summary>
        public abstract object WeakSmartValue { get; set; }

        /// <summary>
        /// The type from which this value entry comes. If this value entry represents a member value, this is the declaring type of the member. If the value entry represents a collection element, this is the type of the collection.
        /// </summary>
        public abstract Type ParentType { get; }

        /// <summary>
        /// The most precise known contained type of the value entry. If polymorphism is in effect, this will be some type derived from <see cref="BaseValueType" />.
        /// </summary>
        public abstract Type TypeOfValue { get; }

        /// <summary>
        /// The base type of the value entry. If this is value entry represents a member value, this is the type of the member. If the value entry represents a collection element, this is the element type of the collection.
        /// </summary>
        public abstract Type BaseValueType { get; }

        /// <summary>
        /// The special state of the value entry.
        /// </summary>
        public abstract PropertyValueState ValueState { get; }

        /// <summary>
        /// Whether this value entry is an alias, or not. Value entry aliases are used to provide strongly typed value entries in the case of polymorphism.
        /// </summary>
        public bool IsAlias { get { return true; } }

        /// <summary>
        /// The context container of this property.
        /// </summary>
        public PropertyContextContainer Context { get { return this.Property.Context; } }

        /// <summary>
        /// Whether this type is marked as an atomic type using a <see cref="IAtomHandler"/>.
        /// </summary>
        public abstract bool IsMarkedAtomic { get; }

        /// <summary>
        /// An event that is invoked during <see cref="ApplyChanges" />, when any values have changed.
        /// </summary>
        public abstract event Action<int> OnValueChanged;

        /// <summary>
        /// An event that is invoked during <see cref="ApplyChanges" />, when any child values have changed.
        /// </summary>
        public abstract event Action<int> OnChildValueChanged;

        /// <summary>
        /// Applies the changes made to this value entry to the target objects, and registers prefab modifications as necessary.
        /// </summary>
        /// <returns>
        /// True if any changes were made, otherwise, false.
        /// </returns>
        public abstract bool ApplyChanges();

        /// <summary>
        /// Gets a <see cref="PropertyListValueEntryChanger" /> for this value entry, used to apply modifications to value entries representing a list.
        /// </summary>
        public abstract PropertyListValueEntryChanger GetListValueEntryChanger();

        /// <summary>
        /// Gets an <see cref="IDictionaryHandler" /> for this value entry, used to apply modifications and get contextual data from value entries representing a dictionary.
        /// </summary>
        public abstract IDictionaryHandler GetDictionaryHandler();

        /// <summary>
        /// Whether any of the list instances that this value entry represents are read only.
        /// </summary>
        public abstract bool ListIsReadOnly();

        /// <summary>
        /// Updates the values contained in this value entry to the actual values in the target objects, and updates its state (override, type of value, etc.) accordingly.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// <para>Checks whether the values in this value entry are equal to the values in another value entry.</para>
        /// <para>Note, both value entries must have the same value type, and must represent values that are .NET value types.</para>
        /// </summary>
        public abstract bool ValueTypeValuesAreEqual(IPropertyValueEntry other);

        void IValueEntryActualValueSetter.SetActualValue(int index, object value)
        {
            this.SetActualValue(index, value);
        }

        /// <summary>
        /// Sets the actual value of a value entry, for a given selection index.
        /// </summary>
        protected abstract void SetActualValue(int index, object value);

        /// <summary>
        /// <para>Determines whether the value at the given selection index is different from the given prefab value, as is relevant for prefab modification checks.</para>
        /// <para>If the value is a reference type, null and type difference is checked. If value is a value type, a comparer from <see cref="Utilities.TypeExtensions.GetEqualityComparerDelegate{T}" /> is used.</para>
        /// <para>This method is best ignored unless you know what you are doing.</para>
        /// </summary>
        /// <param name="value">The value to check differences against.</param>
        /// <param name="index">The selection index to compare against.</param>
        public abstract bool ValueIsPrefabDifferent(object value, int index);
    }

    /// <summary>
    /// A polymorphic alias for a <see cref="PropertyValueEntry"/> instance, used to implement strongly typed polymorphism in Odin.
    /// </summary>
    public sealed class PropertyValueEntryAlias<TActualValue, TValue> : PropertyValueEntryAlias, IPropertyValueEntry<TValue>, IValueEntryActualValueSetter<TValue> where TValue : TActualValue
    {
        private static readonly bool ValueIsMarkedAtomic = typeof(TValue).IsMarkedAtomic();
        private static readonly IAtomHandler<TValue> AtomHandler = ValueIsMarkedAtomic ? AtomHandlerLocator.GetAtomHandler<TValue>() : null;
        private static readonly bool ValueIsValueType = typeof(TValue).IsValueType;

        private PropertyValueEntry<TActualValue> entry;
        private PropertyValueCollectionAlias<TActualValue, TValue> aliasValues;

        private bool valueIsWeakList;
        private bool valueIsStrongList;
        private bool valueIsValidDictionary;

        private PropertyListValueEntryChanger listValueChanger;
        private IDictionaryHandler dictionaryHandler;

        private TValue[] atomicValues;
        private TValue[] originalAtomicValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueEntryAlias{TActualValue, TValue}"/> class.
        /// </summary>
        /// <param name="valueEntry">The value entry to alias.</param>
        /// <exception cref="System.ArgumentNullException">valueEntry is null</exception>
        public PropertyValueEntryAlias(PropertyValueEntry<TActualValue> valueEntry)
        {
            if (valueEntry == null)
            {
                throw new ArgumentNullException("valueEntry");
            }

            this.entry = valueEntry;

            if (ValueIsMarkedAtomic)
            {
                this.atomicValues = new TValue[this.entry.ValueCount];
                this.originalAtomicValues = new TValue[this.entry.ValueCount];
            }

            this.aliasValues = new PropertyValueCollectionAlias<TActualValue, TValue>(valueEntry.Property, this.entry.Values, this.atomicValues, this.originalAtomicValues);

            this.valueIsWeakList = this.entry.ValueIsWeakList || (typeof(IList).IsAssignableFrom(typeof(TValue)) && !(typeof(TValue).IsArray && typeof(TValue).GetArrayRank() > 1));
            this.valueIsStrongList = this.entry.ValueIsStrongList || typeof(TValue).ImplementsOpenGenericInterface(typeof(IList<>));
            this.valueIsValidDictionary = this.entry.ValueIsValidDictionary || (typeof(TValue).ImplementsOpenGenericInterface(typeof(IDictionary<,>)));// && DictionaryKeyUtility.KeyTypeSupportsPersistentPaths(typeof(TValue).GetArgumentsOfInheritedOpenGenericInterface(typeof(IDictionary<,>))[0]));

            if (this.valueIsStrongList)
            {
                Type elementType = this.TypeOfValue.GetArgumentsOfInheritedOpenGenericInterface(typeof(IList<>))[0];

                this.listValueChanger = (PropertyListValueEntryChanger)Activator.CreateInstance(typeof(PropertyStrongListValueEntryChanger<,>).MakeGenericType(this.TypeOfValue, elementType), this);
            }
            else if (this.valueIsWeakList)
            {
                this.listValueChanger = (PropertyListValueEntryChanger)Activator.CreateInstance(typeof(PropertyWeakListValueEntryChanger<>).MakeGenericType(this.TypeOfValue), this);
            }

            if (this.valueIsValidDictionary)
            {
                var dictArgs = this.TypeOfValue.GetArgumentsOfInheritedOpenGenericInterface(typeof(IDictionary<,>));

                Type[] args = new Type[3]
                {
                    this.TypeOfValue,
                    dictArgs[0],
                    dictArgs[1]
                };

                this.dictionaryHandler = (IDictionaryHandler)Activator.CreateInstance(typeof(DictionaryHandler<,,>).MakeGenericType(args), this);
            }
        }

        /// <summary>
        /// Provides access to the strongly typed values of this value entry.
        /// </summary>
        public IPropertyValueCollection<TValue> Values { get { return this.aliasValues; } }

        /// <summary>
        /// <para>A strongly typed smart value that represents the first element of the value entry's value collection, but has "smart logic" for setting the value that detects relevant changes and applies them in parallel.</para>
        /// <para>This lets you often just use the smart value instead of having to deal with the tedium of multiple parallel values.</para>
        /// </summary>
        public TValue SmartValue
        {
            get { return (TValue)this.entry.SmartValue; }
            set
            {
                if (ValueIsMarkedAtomic)
                {
                    if (!AtomHandler.Compare(value, this.atomicValues[0]))
                    {
                        if (this.IsEditable == false)
                        {
                            Debug.LogWarning("Tried to change value of non-editable property '" + this.Property.NiceName + "' of type '" + this.TypeOfValue.GetNiceName() + "' at path '" + this.Property.Path + "'.");

                            // Reset value, as this is illegal
                            if (!ValueIsValueType)
                            {
                                AtomHandler.Copy(ref this.atomicValues[0], ref value);
                            }
                            return;
                        }

                        for (int i = 0; i < this.ValueCount; i++)
                        {
                            this.Values[i] = value;
                        }
                    }
                }

                this.entry.SmartValue = value;
            }
        }

        /// <summary>
        /// <para>A weakly typed smart value that represents the first element of the value entry's value collection, but has "smart logic" for setting the value that detects relevant changes and applies them in parallel.</para>
        /// <para>This lets you often just use the smart value instead of having to deal with the tedium of multiple parallel values.</para>
        /// </summary>
        public override object WeakSmartValue
        {
            get { return this.entry.WeakSmartValue; }
            set
            {
                try
                {
                    this.SmartValue = (TValue)value;
                }
                catch (InvalidCastException)
                {
                    this.entry.WeakSmartValue = value;
                }
            }
        }

        /// <summary>
        /// The number of parallel values this entry represents. This will always be exactly equal to the count of <see cref="PropertyTree.WeakTargets" />.
        /// </summary>
        public override int ValueCount { get { return this.entry.ValueCount; } }

        /// <summary>
        /// Whether this value entry is editable or not.
        /// </summary>
        public override bool IsEditable { get { return this.entry.IsEditable; } }

        /// <summary>
        /// If this value entry has the override type <see cref="PropertyValueState.Reference" />, this is the path of the property it references.
        /// </summary>
        public override string TargetReferencePath { get { return this.entry.TargetReferencePath; } }

        /// <summary>
        /// <para>The actual serialization backend for this value entry, possibly inherited from the serialization backend of the root property this entry is a child of.</para>
        /// <para>Note that this is *not* always equal to <see cref="InspectorPropertyInfo.SerializationBackend" />.</para>
        /// </summary>
        public override SerializationBackend SerializationBackend { get { return this.entry.SerializationBackend; } }

        /// <summary>
        /// The property whose values this value entry represents.
        /// </summary>
        public override InspectorProperty Property { get { return this.entry.Property; } }

        /// <summary>
        /// Whether the type of this entry implements <see cref="System.Collections.IList" />.
        /// </summary>
        public override bool ValueIsWeakList { get { return this.valueIsWeakList; } }

        /// <summary>
        /// Whether the type of this entry implements <see cref="System.Collections.Generic.IList{T}" />.
        /// </summary>
        public override bool ValueIsStrongList { get { return this.valueIsStrongList; } }

        /// <summary>
        /// Whether the type of this entry implements <see cref="System.Collections.Generic.IDictionary{TKey, TValue}" />.
        /// </summary>
        public override bool ValueIsValidDictionary { get { return this.valueIsValidDictionary; } }

        /// <summary>
        /// The value category of this value entry.
        /// </summary>
        public override PropertyValueCategory ValueCategory { get { return this.entry.ValueCategory; } }

        /// <summary>
        /// Provides access to the weakly typed values of this value entry.
        /// </summary>
        public override IPropertyValueCollection WeakValues { get { return this.entry.WeakValues; } }

        /// <summary>
        /// Whether this value entry has been changed from its prefab counterpart.
        /// </summary>
        public override bool ValueChangedFromPrefab { get { return this.entry.ValueChangedFromPrefab; } }

        /// <summary>
        /// Whether this value entry has had its list length changed from its prefab counterpart.
        /// </summary>
        public override bool ListLengthChangedFromPrefab { get { return this.entry.ListLengthChangedFromPrefab; } }

        /// <summary>
        /// Whether this value entry has had its dictionary values changes from its prefab counterpart.
        /// </summary>
        public override bool DictionaryChangedFromPrefab { get { return this.entry.DictionaryChangedFromPrefab; } }

        /// <summary>
        /// The type from which this value entry comes. If this value entry represents a member value, this is the declaring type of the member. If the value entry represents a collection element, this is the type of the collection.
        /// </summary>
        public override Type ParentType { get { return this.entry.ParentType; } }

        /// <summary>
        /// The most precise known contained type of the value entry. If polymorphism is in effect, this will be some type derived from <see cref="BaseValueType" />.
        /// </summary>
        public override Type TypeOfValue { get { return typeof(TValue); } }

        /// <summary>
        /// The base type of the value entry. If this is value entry represents a member value, this is the type of the member. If the value entry represents a collection element, this is the element type of the collection.
        /// </summary>
        public override Type BaseValueType { get { return this.entry.BaseValueType; } }

        /// <summary>
        /// The special state of the value entry.
        /// </summary>
        public override PropertyValueState ValueState { get { return this.entry.ValueState; } }

        /// <summary>
        /// Whether this type is marked as an atomic type using a <see cref="IAtomHandler"/>.
        /// </summary>
        public override bool IsMarkedAtomic { get { return ValueIsMarkedAtomic; } }

        /// <summary>
        /// An event that is invoked during <see cref="ApplyChanges" />, when any values have changed.
        /// </summary>
        public override event Action<int> OnValueChanged { add { this.entry.OnValueChanged += value; } remove { this.entry.OnValueChanged -= value; } }

        /// <summary>
        /// An event that is invoked during <see cref="ApplyChanges" />, when any child values have changed.
        /// </summary>
        public override event Action<int> OnChildValueChanged { add { this.entry.OnChildValueChanged += value; } remove { this.entry.OnChildValueChanged -= value; } }

        /// <summary>
        /// Applies the changes made to this value entry to the target objects, and registers prefab modifications as necessary.
        /// </summary>
        /// <returns>
        /// True if any changes were made, otherwise, false.
        /// </returns>
        public override bool ApplyChanges()
        {
            bool changed = this.entry.ApplyChanges();
            var tree = this.Property.Tree;

            if (this.listValueChanger != null && this.listValueChanger.ApplyChanges())
            {
                changed = true;
                this.Property.Children.Update();

                if (tree.HasPrefabs)
                {
                    if (this.SerializationBackend == SerializationBackend.Odin)
                    {
                        for (int i = 0; i < this.ValueCount; i++)
                        {
                            if (tree.TargetPrefabs[i] != null)
                            {
                                tree.RegisterPrefabListLengthModification(this.Property, i, this.Property.Children.Count);
                            }
                        }

                        for (int i = 0; i < this.Property.Children.Count; i++)
                        {
                            this.Property.Children[i].Update(true);
                        }
                    }
                }

                tree.DelayAction(() =>
                {
                    tree.UpdateTree();
                });

                for (int i = 0; i < this.ValueCount; i++)
                {
                    this.entry.TriggerOnValueChanged(i);
                }
            }

            if (this.dictionaryHandler != null && this.dictionaryHandler.ApplyChanges())
            {
                changed = true;
                this.Property.Children.Update();

                for (int i = 0; i < this.ValueCount; i++)
                {
                    this.entry.TriggerOnValueChanged(i);
                }
            }

            return changed;
        }

        /// <summary>
        /// Gets a <see cref="PropertyListValueEntryChanger" /> for this value entry, used to apply modifications to value entries representing a list.
        /// </summary>
        public override PropertyListValueEntryChanger GetListValueEntryChanger()
        {
            return this.listValueChanger;
        }

        /// <summary>
        /// Gets an <see cref="IDictionaryHandler" /> for this value entry, used to apply modifications and get contextual data from value entries representing a dictionary.
        /// </summary>
        public override IDictionaryHandler GetDictionaryHandler()
        {
            return this.dictionaryHandler;
        }

        /// <summary>
        /// Whether any of the list instances that this value entry represents are read only.
        /// </summary>
        public override bool ListIsReadOnly()
        {
            return this.entry.ListIsReadOnly();
        }

        /// <summary>
        /// Updates the values contained in this value entry to the actual values in the target objects, and updates its state (override, type of value, etc.) accordingly.
        /// </summary>
        public override void Update()
        {
            this.entry.Update();

            if (ValueIsMarkedAtomic)
            {
                for (int i = 0; i < this.ValueCount; i++)
                {
                    try
                    {
                        TValue value = (TValue)this.entry.Values[i];

                        AtomHandler.Copy(ref value, ref this.atomicValues[i]);
                        AtomHandler.Copy(ref value, ref this.originalAtomicValues[i]);
                    }
                    catch (InvalidCastException) { }
                }
            }
        }

        /// <summary>
        /// <para>Checks whether the values in this value entry are equal to the values in another value entry.</para>
        /// <para>Note, both value entries must have the same value type, and must represent values that are .NET value types.</para>
        /// </summary>
        public override bool ValueTypeValuesAreEqual(IPropertyValueEntry other)
        {
            if (!this.TypeOfValue.IsValueType || !other.TypeOfValue.IsValueType || other.TypeOfValue != this.TypeOfValue)
            {
                return false;
            }

            IPropertyValueEntry<TValue> castOther = (IPropertyValueEntry<TValue>)other;

            if (other.ValueCount == 1 || other.ValueState == PropertyValueState.None)
            {
                TValue otherValue = castOther.Values[0];

                for (int i = 0; i < this.ValueCount; i++)
                {
                    if (!PropertyValueEntry<TValue>.EqualityComparer(this.Values[i], otherValue))
                    {
                        return false;
                    }
                }

                return true;
            }
            else if (this.ValueCount == 1 || this.ValueState == PropertyValueState.None)
            {
                TValue thisValue = this.Values[0];

                for (int i = 0; i < this.ValueCount; i++)
                {
                    if (!PropertyValueEntry<TValue>.EqualityComparer(thisValue, castOther.Values[i]))
                    {
                        return false;
                    }
                }

                return true;
            }
            else if (this.ValueCount == other.ValueCount)
            {
                for (int i = 0; i < this.ValueCount; i++)
                {
                    if (!PropertyValueEntry<TValue>.EqualityComparer(this.Values[i], castOther.Values[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the actual value of a value entry, for a given selection index.
        /// </summary>
        protected sealed override void SetActualValue(int index, object value)
        {
            (this.entry as IValueEntryActualValueSetter).SetActualValue(index, value);
        }

        void IValueEntryActualValueSetter<TValue>.SetActualValue(int index, TValue value)
        {
            (this.entry as IValueEntryActualValueSetter).SetActualValue(index, value);
        }

        /// <summary>
        /// <para>Determines whether the value at the given selection index is different from the given prefab value, as is relevant for prefab modification checks.</para>
        /// <para>If the value is a reference type, null and type difference is checked. If value is a value type, a comparer from <see cref="M:Sirenix.Utilities.TypeExtensions.GetEqualityComparerDelegate``1" /> is used.</para>
        /// <para>This method is best ignored unless you know what you are doing.</para>
        /// </summary>
        /// <param name="value">The value to check differences against.</param>
        /// <param name="index">The selection index to compare against.</param>
        public bool ValueIsPrefabDifferent(TValue value, int index)
        {
            if (ValueIsMarkedAtomic)
            {
                return !AtomHandler.Compare(value, this.Values[index]);
            }

            return this.entry.ValueIsPrefabDifferent((TActualValue)value, index);
        }

        /// <summary>
        /// <para>Determines whether the value at the given selection index is different from the given prefab value, as is relevant for prefab modification checks.</para>
        /// <para>If the value is a reference type, null and type difference is checked. If value is a value type, a comparer from <see cref="Utilities.TypeExtensions.GetEqualityComparerDelegate{T}" /> is used.</para>
        /// <para>This method is best ignored unless you know what you are doing.</para>
        /// </summary>
        /// <param name="value">The value to check differences against.</param>
        /// <param name="index">The selection index to compare against.</param>
        public override bool ValueIsPrefabDifferent(object value, int index)
        {
            if (object.ReferenceEquals(value, null))
            {
                if (ValueIsValueType)
                {
                    return true;
                }
            }
            else if (ValueIsValueType)
            {
                if (typeof(TValue) != value.GetType())
                {
                    return true;
                }
            }
            else if (!typeof(TValue).IsAssignableFrom(value.GetType()))
            {
                return true;
            }

            return this.ValueIsPrefabDifferent((TValue)value, index);
        }
    }
}
#endif