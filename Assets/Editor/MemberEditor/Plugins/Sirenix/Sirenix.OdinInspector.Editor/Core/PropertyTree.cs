#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyTree.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

//#define PREFAB_DEBUG

namespace Sirenix.OdinInspector.Editor
{
    using Serialization;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// <para>Represents a set of values of the same type as a tree of properties that can be drawn in the inspector, and provides an array of utilities for querying the tree of properties.</para>
    /// <para>This class also handles management of prefab modifications.</para>
    /// </summary>
    public abstract class PropertyTree
    {
        public delegate void OnPropertyValueChangedDelegate(InspectorProperty property, int selectionIndex);

        private MethodInfo onValidateMethod;

        internal bool WillUndo;

        /// <summary>
        /// The <see cref="SerializedObject"/> that this tree represents, if the tree was created for a <see cref="SerializedObject"/>.
        /// </summary>
        public abstract SerializedObject UnitySerializedObject { get; }

        /// <summary>
        /// The current update ID of the tree. This is incremented once, each update, and is used by <see cref="InspectorProperty.Update(bool)"/> to avoid updating multiple times in the same update round.
        /// </summary>
        public abstract int UpdateID { get; }

        /// <summary>
        /// The type of the values that the property tree represents.
        /// </summary>
        public abstract Type TargetType { get; }

        /// <summary>
        /// The actual values that the property tree represents.
        /// </summary>
        public abstract ImmutableList<object> WeakTargets { get; }

        /// <summary>
        /// Whether any of the values the property tree represents are prefab instances.
        /// </summary>
        public abstract bool HasPrefabs { get; }

        /// <summary>
        /// The prefabs for each prefab instance represented by the property tree, if any.
        /// </summary>
        public abstract ImmutableList<UnityEngine.Object> TargetPrefabs { get; }

        /// <summary>
        /// The number of root properties in the tree.
        /// </summary>
        public abstract int RootPropertyCount { get; }

        /// <summary>
        /// A prefab tree for the prefabs of this property tree's prefab instances, if any exist.
        /// </summary>
        public abstract PropertyTree PrefabPropertyTree { get; }

        /// <summary>
        /// Whether this property tree also represents members that are specially serialized by Odin.
        /// </summary>
        public abstract bool IncludesSpeciallySerializedMembers { get; }

        /// <summary>
        /// Gets a value indicating whether or not to draw the mono script object field at the top of the property tree.
        /// </summary>
        public bool DrawMonoScriptObjectField { get; set; }

        /// <summary>
        /// An event that is invoked whenever an undo or a redo is performed in the inspector.
        /// The advantage of using this event on a property tree instance instead of
        /// <see cref="Undo.undoRedoPerformed"/> is that this event will be desubscribed from
        /// <see cref="Undo.undoRedoPerformed"/> when the selection changes and the property
        /// tree is no longer being used, allowing the GC to collect the property tree.
        /// </summary>
        public event Action OnUndoRedoPerformed;

        /// <summary>
        /// This event is invoked whenever the value of any property in the entire property tree is changed through the property system.
        /// </summary>
        public event OnPropertyValueChangedDelegate OnPropertyValueChanged;

