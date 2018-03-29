#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyValueEntry.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
    using Serialization;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Represents the values of an <see cref="InspectorProperty"/>, and contains utilities for querying the values' type and getting and setting them.
    /// </summary>
    /// <seealso cref="Sirenix.OdinInspector.Editor.IPropertyValueEntry" />
    public abstract class PropertyValueEntry : IPropertyValueEntry
    {
        /// <summary>
        /// Delegate type used for the events <see cref="OnValueChanged"/> and <see cref="OnChildValueChanged"/>.
        /// </summary>
        public delegate void ValueChangedDelegate(int targetIndex);

        private InspectorProperty parentValueProperty;
        private InspectorProperty property;
        private bool isBaseEditable;
        private Type actualTypeOfValue;

        /// <summary>
        /// <para>The nearest parent property that has a value.
        /// That is, the property from which this value
        /// entry will fetch its parentvalues from in order
        /// to extract its own values.</para>
        ///
        /// <para>If <see cref="ParentValueProperty"/> is null, this is a root property.</para>
        /// </summary>
        protected InspectorProperty ParentValueProperty { get { return this.parentValueProperty; } }

        /// <summary>
        /// Whether this value entry represents a boxed value type.
        /// </summary>
        protected bool IsBoxedValueType { get; private set; }

        /// <summary>
        /// The number of parallel values this entry represents. This will always be exactly equal to the count of <see cref="PropertyTree.WeakTargets" />.
        /// </summary>
        public int ValueCount { get; private set; }

        /// <summary>
        /// Whether this value entry is editable or not.
        /// </summary>
        public bool IsEditable
        {
            get
            {
                if (this.isBaseEditable)
                {
                    if (this.parentValueProperty != null)
                    {
                        var parentValueEntry = this.parentValueProperty.ValueEntry;

                        if (!parentValueEntry.IsEditable)
                        {
                            return false;
                        }

                        if (parentValueEntry.ValueIsStrongList || parentValueEntry.ValueIsWeakList)
                        {
                            bool parentListIsReadOnly = parentValueEntry.ListIsReadOnly();

                            if (parentListIsReadOnly)
                            {
                                return false;
                            }

                            return true;
                        }
                    }

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// If this value entry has the override type <see cref="PropertyValueState.Reference" />, this is the path of the property it references.
        /// </summary>
        public string TargetReferencePath { get; private set; }

        /// <summary>
        /// <para>The actual serialization backend for this value entry, possibly inherited from the serialization backend of the root property this entry is a child of.</para>
        /// <para>Note that this is *not* always equal to <see cref="InspectorPropertyInfo.SerializationBackend" />.</para>
        /// </summary>
        public SerializationBackend SerializationBackend { get; private set; }

        /// <summary>
        /// The property whose values this value entry represents.
        /// </summary>
        public InspectorProperty Property { get { return this.property; } }

        /// <summary>
        /// Whether the type of this entry implements <see cref="System.Collections.Generic.IList{T}" />.
        /// </summary>
        public bool ValueIsStrongList { get; private set; }

        /// <summary>
        /// Whether the type of this entry implements <see cref="System.Collections.IList" />.
        /// </summary>
        public bool ValueIsWeakList { get; private set; }

        /// <summary>
        /// Whether the type of this entry implements <see cref="System.Collections.Generic.IDictionary{TKey, TValue}" />.
        /// </summary>
        public bool ValueIsValidDictionary { get; private set; }

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
        public bool ValueChangedFromPrefab { get; internal set; }

        /// <summary>
        /// Whether this value entry has had its list length changed from its prefab counterpart.
        /// </summary>
        public bool ListLengthChangedFromPrefab { get; internal set; }

        /// <summary>
        /// Whether this value entry has had its dictionary values changes from its prefab counterpart.
        /// </summary>
        public bool DictionaryChangedFromPrefab { get; internal set; }

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
        public Type TypeOfValue
        {
            get
            {
                if (this.actualTypeOfValue == null)
                {
                    this.actualTypeOfValue = this.BaseValueType;
                }

                return this.actualTypeOfValue;
            }
        }

        /// <summary>
        /// The base type of the value entry. If this is value entry represents a member value, this is the type of the member. If the value entry represents a collection element, this is the element type of the collection.
        /// </summary>
        public Type BaseValueType { get; private set; }

        /// <summary>
        /// The special state of the value entry.
        /// </summary>
        public PropertyValueState ValueState { get; private set; }

        /// <summary>
        /// Whether this value entry is an alias, or not. Value entry aliases are used to provide strongly typed value entries in the case of polymorphism.
        /// </summary>
        public bool IsAlias { get { return false; } }

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
        public event Action<int> OnValueChanged;

        /// <summary>
        /// An event that is invoked during <see cref="ApplyChanges" />, when any child values have changed.
        /// </summary>
        public event Action<int> OnChildValueChanged;

        /// <summary>
        /// Updates the values contained in this value entry to the actual values in the target objects, and updates its state (override, type of value, etc.) accordingly.
        /// </summary>
        public void Update()
        {
            this.UpdateValues();

            if (!this.BaseValueType.IsValueType && (this.SerializationBackend != SerializationBackend.Unity || typeof(UnityEngine.Object).IsAssignableFrom(this.BaseValueType)))
            {
                var type = this.GetMostPreciseContainedType();

                if (this.actualTypeOfValue != type)
                {
                    this.actualTypeOfValue = type;

                    this.ValueIsWeakList = typeof(IList).IsAssignableFrom(type) && !(type.IsArray && type.GetArrayRank() > 1); ;
                    this.ValueIsStrongList = type.ImplementsOpenGenericInterface(typeof(IList<>));
                    this.ValueIsValidDictionary = type.ImplementsOpenGenericInterface(typeof(IDictionary<,>));// && DictionaryKeyUtility.KeyTypeSupportsPersistentPaths(type.GetArgumentsOfInheritedOpenGenericInterface(typeof(IDictionary<,>))[0]);
                    this.IsBoxedValueType = this.BaseValueType == typeof(object) && type.IsValueType;
                }
            }

            this.ValueState = this.GetValueState();

            if (this.ValueState == PropertyValueState.Reference)
            {
                string targetReferencePath;
                this.property.Tree.ObjectIsReferenced(this.WeakValues[0], out targetReferencePath);
                this.TargetReferencePath = targetReferencePath;
            }
            else
            {
                this.TargetReferencePath = null;
            }
        }

        /// <summary>
        /// <para>Checks whether the values in this value entry are equal to the values in another value entry.</para>
        /// <para>Note, both value entries must have the same value type, and must represent values that are .NET value types.</para>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public abstract bool ValueTypeValuesAreEqual(IPropertyValueEntry other);

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
        /// Determines the value state of this value entry.
        /// </summary>
        protected abstract PropertyValueState GetValueState();

        /// <summary>
        /// Determines what the most precise contained type is on this value entry.
        /// </summary>
        protected abstract Type GetMostPreciseContainedType();

        /// <summary>
        /// Updates all values in this value entry from the target tree values.
        /// </summary>
        protected abstract void UpdateValues();

        /// <summary>
        /// Initializes this value entry.
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// Whether any of the list instances that this value entry represents are read only.
        /// </summary>
        public abstract bool ListIsReadOnly();

        internal void TriggerOnValueChanged(int index)
        {
            this.Property.Tree.DelayActionUntilRepaint(() =>
            {
                if (this.OnValueChanged != null)
                {
                    try
                    {
                        this.OnValueChanged(index);
                    }
                    catch (ExitGUIException ex)
                    {
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                this.Property.Tree.InvokeOnPropertyValueChanged(this.Property, index);
            });

            if (this.ParentValueProperty != null)
            {
                this.ParentValueProperty.BaseValueEntry.TriggerOnChildValueChanged(index);
            }
        }

        internal void TriggerOnChildValueChanged(int index)
        {
            this.Property.Tree.DelayActionUntilRepaint(() =>
            {
                if (this.OnChildValueChanged != null)
                {
                    try
                    {
                        this.OnChildValueChanged(index);
                    }
                    catch (ExitGUIException ex)
                    {
                        throw ex;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            });

            if (this.ParentValueProperty != null)
            {
                this.ParentValueProperty.BaseValueEntry.TriggerOnChildValueChanged(index);
            }
        }

        /// <summary>
        /// Creates an alias value entry of a given type, for a given value entry. This is used to implement polymorphism in Odin.
        /// </summary>
        public static IPropertyValueEntry CreateAlias(PropertyValueEntry entry, Type valueType)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }

            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            Type aliasEntryType = typeof(PropertyValueEntryAlias<,>).MakeGenericType(entry.BaseValueType, valueType);
            return (IPropertyValueEntry)Activator.CreateInstance(aliasEntryType, entry);
        }

        /// <summary>
        /// Creates a value entry for a given property, of a given value type. Note that the created value entry is returned un-updated, and needs to have <see cref="Update"/> called on it before it can be used.
        /// </summary>
        public static PropertyValueEntry Create(InspectorProperty property, Type valueType)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            if (property.Info.PropertyType != PropertyType.ReferenceType && property.Info.PropertyType != PropertyType.ValueType)
            {
                throw new ArgumentException("Cannot create a " + typeof(PropertyValueEntry).Name + " for a property which is not a reference type or a value type.");
            }

            Type parentType;
            InspectorProperty parentValueProperty = property.ParentValueProperty;

            // We have a parent value property
            if (parentValueProperty != null)
            {
                parentType = parentValueProperty.ValueEntry.TypeOfValue;
            }
            // We are a root property, and our parent values are the tree targets
            else
            {
                parentType = property.Tree.TargetType;
            }

            PropertyValueEntry result;

            if (parentType.ImplementsOpenGenericInterface(typeof(IDictionary<,>)))
            {
                Type[] args = parentType.GetArgumentsOfInheritedOpenGenericInterface(typeof(IDictionary<,>));
                Type editableKeyValuePairType = typeof(EditableKeyValuePair<,>).MakeGenericType(args);

                if (valueType != editableKeyValuePairType)
                {
                    throw new ArgumentException("Parent is a dictionary, but given value type '" + valueType.GetNiceName() + "' is not an '" + editableKeyValuePairType.GetNiceName() + "'.");
                }

                Type entryType = typeof(PropertyDictionaryElementValueEntry<,,>).MakeGenericType(parentType, args[0], args[1]);
                result = (PropertyValueEntry)Activator.CreateInstance(entryType, nonPublic: true);
                result.BaseValueType = editableKeyValuePairType;
            }
            else if (parentType.ImplementsOpenGenericInterface(typeof(IList<>)))
            {
                Type elementBaseType = parentType.GetArgumentsOfInheritedOpenGenericInterface(typeof(IList<>))[0];

                if (elementBaseType.IsAssignableFrom(valueType) == false)
                {
                    throw new ArgumentException("Parent type is a strong list (" + parentType.GetNiceName() + "), but the given value type '" + valueType.GetNiceName() + "' cannot be assigned to the list element type '" + elementBaseType.GetNiceName() + "'.");
                }

                result = (PropertyValueEntry)Activator.CreateInstance(typeof(PropertyStrongListElementValueEntry<,,>).MakeGenericType(parentType, elementBaseType, valueType), nonPublic: true);
                result.BaseValueType = elementBaseType;
            }
            else if (typeof(IList).IsAssignableFrom(parentType))
            {
                result = (PropertyValueEntry)Activator.CreateInstance(typeof(PropertyWeakListElementValueEntry<,>).MakeGenericType(parentType, valueType), nonPublic: true);
                result.BaseValueType = typeof(object);
            }
            else
            {
                var baseType = property.Info.TypeOfValue;

                if (baseType.IsAssignableFrom(valueType) == false)
                {
                    throw new ArgumentException("Cannot assign the given value type '" + valueType.GetNiceName() + "' to the member type '" + baseType.GetNiceName() + "'.");
                }

                if (property.Info.IsUnityPropertyOnly)
                {
                    result = (PropertyValueEntry)Activator.CreateInstance(typeof(UnityPropertyValueEntry<,>).MakeGenericType(parentType, valueType), nonPublic: true);
                    result.BaseValueType = baseType;
                }
                else
                {
                    result = (PropertyValueEntry)Activator.CreateInstance(typeof(PropertyMemberValueEntry<,>).MakeGenericType(parentType, valueType), nonPublic: true);
                    result.BaseValueType = baseType;
                }
            }

            result.property = property;
            result.ValueCount = property.Tree.WeakTargets.Count;
            result.parentValueProperty = parentValueProperty;

            result.ValueIsValidDictionary = valueType.ImplementsOpenGenericInterface(typeof(IDictionary<,>));// && DictionaryKeyUtility.KeyTypeSupportsPersistentPaths(valueType.GetArgumentsOfInheritedOpenGenericInterface(typeof(IDictionary<,>))[0]);
            result.ValueIsStrongList = valueType.ImplementsOpenGenericInterface(typeof(IList<>));
            result.ValueIsWeakList = typeof(IList).IsAssignableFrom(valueType) && !(valueType.IsArray && valueType.GetArrayRank() > 1);
            result.IsBoxedValueType = result.BaseValueType == typeof(object) && result.TypeOfValue.IsValueType;

            if (parentValueProperty != null)
            {
                result.SerializationBackend = property.Info.SerializationBackend == SerializationBackend.None ? SerializationBackend.None : parentValueProperty.BaseValueEntry.SerializationBackend;
                result.isBaseEditable = parentValueProperty.BaseValueEntry.isBaseEditable && property.Info.IsEditable;
            }
            else
            {
                result.SerializationBackend = property.Info.SerializationBackend;
                result.isBaseEditable = property.Info.IsEditable;
            }

            result.Initialize();

            return result;
        }

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
    /// Represents the values of an <see cref="InspectorProperty" />, and contains utilities for querying the values' type and getting and setting them.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="Sirenix.OdinInspector.Editor.IPropertyValueEntry" />
    public abstract class PropertyValueEntry<TValue> : PropertyValueEntry, IPropertyValueEntry<TValue>, IValueEntryActualValueSetter<TValue>
    {
        /// <summary>
        /// An equality comparer for comparing values of type <see cref="TValue"/>. This is gotten using <see cref="TypeExtensions.GetEqualityComparerDelegate{T}"/>.
        /// </summary>
        public static readonly Func<TValue, TValue, bool> EqualityComparer = TypeExtensions.GetEqualityComparerDelegate<TValue>();

        /// <summary>
        /// Whether <see cref="TValue"/>.is a primitive type; that is, the type is primitive, a string, or an enum.
        /// </summary>
        protected static readonly bool ValueIsPrimitive = typeof(TValue).IsPrimitive || typeof(TValue) == typeof(string) || typeof(TValue).IsEnum;

        /// <summary>
        /// Whether <see cref="TValue"/> is a value type.
        /// </summary>
        protected static readonly bool ValueIsValueType = typeof(TValue).IsValueType;

        /// <summary>
        /// Whether <see cref="TValue"/> implements <see cref="IList"/>.
        /// </summary>
        protected static readonly bool BaseValueIsWeakList = typeof(IList).IsAssignableFrom(typeof(TValue)) && !(typeof(TValue).IsArray && typeof(TValue).GetArrayRank() > 1);

        /// <summary>
        /// Whether <see cref="TValue"/> implements <see cref="IList{T}"/>.
        /// </summary>
        protected static readonly bool BaseValueIsStrongList = typeof(TValue).ImplementsOpenGenericInterface(typeof(IList<>));

        /// <summary>
        /// Whether <see cref="PropertyValueEntry.TypeOfValue"/> is derived from <see cref="UnityEngine.Object"/>.
        /// </summary>
        protected bool ValueIsUnityObject { get { return typeof(UnityEngine.Object).IsAssignableFrom(this.TypeOfValue); } }

        /// <summary>
        /// A delegate that gets the count of a strong list instance.
        /// </summary>
        protected static readonly ValueGetter<TValue, int> StrongListCountGetter;

        /// <summary>
        /// A delegate that gets the count of a weak list instance.
        /// </summary>
        protected static readonly ValueGetter<TValue, bool> StrongListIsReadOnlyGetter;

        protected static readonly bool ValueIsMarkedAtomic = typeof(TValue).IsMarkedAtomic();

        protected static readonly IAtomHandler<TValue> AtomHandler = ValueIsMarkedAtomic ? AtomHandlerLocator.GetAtomHandler<TValue>() : null;

        private PropertyValueCollection<TValue> values;

        static PropertyValueEntry()
        {
            if (BaseValueIsStrongList && !BaseValueIsWeakList)
            {
                Type arg = typeof(TValue).GetArgumentsOfInheritedOpenGenericInterface(typeof(IList<>))[0];
                Type strongCollectionInterface = typeof(ICollection<>).MakeGenericType(arg);

                PropertyInfo countProperty = strongCollectionInterface.GetProperty("Count");
                PropertyInfo isReadOnlyProperty = strongCollectionInterface.GetProperty("IsReadOnly");

                StrongListCountGetter = EmitUtilities.CreateInstancePropertyGetter<TValue, int>(countProperty);
                StrongListIsReadOnlyGetter = EmitUtilities.CreateInstancePropertyGetter<TValue, bool>(isReadOnlyProperty);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueEntry{TValue}"/> class.
        /// </summary>
        protected PropertyValueEntry()
        {
        }

        /// <summary>
        /// Provides access to the weakly typed values of this value entry.
        /// </summary>
        public sealed override IPropertyValueCollection WeakValues { get { return this.values; } }

        /// <summary>
        /// Provides access to the strongly typed values of this value entry.
        /// </summary>
        public IPropertyValueCollection<TValue> Values { get { return this.values; } }

        /// <summary>
        /// Whether this type is marked as an atomic type using a <see cref="IAtomHandler"/>.
        /// </summary>
        public override bool IsMarkedAtomic { get { return ValueIsMarkedAtomic; } }

        /// <summary>
        /// <para>A weakly typed smart value that represents the first element of the value entry's value collection, but has "smart logic" for setting the value that detects relevant changes and applies them in parallel.</para>
        /// <para>This lets you often just use the smart value instead of having to deal with the tedium of multiple parallel values.</para>
        /// </summary>
        public override object WeakSmartValue
        {
            get { return this.SmartValue; }
            set
            {
                try
                {
                    this.SmartValue = (TValue)value;
                }
                catch (InvalidCastException)
                {
                    if (object.ReferenceEquals(value, null))
                    {
                        Debug.LogError("Invalid cast on set weak value! Could not cast value 'null' to the type '" + typeof(TValue).GetNiceName() + "' on property " + this.Property.Path + ".");
                    }
                    else
                    {
                        Debug.LogError("Invalid cast on set weak value! Could not cast value of type '" + value.GetType().GetNiceName() + "' to '" + typeof(TValue).GetNiceName() + "' on property " + this.Property.Path + ".");
                    }
                }
            }
        }

        /// <summary>
        /// <para>A strongly typed smart value that represents the first element of the value entry's value collection, but has "smart logic" for setting the value that detects relevant changes and applies them in parallel.</para>
        /// <para>This lets you often just use the smart value instead of having to deal with the tedium of multiple parallel values.</para>
        /// </summary>
        public TValue SmartValue
        {
            get
            {
                return this.values[0];
            }

            set
            {
                if (ValueIsMarkedAtomic)
                {
                    if (!AtomHandler.Compare(value, this.AtomValuesArray[0]))
                    {
                        if (this.IsEditable == false)
                        {
                            Debug.LogWarning("Tried to change value of non-editable property '" + this.Property.NiceName + "' of type '" + this.TypeOfValue.GetNiceName() + "' at path '" + this.Property.Path + "'.");

                            // Reset value, as this is illegal
                            if (!ValueIsValueType)
                            {
                                AtomHandler.Copy(ref this.AtomValuesArray[0], ref value);
                            }
                            return;
                        }

                        for (int i = 0; i < this.ValueCount; i++)
                        {
                            this.values[i] = value;
                        }
                    }
                }
                else if (ValueIsPrimitive || ValueIsValueType)
                {
                    // Determine if the value has changed

                    if (!EqualityComparer(value, this.values[0]))
                    {
                        if (this.IsEditable == false)
                        {
                            Debug.LogWarning("Tried to change value of non-editable property '" + this.Property.NiceName + "' of type '" + this.TypeOfValue.GetNiceName() + "' at path '" + this.Property.Path + "'.");
                            return;
                        }

                        for (int i = 0; i < this.ValueCount; i++)
                        {
                            this.values[i] = value;
                        }
                    }
                }
                else if (!object.ReferenceEquals(value, this.SmartValue))    // If the reference has not changed; there is no reason to run all this code
                {
                    if (this.IsEditable == false)
                    {
                        Debug.LogWarning("Tried to change value of non-editable property '" + this.Property.NiceName + "' of type '" + this.TypeOfValue.GetNiceName() + "' at path '" + this.Property.Path + "'.");
                        return;
                    }

                    if (this.ValueCount == 1 || object.ReferenceEquals(value, null))
                    {
                        for (int i = 0; i < this.ValueCount; i++)
                        {
                            this.values[i] = value;
                        }
                    }
                    else
                    {
                        // We are dealing with multiple references, meaning we have multiple *parallel trees* of references.
                        // Now we need to haxx this to make it all work in a somewhat sensible way.
                        //
                        // In short, the idea of the code below is to determine whether we can mirror the reference
                        // assignment "horizontally" through the tree on all targets by matching reference paths.
                        //
                        // If that isn't possible because the assigned reference has not been seen before, then we
                        // simply assign the same reference to all targets. It will become "split" between them
                        // when they are individually serialized and deserialized later in the editor.

                        bool valueIsSet = false;
                        string seenBeforePath;

                        if (this.Property.Tree.ObjectIsReferenced(value, out seenBeforePath))
                        {
                            InspectorProperty referencedProperty = this.Property.Tree.GetPropertyAtPath(seenBeforePath);

                            if (referencedProperty != null && referencedProperty.Info.PropertyType == PropertyType.ReferenceType)
                            {
                                // We can mirror the values directly
                                for (int i = 0; i < this.ValueCount; i++)
                                {
                                    TValue mirroredValue = (TValue)referencedProperty.ValueEntry.WeakValues[i];
                                    this.values[i] = mirroredValue;
                                }

                                valueIsSet = true;
                            }
                        }

                        if (!valueIsSet)
                        {
                            for (int i = 0; i < this.ValueCount; i++)
                            {
                                this.values[i] = value;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// An array containing the original values as they were at the beginning of frame.
        /// </summary>
        protected TValue[] OriginalValuesArray { get; private set; }

        /// <summary>
        /// An array containing the current modified set of values.
        /// </summary>
        protected TValue[] InternalValuesArray { get; private set; }

        /// <summary>
        /// An array containing the current modified set of atomic values.
        /// </summary>
        protected TValue[] AtomValuesArray { get; private set; }

        /// <summary>
        /// An array containing the original set of atomic values.
        /// </summary>
        protected TValue[] OriginalAtomValuesArray { get; private set; }

        /// <summary>
        /// Initializes this value entry.
        /// </summary>
        protected override void Initialize()
        {
            this.OriginalValuesArray = new TValue[this.Property.Tree.WeakTargets.Count];
            this.InternalValuesArray = new TValue[this.Property.Tree.WeakTargets.Count];

            if (this.IsMarkedAtomic)
            {
                this.AtomValuesArray = new TValue[this.Property.Tree.WeakTargets.Count];
                this.OriginalAtomValuesArray = new TValue[this.Property.Tree.WeakTargets.Count];
            }

            this.values = new PropertyValueCollection<TValue>(this.Property, this.InternalValuesArray, this.OriginalValuesArray, this.AtomValuesArray, this.OriginalAtomValuesArray);
        }

        /// <summary>
        /// Sets the actual target tree value.
        /// </summary>
        protected abstract void SetActualBoxedValueImplementation(int index, object value);

        /// <summary>
        /// Sets the actual target tree value.
        /// </summary>
        protected abstract void SetActualValueImplementation(int index, TValue value);

        /// <summary>
        /// Whether any of the list instances that this value entry represents are read only.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Cannot call ListIsReadOnly on a property value entry that is not either a weak or a strong list.</exception>
        public override bool ListIsReadOnly()
        {
            if (this.ValueIsWeakList)
            {
                for (int i = 0; i < this.ValueCount; i++)
                {
                    IList list = (IList)this.values[i];

                    try
                    {
                        if (!object.ReferenceEquals(list, null) && list.IsReadOnly)
                        {
                            return true;
                        }
                    }
                    catch (NotSupportedException) { return false; }
                    catch (NotImplementedException) { return false; }
                }

                return false;
            }
            else if (this.ValueIsStrongList)
            {
                for (int i = 0; i < this.ValueCount; i++)
                {
                    TValue list = this.values[i];

                    try
                    {
                        if (!object.ReferenceEquals(list, null) && StrongListIsReadOnlyGetter(ref list))
                        {
                            return true;
                        }
                    }
                    catch (NotSupportedException) { return false; }
                    catch (NotImplementedException) { return false; }
                }

                return false;
            }

            throw new InvalidOperationException("Cannot call ListIsReadOnly on a property value entry that is not either a weak or a strong list.");
        }

        /// <summary>
        /// <para>Checks whether the values in this value entry are equal to the values in another value entry.</para>
        /// <para>Note, both value entries must have the same value type, and must represent values that are .NET value types.</para>
        /// </summary>
        public override bool ValueTypeValuesAreEqual(IPropertyValueEntry other)
        {
            if (!ValueIsValueType || !other.TypeOfValue.IsValueType || other.TypeOfValue != this.TypeOfValue)
            {
                return false;
            }

            IPropertyValueEntry<TValue> castOther = (IPropertyValueEntry<TValue>)other;

            if (other.ValueCount == 1 || other.ValueState == PropertyValueState.None)
            {
                TValue otherValue = castOther.Values[0];

                for (int i = 0; i < this.ValueCount; i++)
                {
                    if (!EqualityComparer(this.Values[i], otherValue))
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
                    if (!EqualityComparer(thisValue, castOther.Values[i]))
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
                    if (!EqualityComparer(this.Values[i], castOther.Values[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        void IValueEntryActualValueSetter<TValue>.SetActualValue(int index, TValue value)
        {
            this.InternalValuesArray[index] = value;

            if (this.IsBoxedValueType)
            {
                this.SetActualBoxedValueImplementation(index, value);
            }
            else
            {
                this.SetActualValueImplementation(index, value);
            }
        }

        void IValueEntryActualValueSetter.SetActualValue(int index, object value)
        {
            this.InternalValuesArray[index] = (TValue)value;

            if (this.IsBoxedValueType)
            {
                this.SetActualBoxedValueImplementation(index, value);
            }
            else
            {
                this.SetActualValueImplementation(index, (TValue)value);
            }
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

        /// <summary>
        /// <para>Determines whether the value at the given selection index is different from the given prefab value, as is relevant for prefab modification checks.</para>
        /// <para>If the value is a reference type, null and type difference is checked. If value is a value type, a comparer from <see cref="M:Sirenix.Utilities.TypeExtensions.GetEqualityComparerDelegate``1" /> is used.</para>
        /// <para>This method is best ignored unless you know what you are doing.</para>
        /// </summary>
        /// <param name="value">The value to check differences against.</param>
        /// <param name="index">The selection index to compare against.</param>
        public bool ValueIsPrefabDifferent(TValue value, int index)
        {
            TValue thisValue = this.Values[index];

            if (IsMarkedAtomic)
            {
                return !AtomHandler.Compare(value, thisValue);
            }

            if (ValueIsValueType)
            {
                if (ValueIsPrimitive)
                {
                    return !EqualityComparer(value, thisValue);
                }
                else
                {
                    return false;
                }
            }

            if (this.ValueIsUnityObject)
            {
                return !object.ReferenceEquals(thisValue, value);
            }

            Type a = null;
            Type b = null;

            if (!object.ReferenceEquals(value, null))
            {
                a = value.GetType();
            }

            if (!object.ReferenceEquals(thisValue, null))
            {
                b = thisValue.GetType();
            }

            return a != b;
        }
    }

    /// <summary>
    /// Represents the values of an <see cref="InspectorProperty" />, and contains utilities for querying the values' type and getting and setting them.
    /// </summary>
    /// <typeparam name="TParent">The type of the parent.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="Sirenix.OdinInspector.Editor.IPropertyValueEntry" />
    public abstract class PropertyValueEntry<TParent, TValue> : PropertyValueEntry<TValue>
    {
        private PropertyListValueEntryChanger listValueChanger;
        private IDictionaryHandler dictionaryHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValueEntry{TParent, TValue}"/> class.
        /// </summary>
        protected internal PropertyValueEntry()
        {
        }

        /// <summary>
        /// The type from which this value entry comes. If this value entry represents a member value, this is the declaring type of the member. If the value entry represents a collection element, this is the type of the collection.
        /// </summary>
        public sealed override Type ParentType { get { return typeof(TParent); } }

        /// <summary>
        /// Gets a <see cref="PropertyListValueEntryChanger" /> for this value entry, used to apply modifications to value entries representing a list.
        /// </summary>
        public override PropertyListValueEntryChanger GetListValueEntryChanger()
        {
            if (this.Property.ValueEntry != this)
            {
                // If we're aliased, we want to return the alias' list value entry changer.
                return this.Property.ValueEntry.GetListValueEntryChanger();
            }

            return this.listValueChanger;
        }

        /// <summary>
        /// Gets an <see cref="IDictionaryHandler" /> for this value entry, used to apply modifications and get contextual data from value entries representing a dictionary.
        /// </summary>
        public override IDictionaryHandler GetDictionaryHandler()
        {
            if (this.Property.ValueEntry != this)
            {
                // If we're aliased, we want to return the alias' dictionary key mapper.
                return this.Property.ValueEntry.GetDictionaryHandler();
            }

            return this.dictionaryHandler;
        }

        /// <summary>
        /// Determines what the most precise contained type is on this value entry.
        /// </summary>
        protected sealed override Type GetMostPreciseContainedType()
        {
            if (ValueIsValueType)
            {
                return typeof(TValue);
            }

            var values = this.InternalValuesArray;
            Type type = null;

            for (int i = 0; i < values.Length; i++)
            {
                TValue value = values[i];

                if (object.ReferenceEquals(value, null))
                {
                    return InspectorProperty.GetBaseContainedValueType(this.Property);
                }

                if (i == 0)
                {
                    type = value.GetType();
                }
                else if (type != value.GetType())
                {
                    return InspectorProperty.GetBaseContainedValueType(this.Property);
                }
            }

            return type;
        }

        /// <summary>
        /// Initializes this value entry.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            if (this.ValueIsStrongList)
            {
                Type elementType = this.TypeOfValue.GetArgumentsOfInheritedOpenGenericInterface(typeof(IList<>))[0];

                this.listValueChanger = (PropertyListValueEntryChanger)Activator.CreateInstance(typeof(PropertyStrongListValueEntryChanger<,>).MakeGenericType(this.TypeOfValue, elementType), this);
            }
            else if (this.ValueIsWeakList)
            {
                this.listValueChanger = (PropertyListValueEntryChanger)Activator.CreateInstance(typeof(PropertyWeakListValueEntryChanger<>).MakeGenericType(this.TypeOfValue), this);
            }
            else if (this.ValueIsValidDictionary)
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
        /// Updates all values in this value entry from the target tree values.
        /// </summary>
        protected sealed override void UpdateValues()
        {
            for (int i = 0; i < this.ValueCount; i++)
            {
                TParent parent = this.GetParent(i);
                TValue value;

                if (this.IsBoxedValueType)
                {
                    object obj = this.GetActualBoxedValue(parent);

                    value = (TValue)obj;
                }
                else
                {
                    value = this.GetActualValue(parent);
                }

                if (ValueIsMarkedAtomic)
                {
                    AtomHandler.Copy(ref value, ref this.AtomValuesArray[i]);
                    AtomHandler.Copy(ref value, ref this.OriginalAtomValuesArray[i]);
                }

                this.OriginalValuesArray[i] = value;
                this.InternalValuesArray[i] = value;
            }

            this.Values.MarkClean();
        }

        /// <summary>
        /// Determines the value state of this value entry.
        /// </summary>
        protected sealed override PropertyValueState GetValueState()
        {
            TValue[] values = this.InternalValuesArray;

            if (!ValueIsValueType && !ValueIsPrimitive && !ValueIsMarkedAtomic)
            {
                TValue value = values[0];
                string referencePath;

                if (object.ReferenceEquals(value, null) || (this.ValueIsUnityObject && (((UnityEngine.Object)(object)value) == null /*|| ((UnityEngine.Object)(object)value).SafeIsUnityNull()*/)))
                {
                    for (int i = 1; i < values.Length; i++)
                    {
                        if (this.ValueIsUnityObject)
                        {
                            if (((UnityEngine.Object)(object)values[i]) != null /*|| ((UnityEngine.Object)(object)values[i]).SafeIsUnityNull()*/)
                            {
                                return PropertyValueState.ReferenceValueConflict;
                            }
                        }
                        else if (!object.ReferenceEquals(values[i], null))
                        {
                            return PropertyValueState.ReferenceValueConflict;
                        }
                    }

                    return PropertyValueState.NullReference;
                }
                else if (!this.ValueIsUnityObject && this.Property.Tree.ObjectIsReferenced(value, out referencePath) && referencePath != this.Property.Path)
                {
                    // Same property may be updated multiple times in same frame, sometimes - do not allow a property to be a reference to itself; that way lies madness
                    // This is the reason for the path inequality check

                    bool valueWasNull = false;
                    string otherReferencePath;

                    for (int i = 1; i < values.Length; i++)
                    {
                        TValue v = values[i];

                        if (object.ReferenceEquals(v, null))
                        {
                            valueWasNull = true;
                        }
                        else if (!this.Property.Tree.ObjectIsReferenced(v, out otherReferencePath) || otherReferencePath != referencePath)
                        {
                            return PropertyValueState.ReferencePathConflict;
                        }
                    }

                    if (valueWasNull)
                    {
                        return PropertyValueState.ReferenceValueConflict;
                    }
                    else
                    {
                        return PropertyValueState.Reference;
                    }
                }
                else
                {
                    var prop = this.Property;
                    var tree = prop.Tree;

                    Type type = value.GetType();

                    bool isReferenceValueConflict = false;

                    tree.ForceRegisterObjectReference(value, prop);

                    for (int i = 1; i < values.Length; i++)
                    {
                        TValue v = values[i];

                        bool isNull = object.ReferenceEquals(null, v);

                        if (!isNull)
                        {
                            tree.ForceRegisterObjectReference(v, prop);
                        }

                        if (isNull || v.GetType() != type)
                        {
                            // Continue looping; we want all value references force registered
                            isReferenceValueConflict = true;
                        }

                        if (this.ValueIsUnityObject && !object.ReferenceEquals(value, v))
                        {
                            isReferenceValueConflict = true;
                        }
                    }

                    if (isReferenceValueConflict)
                    {
                        return PropertyValueState.ReferenceValueConflict;
                    }
                }

                if (this.ValueIsWeakList)
                {
                    int count = 0;

                    for (int i = 0; i < values.Length; i++)
                    {
                        IList list = (IList)values[i];

                        if (i == 0)
                        {
                            count = list.Count;
                        }
                        else if (count != list.Count)
                        {
                            return PropertyValueState.CollectionLengthConflict;
                        }
                    }
                }
                else if (this.ValueIsStrongList)
                {
                    int count = 0;

                    for (int i = 0; i < values.Length; i++)
                    {
                        TValue list = values[i];

                        if (i == 0)
                        {
                            count = StrongListCountGetter(ref list);
                        }
                        else if (count != StrongListCountGetter(ref list))
                        {
                            return PropertyValueState.CollectionLengthConflict;
                        }
                    }
                }

                return PropertyValueState.None;
            }
            else if (ValueIsMarkedAtomic)
            {
                TValue value = values[0];

                if (!ValueIsValueType && object.ReferenceEquals(value, null))
                {
                    for (int i = 1; i < values.Length; i++)
                    {
                        if (!object.ReferenceEquals(values[i], null))
                        {
                            return PropertyValueState.ReferenceValueConflict;
                        }
                    }

                    return PropertyValueState.NullReference;
                }
                else
                {
                    for (int i = 1; i < values.Length; i++)
                    {
                        if (!AtomHandler.Compare(value, values[i]))
                        {
                            return PropertyValueState.PrimitiveValueConflict;
                        }
                    }
                }

                return PropertyValueState.None;
            }
            else if (ValueIsPrimitive || ValueIsValueType)
            {
                TValue value = values[0];

                for (int i = 1; i < values.Length; i++)
                {
                    if (!EqualityComparer(value, values[i]))
                    {
                        return PropertyValueState.PrimitiveValueConflict;
                    }
                }

                return PropertyValueState.None;
            }
            else
            {
                // Value is a non-primitive value type
                return PropertyValueState.None;
            }
        }

        /// <summary>
        /// Applies the changes made to this value entry to the target objects, and registers prefab modifications as necessary.
        /// </summary>
        /// <returns>
        /// True if any changes were made, otherwise, false.
        /// </returns>
        public sealed override bool ApplyChanges()
        {
            bool changed = false;
            var tree = this.Property.Tree;

            if (this.Values.AreDirty)
            {
                changed = true;

                for (int i = 0; i < this.ValueCount; i++)
                {
                    if (object.ReferenceEquals(this.GetParent(i), null))
                    {
                        Debug.LogError("Parent is null!");
                        continue;
                    }

                    var value = this.InternalValuesArray[i];

                    if (this.IsBoxedValueType)
                    {
                        this.SetActualBoxedValueImplementation(i, value);
                    }
                    else
                    {
                        this.SetActualValueImplementation(i, value);
                    }

                    this.TriggerOnValueChanged(i);

                    if (this.SerializationBackend == SerializationBackend.Odin && tree.HasPrefabs)
                    {
                        if (tree.TargetPrefabs[i] != null)
                        {
                            tree.RegisterPrefabValueModification(this.Property, i);
                        }
                    }
                }
            }

            this.Values.MarkClean();

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
                    this.TriggerOnValueChanged(i);
                }
            }

            if (this.dictionaryHandler != null && this.dictionaryHandler.ApplyChanges())
            {
                changed = true;
                this.Property.Children.Update();

                for (int i = 0; i < this.ValueCount; i++)
                {
                    this.TriggerOnValueChanged(i);
                }
            }

            return changed;
        }

        /// <summary>
        /// Gets the actual boxed value of the tree target.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        protected abstract object GetActualBoxedValue(TParent parent);

        /// <summary>
        /// Gets the actual value of the tree target.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        protected abstract TValue GetActualValue(TParent parent);

        /// <summary>
        /// Gets the parent value at the given index.
        /// </summary>
        protected TParent GetParent(int index)
        {
            if (this.ParentValueProperty != null)
            {
                IPropertyValueEntry<TParent> parentValueEntry = (IPropertyValueEntry<TParent>)this.ParentValueProperty.ValueEntry;
                return parentValueEntry.Values[index];
            }
            else
            {
                return (TParent)this.Property.Tree.WeakTargets[index];
            }
        }
    }
}
#endif