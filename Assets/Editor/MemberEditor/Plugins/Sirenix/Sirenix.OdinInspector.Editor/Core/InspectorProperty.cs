#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorProperty.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
    using Serialization;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Represents a property in the inspector, and provides the hub for all functionality related to that property.
    /// </summary>
    public sealed class InspectorProperty
    {
        private static readonly Dictionary<Type, Type> ParentCollectionTypeCache = new Dictionary<Type, Type>();

        private int maxDrawCount = 0;
        private Stack<int> drawCountStack = new Stack<int>();
        private int lastUpdatedTreeID = -1;
        private string unityPropertyPath;
        private string deepReflectionPath;

        private Type children_LastSeenValueEntryType;

        private List<int> drawerChainIndices = new List<int>();

        private bool? supportsPrefabModifications = null;

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The nice name of the property, usually as converted by <see cref="ObjectNames.NicifyVariableName(string)"/>.
        /// </summary>
        public string NiceName { get; private set; }

        /// <summary>
        /// The cached label of the property, usually containing <see cref="NiceName"/>.
        /// </summary>
        public GUIContent Label { get; set; }

        /// <summary>
        /// The full Odin path of the property. To get the Unity property path, see <see cref="UnityPropertyPath"/>.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// The child index of this property.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// <para>The current recursive draw depth, incremented for each time that the property has caused itself to be drawn recursively.</para>
        /// <para>Note that this is the <i>current</i> recursion level, not the total amount of recursions so far this frame.</para>
        /// </summary>
        public int RecursiveDrawDepth
        {
            get
            {
                return this.drawCountStack.Count;
            }
        }

        /// <summary>
        /// The amount of times that the property has been drawn so far this frame.
        /// </summary>
        public int DrawCount
        {
            get
            {
                if (this.drawCountStack.Count == 0)
                {
                    return this.maxDrawCount;
                }

                return this.drawCountStack.Peek();
            }
        }

        /// <summary>
        /// How deep in the drawer chain the property currently is, in the current drawing session as determined by <see cref="DrawCount"/>.
        /// </summary>
        public int DrawerChainIndex
        {
            get
            {
                while (this.drawerChainIndices.Count <= this.DrawCount)
                {
                    this.drawerChainIndices.Add(0);
                }

                return this.drawerChainIndices[this.DrawCount];
            }
        }

        /// <summary>
        /// Whether this property supports having prefab modifications applied or not.
        /// </summary>
        public bool SupportsPrefabModifications
        {
            get
            {
                if (this.supportsPrefabModifications == null)
                {
                    if (this.ValueEntry == null || (this.ParentValueProperty != null && !this.ParentValueProperty.SupportsPrefabModifications))
                    {
                        this.supportsPrefabModifications = false;
                    }
                    else if (this.Index == 0 && this.Parent != null && this.Parent.ValueEntry != null && this.Parent.ValueEntry.ValueCategory == PropertyValueCategory.DictionaryElement)
                    {
                        this.supportsPrefabModifications = false;
                    }
                    else
                    {
                        var handler = this.ValueEntry.GetDictionaryHandler();
                        this.supportsPrefabModifications = handler == null || handler.SupportsPrefabModifications;
                    }
                }

                return this.supportsPrefabModifications.Value;
            }
        }

        /// <summary>
        /// The value entry that represents the base value of this property.
        /// </summary>
        public PropertyValueEntry BaseValueEntry { get; private set; }

        /// <summary>
        /// The value entry that represents the strongly typed value of the property; this is possibly an alias entry in case of polymorphism.
        /// </summary>
        public IPropertyValueEntry ValueEntry { get; private set; }

        /// <summary>
        /// The parent of the property. If null, this property is a root-level property in the <see cref="PropertyTree"/>.
        /// </summary>
        public InspectorProperty Parent { get; private set; }

        /// <summary>
        /// The <see cref="InspectorPropertyInfo"/> of this property.
        /// </summary>
        public InspectorPropertyInfo Info { get; private set; }

        /// <summary>
        /// The <see cref="PropertyTree"/> that this property exists in.
        /// </summary>
        public PropertyTree Tree { get; private set; }

        /// <summary>
        /// The children of this property.
        /// </summary>
        public PropertyChildren Children { get; private set; }

        /// <summary>
        /// The context container of this property.
        /// </summary>
        public PropertyContextContainer Context { get; private set; }

        /// <summary>
        /// The last rect that this property was drawn within.
        /// </summary>
        public Rect LastDrawnValueRect
        {
            get { return this.Context.Get<Rect>(typeof(InspectorProperty), "LastDrawnValueRect").Value; }
            set { this.Context.Get<Rect>(typeof(InspectorProperty), "LastDrawnValueRect").Value = value; }
        }

        /// <summary>
        /// The type on which this property is declared. This is the same as <see cref="InspectorPropertyInfo.TypeOfOwner"/>.
        /// </summary>
        public Type ParentType { get; private set; }

        /// <summary>
        /// The parent values of this property, by selection index; this represents the values that 'own' this property, on which it is declared.
        /// </summary>
        public ImmutableList ParentValues { get; private set; }

        internal InspectorProperty ParentValueProperty { get; private set; }

        /// <summary>
        /// <para>The full Unity property path of this property; note that this is merely a converted version of <see cref="Path"/>, and not necessarily a path to an actual Unity property.</para>
        /// <para>In the case of Odin-serialized data, for example, no Unity properties will exist at this path.</para>
        /// </summary>
        public string UnityPropertyPath
        {
            get
            {
                if (this.unityPropertyPath == null)
                {
                    var pathWithoutGroups = this.GetPathWithoutGroups();
                    this.unityPropertyPath = InspectorUtilities.ConvertToUnityPropertyPath(pathWithoutGroups);
                }

                return this.unityPropertyPath;
            }
        }

        /// <summary>
        /// <para>The full path of this property as used by deep reflection, containing all the necessary information to find this property through reflection only. This is used as the path for prefab modifications.</para>
        /// </summary>
        public string DeepReflectionPath
        {
            get
            {
                if (this.deepReflectionPath == null)
                {
                    var pathWithoutGroups = this.GetPathWithoutGroups();
                    this.deepReflectionPath = InspectorUtilities.ConvertToDeepReflectionPath(pathWithoutGroups);
                }

                return this.deepReflectionPath;
            }
        }

        private InspectorProperty()
        {
        }

        /// <summary>
        /// Draws this property in the inspector. This is a shorthand for <see cref="InspectorUtilities.DrawProperty(InspectorProperty)"/>.
        /// </summary>
        public void Draw()
        {
            InspectorUtilities.DrawProperty(this);
        }

        /// <summary>
        /// Draws this property in the inspector with a given label. This is a shorthand for <see cref="InspectorUtilities.DrawProperty(InspectorProperty, GUIContent)"/>.
        /// </summary>
        public void Draw(GUIContent label)
        {
            InspectorUtilities.DrawProperty(this, label);
        }

        /// <summary>
        /// Push a draw session. This is used by <see cref="DrawCount"/> and <see cref="RecursiveDrawDepth"/>.
        /// </summary>
        public void PushDraw()
        {
            this.maxDrawCount++;
            this.drawCountStack.Push(this.maxDrawCount);
        }

        /// <summary>
        /// Increments the current drawer chain index. This is used by <see cref="DrawerChainIndex"/>.
        /// </summary>
        public void IncrementDrawerChainIndex()
        {
            while (this.drawerChainIndices.Count <= this.DrawCount)
            {
                this.drawerChainIndices.Add(0);
            }

            this.drawerChainIndices[this.DrawCount]++;
        }

        /// <summary>
        /// Pop a draw session. This is used by <see cref="DrawCount"/> and <see cref="RecursiveDrawDepth"/>.
        /// </summary>
        public void PopDraw()
        {
            this.drawCountStack.Pop();
        }

        /// <summary>
        /// Gets the next property in the <see cref="PropertyTree"/>, or null if none is found.
        /// </summary>
        /// <param name="includeChildren">Whether to include children or not.</param>
        public InspectorProperty NextProperty(bool includeChildren)
        {
            if (includeChildren && this.Children.Count > 0)
            {
                return this.Children.Get(0);
            }
            else
            {
                InspectorProperty former = null;
                InspectorProperty current = this;

                do
                {
                    former = current;
                    current = current.Parent;
                }
                while (current != null && former.Index + 1 >= former.Parent.Children.Count);

                if (current != null)
                {
                    return current.Children[former.Index + 1];
                }
                else if (former.Index + 1 < this.Tree.RootPropertyCount)
                {
                    return this.Tree.GetRootProperty(former.Index + 1);
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the first parent property with the specified value category.
        /// </summary>
        public InspectorProperty FindParent(PropertyValueCategory category, bool includeSelf)
        {
            if (includeSelf)
            {
                if (this.ValueEntry != null && this.ValueEntry.ValueCategory == category)
                {
                    return this;
                }
            }

            if (this.Parent == null)
            {
                return null;
            }

            if (this.Parent.ValueEntry == null)
            {
                return this.Parent.FindParent(category, false);
            }

            if (this.Parent.ValueEntry.ValueCategory == category)
            {
                return this.Parent;
            }

            return this.Parent.FindParent(category, false);
        }

        internal void ClearDrawCount()
        {
            this.maxDrawCount = 0;
            this.drawCountStack.Clear();

            for (int i = 0; i < this.drawerChainIndices.Count; i++)
            {
                this.drawerChainIndices[i] = 0;
            }
        }

        /// <summary>
        /// Updates the property. This method resets the temporary context, and updates the value entry and the property children.
        /// </summary>
        /// <param name="forceUpdate">If true, the property will update regardless of whether it has already updated for the current <see cref="PropertyTree.UpdateID"/>.</param>
        public void Update(bool forceUpdate = false)
        {
            if (forceUpdate == false && this.Tree.UpdateID == this.lastUpdatedTreeID)
            {
                // We've already updated this property during this property tree update
                return;
            }

            this.lastUpdatedTreeID = this.Tree.UpdateID;

            this.Context.ResetTemporaryContexts();
            this.UpdateValueEntry();
            this.UpdateChildren();

            //
            // Updating the prefab modification state of the property
            // must happen after everything else, including children,
            // has been updated.
            //

            if (this.ValueEntry != null)
            {
                if (this.ValueEntry.SerializationBackend == SerializationBackend.Odin)
                {
                    var change = this.Tree.GetPrefabModificationType(this);

                    this.BaseValueEntry.ValueChangedFromPrefab = change == PrefabModificationType.Value;
                    this.BaseValueEntry.ListLengthChangedFromPrefab = change == PrefabModificationType.ListLength;
                    this.BaseValueEntry.DictionaryChangedFromPrefab = change == PrefabModificationType.Dictionary;
                }
                else
                {
                    this.BaseValueEntry.ValueChangedFromPrefab = false;
                    this.BaseValueEntry.ListLengthChangedFromPrefab = false;
                }
            }
        }

        /// <summary>
        /// Populates a generic menu with items from all drawers for this property that implement <see cref="IDefinesGenericMenuItems"/>.
        /// </summary>
        public void PopulateGenericMenu(GenericMenu genericMenu)
        {
            if (genericMenu == null)
            {
                throw new ArgumentNullException("genericMenu");
            }

            OdinDrawer[] drawers = DrawerLocator.GetDrawersForProperty(this);

            for (int i = 0; i < drawers.Length; i++)
            {
                var drawer = drawers[i] as IDefinesGenericMenuItems;

                if (drawer != null)
                {
                    drawer.PopulateGenericMenu(this, genericMenu);
                }
            }
        }

        /// <summary>
        /// Determines whether this property is the child of another property in the hierarchy.
        /// </summary>
        /// <param name="other">The property to check whether this property is the child of.</param>
        /// <exception cref="System.ArgumentNullException">other is null</exception>
        public bool IsChildOf(InspectorProperty other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            InspectorProperty parent = this.Parent;

            while (parent != null)
            {
                if (parent == other)
                {
                    return true;
                }

                parent = parent.Parent;
            }

            return false;
        }

        /// <summary>
        /// Determines whether this property is a parent of another property in the hierarchy.
        /// </summary>
        /// <param name="other">The property to check whether this property is the parent of.</param>
        /// <exception cref="System.ArgumentNullException">other is null</exception>
        public bool IsParentOf(InspectorProperty other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            InspectorProperty parent = other.Parent;

            while (parent != null)
            {
                if (parent == this)
                {
                    return true;
                }

                parent = parent.Parent;
            }

            return false;
        }

        internal void ForceUpdatePropertyNameAndPath(int? newIndex)
        {
            if (newIndex != null)
            {
                this.Index = newIndex.Value;
            }

            this.unityPropertyPath = null;
            this.deepReflectionPath = null;

            if (this.Parent != null)
            {
                this.Path = this.Parent.Children.GetPath(this.Index);
            }
            else
            {
                this.Path = this.Info.PropertyName;
            }

            if (this.ValueEntry != null)
            {
                // Remember, collection elements inherit the info of the collection itself

                if (this.ValueEntry.ValueCategory == PropertyValueCategory.DictionaryElement)
                {
                    object key = this.Parent.ValueEntry.GetDictionaryHandler().GetKey(0, this.Index);

                    this.Name = DictionaryKeyUtility.GetDictionaryKeyString(key);
                    this.NiceName = this.Parent.NiceName + " [" + key.ToString() + "]";
                    this.Label = new GUIContent(this.NiceName);
                }
                else if (this.ValueEntry.ValueCategory == PropertyValueCategory.StrongListElement || this.ValueEntry.ValueCategory == PropertyValueCategory.WeakListElement)
                {
                    this.Name = "$" + this.Index;
                    this.NiceName = this.Parent.NiceName + " [" + this.Index + "]";
                    this.Label = new GUIContent(this.NiceName);
                }
            }

            // Children may be null, as this will be called before the property has ever updated itself
            if (this.Children != null)
            {
                this.Children.ClearPathCache();

                for (int i = 0; i < this.Children.Count; i++)
                {
                    this.Children[i].ForceUpdatePropertyNameAndPath(null);
                }
            }
        }

        private void UpdateValueEntry()
        {
            // Ensure we have the right sort of value entry

            if (this.Info.PropertyType != PropertyType.ReferenceType && this.Info.PropertyType != PropertyType.ValueType)
            {
                // Groups and methods have no value entries
                this.ValueEntry = null;
                return;
            }

            this.BaseValueEntry.Update();

            if (this.Info.PropertyType == PropertyType.ReferenceType)
            {
                Type containedType = this.BaseValueEntry.TypeOfValue;

                if (containedType != this.BaseValueEntry.BaseValueType)
                {
                    if (this.ValueEntry == null || (this.ValueEntry.IsAlias && this.ValueEntry.TypeOfValue != containedType) || (!this.ValueEntry.IsAlias && this.ValueEntry.TypeOfValue != this.ValueEntry.BaseValueType))
                    {
                        this.ValueEntry = PropertyValueEntry.CreateAlias(this.BaseValueEntry, containedType);
                    }
                }
                else
                {
                    this.ValueEntry = this.BaseValueEntry;
                }
            }
            else
            {
                if (this.ValueEntry == null)
                {
                    this.ValueEntry = this.BaseValueEntry;
                }
            }

            this.ValueEntry.Update();
        }

        private void UpdateChildren()
        {
            // Ensure we have the right sort of children for our value entry

            if (this.ValueEntry == null)
            {
                if (this.Info.PropertyType == PropertyType.Group)
                {
                    if (this.Children == null || (this.Children is GroupPropertyChildren) == false)
                    {
                        this.Children = new GroupPropertyChildren(this);
                    }
                }
                else
                {
                    if (this.Children == null || (this.Children is DisabledPropertyChildren) == false)
                    {
                        this.Children = new DisabledPropertyChildren(this);
                    }
                }

                this.children_LastSeenValueEntryType = null;
                goto UPDATE_CHILDREN;
            }

            if (this.children_LastSeenValueEntryType == this.ValueEntry.TypeOfValue)
            {
                goto UPDATE_CHILDREN;
            }

            this.children_LastSeenValueEntryType = this.ValueEntry.TypeOfValue;

            if (this.ValueEntry.TypeOfValue.IsEnum)
            {
                if (this.Children == null || (this.Children is DisabledPropertyChildren) == false)
                {
                    this.Children = new DisabledPropertyChildren(this);
                }
            }
            else if (this.ValueEntry.IsMarkedAtomic)
            {
                if (this.Children == null || (this.Children is DisabledPropertyChildren) == false)
                {
                    this.Children = new DisabledPropertyChildren(this);
                }
            }
            else if (this.ValueEntry.ValueIsValidDictionary)
            {
                if (!(this.Children == null || this.Children.GetType().IsGenericType == false || this.Children.GetType() != typeof(DictionaryPropertyChildren<,,>)))
                {
                    goto UPDATE_CHILDREN;
                }

                var args = this.ValueEntry.TypeOfValue.GetArgumentsOfInheritedOpenGenericInterface(typeof(IDictionary<,>));
                var type = typeof(DictionaryPropertyChildren<,,>).MakeGenericType(this.ValueEntry.TypeOfValue, args[0], args[1]);

                if (this.Children == null || this.Children.GetType() != type)
                {
                    this.Children = (PropertyChildren)Activator.CreateInstance(type, this);
                }
            }
            else if (this.ValueEntry.ValueIsStrongList)
            {
                if (!(this.Children == null || this.Children.GetType().IsGenericType == false || this.Children.GetType().GetGenericTypeDefinition() != typeof(StrongListPropertyChildren<,>)))
                {
                    goto UPDATE_CHILDREN;
                }

                var type = typeof(StrongListPropertyChildren<,>).MakeGenericType(this.ValueEntry.TypeOfValue, this.ValueEntry.TypeOfValue.GetArgumentsOfInheritedOpenGenericInterface(typeof(IList<>))[0]);

                if (this.Children == null || this.Children.GetType() != type)
                {
                    this.Children = (PropertyChildren)Activator.CreateInstance(type, this);
                }
            }
            else if (this.ValueEntry.ValueIsWeakList)
            {
                if (this.Children == null || (this.Children is WeakListPropertyChildren) == false)
                {
                    this.Children = new WeakListPropertyChildren(this);
                }
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(this.ValueEntry.TypeOfValue))
            {
                if (this.Children == null || (this.Children is DisabledPropertyChildren) == false)
                {
                    // Unity objects *never* have children
                    this.Children = new DisabledPropertyChildren(this);
                }
            }
            else
            {
                if (this.Children == null || (this.Children is ComplexTypePropertyChildren) == false || ((ComplexTypePropertyChildren)this.Children).ComplexType != this.ValueEntry.TypeOfValue)
                {
                    this.Children = new ComplexTypePropertyChildren(this);
                }
            }

            UPDATE_CHILDREN:

            // Finally update the children
            this.Children.Update();
        }

        internal static InspectorProperty Create(PropertyTree tree, InspectorProperty parent, InspectorPropertyInfo info, int index)
        {
            // Validate parameters first
            if (tree == null)
            {
                throw new ArgumentNullException("tree");
            }

            if (info == null)
            {
                if (parent == null)
                {
                    throw new ArgumentException("A parent is expected when the given InspectorPropertyInfo is null.");
                }

                if (parent.Children.IsCollection == false)
                {
                    throw new ArgumentException("The children of the given parent must be from a collection when the given InspectorPropertyInfo is null.");
                }
            }

            if (parent != null)
            {
                if (tree != parent.Tree)
                {
                    throw new ArgumentException("The given tree and the given parent's tree are not the same tree.");
                }

                if (parent.Children is DisabledPropertyChildren)
                {
                    throw new ArgumentException("A given parent must be able to have children to create a child property for it.");
                }

                if (index < 0 || index >= parent.Children.Count)
                {
                    throw new IndexOutOfRangeException("The given index for the property to create is out of bounds.");
                }
            }
            else
            {
                index = -1;
            }

            // Now start building a property
            InspectorProperty property = new InspectorProperty();

            property.Tree = tree;

            if (parent != null)
            {
                property.Path = parent.Children.GetPath(index);
            }
            else
            {
                property.Path = info.PropertyName;
            }

            property.Parent = parent;

            property.Index = index;
            property.Context = new PropertyContextContainer(property);

            if (property.Path == null)
            {
                Debug.Log("Property path is null for property " + property.NiceName + "!");
            }

            {
                InspectorProperty current = property;

                do
                {
                    current = current.Parent;
                }
                while (current != null && current.BaseValueEntry == null);

                property.ParentValueProperty = current;
            }

            if (property.ParentValueProperty != null)
            {
                property.ParentType = property.ParentValueProperty.ValueEntry.TypeOfValue;
                property.ParentValues = new ImmutableList(property.ParentValueProperty.ValueEntry.WeakValues);
            }
            else
            {
                property.ParentType = tree.TargetType;
                property.ParentValues = new ImmutableList(tree.WeakTargets);
            }

            if (info != null)
            {
                property.Info = info;
                property.Name = info.PropertyName;
                property.NiceName = ObjectNames.NicifyVariableName(property.Name.TrimStart('#'));
                property.Label = new GUIContent(property.NiceName);
            }
            else
            {
                // Collection elements inherit the info of the collection itself
                // and have their name set a little further down
                property.Info = parent.Info;
            }

            if (property.Info.PropertyType == PropertyType.ValueType || property.Info.PropertyType == PropertyType.ReferenceType)
            {
                property.BaseValueEntry = PropertyValueEntry.Create(property, InspectorProperty.GetBaseContainedValueType(property));
                property.ValueEntry = property.BaseValueEntry;
            }

            if (info == null)
            {
                property.ForceUpdatePropertyNameAndPath(index);
            }

            // Do NOT update the property here. Property updating may cause this property to be requested before
            // it has been registered, resulting in an infinite loop. It is the calling code's responsibility to
            // update the property before usage.

            return property;
        }

        /// <summary>
        /// Gets the base contained value type of an <see cref="InspectorProperty"/>, IE, the type that this property is absent polymorphism.
        /// </summary>
        public static Type GetBaseContainedValueType(InspectorProperty property)
        {
            Type checkParentCollectionType = null;

            if (property.ParentValueProperty == null)
            {
                checkParentCollectionType = property.Tree.TargetType; // Might be a list
            }
            else if (property.ParentValueProperty.Info == property.Info) // Happens for list and dictionary elements only
            {
                checkParentCollectionType = property.ParentValueProperty.ValueEntry.TypeOfValue;
            }

            if (checkParentCollectionType != null)
            {
                Type result;
                if (ParentCollectionTypeCache.TryGetValue(checkParentCollectionType, out result) == false)
                {
                    if (checkParentCollectionType.ImplementsOpenGenericInterface(typeof(IList<>)))
                    {
                        result = ParentCollectionTypeCache[checkParentCollectionType] = checkParentCollectionType.GetArgumentsOfInheritedOpenGenericInterface(typeof(IList<>))[0];
                    }
                    else if (typeof(IList).IsAssignableFrom(checkParentCollectionType))
                    {
                        result = ParentCollectionTypeCache[checkParentCollectionType] = typeof(object);
                    }
                    else if (checkParentCollectionType.ImplementsOpenGenericInterface(typeof(IDictionary<,>)))
                    {
                        // Special exception: we pretend dictionaries contain EditableKeyValuePair<TKey, TValue> - not, KeyValuePair<TKey, TValue>
                        result = ParentCollectionTypeCache[checkParentCollectionType] = typeof(EditableKeyValuePair<,>).MakeGenericType(checkParentCollectionType.GetArgumentsOfInheritedOpenGenericInterface(typeof(IDictionary<,>)));
                    }
                    else
                    {
                        result = ParentCollectionTypeCache[checkParentCollectionType] = null;
                    }
                }

                if (result != null)
                {
                    return result;
                }
            }

            return property.Info.TypeOfValue;
        }

        private string GetPathWithoutGroups()
        {
            Stack<InspectorProperty> parentStackWithoutGroups = new Stack<InspectorProperty>();

            {
                InspectorProperty current = this;

                do
                {
                    if (current.Info.PropertyType != PropertyType.Group)
                    {
                        parentStackWithoutGroups.Push(current);
                    }

                    current = current.Parent;
                } while (current != null);
            }

            StringBuilder pathWithoutGroups = new StringBuilder(this.Path.Length);

            {
                bool first = true;

                while (parentStackWithoutGroups.Count > 0)
                {
                    var prop = parentStackWithoutGroups.Pop();
                    string name = prop.Name;

                    // Special rule for values of dictionary key value pairs; they omit their own name (Value) and attach directly to the dictionary key specifier
                    if (prop.Parent != null && prop.Parent.ValueEntry != null && prop.Parent.ValueEntry.ValueCategory == PropertyValueCategory.DictionaryElement && prop.Index == 1) // Value is always at index 1
                    {
                        continue;
                    }

                    if (first == false)
                    {
                        pathWithoutGroups.Append('.');
                    }
                    else
                    {
                        first = false;
                    }

                    pathWithoutGroups.Append(name);
                }

                // Another special rule, here for actual key value pairs, which have ".#entry" appended to their name
                if (this.ValueEntry != null && this.ValueEntry.ValueCategory == PropertyValueCategory.DictionaryElement)
                {
                    pathWithoutGroups.Append(".#entry");
                }
            }

            return pathWithoutGroups.ToString();
        }
    }
}
#endif