        /// <summary>
        /// Creates a new <see cref="PropertyTree" /> for all target values of a <see cref="SerializedObject" />.
        /// </summary>
        public PropertyTree()
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(this.TargetType))
            {
                this.onValidateMethod = this.TargetType.FindMember()
                                                       .IsNamed("OnValidate")
                                                       .IsMethod()
                                                       .IsInstance()
                                                       .HasNoParameters()
                                                       .GetMember<MethodInfo>();

                Undo.undoRedoPerformed += this.InvokeOnUndoRedoPerformed;
                Selection.selectionChanged += this.OnSelectionChanged;
            }
        }

        internal void InvokeOnPropertyValueChanged(InspectorProperty property, int selectionIndex)
        {
            if (this.OnPropertyValueChanged != null)
            {
                try
                {
                    this.OnPropertyValueChanged(property, selectionIndex);
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
        }

        /// <summary>
        /// Schedules a delegate to be invoked at the end of the current GUI frame.
        /// </summary>
        /// <param name="action">The action delegate to be delayed.</param>
        public abstract void DelayAction(Action action);

        /// <summary>
        /// Schedules a delegate to be invoked at the end of the next Repaint GUI frame.
        /// </summary>
        /// <param name="action">The action to be delayed.</param>
        public abstract void DelayActionUntilRepaint(Action action);

        /// <summary>
        /// Enumerates over the properties of the tree.
        /// </summary>
        /// <param name="includeChildren">Whether to include children of the root properties or not. If set to true, every property in the entire tree will be enumerated.</param>
        /// <returns></returns>
        public abstract IEnumerable<InspectorProperty> EnumerateTree(bool includeChildren);

        /// <summary>
        /// Gets the property at the given path. Note that this is the path found in <see cref="InspectorProperty.Path" />, not the Unity path. This is a dictionary look-up.
        /// </summary>
        /// <param name="path">The path of the property to get.</param>
        public abstract InspectorProperty GetPropertyAtPath(string path);

        /// <summary>
        /// Gets the property at the given Unity path. This is a dictionary look-up.
        /// </summary>
        /// <param name="path">The Unity path of the property to get.</param>
        public abstract InspectorProperty GetPropertyAtUnityPath(string path);

        /// <summary>
        /// Gets the property at the given deep reflection path. This is a dictionary look-up.
        /// </summary>
        /// <param name="path">The deep reflection path of the property to get.</param>
        public abstract InspectorProperty GetPropertyAtDeepReflectionPath(string path);

        /// <summary>
        /// <para>Draw the property tree, and handles management of undo, as well as marking scenes and drawn assets dirty.</para>
        /// <para>
        /// This is a shorthand for calling
        /// <see cref="InspectorUtilities.BeginDrawPropertyTree(PropertyTree, bool)"/>,
        /// <see cref="InspectorUtilities.DrawPropertiesInTree(PropertyTree)"/> and .
        /// <see cref="InspectorUtilities.EndDrawPropertyTree(PropertyTree)"/>.
        /// </para>
        /// </summary>
        public void Draw(bool applyUndo = true)
        {
            InspectorUtilities.BeginDrawPropertyTree(this, applyUndo);
            InspectorUtilities.DrawPropertiesInTree(this);
            InspectorUtilities.EndDrawPropertyTree(this);
        }

        /// <summary>
        /// Gets a Unity property for the given Odin or Unity path. If there is no <see cref="SerializedObject" /> for this property tree, or no such property is found in the <see cref="SerializedObject" />, a property will be emitted using <see cref="UnityPropertyEmitter" />.
        /// </summary>
        /// <param name="path">The Odin or Unity path to the property to get.</param>
        public SerializedProperty GetUnityPropertyForPath(string path)
        {
            FieldInfo fieldInfo;
            return this.GetUnityPropertyForPath(path, out fieldInfo);
        }

        /// <summary>
        /// Gets a Unity property for the given Odin or Unity path. If there is no <see cref="SerializedObject" /> for this property tree, or no such property is found in the <see cref="SerializedObject" />, a property will be emitted using <see cref="UnityPropertyEmitter" />.
        /// </summary>
        /// <param name="path">The Odin or Unity path to the property to get.</param>
        /// <param name="backingField">The backing field of the Unity property.</param>
        public abstract SerializedProperty GetUnityPropertyForPath(string path, out FieldInfo backingField);

        /// <summary>
        /// Checks whether a given object instance is referenced anywhere in the tree, and if it is, gives the path of the first time the object reference was encountered as an out parameter.
        /// </summary>
        /// <param name="value">The reference value to check.</param>
        /// <param name="referencePath">The first found path of the object.</param>
        public abstract bool ObjectIsReferenced(object value, out string referencePath);

        /// <summary>
        /// Gets the number of references to a given object instance in this tree.
        /// </summary>
        public abstract int GetReferenceCount(object reference);

        /// <summary>
        /// Updates all properties in the entire tree, and validates the prefab state of the tree, if applicable.
        /// </summary>
        public abstract void UpdateTree();

        /// <summary>
        /// Replaces all occurrences of a value with another value, in the entire tree.
        /// </summary>
        /// <param name="from">The value to find all instances of.</param>
        /// <param name="to">The value to replace the found values with.</param>
        public abstract void ReplaceAllReferences(object from, object to);

        /// <summary>
        /// Gets the root tree property at a given index.
        /// </summary>
        /// <param name="index">The index of the property to get.</param>
        public abstract InspectorProperty GetRootProperty(int index);

        /// <summary>
        /// Invokes the actions that have been delayed using <see cref="DelayAction(Action)"/> and <see cref="DelayActionUntilRepaint(Action)"/>.
        /// </summary>
        public abstract void InvokeDelayedActions();

        /// <summary>
        /// Gets the prefab modification type of a given property, if any.
        /// </summary>
        /// <param name="property">The property to check.</param>
        /// <returns>The prefab modification type of the property if it has one, otherwise null.</returns>
        public abstract PrefabModificationType? GetPrefabModificationType(InspectorProperty property);

        /// <summary>
        /// Registers a modification of type <see cref="PrefabModificationType.ListLength"/> for a given property.
        /// </summary>
        /// <param name="property">The property to register a modification for.</param>
        /// <param name="targetIndex">Selection index of the target to register a modification for.</param>
        /// <param name="newLength">The modified list length.</param>
        public abstract void RegisterPrefabListLengthModification(InspectorProperty property, int targetIndex, int newLength);

        /// <summary>
        /// Registers a modification of type <see cref="PrefabModificationType.Value"/> for a given property.
        /// </summary>
        /// <param name="property">The property to register a modification for.</param>
        /// <param name="targetIndex">Selection index of the target to register a modification for.</param>
        /// <param name="forceImmediate">Whether to force the change to be registered immediately, rather than at the end of frame.</param>
        public abstract void RegisterPrefabValueModification(InspectorProperty property, int targetIndex, bool forceImmediate = false);

        /// <summary>
        /// Calculates a delta between the current dictionary property and its prefab counterpart, and registers that delta as a <see cref="PrefabModificationType.Dictionary"/> modification.
        /// </summary>
        /// <param name="property">The property to register a modification for.</param>
        /// <param name="targetIndex">Selection index of the target.</param>
        public abstract void RegisterPrefabDictionaryDeltaModification(InspectorProperty property, int targetIndex);

        /// <summary>
        /// Adds a remove key modification to the dictionary modifications of a given property.
        /// </summary>
        /// <param name="property">The property to register a modification for.</param>
        /// <param name="targetIndex">Selection index of the target.</param>
        /// <param name="key">The key to be removed.</param>
        public abstract void RegisterPrefabDictionaryRemoveKeyModification(InspectorProperty property, int targetIndex, object key);

        /// <summary>
        /// Adds an add key modification to the dictionary modifications of a given property.
        /// </summary>
        /// <param name="property">The property to register a modification for.</param>
        /// <param name="targetIndex">Selection index of the target.</param>
        /// <param name="key">The key to be added.</param>
        public abstract void RegisterPrefabDictionaryAddKeyModification(InspectorProperty property, int targetIndex, object key);

        /// <summary>
        /// Removes all dictionary modifications on a property for a given dictionary key value.
        /// </summary>
        /// <param name="property">The property to remove a key modification for.</param>
        /// <param name="targetIndex">Selection index of the target.</param>
        /// <param name="key">The key to remove modifications for.</param>
        public abstract void RemovePrefabDictionaryModification(InspectorProperty property, int targetIndex, object key);

        /// <summary>
        /// Removes all prefab modifications of a given type on a given property.
        /// </summary>
        /// <param name="property">The property to remove modifications for.</param>
        /// <param name="targetIndex">Selection index of the target.</param>
        /// <param name="modificationType">Type of the modification to remove.</param>
        public abstract void RemovePrefabModification(InspectorProperty property, int targetIndex, PrefabModificationType modificationType);

        /// <summary>
        /// Gets all prefab modifications in this property tree for a given selection index.
        /// </summary>
        public abstract List<PrefabModification> GetPrefabModifications(int targetIndex);

        /// <summary>
        /// Applies all changes made with properties to the inspected target tree values.
        /// </summary>
        /// <returns>true if any values were changed, otherwise false</returns>
        public bool ApplyChanges()
        {
            bool changed = false;

            foreach (var property in this.EnumerateTree(true))
            {
                if (property.ValueEntry != null)
                {
                    if (property.ValueEntry.ApplyChanges())
                    {
                        changed = true;
                    }
                }
            }

            if (changed && this.HasPrefabs && this.UnitySerializedObject != null)
            {
                this.DelayActionUntilRepaint(() =>
                {
                    // We make ABSOLUTELY SURE that this code runs at the *very end* of Repaint, after *all* other delayed Repaint invokes.

                    this.DelayActionUntilRepaint(() =>
                    {
                        for (int i = 0; i < this.WeakTargets.Count; i++)
                        {
                            // Before we ever call PrefabUtility.RecordPrefabInstancePropertyModifications, we MUST
                            // make sure that prefab modifications are registered and applied on the object.
                            //
                            // If we don't, there is a chance that Unity will crash, for unknown reasons.

                            var receiver = this.WeakTargets[i] as ISerializationCallbackReceiver;

                            if (receiver != null)
                            {
                                receiver.OnBeforeSerialize();
                            }

                            PrefabUtility.RecordPrefabInstancePropertyModifications((UnityEngine.Object)this.WeakTargets[i]);
                        }
                    });
                });
            }

            return changed;
        }

        /// <summary>
        /// Invokes the OnValidate method on the property tree's targets if they are derived from <see cref="UnityEngine.Object"/> and have the method defined.
        /// </summary>
        public void InvokeOnValidate()
        {
            if (this.onValidateMethod != null)
            {
                for (int i = 0; i < this.WeakTargets.Count; i++)
                {
                    try
                    {
                        this.onValidateMethod.Invoke(this.WeakTargets[i], null);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Registers an object reference to a given path; this is used to ensure that objects are always registered after having been encountered once.
        /// </summary>
        /// <param name="reference">The referenced object.</param>
        /// <param name="property">The property that contains the reference.</param>
        internal abstract void ForceRegisterObjectReference(object reference, InspectorProperty property);

        /// <summary>
        /// Creates a new <see cref="PropertyTree" /> for a given target value.
        /// </summary>
        /// <param name="target">The target to create a tree for.</param>
        /// <exception cref="System.ArgumentNullException">target is null</exception>
        public static PropertyTree Create(object target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            return Create((IList)new object[] { target });
        }

        /// <summary>
        /// <para>Creates a new <see cref="PropertyTree" /> for a set of given target values.</para>
        /// <para>Note that the targets all need to be of the same type.</para>
        /// </summary>
        /// <param name="targets">The targets to create a tree for.</param>
        /// <exception cref="System.ArgumentNullException">targets is null</exception>
        public static PropertyTree Create(params object[] targets)
        {
            if (targets == null)
            {
                throw new ArgumentNullException("targets");
            }

            return Create((IList)targets);
        }

        /// <summary>
        /// Creates a new <see cref="PropertyTree" /> for all target values of a <see cref="SerializedObject" />.
        /// </summary>
        /// <param name="serializedObject">The serialized object to create a tree for.</param>
        /// <exception cref="System.ArgumentNullException">serializedObject is null</exception>
        public static PropertyTree Create(SerializedObject serializedObject)
        {
            if (serializedObject == null)
            {
                throw new ArgumentNullException("serializedObject");
            }

            return Create(serializedObject.targetObjects, serializedObject);
        }

        /// <summary>
        /// <para>Creates a new <see cref="PropertyTree"/> for a set of given target values.</para>
        /// <para>Note that the targets all need to be of the same type.</para>
        /// </summary>
        /// <param name="targets">The targets to create a tree for.</param>
        public static PropertyTree Create(IList targets)
        {
            return Create(targets, null);
        }

        /// <summary>
        /// <para>Creates a new <see cref="PropertyTree"/> for a set of given target values, represented by a given <see cref="SerializedObject"/>.</para>
        /// <para>Note that the targets all need to be of the same type.</para>
        /// </summary>
        /// <param name="targets">The targets to create a tree for.</param>
        /// <param name="serializedObject">The serialized object to create a tree for. Note that the target values of the given <see cref="SerializedObject"/> must be the same values given in the targets parameter.</param>
        public static PropertyTree Create(IList targets, SerializedObject serializedObject)
        {
            if (targets == null)
            {
                throw new ArgumentNullException("targets");
            }

            if (targets.Count == 0)
            {
                throw new ArgumentException("There must be at least one target.");
            }

            if (serializedObject != null)
            {
                bool valid = true;
                var targetObjects = serializedObject.targetObjects;

                if (targets.Count != targetObjects.Length)
                {
                    valid = false;
                }
                else
                {
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (!object.ReferenceEquals(targets[i], targetObjects[i]))
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                if (!valid)
                {
                    throw new ArgumentException("Given target array must be identical in length and content to the target objects array in the given serializedObject.");
                }
            }

            Type targetType = null;

            for (int i = 0; i < targets.Count; i++)
            {
                Type otherType;
                object target = targets[i];

                if (object.ReferenceEquals(target, null))
                {
                    throw new ArgumentException("Target at index " + i + " was null.");
                }

                if (i == 0)
                {
                    targetType = target.GetType();
                }
                else if (targetType != (otherType = target.GetType()))
                {
                    if (targetType.IsAssignableFrom(otherType))
                    {
                        continue;
                    }
                    else if (otherType.IsAssignableFrom(targetType))
                    {
                        targetType = otherType;
                        continue;
                    }

                    throw new ArgumentException("Expected targets of type " + targetType.Name + ", but got an incompatible target of type " + otherType.Name + " at index " + i + ".");
                }
            }

            Type treeType = typeof(PropertyTree<>).MakeGenericType(targetType);
            Array targetArray;

            if (targets.GetType().IsArray && targets.GetType().GetElementType() == targetType)
            {
                targetArray = (Array)targets;
            }
            else
            {
                targetArray = Array.CreateInstance(targetType, targets.Count);
                targets.CopyTo(targetArray, 0);
            }

            if (serializedObject == null && targetType.IsAssignableFrom(typeof(UnityEngine.Object)))
            {
                UnityEngine.Object[] objs = new UnityEngine.Object[targets.Count];
                targets.CopyTo(objs, 0);

                serializedObject = new SerializedObject(objs);
            }

            return (PropertyTree)Activator.CreateInstance(treeType, targetArray, serializedObject);
        }

        private void InvokeOnUndoRedoPerformed()
        {
            if (this.OnUndoRedoPerformed != null)
            {
                this.OnUndoRedoPerformed();
            }
        }

        private void OnSelectionChanged()
        {
            Undo.undoRedoPerformed -= this.InvokeOnUndoRedoPerformed;
            Selection.selectionChanged -= this.OnSelectionChanged;
        }
    }

    /// <summary>
    /// <para>Represents a set of strongly typed values as a tree of properties that can be drawn in the inspector, and provides an array of utilities for querying the tree of properties.</para>
    /// <para>This class also handles management of prefab modifications.</para>
    /// </summary>
    public sealed class PropertyTree<T> : PropertyTree
    {
        private Dictionary<object, int> objectReferenceCounts = new Dictionary<object, int>(ReferenceEqualityComparer<object>.Default);
        private Dictionary<object, string> objectReferences = new Dictionary<object, string>(ReferenceEqualityComparer<object>.Default);

        private Dictionary<string, InspectorProperty> properties = new Dictionary<string, InspectorProperty>();
        private Dictionary<string, InspectorProperty> propertiesUnityPath = new Dictionary<string, InspectorProperty>();
        private Dictionary<string, InspectorProperty> propertiesDeepReflectionPath = new Dictionary<string, InspectorProperty>();

        private Dictionary<string, Dictionary<Type, SerializedProperty>> emittedUnityPropertyCache = new Dictionary<string, Dictionary<Type, SerializedProperty>>();
        private List<Action> delayedActions = new List<Action>();
        private List<Action> delayedRepaintActions = new List<Action>();

        private static readonly bool TargetIsValueType = typeof(T).IsValueType;
        private static readonly bool TargetIsUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(typeof(T));
        private static readonly bool TargetSupportsPrefabSerialization = typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)) && typeof(ISupportsPrefabSerialization).IsAssignableFrom(typeof(T));

        private T[] targets;
        private InspectorPropertyInfo[] rootPropertyInfos;
        private InspectorProperty[] rootProperties;
        private int updateID;
        private SerializedObject serializedObject;
        private object[] weakTargets;
        private ImmutableList<T> immutableTargets;
        private ImmutableList<object> immutableWeakTargets;
        private ImmutableList<UnityEngine.Object> immutableTargetPrefabs;
        private bool hasPrefabs;
        private bool allTargetsHaveSamePrefab;
        private Dictionary<string, PrefabModification>[] prefabValueModifications;
        private Dictionary<string, PrefabModification>[] prefabListLengthModifications;
        private Dictionary<string, PrefabModification>[] prefabDictionaryModifications;
        private PropertyTree<T> prefabPropertyTree;
        private int[] prefabPropertyTreeIndexMap;
        private bool includesSpeciallySerializedMembers;

        private bool allowAutoRegisterPrefabModifications = true;

        /// <summary>
        /// The current update ID of the tree. This is incremented once, each update, and is used by <see cref="InspectorProperty.Update(bool)" /> to avoid updating multiple times in the same update round.
        /// </summary>
        public override int UpdateID { get { return this.updateID; } }

        /// <summary>
        /// The <see cref="SerializedObject" /> that this tree represents, if the tree was created for a <see cref="SerializedObject" />.
        /// </summary>
        public override SerializedObject UnitySerializedObject { get { return this.serializedObject; } }

        /// <summary>
        /// The type of the values that the property tree represents.
        /// </summary>
        public override Type TargetType { get { return typeof(T); } }

        /// <summary>
        /// The prefabs for each prefab instance represented by the property tree, if any.
        /// </summary>
        public override ImmutableList<UnityEngine.Object> TargetPrefabs { get { return this.immutableTargetPrefabs; } }

        /// <summary>
        /// Whether any of the values the property tree represents are prefab instances.
        /// </summary>
        public override bool HasPrefabs { get { return this.hasPrefabs; } }

        /// <summary>
        /// The strongly types actual values that the property tree represents.
        /// </summary>
        public ImmutableList<T> Targets
        {
            get
            {
                if (this.immutableTargets == null)
                {
                    this.immutableTargets = new ImmutableList<T>(this.targets);
                }

                return this.immutableTargets;
            }
        }

        /// <summary>
        /// The weakly types actual values that the property tree represents.
        /// </summary>
        public override ImmutableList<object> WeakTargets
        {
            get
            {
                if (this.immutableWeakTargets == null)
                {
                    if (this.weakTargets == null)
                    {
                        this.weakTargets = new object[this.targets.Length];
                        this.targets.CopyTo(this.weakTargets, 0);
                    }

                    this.immutableWeakTargets = new ImmutableList<object>(this.weakTargets);
                }
                else if (TargetIsValueType)
                {
                    this.targets.CopyTo(this.weakTargets, 0);
                }

                return this.immutableWeakTargets;
            }
        }

        /// <summary>
        /// The number of root properties in the tree.
        /// </summary>
        public override int RootPropertyCount { get { return this.rootProperties.Length; } }

        /// <summary>
        /// A prefab tree for the prefabs of this property tree's prefab instances, if any exist.
        /// </summary>
        public override PropertyTree PrefabPropertyTree { get { return this.prefabPropertyTree; } }

        /// <summary>
        /// Whether this property tree also represents members that are specially serialized by Odin.
        /// </summary>
        public override bool IncludesSpeciallySerializedMembers { get { return this.includesSpeciallySerializedMembers; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTree{T}"/> class.
        /// </summary>
        /// <param name="serializedObject">The serialized object to represent.</param>
        public PropertyTree(SerializedObject serializedObject)
            : this(serializedObject.targetObjects.Cast<T>().ToArray(), serializedObject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTree{T}"/> class.
        /// </summary>
        /// <param name="targets">The targets to represent.</param>
        public PropertyTree(T[] targets)
            : this(targets, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTree{T}"/> class.
        /// </summary>
        /// <param name="targets">The targets to represent.</param>
        /// <param name="serializedObject">The serialized object to represent. Note that the target values of the given <see cref="SerializedObject"/> must be the same values given in the targets parameter.</param>
        /// <exception cref="System.ArgumentNullException">targets is null</exception>
        /// <exception cref="System.ArgumentException">
        /// There must be at least one target.
        /// or
        /// A given target is a null value.
        /// </exception>
        public PropertyTree(T[] targets, SerializedObject serializedObject)
        {
            if (targets == null)
            {
                throw new ArgumentNullException("targets");
            }

            if (targets.Length == 0)
            {
                throw new ArgumentException("There must be at least one target.");
            }

            for (int i = 0; i < targets.Length; i++)
            {
                if (object.ReferenceEquals(targets[i], null))
                {
                    throw new ArgumentException("A target at index '" + i + "' is a null value.");
                }
            }

            this.includesSpeciallySerializedMembers = typeof(T).IsDefined<ShowOdinSerializedPropertiesInInspectorAttribute>(inherit: true);
            this.serializedObject = serializedObject;
            this.targets = targets;
            this.rootPropertyInfos = InspectorPropertyInfo.Get(typeof(T), this.includesSpeciallySerializedMembers);
            this.rootProperties = new InspectorProperty[this.rootPropertyInfos.Length];
            this.prefabValueModifications = new Dictionary<string, PrefabModification>[this.WeakTargets.Count];
            this.prefabListLengthModifications = new Dictionary<string, PrefabModification>[this.WeakTargets.Count];
            this.prefabDictionaryModifications = new Dictionary<string, PrefabModification>[this.WeakTargets.Count];
            this.prefabPropertyTreeIndexMap = new int[this.WeakTargets.Count];

            this.UpdatePrefabsAndModifications();
        }

        private void UpdatePrefabsAndModifications()
        {
            this.hasPrefabs = false;
            T[] prefabs = new T[this.WeakTargets.Count];

            if (typeof(UnityEngine.Object).IsAssignableFrom(this.TargetType))
            {
                int prefabCount = 0;
                for (int i = 0; i < this.WeakTargets.Count; i++)
                {
                    var target = (UnityEngine.Object)this.WeakTargets[i];
                    var prefabType = PrefabUtility.GetPrefabType(target);
                    T prefab;

                    if (prefabType == PrefabType.PrefabInstance && (prefab = (T)(object)PrefabUtility.GetPrefabParent(target)) != null)
                    {
                        prefabs[i] = prefab;
                        this.hasPrefabs = true;

                        this.prefabPropertyTreeIndexMap[i] = prefabCount;
                        prefabCount++;

                        if (TargetSupportsPrefabSerialization)
                        {
                            ISupportsPrefabSerialization cast = (ISupportsPrefabSerialization)this.WeakTargets[i];
                            var modificationList = UnitySerializationUtility.DeserializePrefabModifications(cast.SerializationData.PrefabModifications, cast.SerializationData.PrefabModificationsReferencedUnityObjects);

                            var listLengthModifications = this.prefabListLengthModifications[i] ?? new Dictionary<string, PrefabModification>();
                            var valueModifications = this.prefabValueModifications[i] ?? new Dictionary<string, PrefabModification>();
                            var dictionaryModifications = this.prefabDictionaryModifications[i] ?? new Dictionary<string, PrefabModification>();

                            listLengthModifications.Clear();
                            valueModifications.Clear();
                            dictionaryModifications.Clear();

                            //
                            // We have to be careful about Unity's crappy prefab system screwing us with
                            // duplicate modifications.
                            //
                            // As a rule, modifications that come earliest are the ones we want to keep.
                            //

                            for (int j = 0; j < modificationList.Count; j++)
                            {
                                var mod = modificationList[j];

                                switch (mod.ModificationType)
                                {
                                    case PrefabModificationType.Value:
                                        if (!valueModifications.ContainsKey(mod.Path))
                                        {
                                            valueModifications[mod.Path] = mod;
                                        }
                                        break;

                                    case PrefabModificationType.ListLength:
                                        if (!listLengthModifications.ContainsKey(mod.Path))
                                        {
                                            listLengthModifications[mod.Path] = mod;
                                        }
                                        break;

                                    case PrefabModificationType.Dictionary:
                                        if (!dictionaryModifications.ContainsKey(mod.Path))
                                        {
                                            dictionaryModifications[mod.Path] = mod;
                                        }
                                        break;

                                    default:
                                        throw new NotImplementedException(mod.ModificationType.ToString());
                                }
                            }

                            //
                            // There might be modifications already registered this frame, that haven't been serialized yet
                            // If so, we must not lose them. They *always* override pre-existing modifications.
                            //

                            var registeredModifications = UnitySerializationUtility.GetRegisteredPrefabModifications(target);

                            if (registeredModifications != null)
                            {
                                for (int j = 0; j < registeredModifications.Count; j++)
                                {
                                    var mod = registeredModifications[j];

                                    if (mod.ModificationType == PrefabModificationType.Value)
                                    {
                                        valueModifications[mod.Path] = mod;
                                    }
                                    else if (mod.ModificationType == PrefabModificationType.ListLength)
                                    {
                                        listLengthModifications[mod.Path] = mod;
                                    }
                                    else if (mod.ModificationType == PrefabModificationType.Dictionary)
                                    {
                                        dictionaryModifications[mod.Path] = mod;
                                    }
                                }
                            }

                            this.prefabListLengthModifications[i] = listLengthModifications;
                            this.prefabValueModifications[i] = valueModifications;
                            this.prefabDictionaryModifications[i] = dictionaryModifications;
                        }
                    }
                    else
                    {
                        this.prefabPropertyTreeIndexMap[i] = -1;
                    }
                }

                if (prefabCount > 0)
                {
                    var prefabsNoNull = new T[prefabCount];

                    for (int i = 0; i < prefabs.Length; i++)
                    {
                        int index = this.prefabPropertyTreeIndexMap[i];

                        if (index >= 0)
                        {
                            prefabsNoNull[index] = prefabs[i];
                        }
                    }

                    if (this.prefabPropertyTree != null)
                    {
                        if (this.prefabPropertyTree.Targets.Count != prefabsNoNull.Length)
                        {
                            this.prefabPropertyTree = null;
                        }
                        else
                        {
                            for (int i = 0; i < this.prefabPropertyTree.Targets.Count; i++)
                            {
                                if (!object.ReferenceEquals(this.prefabPropertyTree.Targets[i], prefabsNoNull[i]))
                                {
                                    this.prefabPropertyTree = null;
                                    break;
                                }
                            }
                        }
                    }

                    if (this.prefabPropertyTree == null)
                    {
                        this.prefabPropertyTree = (PropertyTree<T>)PropertyTree.Create(prefabsNoNull);
                    }

                    this.prefabPropertyTree.UpdateTree();
                }

                this.allTargetsHaveSamePrefab = false;

                if (prefabCount == this.targets.Length)
                {
                    this.allTargetsHaveSamePrefab = true;

                    var firstPrefab = prefabs[0];

                    for (int i = 1; i < prefabs.Length; i++)
                    {
                        if (!object.ReferenceEquals(firstPrefab, prefabs[i]))
                        {
                            this.allTargetsHaveSamePrefab = false;
                            break;
                        }
                    }
                }
            }

            this.immutableTargetPrefabs = new ImmutableList<UnityEngine.Object>(prefabs.Cast<UnityEngine.Object>().ToArray());
        }

        /// <summary>
        /// Gets the prefab modification type of a given property, if any. Note that this only works on Odin-serialized properties.
        /// </summary>
        /// <param name="property">The property to check.</param>
        /// <returns>
        /// The prefab modification type of the property if it has one, otherwise null.
        /// </returns>
        public override PrefabModificationType? GetPrefabModificationType(InspectorProperty property)
        {
            if (property.ValueEntry != null && property.ValueEntry.SerializationBackend != SerializationBackend.Odin)
            {
                return null;
            }

            bool registerModification;
            var result = this.PrivateGetPrefabModificationType(property, out registerModification);

            //
            // If there is a change, and this change is not verified with the registered
            // modifications or has changed from the registered modifications, and all
            // targets have the same prefab, then we have to make sure this change is
            // registered.
            //
            // This case happens when, for example, somebody changes a prefab value outside
            // of the inspector.
            //

            if (result != null && this.allowAutoRegisterPrefabModifications && registerModification && this.allTargetsHaveSamePrefab/* && Event.current.type == EventType.Repaint*/)
            {
                switch (result.Value)
                {
                    case PrefabModificationType.Value:

                        for (int i = 0; i < this.targets.Length; i++)
                        {
#if PREFAB_DEBUG
                            Debug.Log(Event.current.type + " (id " + this.updateID + "): Registering non-inspector-triggered prefab value change for property " + property.Path);
                            result = this.PrivateGetPrefabModificationType(property, out registerModification);
#endif

                            this.RegisterPrefabValueModification(property, i);
                        }

                        break;

                    case PrefabModificationType.ListLength:

                        for (int i = 0; i < this.targets.Length; i++)
                        {
                            //Debug.Log(this.updateID + " (id): Registering non-inspector-triggered prefab list length change for property " + property.Path);
                            //result = this.PrivateGetPrefabModificationType(property, out registerModification);

                            property.Children.Update();
                            this.RegisterPrefabListLengthModification(property, i, property.Children.Count);
                        }

                        break;

                    case PrefabModificationType.Dictionary:

                        for (int i = 0; i < this.targets.Length; i++)
                        {
                            //Debug.Log(this.updateID + " (id): Registering non-inspector-triggered prefab dictionary change for property " + property.Path);
                            //result = this.PrivateGetPrefabModificationType(property, out registerModification);

                            property.Children.Update();
                            this.RegisterPrefabDictionaryDeltaModification(property, i);
                        }

                        break;

                    default:
                        break;
                }
            }

            return result;
        }

        private static bool PropertyCanHaveModifications(InspectorProperty property)
        {
            if (!property.SupportsPrefabModifications) return false;

            // Special rule: dictionary key value pairs never *directly* have modifications,
            // nor do their keys. Their parent dictionary can have key modifications, and their values
            // can have modifications, but pairs and their keys *can't* have them.
            if (property.ValueEntry != null && (property.ValueEntry.ValueCategory == PropertyValueCategory.DictionaryElement || (property.Index == 0 && property.Parent != null && property.Parent.ValueEntry != null && property.Parent.ValueEntry.ValueCategory == PropertyValueCategory.DictionaryElement)))
            {
                return false;
            }

            return true;
        }

        private PrefabModificationType? PrivateGetPrefabModificationType(InspectorProperty property, out bool registerModification)
        {
            if (Application.isPlaying || !this.HasPrefabs || !this.allTargetsHaveSamePrefab)
            {
                // We do not display prefab modifications if we are playing, or
                // there are no prefabs, or if not all targets have the same prefab
                // or if the property cannot have modifications.

                registerModification = false;
                return null;
            }

            if (!PropertyCanHaveModifications(property))
            {
                registerModification = false;

                if (property.Index == 0 && property.Parent != null && property.Parent.ValueEntry != null && property.Parent.ValueEntry.ValueCategory == PropertyValueCategory.DictionaryElement)
                {
                    var dictionaryProp = property.Parent.Parent;

                    for (int i = 0; i < this.prefabDictionaryModifications.Length; i++)
                    {
                        var key = dictionaryProp.ValueEntry.GetDictionaryHandler().GetKey(i, property.Parent.Index);
                        var mods = this.prefabDictionaryModifications[i];

                        PrefabModification mod;

                        if (mods != null && mods.TryGetValue(dictionaryProp.DeepReflectionPath, out mod))
                        {
                            if (mod.DictionaryKeysAdded != null && mod.DictionaryKeysAdded.Contains(key))
                            {
                                return PrefabModificationType.Value;
                            }
                        }
                    }
                }

                return null;
            }

            registerModification = true;

            for (int i = 0; i < this.prefabValueModifications.Length; i++)
            {
                var mods = this.prefabValueModifications[i];

                if (mods != null)
                {
                    var prop = property;

                    do
                    {
                        if (mods.ContainsKey(prop.DeepReflectionPath))
                        {
                            var entry = prop.ValueEntry;

                            if (entry != null)
                            {
                                var mod = mods[prop.DeepReflectionPath];
                                registerModification = entry.ValueIsPrefabDifferent(mod.ModifiedValue, i);
                            }
                            else
                            {
                                registerModification = false;
                            }

                            return PrefabModificationType.Value;
                        }

                        prop = prop.ParentValueProperty;
                    } while (prop != null && prop.ValueEntry.TypeOfValue.IsValueType);
                }
            }

            for (int i = 0; i < this.prefabListLengthModifications.Length; i++)
            {
                var mods = this.prefabListLengthModifications[i];

                if (mods != null && mods.ContainsKey(property.DeepReflectionPath))
                {
                    registerModification = false;
                    return PrefabModificationType.ListLength;
                }
            }

            for (int i = 0; i < this.prefabDictionaryModifications.Length; i++)
            {
                var mods = this.prefabDictionaryModifications[i];

                if (mods != null && mods.ContainsKey(property.DeepReflectionPath))
                {
                    registerModification = false;
                    return PrefabModificationType.Dictionary;
                }
            }

            if (this.prefabPropertyTree == null || property.ValueEntry == null)
            {
                registerModification = false;
                return null;
            }

            var prefabProperty = this.prefabPropertyTree.GetPropertyAtPath(property.Path);

            if (prefabProperty == null || prefabProperty.ValueEntry == null)
            {
                // This property doesn't even exist as a value in the prefab
                // This happens, for example, for collection elements that exist
                // because a collection length has been increased in the instance
                return PrefabModificationType.Value;
            }

            if (!prefabProperty.ValueEntry.TypeOfValue.IsValueType && prefabProperty.ValueEntry.TypeOfValue != property.ValueEntry.TypeOfValue)
            {
                // Different types means there's a modification
                return PrefabModificationType.Value;
            }

            //if (prefabProperty.ValueEntry.ValueState != property.ValueEntry.ValueState && prefabProperty.ValueEntry.ValueState != PropertyValueState.Reference && property.ValueEntry.ValueState != PropertyValueState.Reference)
            //{
            //    // Different (non-reference) value states means there some sort of difference
            //    return PrefabModificationType.Value;
            //}

            if (prefabProperty.ValueEntry.IsMarkedAtomic)
            {
                // Compare atomic type values directly

                for (int i = 0; i < prefabProperty.ValueEntry.ValueCount; i++)
                {
                    if (property.ValueEntry.ValueIsPrefabDifferent(prefabProperty.ValueEntry.WeakValues[i], i))
                    {
                        return PrefabModificationType.Value;
                    }
                }
            }

            if (prefabProperty.ValueEntry.TypeOfValue.IsValueType && !prefabProperty.ValueEntry.ValueTypeValuesAreEqual(property.ValueEntry))
            {
                // Compare value type values directly
                return PrefabModificationType.Value;
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(property.ValueEntry.TypeOfValue) && !object.ReferenceEquals(property.ValueEntry.WeakSmartValue, prefabProperty.ValueEntry.WeakSmartValue))
            {
                UnityEngine.Object instanceValue = (UnityEngine.Object)property.ValueEntry.WeakSmartValue;
                UnityEngine.Object prefabValue = (UnityEngine.Object)prefabProperty.ValueEntry.WeakSmartValue;

                if (instanceValue == null || prefabValue == null)
                {
                    // One is null while the other is not, since the references didn't match
                    return PrefabModificationType.Value;
                }

                var instanceParentAsset = PrefabUtility.GetPrefabParent(instanceValue);

                if (instanceParentAsset != prefabValue)
                {
                    // This value is not the same "conceptual" local value as is there on the prefab
                    return PrefabModificationType.Value;
                }
            }

            if (prefabProperty.Children != null && property.Children != null && prefabProperty.Children.IsCollection && prefabProperty.Children.Count != property.Children.Count)
            {
                // Different amount of children in a collection means a dictionary or list length change

                if (prefabProperty.ValueEntry.ValueIsValidDictionary)
                {
                    return PrefabModificationType.Dictionary;
                }
                else
                {
                    return PrefabModificationType.ListLength;
                }
            }

            registerModification = false;
            return null;
        }

        /// <summary>
        /// Registers a modification of type <see cref="PrefabModificationType.ListLength" /> for a given property.
        /// </summary>
        /// <param name="property">The property to register a modification for.</param>
        /// <param name="targetIndex">Selection index of the target to register a modification for.</param>
        /// <param name="newLength">The modified list length.</param>
        /// <exception cref="System.ArgumentException">
        /// Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.
        /// or
        /// newLength cannot be negative!
        /// </exception>
        public override void RegisterPrefabListLengthModification(InspectorProperty property, int targetIndex, int newLength)
        {
            if (!TargetSupportsPrefabSerialization)
            {
                Debug.LogError("Target of type " + typeof(T) + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
                return;
            }

            if (property.ValueEntry == null)
            {
                throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
            }

            if (!PropertyCanHaveModifications(property))
            {
                return;
            }

            var listLengthMods = this.prefabListLengthModifications[targetIndex];
            var valueMods = this.prefabValueModifications[targetIndex];
            var dictionaryMods = this.prefabDictionaryModifications[targetIndex];

            if (listLengthMods == null)
            {
                Debug.LogError("Target of type " + typeof(T) + " at index " + targetIndex + " is not a prefab!");
                return;
            }

            if (newLength < 0)
            {
                throw new ArgumentException("newLength cannot be negative!");
            }

            PrefabModification mod = new PrefabModification()
            {
                ModificationType = PrefabModificationType.ListLength,
                Path = property.DeepReflectionPath,
                NewLength = newLength
            };

            this.UpdatePrefabsAndModifications();

            this.RemovePrefabModificationsForInvalidIndices(property, listLengthMods, valueMods, dictionaryMods, newLength);
            listLengthMods[property.DeepReflectionPath] = mod;

            UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)this.WeakTargets[targetIndex], this.GetPrefabModifications(targetIndex));
        }

        /// <summary>
        /// Registers a modification of type <see cref="PrefabModificationType.Value" /> for a given property.
        /// </summary>
        /// <param name="property">The property to register a modification for.</param>
        /// <param name="targetIndex">Selection index of the target to register a modification for.</param>
        /// <param name="forceImmediate">Whether to force the change to be registered immediately, rather than at the end of frame.</param>
        /// <exception cref="System.ArgumentException">Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.</exception>
        public override void RegisterPrefabValueModification(InspectorProperty property, int targetIndex, bool forceImmediate = false)
        {
            const int MAX_REFERENCE_PATHS_COUNT = 5;

            if (!TargetSupportsPrefabSerialization)
            {
                Debug.LogError("Target of type " + typeof(T) + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
                return;
            }

            if (property.ValueEntry == null)
            {
                throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
            }

            if (!PropertyCanHaveModifications(property))
            {
                return;
            }

            var valueMods = this.prefabValueModifications[targetIndex];
            var listLengthMods = this.prefabListLengthModifications[targetIndex];
            var dictionaryMods = this.prefabDictionaryModifications[targetIndex];

            if (valueMods == null)
            {
                Debug.LogError("Target of type " + typeof(T) + " at index " + targetIndex + " is not a prefab!");
                return;
            }

            var propPath = property.DeepReflectionPath;
            PrefabModification mod = new PrefabModification();

            Dictionary<string, PrefabModification> extraModChanges = null;

            // Initialize modification with property values and reference paths
            {
                property.ValueEntry.Update(); // Make damn sure we have the latest values

                mod.Path = propPath;
                mod.ModifiedValue = property.ValueEntry.WeakValues[targetIndex];

                //
                // To handle references properly, we'll need to trawl the entire
                // property tree for reference paths to save.
                //

                var value = property.ValueEntry.WeakValues[targetIndex];

                if (!object.ReferenceEquals(value, null) && !(value is UnityEngine.Object) && (property.ValueEntry.ValueState == PropertyValueState.Reference || property.Info.PropertyType == PropertyType.ReferenceType))
                {
                    //string refPath;
                    //if (this.ObjectIsReferenced(value, out refPath) && this.GetReferenceCount(mod.ModifiedValue) == 1 && property.ValueEntry.TargetReferencePath != null)
                    //{
                    //    var refProp = this.GetPropertyAtPath(property.ValueEntry.TargetReferencePath);
                    //    mod.ReferencePaths = new List<string>() { refProp.DeepReflectionPath };
                    //}
                    //else
                    {
                        mod.ReferencePaths = new List<string>();

                        foreach (var prop in this.EnumerateTree(true))
                        {
                            if (prop.ValueEntry == null || prop.Info.PropertyType != PropertyType.ReferenceType || prop.Path == property.Path)
                            {
                                continue;
                            }

                            prop.ValueEntry.Update();

                            if (object.ReferenceEquals(value, prop.ValueEntry.WeakValues[targetIndex]))
                            {
                                if (mod.ReferencePaths.Count < MAX_REFERENCE_PATHS_COUNT)
                                {
                                    mod.ReferencePaths.Add(prop.DeepReflectionPath);
                                }

                                // Also update the reference value to know about this new reference to it

                                PrefabModification refMod;
                                if (valueMods.TryGetValue(prop.DeepReflectionPath, out refMod))
                                {
                                    if (refMod.ReferencePaths == null || !refMod.ReferencePaths.Contains(property.DeepReflectionPath))
                                    {
                                        if (refMod.ReferencePaths == null)
                                        {
                                            refMod.ReferencePaths = new List<string>();
                                        }

                                        if (refMod.ReferencePaths.Count < MAX_REFERENCE_PATHS_COUNT)
                                        {
                                            refMod.ReferencePaths.Add(property.DeepReflectionPath);

                                            if (extraModChanges == null)
                                            {
                                                extraModChanges = new Dictionary<string, PrefabModification>();
                                            }

                                            extraModChanges[prop.DeepReflectionPath] = refMod;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // This may hang around from before references were disabled for Unity objects, so set that to null just to be sure
                else if (value is UnityEngine.Object)
                {
                    mod.ReferencePaths = null;
                }
            }

            if (forceImmediate)
            {
                this.UpdatePrefabsAndModifications();
                this.RemoveInvalidPrefabModifications("", listLengthMods, valueMods, dictionaryMods);
                valueMods[propPath] = mod;

                if (extraModChanges != null)
                {
                    foreach (var item in extraModChanges)
                    {
                        valueMods[item.Key] = item.Value;
                    }
                }

                UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)this.WeakTargets[targetIndex], this.GetPrefabModifications(targetIndex));
            }
            else
            {
                this.DelayAction(() =>
                {
                    this.UpdatePrefabsAndModifications();

                    this.allowAutoRegisterPrefabModifications = false;
                    this.UpdateTree();
                    this.allowAutoRegisterPrefabModifications = true;

                    this.RemoveInvalidPrefabModifications("", listLengthMods, valueMods, dictionaryMods);

                    valueMods[propPath] = mod;

                    if (extraModChanges != null)
                    {
                        foreach (var item in extraModChanges)
                        {
                            valueMods[item.Key] = item.Value;
                        }
                    }

                    UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)this.WeakTargets[targetIndex], this.GetPrefabModifications(targetIndex));
                });
            }
        }

        /// <summary>
        /// Calculates a delta between the current dictionary property and its prefab counterpart, and registers that delta as a <see cref="PrefabModificationType.Dictionary" /> modification.
        /// </summary>
        /// <param name="property">The property to register a modification for.</param>
        /// <param name="targetIndex">Selection index of the target.</param>
        /// <exception cref="System.ArgumentException">Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.</exception>
        public override void RegisterPrefabDictionaryDeltaModification(InspectorProperty property, int targetIndex)
        {
            if (!TargetSupportsPrefabSerialization)
            {
                Debug.LogError("Target of type " + typeof(T) + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
                return;
            }

            if (property.ValueEntry == null)
            {
                throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
            }

            if (!property.ValueEntry.ValueIsValidDictionary)
            {
                throw new ArgumentException("Property " + property.Path + " is not a dictionary property; cannot register dictionary prefab modification to this property.");
            }

            if (!PropertyCanHaveModifications(property))
            {
                return;
            }

            var valueMods = this.prefabValueModifications[targetIndex];
            var listLengthMods = this.prefabListLengthModifications[targetIndex];
            var dictionaryMods = this.prefabDictionaryModifications[targetIndex];

            if (valueMods == null)
            {
                Debug.LogError("Target of type " + typeof(T) + " at index " + targetIndex + " is not a prefab!");
                return;
            }

            var propPath = property.DeepReflectionPath;
            PrefabModification mod;

            if (!dictionaryMods.TryGetValue(propPath, out mod))
            {
                mod = new PrefabModification();
                mod.ModificationType = PrefabModificationType.Dictionary;
                mod.Path = propPath;
            }

            InspectorProperty prefabProperty = this.prefabPropertyTree.GetPropertyAtPath(property.Path);

            if (prefabProperty == null)
            {
                // Cannot register a delta modification when there is no prefab dictionary to perform a delta on
                return;
            }

            //
            // First find removed keys
            //

            Type keyType = property.ValueEntry.TypeOfValue.GetArgumentsOfInheritedOpenGenericInterface(typeof(IDictionary<,>))[0];

            for (int i = 0; i < prefabProperty.Children.Count; i++)
            {
                var prefabChild = prefabProperty.Children[i];
                var propChild = property.Children[prefabChild.Name];

                if (propChild == null)
                {
                    // This is a removed key
                    object key = DictionaryKeyUtility.GetDictionaryKeyValue(prefabChild.Name, keyType);

                    if (mod.DictionaryKeysRemoved == null)
                    {
                        mod.DictionaryKeysRemoved = new object[] { key };
                    }
                    else
                    {
                        mod.DictionaryKeysRemoved = ArrayUtilities.CreateNewArrayWithAddedElement(mod.DictionaryKeysRemoved, key);
                    }
                }
            }

            //
            // Then find added keys
            //

            for (int i = 0; i < property.Children.Count; i++)
            {
                var propChild = property.Children[i];
                var prefabChild = prefabProperty.Children[propChild.Name];

                if (prefabChild == null)
                {
                    // This is a removed key
                    object key = DictionaryKeyUtility.GetDictionaryKeyValue(propChild.Name, keyType);

                    if (mod.DictionaryKeysAdded == null)
                    {
                        mod.DictionaryKeysAdded = new object[] { key };
                    }
                    else
                    {
                        mod.DictionaryKeysAdded = ArrayUtilities.CreateNewArrayWithAddedElement(mod.DictionaryKeysAdded, key);
                    }
                }
            }

            this.UpdatePrefabsAndModifications();
            this.RemoveInvalidPrefabModifications("", listLengthMods, valueMods, dictionaryMods);
            dictionaryMods[propPath] = mod;

            UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)this.WeakTargets[targetIndex], this.GetPrefabModifications(targetIndex));
        }

        /// <summary>
        /// Adds a remove key modification to the dictionary modifications of a given property.
        /// </summary>
        /// <param name="property">The property to register a modification for.</param>
        /// <param name="targetIndex">Selection index of the target.</param>
        /// <param name="key">The key to be removed.</param>
        /// <exception cref="System.ArgumentException">Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.</exception>
        public override void RegisterPrefabDictionaryRemoveKeyModification(InspectorProperty property, int targetIndex, object key)
        {
            if (!TargetSupportsPrefabSerialization)
            {
                Debug.LogError("Target of type " + typeof(T) + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
                return;
            }

            if (property.ValueEntry == null)
            {
                throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
            }

            if (!PropertyCanHaveModifications(property))
            {
                return;
            }

            var valueMods = this.prefabValueModifications[targetIndex];
            var listLengthMods = this.prefabListLengthModifications[targetIndex];
            var dictionaryMods = this.prefabDictionaryModifications[targetIndex];

            if (valueMods == null)
            {
                Debug.LogError("Target of type " + typeof(T) + " at index " + targetIndex + " is not a prefab!");
                return;
            }

            var propPath = property.DeepReflectionPath;
            PrefabModification mod;

            if (!dictionaryMods.TryGetValue(propPath, out mod))
            {
                mod = new PrefabModification();
                mod.ModificationType = PrefabModificationType.Dictionary;
                mod.Path = propPath;
            }

            if (mod.DictionaryKeysRemoved == null)
            {
                mod.DictionaryKeysRemoved = new object[] { key };
            }
            else
            {
                mod.DictionaryKeysRemoved = ArrayUtilities.CreateNewArrayWithAddedElement(mod.DictionaryKeysRemoved, key);
            }

            // If this key is in the added keys array, remove it from there
            if (mod.DictionaryKeysAdded != null)
            {
                for (int i = 0; i < mod.DictionaryKeysAdded.Length; i++)
                {
                    if (key.Equals(mod.DictionaryKeysAdded[i]))
                    {
                        mod.DictionaryKeysAdded = ArrayUtilities.CreateNewArrayWithRemovedElement(mod.DictionaryKeysAdded, i);
                        i--;
                    }
                }
            }

            this.UpdatePrefabsAndModifications();
            this.RemoveInvalidPrefabModifications("", listLengthMods, valueMods, dictionaryMods);
            dictionaryMods[propPath] = mod;

            UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)this.WeakTargets[targetIndex], this.GetPrefabModifications(targetIndex));
        }

        /// <summary>
        /// Adds an add key modification to the dictionary modifications of a given property.
        /// </summary>
        /// <param name="property">The property to register a modification for.</param>
        /// <param name="targetIndex">Selection index of the target.</param>
        /// <param name="key">The key to be added.</param>
        /// <exception cref="System.ArgumentException">Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.</exception>
        public override void RegisterPrefabDictionaryAddKeyModification(InspectorProperty property, int targetIndex, object key)
        {
            if (!TargetSupportsPrefabSerialization)
            {
                Debug.LogError("Target of type " + typeof(T) + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
                return;
            }

            if (property.ValueEntry == null)
            {
                throw new ArgumentException("Property " + property.Path + " does not have a value entry; cannot register prefab modification to this property.");
            }

            if (!PropertyCanHaveModifications(property))
            {
                return;
            }

            var valueMods = this.prefabValueModifications[targetIndex];
            var listLengthMods = this.prefabListLengthModifications[targetIndex];
            var dictionaryMods = this.prefabDictionaryModifications[targetIndex];

            if (valueMods == null)
            {
                Debug.LogError("Target of type " + typeof(T) + " at index " + targetIndex + " is not a prefab!");
                return;
            }

            var propPath = property.DeepReflectionPath;
            PrefabModification mod;

            if (!dictionaryMods.TryGetValue(propPath, out mod))
            {
                mod = new PrefabModification();
                mod.ModificationType = PrefabModificationType.Dictionary;
                mod.Path = propPath;
            }

            if (mod.DictionaryKeysAdded == null)
            {
                mod.DictionaryKeysAdded = new object[] { key };
            }
            else
            {
                mod.DictionaryKeysAdded = ArrayUtilities.CreateNewArrayWithAddedElement(mod.DictionaryKeysAdded, key);
            }

            // If this key is in the removed keys array, remove it from there
            if (mod.DictionaryKeysRemoved != null)
            {
                for (int i = 0; i < mod.DictionaryKeysRemoved.Length; i++)
                {
                    if (key.Equals(mod.DictionaryKeysRemoved[i]))
                    {
                        mod.DictionaryKeysRemoved = ArrayUtilities.CreateNewArrayWithRemovedElement(mod.DictionaryKeysRemoved, i);
                        i--;
                    }
                }
            }

            this.UpdatePrefabsAndModifications();
            this.RemoveInvalidPrefabModifications("", listLengthMods, valueMods, dictionaryMods);
            dictionaryMods[propPath] = mod;

            UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)this.WeakTargets[targetIndex], this.GetPrefabModifications(targetIndex));
        }

        /// <summary>
        /// Removes all dictionary modifications on a property for a given dictionary key value.
        /// </summary>
        /// <param name="property">The property to remove a key modification for.</param>
        /// <param name="targetIndex">Selection index of the target.</param>
        /// <param name="key">The key to remove modifications for.</param>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public override void RemovePrefabDictionaryModification(InspectorProperty property, int targetIndex, object key)
        {
            if (object.ReferenceEquals(key, null))
            {
                throw new ArgumentNullException("key");
            }

            if (property.ValueEntry == null || !TargetIsUnityObject)
            {
                // Nothing to do here
                return;
            }

            if (!TargetSupportsPrefabSerialization)
            {
                Debug.LogError("Target of type " + typeof(T) + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
                return;
            }

            Dictionary<string, PrefabModification> listLengthMods = this.prefabListLengthModifications[targetIndex];
            Dictionary<string, PrefabModification> valueMods = this.prefabValueModifications[targetIndex];
            Dictionary<string, PrefabModification> dictionaryMods = this.prefabDictionaryModifications[targetIndex];

            if (listLengthMods == null || valueMods == null || dictionaryMods == null)
            {
                Debug.LogError("Target of type " + typeof(T) + " at index " + targetIndex + " is not a prefab!");
                return;
            }

            bool changed = false;
            var removePath = property.DeepReflectionPath;
            PrefabModification mod;

            if (dictionaryMods.TryGetValue(removePath, out mod))
            {
                if (mod.DictionaryKeysRemoved != null)
                {
                    for (int i = 0; i < mod.DictionaryKeysRemoved.Length; i++)
                    {
                        if (key.Equals(mod.DictionaryKeysRemoved[i]))
                        {
                            changed = true;
                            mod.DictionaryKeysRemoved = ArrayUtilities.CreateNewArrayWithRemovedElement(mod.DictionaryKeysRemoved, i);
                            i--;
                        }
                    }
                }

                if (mod.DictionaryKeysAdded != null)
                {
                    for (int i = 0; i < mod.DictionaryKeysAdded.Length; i++)
                    {
                        if (key.Equals(mod.DictionaryKeysAdded[i]))
                        {
                            changed = true;
                            mod.DictionaryKeysAdded = ArrayUtilities.CreateNewArrayWithRemovedElement(mod.DictionaryKeysAdded, i);
                            i--;
                        }
                    }

                    if (changed)
                    {
                        // Also remove all modifications that were added because of this added element
                        string checkPath = removePath + "." + DictionaryKeyUtility.GetDictionaryKeyString(key);
                        HashSet<string> toRemove = new HashSet<string>();

                        foreach (string path in listLengthMods.Keys.Append(valueMods.Keys).Append(dictionaryMods.Keys))
                        {
                            if (path.StartsWith(checkPath, StringComparison.InvariantCulture))
                            {
                                toRemove.Add(path);
                            }
                        }

                        foreach (string path in toRemove)
                        {
                            listLengthMods.Remove(path);
                            valueMods.Remove(path);
                            dictionaryMods.Remove(path);
                        }
                    }
                }

                if ((mod.DictionaryKeysRemoved == null || mod.DictionaryKeysRemoved.Length == 0)
                    && (mod.DictionaryKeysRemoved == null || mod.DictionaryKeysRemoved.Length == 0))
                {
                    // If there are no modifications left, completely remove this modification
                    dictionaryMods.Remove(removePath);
                }

                changed = true;
            }

            if (changed)
            {
                // Register modification changes for next serialization call
                UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)this.WeakTargets[targetIndex], this.GetPrefabModifications(targetIndex));
            }
        }

        /// <summary>
        /// Removes all prefab modifications of a given type on a given property.
        /// </summary>
        /// <param name="property">The property to remove modifications for.</param>
        /// <param name="targetIndex">Selection index of the target.</param>
        /// <param name="modificationType">Type of the modification to remove.</param>
        public override void RemovePrefabModification(InspectorProperty property, int targetIndex, PrefabModificationType modificationType)
        {
            if (property.ValueEntry == null || !TargetIsUnityObject)
            {
                // Nothing to do here
                return;
            }

            if (property.ValueEntry.SerializationBackend == SerializationBackend.Unity)
            {
                var target = (UnityEngine.Object)(object)this.targets[targetIndex];
                var prefab = this.TargetPrefabs[targetIndex];
                var unityMods = PrefabUtility.GetPropertyModifications(target).ToList();

                if (modificationType == PrefabModificationType.Value)
                {
                    for (int i = 0; i < unityMods.Count; i++)
                    {
                        var mod = unityMods[i];

                        if (mod.target == prefab && mod.propertyPath.StartsWith(property.UnityPropertyPath, StringComparison.InvariantCulture))
                        {
                            // Remove modifications on both the path, and the children of the path
                            unityMods.RemoveAt(i);
                            i--;
                        }
                    }
                }
                else if (modificationType == PrefabModificationType.ListLength)
                {
                    var sizePath = property.UnityPropertyPath + ".Array.size";

                    // Remove the actual size modification
                    for (int i = 0; i < unityMods.Count; i++)
                    {
                        var mod = unityMods[i];

                        if (mod.target == prefab && mod.propertyPath == sizePath)
                        {
                            unityMods.RemoveAt(i);
                            i--;
                        }
                    }

                    // Also remove all modifications on new elements of the list
                    // created by the list length modification we're removing
                    this.RemovePrefabModificationsForInvalidIndices(property, prefab, unityMods);
                }

                // Make sure we play nice with undo, and that the value isn't registered
                // as changed and marked dirty from the prefab again.

                PrefabUtility.SetPropertyModifications(target, unityMods.ToArray());
                string name = Undo.GetCurrentGroupName();
                Undo.FlushUndoRecordObjects();

                for (int i = 0; i < this.targets.Length; i++)
                {
                    Undo.RecordObject((UnityEngine.Object)(object)this.targets[i], name);
                }

                PrefabUtility.SetPropertyModifications(target, unityMods.ToArray());
            }
            else if (property.ValueEntry.SerializationBackend == SerializationBackend.Odin)
            {
                // Removing sirenix prefab modifications is a little more tricky

                if (!TargetSupportsPrefabSerialization)
                {
                    Debug.LogError("Target of type " + typeof(T) + " does not support prefab serialization! Did you apply [ShowOdinSerializedPropertiesInInspector] without implementing the ISerializationCallbackReceiver and ISupportsPrefabSerialization interface as noted in the Serialize Anything section of the manual?");
                    return;
                }

                Dictionary<string, PrefabModification> listLengthMods = this.prefabListLengthModifications[targetIndex];
                Dictionary<string, PrefabModification> valueMods = this.prefabValueModifications[targetIndex];
                Dictionary<string, PrefabModification> dictionaryMods = this.prefabDictionaryModifications[targetIndex];

                if (listLengthMods == null || valueMods == null || dictionaryMods == null)
                {
                    Debug.LogError("Target of type " + typeof(T) + " at index " + targetIndex + " is not a prefab!");
                    return;
                }

                string removePath = property.DeepReflectionPath;
                PrefabModification mod;

                bool removed = false;

                if (modificationType == PrefabModificationType.Value && valueMods.ContainsKey(removePath))
                {
                    this.UpdatePrefabsAndModifications();

                    // Remove the actual mod
                    valueMods.Remove(removePath);

                    // Also remove all modifications on children of the path
                    {
                        string checkPath = removePath + ".";
                        HashSet<string> toRemove = new HashSet<string>();

                        foreach (string path in listLengthMods.Keys.Append(valueMods.Keys).Append(dictionaryMods.Keys))
                        {
                            if (path.StartsWith(checkPath, StringComparison.InvariantCulture))
                            {
                                toRemove.Add(path);
                            }
                        }

                        foreach (string path in toRemove)
                        {
                            listLengthMods.Remove(path);
                            valueMods.Remove(path);
                            dictionaryMods.Remove(path);
                        }
                    }

                    removed = true;
                }
                else if (modificationType == PrefabModificationType.ListLength && listLengthMods.TryGetValue(removePath, out mod))
                {
                    this.UpdatePrefabsAndModifications();

                    // Remove the actual mod
                    listLengthMods.Remove(removePath);

                    InspectorProperty prefabProperty = this.prefabPropertyTree.GetPropertyAtPath(property.Path);

                    if (prefabProperty != null)
                    {
                        var listChildren = prefabProperty.Children as ListPropertyChildren;
                        int prefabChildCount = listChildren != null ? listChildren.MaxListChildCount : prefabProperty.Children.Count;

                        // Also remove all modifications on new elements of the list
                        // created by the list length modification we're removing
                        this.RemovePrefabModificationsForInvalidIndices(property, listLengthMods, valueMods, dictionaryMods, prefabChildCount);
                    }

                    removed = true;
                }
                else if (modificationType == PrefabModificationType.Dictionary && dictionaryMods.TryGetValue(removePath, out mod))
                {
                    this.UpdatePrefabsAndModifications();

                    // Remove the actual mod
                    dictionaryMods.Remove(removePath);

                    // Also remove all modifications on dictionary items
                    // added by the modification we're removing
                    if (mod.DictionaryKeysAdded != null)
                    {
                        HashSet<string> toRemove = new HashSet<string>();

                        for (int i = 0; i < mod.DictionaryKeysAdded.Length; i++)
                        {
                            string keyStr = DictionaryKeyUtility.GetDictionaryKeyString(mod.DictionaryKeysAdded[i]);
                            string checkPath = removePath + "." + keyStr;

                            foreach (string path in listLengthMods.Keys.Append(valueMods.Keys).Append(dictionaryMods.Keys))
                            {
                                if (path.StartsWith(checkPath, StringComparison.InvariantCulture))
                                {
                                    toRemove.Add(path);
                                }
                            }
                        }

                        foreach (string path in toRemove)
                        {
                            listLengthMods.Remove(path);
                            valueMods.Remove(path);
                            dictionaryMods.Remove(path);
                        }
                    }

                    removed = true;
                }

                if (removed)
                {
                    // Register modification changes for next serialization call
                    UnitySerializationUtility.RegisterPrefabModificationsChange((UnityEngine.Object)this.WeakTargets[targetIndex], this.GetPrefabModifications(targetIndex));
                }
            }
        }

        private void RemoveInvalidPrefabModifications(string startPath, Dictionary<string, PrefabModification> listLengthMods, Dictionary<string, PrefabModification> valueMods, Dictionary<string, PrefabModification> dictionaryMods)
        {
            HashSet<string> toRemove = new HashSet<string>();

            foreach (string path in listLengthMods.Keys.Append(valueMods.Keys).Append(dictionaryMods.Keys))
            {
                if (!path.StartsWith(startPath)) continue;

                var prop = this.GetPropertyAtDeepReflectionPath(path);

                if (prop == null || !prop.SupportsPrefabModifications)
                {
                    toRemove.Add(path);
                }
            }

            foreach (string path in toRemove)
            {
                listLengthMods.Remove(path);
                valueMods.Remove(path);
                dictionaryMods.Remove(path);
            }
        }

        private void RemovePrefabModificationsForInvalidIndices(InspectorProperty property, Dictionary<string, PrefabModification> listLengthMods, Dictionary<string, PrefabModification> valueMods, Dictionary<string, PrefabModification> dictionaryMods, int newLength)
        {
            string removePath = property.DeepReflectionPath;

            HashSet<string> toRemove = new HashSet<string>();
            string checkPath = removePath + ".[";

            foreach (string path in listLengthMods.Keys.Append(valueMods.Keys).Append(dictionaryMods.Keys))
            {
                if (!path.StartsWith(checkPath, StringComparison.InvariantCulture))
                {
                    continue;
                }

                int arrayEndIndex = path.IndexOf("]", checkPath.Length, StringComparison.InvariantCulture);

                if (arrayEndIndex <= checkPath.Length)
                {
                    continue;
                }

                string indexStr = path.Substring(checkPath.Length, arrayEndIndex - checkPath.Length);
                int index;

                if (!int.TryParse(indexStr, out index))
                {
                    continue;
                }

                if (index >= newLength) // It's an invalid element modification
                {
                    toRemove.Add(path);
                }
            }

            foreach (string path in toRemove)
            {
                listLengthMods.Remove(path);
                valueMods.Remove(path);
                dictionaryMods.Remove(path);
            }
        }

        private void RemovePrefabModificationsForInvalidIndices(InspectorProperty property, UnityEngine.Object prefab, List<PropertyModification> unityMods)
        {
            var checkPath = property.UnityPropertyPath + ".Array.data[";
            InspectorProperty prefabProperty = this.prefabPropertyTree.GetPropertyAtPath(property.Path);

            if (prefabProperty != null)
            {
                HashSet<string> toRemove = new HashSet<string>();
                var listChildren = prefabProperty.Children as ListPropertyChildren;
                int prefabChildCount = listChildren != null ? listChildren.MaxListChildCount : prefabProperty.Children.Count;

                if (prefabChildCount < property.Children.Count)
                {
                    foreach (var mod in unityMods)
                    {
                        var path = mod.propertyPath;

                        if (!path.StartsWith(checkPath, StringComparison.InvariantCulture))
                        {
                            continue;
                        }

                        int arrayEndIndex = path.IndexOf("]", checkPath.Length, StringComparison.InvariantCulture);

                        if (arrayEndIndex <= checkPath.Length)
                        {
                            continue;
                        }

                        string indexStr = path.Substring(checkPath.Length, arrayEndIndex - checkPath.Length);
                        int index;

                        if (!int.TryParse(indexStr, out index))
                        {
                            continue;
                        }

                        if (index >= prefabChildCount) // It's an invalid element modifications
                        {
                            toRemove.Add(path);
                        }
                    }
                }

                for (int i = 0; i < unityMods.Count; i++)
                {
                    var mod = unityMods[i];

                    if (mod.target == prefab && toRemove.Contains(mod.propertyPath))
                    {
                        unityMods.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all prefab modifications in this property tree for a given selection index.
        /// </summary>
        /// <param name="targetIndex"></param>
        /// <returns></returns>
        public override List<PrefabModification> GetPrefabModifications(int targetIndex)
        {
            if (!TargetSupportsPrefabSerialization)
            {
                return new List<PrefabModification>();
            }

            var valueMods = this.prefabValueModifications[targetIndex];
            var listLengthMods = this.prefabListLengthModifications[targetIndex];
            var dictionaryMods = this.prefabDictionaryModifications[targetIndex];

            if (valueMods == null || listLengthMods == null || dictionaryMods == null)
            {
                return new List<PrefabModification>();
            }

            return listLengthMods.Values
                   .Append(valueMods.Values)
                   .Append(dictionaryMods.Values)
                   .ToList();
        }

        /// <summary>
        /// Updates all properties in the entire tree, and validates the prefab state of the tree, if applicable.
        /// </summary>
        public override void UpdateTree()
        {
            unchecked
            {
                this.updateID++;
            }

            this.objectReferences.Clear();
            this.objectReferenceCounts.Clear();
            this.properties.Clear();
            this.propertiesUnityPath.Clear();
            this.propertiesDeepReflectionPath.Clear();

            if (TargetIsUnityObject)
            {
                this.UpdatePrefabsAndModifications();
            }

            for (int i = 0; i < this.rootProperties.Length; i++)
            {
                InspectorProperty rootProperty = this.rootProperties[i];

                if (rootProperty == null)
                {
                    rootProperty = InspectorProperty.Create(this, null, this.rootPropertyInfos[i], -1);
                    this.rootProperties[i] = rootProperty;
                    rootProperty.Update();
                }

                this.UpdateProperty(rootProperty);
            }

            if (this.HasPrefabs && this.UnitySerializedObject != null && !Application.isPlaying)
            {
                for (int i = 0; i < this.WeakTargets.Count; i++)
                {
                    var targetPrefab = this.TargetPrefabs[i];

                    if (targetPrefab != null)
                    {
                        var target = (UnityEngine.Object)this.WeakTargets[i];

                        var prefabType = PrefabUtility.GetPrefabType(target);

                        if (prefabType != PrefabType.PrefabInstance)
                        {
                            continue;
                        }

                        PropertyModification[] mods = PrefabUtility.GetPropertyModifications(target);

                        if (mods != null)
                        {
                            for (int j = 0; j < mods.Length; j++)
                            {
                                var mod = mods[j];

                                if (mod.target != targetPrefab)
                                {
                                    continue;
                                }

                                string path = mod.propertyPath;
                                bool isArraySize = false;

                                if (path.EndsWith(".Array.size", StringComparison.InvariantCulture))
                                {
                                    path = path.Substring(0, path.Length - ".Array.size".Length);
                                    isArraySize = true;
                                }

                                var prop = this.GetPropertyAtUnityPath(path);

                                if (prop != null)
                                {
                                    if (isArraySize)
                                    {
                                        prop.BaseValueEntry.ListLengthChangedFromPrefab = true;
                                    }
                                    else
                                    {
                                        prop.BaseValueEntry.ValueChangedFromPrefab = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks whether a given object instance is referenced anywhere in the tree, and if it is, gives the path of the first time the object reference was encountered as an out parameter.
        /// </summary>
        /// <param name="value">The reference value to check.</param>
        /// <param name="referencePath">The first found path of the object.</param>
        public override bool ObjectIsReferenced(object value, out string referencePath)
        {
            return this.objectReferences.TryGetValue(value, out referencePath);
        }

        /// <summary>
        /// Gets the number of references to a given object instance in this tree.
        /// </summary>
        /// <param name="reference"></param>
        public override int GetReferenceCount(object reference)
        {
            int count;
            this.objectReferenceCounts.TryGetValue(reference, out count);
            return count;
        }

        /// <summary>
        /// Gets the property at the given path. Note that this is the path found in <see cref="InspectorProperty.Path" />, not the Unity path. This is a dictionary look-up.
        /// </summary>
        /// <param name="path">The path of the property to get.</param>
        public override InspectorProperty GetPropertyAtPath(string path)
        {
            InspectorProperty result;
            this.properties.TryGetValue(path, out result);
            return result;
        }

        /// <summary>
        /// Gets the property at the given Unity path. This is a dictionary look-up.
        /// </summary>
        /// <param name="path">The Unity path of the property to get.</param>
        public override InspectorProperty GetPropertyAtUnityPath(string path)
        {
            InspectorProperty result;
            this.propertiesUnityPath.TryGetValue(path, out result);
            return result;
        }

        /// <summary>
        /// Gets the property at the given deep reflection path. This is a dictionary look-up.
        /// </summary>
        /// <param name="path">The deep reflection path of the property to get.</param>
        public override InspectorProperty GetPropertyAtDeepReflectionPath(string path)
        {
            InspectorProperty result;
            this.propertiesDeepReflectionPath.TryGetValue(path, out result);
            return result;
        }

        /// <summary>
        /// Gets a Unity property for the given Odin or Unity path. If there is no <see cref="SerializedObject" /> for this property tree, or no such property is found in the <see cref="SerializedObject" />, a property will be emitted using <see cref="UnityPropertyEmitter" />.
        /// </summary>
        /// <param name="path">The Odin or Unity path to the property to get.</param>
        /// <param name="backingField">The backing field of the Unity property.</param>
        public override SerializedProperty GetUnityPropertyForPath(string path, out FieldInfo backingField)
        {
            backingField = null;
            string unityPath;
            InspectorProperty prop = this.GetPropertyAtPath(path);

            if (prop == null)
            {
                unityPath = InspectorUtilities.ConvertToUnityPropertyPath(path);
            }
            else
            {
                unityPath = prop.UnityPropertyPath;
            }

            SerializedProperty result = null;

            if (this.serializedObject != null)
            {
                result = this.serializedObject.FindProperty(unityPath);

                if (prop != null && prop.Info.MemberInfo is FieldInfo)
                {
                    // There is both a Unity property and a Sirenix property and the backing member is a field
                    // We can assign the FieldInfo now, as it's stored in the Sirenix property
                    backingField = prop.Info.MemberInfo as FieldInfo;
                }
            }

            if (result == null && prop != null && (prop.Info.PropertyType == PropertyType.ReferenceType || prop.Info.PropertyType == PropertyType.ValueType))
            {
                Dictionary<Type, SerializedProperty> innerDict;

                if (!this.emittedUnityPropertyCache.TryGetValue(path, out innerDict))
                {
                    innerDict = new Dictionary<Type, SerializedProperty>();
                    this.emittedUnityPropertyCache.Add(path, innerDict);
                }

                if (!innerDict.TryGetValue(prop.ValueEntry.TypeOfValue, out result))
                {
                    result = UnityPropertyEmitter.CreateEmittedScriptableObjectProperty(prop.Info.MemberInfo.Name, prop.ValueEntry.TypeOfValue, this.targets.Length);
                    innerDict.Add(prop.ValueEntry.TypeOfValue, result);
                }
                // TargetObject is sometimes destroyed or the serialized object is disposed, often when the profiler is toggled. Not sure why, but we need to handle the case.
                else if (result != null && result.serializedObject.targetObject == null)
                {
                    result = UnityPropertyEmitter.CreateEmittedScriptableObjectProperty(prop.Info.MemberInfo.Name, prop.ValueEntry.TypeOfValue, this.targets.Length);
                    innerDict[prop.ValueEntry.TypeOfValue] = result;
                }

                if (result != null)
                {
                    result.serializedObject.Update();
                }
            }

            return result;
        }

        /// <summary>
        /// Enumerates over the properties of the tree.
        /// </summary>
        /// <param name="includeChildren">Whether to include children of the root properties or not. If set to true, every property in the entire tree will be enumerated.</param>
        public override IEnumerable<InspectorProperty> EnumerateTree(bool includeChildren)
        {
            for (int i = 0; i < this.rootProperties.Length; i++)
            {
                InspectorProperty root = this.rootProperties[i];

                if (root == null)
                {
                    continue;
                }

                yield return root;

                if (includeChildren)
                {
                    InspectorProperty current = root.NextProperty(true);

                    while (current != null && current.Parent != null)
                    {
                        yield return current;
                        current = current.NextProperty(true);
                    }
                }
            }
        }

        /// <summary>
        /// Replaces all occurrences of a value with another value, in the entire tree.
        /// </summary>
        /// <param name="from">The value to find all instances of.</param>
        /// <param name="to">The value to replace the found values with.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">The value to replace with must either be null or be the same type as the value to replace (" + from.GetType().Name + ").</exception>
        public override void ReplaceAllReferences(object from, object to)
        {
            if (from == null)
            {
                throw new ArgumentNullException();
            }

            if (to != null)
            {
                if (from.GetType() != to.GetType())
                {
                    throw new ArgumentException("The value to replace with must either be null or be the same type as the value to replace (" + from.GetType().Name + ").");
                }
            }

            foreach (var prop in this.EnumerateTree(true))
            {
                if (prop.Info.PropertyType == PropertyType.ReferenceType)
                {
                    var valueEntry = prop.ValueEntry;

                    for (int i = 0; i < valueEntry.ValueCount; i++)
                    {
                        object obj = valueEntry.WeakValues[i];

                        if (object.ReferenceEquals(from, obj))
                        {
                            valueEntry.WeakValues[i] = to;
                        }
                    }
                }
            }
        }

        internal override void ForceRegisterObjectReference(object reference, InspectorProperty property)
        {
            var type = reference.GetType();

            if (type.IsValueType || type == typeof(string) || type.IsEnum)
            {
                // We do not allow references to boxed value types or primitives
                return;
            }

            this.objectReferences[reference] = property.Path;
        }

        private void UpdateProperty(InspectorProperty property)
        {
            try
            {
                property.ClearDrawCount();
                property.Update();

                var propertyType = property.Info.PropertyType;

                this.properties.Add(property.Path, property);

                if (propertyType != PropertyType.Group && propertyType != PropertyType.Method)
                {
                    if (!this.propertiesUnityPath.ContainsKey(property.UnityPropertyPath))
                    {
                        this.propertiesUnityPath.Add(property.UnityPropertyPath, property);
                    }

                    if (!this.propertiesDeepReflectionPath.ContainsKey(property.DeepReflectionPath))
                    {
                        this.propertiesDeepReflectionPath.Add(property.DeepReflectionPath, property);
                    }
                }

                // Register object references and their paths the first time we see each of them
                if (propertyType == PropertyType.ReferenceType)
                {
                    var valueEntry = property.ValueEntry;

                    for (int i = 0; i < valueEntry.ValueCount; i++)
                    {
                        object reference = valueEntry.WeakValues[i];

                        // Also remember to check that it's not null
                        if (object.ReferenceEquals(reference, null) == false)
                        {
                            var type = reference.GetType();

                            if (type.IsValueType || type == typeof(string) || type.IsEnum)
                            {
                                // We do not allow references to boxed value types or primitives
                                continue;
                            }

                            this.objectReferenceCounts[reference] = this.GetReferenceCount(reference) + 1;

                            if (this.objectReferences.ContainsKey(reference) == false)
                            {
                                this.objectReferences.Add(reference, property.Path);
                            }
                        }
                    }
                }

                for (int i = 0; i < property.Children.Count; i++)
                {
                    InspectorProperty child = property.Children.Get(i);
                    this.UpdateProperty(child);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("An exception was thrown while trying to update the property at path: " + property.Path + ".");
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Gets the root tree property at a given index.
        /// </summary>
        /// <param name="index">The index of the property to get.</param>
        public override InspectorProperty GetRootProperty(int index)
        {
            return this.rootProperties[index];
        }

        /// <summary>
        /// Schedules a delegate to be invoked at the end of the current GUI frame.
        /// </summary>
        /// <param name="action">The action delegate to be delayed.</param>
        /// <exception cref="System.ArgumentNullException">action</exception>
        public override void DelayAction(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            this.delayedActions.Add(action);
        }

        /// <summary>
        /// Schedules a delegate to be invoked at the end of the next Repaint GUI frame.
        /// </summary>
        /// <param name="action">The action to be delayed.</param>
        /// <exception cref="System.ArgumentNullException">action</exception>
        public override void DelayActionUntilRepaint(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            this.delayedRepaintActions.Add(action);
        }

        /// <summary>
        /// Invokes the actions that have been delayed using <see cref="DelayAction(Action)" /> and <see cref="DelayActionUntilRepaint(Action)" />.
        /// </summary>
        public override void InvokeDelayedActions()
        {
            for (int i = 0; i < this.delayedActions.Count; i++)
            {
                try
                {
                    this.delayedActions[i]();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            this.delayedActions.Clear();

            if (Event.current.type == EventType.Repaint)
            {
                for (int i = 0; i < this.delayedRepaintActions.Count; i++)
                {
                    try
                    {
                        this.delayedRepaintActions[i]();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                this.delayedRepaintActions.Clear();
            }
        }
    }
}
#endif