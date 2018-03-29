#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorPropertyInfo.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using Serialization;
    using Sirenix.OdinInspector.Internal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.Networking;
    using Utilities;

    /// <summary>
    /// Contains meta-data information about a property in the inspector.
    /// </summary>
    public abstract class InspectorPropertyInfo : IValueGetterSetter
    {
        private static readonly DoubleLookupDictionary<Type, bool, InspectorPropertyInfo[]> PropertyInfoCache = new DoubleLookupDictionary<Type, bool, InspectorPropertyInfo[]>();

        private static readonly HashSet<string> AlwaysSkipUnityProperties = new HashSet<string>()
        {
            "m_PathID",
            "m_FileID",
            "m_ObjectHideFlags",
            "m_PrefabParentObject",
            "m_PrefabInternal",
            "m_PrefabInternal",
            "m_GameObject",
            "m_Enabled",
            "m_Script",
            "m_EditorHideFlags",
            "m_EditorClassIdentifier",
        };

        private static readonly HashSet<string> AlwaysSkipUnityPropertiesForComponents = new HashSet<string>()
        {
            "m_Name",
        };

        //private static readonly Dictionary<Type, HashSet<string>> AlwaysSkipUnityPropertiesForTypes = new Dictionary<Type, HashSet<string>>()
        //{
        //    {  typeof (Transform), new HashSet<string>() { "m_Children", "m_rootOrder", "m_Father" } }
        //};

        private static readonly DoubleLookupDictionary<Type, string, string> UnityPropertyMemberNameReplacements = new DoubleLookupDictionary<Type, string, string>()
        {
            { typeof(Bounds), new Dictionary<string, string>() {
                { "m_Extent", "m_Extents" }
            } },
            { typeof(LayerMask), new Dictionary<string, string>() {
                { "m_Bits", "m_Mask" }
            } },
        };

        //private static readonly DoubleLookupDictionary<Type, string, string> UnityPropertyMemberNameLabels = new DoubleLookupDictionary<Type, string, string>()
        //{
        //    { typeof(Transform), new Dictionary<string, string>() {
        //        { "m_LocalPosition", "Position" },
        //        { "m_LocalRotation", "Rotation" },
        //        { "m_LocalScale", "Scale" },
        //    } },
        //};

        private static readonly HashSet<Type> NeverProcessUnityPropertiesFor = new HashSet<Type>()
        {
            typeof(Matrix4x4),
            typeof(Color32),
            typeof(AnimationCurve),
            typeof(Gradient),
            typeof(Coroutine)
        };

        private static readonly HashSet<Type> AlwaysSkipUnityPropertiesDeclaredBy = new HashSet<Type>()
        {
            typeof(UnityEngine.Object),
            typeof(ScriptableObject),
            typeof(Component),
            typeof(Behaviour),
            typeof(MonoBehaviour),
            typeof(StateMachineBehaviour),
        };

        private readonly MemberInfo[] memberInfos;
        private Attribute[] attributes;
        private Type typeOfOwner;
        private Type typeOfValue;

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// The member info of the property. If the property has many member infos, such as if it is a group property, the first member info of <see cref="MemberInfos"/> is returned.
        /// </summary>
        public MemberInfo MemberInfo { get { return this.memberInfos.Length == 0 ? null : this.memberInfos[0]; } }

        /// <summary>
        /// Indicates which type of property it is.
        /// </summary>
        public PropertyType PropertyType { get; private set; }

        /// <summary>
        /// The serialization backend for this property.
        /// </summary>
        public SerializationBackend SerializationBackend { get; private set; }

        /// <summary>
        /// The type on which this property is declared.
        /// </summary>
        public Type TypeOfOwner { get { return this.typeOfOwner; } }

        /// <summary>
        /// The base type of the value which this property represents.
        /// </summary>
        public Type TypeOfValue { get { return this.typeOfValue; } }

        /// <summary>
        /// Whether this property is editable or not.
        /// </summary>
        public bool IsEditable { get; private set; }

        /// <summary>
        /// All member infos of the property. There will only be more than one member if it is an <see cref="InspectorPropertyGroupInfo"/>.
        /// </summary>
        public MemberInfo[] MemberInfos { get { return this.memberInfos; } }

        /// <summary>
        /// The order value of this property. Properties are ordered by ascending order, IE, lower order values are shown first in the inspector.
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// The attributes associated with this property.
        /// </summary>
        public Attribute[] Attributes { get { return this.attributes; } }

        /// <summary>
        /// Whether this property only exists as a Unity <see cref="SerializedProperty"/>, and has no associated managed member to represent it.
        /// </summary>
        public virtual bool IsUnityPropertyOnly { get { return false; } } // Return false to remove warning?

        /// <summary>
        /// Initializes a new instance of the <see cref="InspectorPropertyInfo"/> class.
        /// </summary>
        /// <param name="memberInfo">The member to represent.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="serializationBackend">The serialization backend.</param>
        /// <param name="allowEditable">Whether the property is editable.</param>
        /// <exception cref="System.ArgumentNullException">memberInfo is null</exception>
        /// <exception cref="System.ArgumentException">Cannot greate a property group for only one member.</exception>
        protected internal InspectorPropertyInfo(MemberInfo memberInfo, PropertyType propertyType, SerializationBackend serializationBackend, bool allowEditable)
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException("memberInfo");
            }

            if (propertyType == PropertyType.Group)
            {
                throw new ArgumentException("Cannot create a property group for only one member.");
            }

            this.memberInfos = new MemberInfo[] { memberInfo };
            this.PropertyName = memberInfo.Name;
            this.PropertyType = propertyType;
            this.SerializationBackend = serializationBackend;

            this.typeOfOwner = memberInfo.DeclaringType;

            if (memberInfo is FieldInfo || memberInfo is PropertyInfo)
            {
                this.typeOfValue = memberInfo.GetReturnType();
            }

            var propertyInfo = memberInfo as PropertyInfo;
            this.IsEditable = memberInfo.IsDefined<ReadOnlyAttribute>(true) == false &&
                              (propertyInfo == null || propertyInfo.CanWrite);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InspectorPropertyInfo"/> class.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="order">The group order.</param>
        /// <param name="infos">The member infos.</param>
        /// <exception cref="System.ArgumentNullException">
        /// groupId is null
        /// or
        /// infos is null
        /// </exception>
        protected internal InspectorPropertyInfo(string groupId, int order, IList<InspectorPropertyInfo> infos)
        {
            if (groupId == null)
            {
                throw new ArgumentNullException("groupId");
            }

            if (infos == null)
            {
                throw new ArgumentNullException("infos");
            }

            this.memberInfos = infos.SelectMany(n => n.MemberInfos).ToArray();
            this.Order = order;
            //for (int i = 0; i < infos.Count; i++)
            //{
            //    int order = infos[i].Order;

            //    if (order > this.Order)
            //    {
            //        this.Order = order;
            //    }
            //}

            this.typeOfOwner = this.memberInfos[0].DeclaringType;

            this.PropertyName = groupId;
            this.PropertyType = PropertyType.Group;
            this.SerializationBackend = SerializationBackend.None;
            this.IsEditable = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InspectorPropertyInfo"/> class.
        /// </summary>
        /// <param name="unityPropertyName">Name of the unity property.</param>
        /// <param name="typeOfOwner">The type of owner.</param>
        /// <param name="typeOfValue">The type of value.</param>
        /// <param name="isEditable">Whether the property is editable.</param>
        /// <exception cref="System.ArgumentNullException">
        /// unityPropertyName is null
        /// or
        /// ownerType is null
        /// or
        /// valueType is null
        /// </exception>
        protected internal InspectorPropertyInfo(string unityPropertyName, Type typeOfOwner, Type typeOfValue, bool isEditable)
        {
            if (unityPropertyName == null)
            {
                throw new ArgumentNullException("unityPropertyName");
            }

            if (typeOfOwner == null)
            {
                throw new ArgumentNullException("ownerType");
            }

            if (typeOfValue == null)
            {
                throw new ArgumentNullException("valueType");
            }

            this.memberInfos = new MemberInfo[0];

            this.typeOfOwner = typeOfOwner;
            this.typeOfValue = typeOfValue;

            this.PropertyName = unityPropertyName;
            this.PropertyType = typeOfValue.IsValueType ? PropertyType.ValueType : PropertyType.ReferenceType;
            this.SerializationBackend = SerializationBackend.Unity;
            this.IsEditable = isEditable;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (this.PropertyType == PropertyType.Group)
            {
                return this.GetAttribute<PropertyGroupAttribute>().GroupID + " (type: " + this.PropertyType + ", order: " + this.Order + ")";
            }
            else
            {
                return this.PropertyName + " (type: " + this.PropertyType + ", backend: " + this.SerializationBackend + ", order: " + this.Order + ")";
            }
        }

        /// <summary>
        /// Sets the value of this property on the given owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="value">The value.</param>
        public abstract void SetValue(object owner, object value);

        /// <summary>
        /// Gets the value of this property from the given owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public abstract object GetValue(object owner);

        /// <summary>
        /// <para>Tries to convert this property to a strongly typed <see cref="IValueGetterSetter{TOwner, TValue}" />.</para>
        /// <para>A polymorphic alias <see cref="AliasGetterSetter{TOwner, TValue, TPropertyOwner, TPropertyValue}" /> will be created if necessary.</para>
        /// </summary>
        /// <typeparam name="TOwner">The type of the owner.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="getterSetter">The converted getter setter.</param>
        /// <returns>True if the conversion succeeded, otherwise false.</returns>
        public abstract bool TryConvertToGetterSetter<TOwner, TValue>(out IValueGetterSetter<TOwner, TValue> getterSetter);

        /// <summary>
        /// Gets the first attribute of a given type on this property.
        /// </summary>
        public T GetAttribute<T>() where T : Attribute
        {
            if (this.attributes != null)
            {
                T result;

                for (int i = 0; i < this.attributes.Length; i++)
                {
                    result = this.attributes[i] as T;

                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the first attribute of a given type on this property, which is not contained in a given hashset.
        /// </summary>
        /// <param name="exclude">The attributes to exclude.</param>
        public T GetAttribute<T>(HashSet<Attribute> exclude) where T : Attribute
        {
            if (this.attributes != null)
            {
                for (int i = 0; i < this.attributes.Length; i++)
                {
                    T attr = this.attributes[i] as T;

                    if (attr != null && (exclude == null || !exclude.Contains(attr)))
                    {
                        return attr;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all attributes of a given type on the property.
        /// </summary>
        public IEnumerable<T> GetAttributes<T>() where T : Attribute
        {
            if (this.attributes != null)
            {
                T result;

                for (int i = 0; i < this.attributes.Length; i++)
                {
                    result = this.attributes[i] as T;

                    if (result != null)
                    {
                        yield return result;
                    }
                }
            }
        }

        private static bool TryCreate(MemberInfo member, bool includeSpeciallySerializedMembers, out InspectorPropertyInfo result)
        {
            SerializationBackend? backEnd = null;

#pragma warning disable 0618 // Type or member is obsolete
            if (member.IsDefined<ExcludeDataFromInspectorAttribute>(true) || member.IsDefined<HideInInspector>(true))
            {
                result = null;
                return false;
            }
#pragma warning restore 0618 // Type or member is obsolete

            bool unityWillSerialize = UnitySerializationUtility.GuessIfUnityWillSerialize(member);

            if (member.IsDefined<OdinSerializeAttribute>(true))
            {
                backEnd = SerializationBackend.Odin;
            }
            else if (unityWillSerialize)
            {
                backEnd = SerializationBackend.Unity;
            }
            else if (SerializationPolicies.Unity.ShouldSerializeMember(member))// member is FieldInfo && ((member as FieldInfo).IsPublic || member.IsDefined<SerializeField>(true)) && !member.IsDefined<NonSerializedAttribute>(true))
            {
                backEnd = SerializationBackend.Odin;
            }
            else if (member.IsDefined<ShowInInspectorAttribute>(true))
            {
                backEnd = SerializationBackend.None;
            }

            if (backEnd == null || (backEnd == SerializationBackend.Odin && !includeSpeciallySerializedMembers))
            {
                if (unityWillSerialize)
                {
                    backEnd = SerializationBackend.Unity;
                }
                else if (member.IsDefined<ShowInInspectorAttribute>(true))
                {
                    backEnd = SerializationBackend.None;
                }
                else
                {
                    result = null;
                    return false;
                }
            }

            return TryCreate(member, backEnd.Value, true, out result);
        }

        private static bool TryCreate(MemberInfo member, SerializationBackend backEnd, bool allowEditable, out InspectorPropertyInfo result)
        {
            result = null;

            if (member is FieldInfo)
            {
                Type genericInfoType = typeof(InspectorValuePropertyInfo<,>).MakeGenericType(member.DeclaringType, (member as FieldInfo).FieldType);
                result = (InspectorPropertyInfo)Activator.CreateInstance(genericInfoType, member, backEnd, allowEditable);
            }
            else if (member is PropertyInfo)
            {
                PropertyInfo propInfo = member as PropertyInfo;
                PropertyInfo nonAliasedPropInfo = propInfo.DeAliasProperty();

                bool valid = false;

                if (backEnd == SerializationBackend.Odin)
                {
                    if (propInfo.IsDefined<ShowInInspectorAttribute>() || nonAliasedPropInfo.IsAutoProperty())
                    {
                        valid = true;
                    }
                }
                else if (propInfo.CanRead)
                {
                    valid = true;
                }

                if (valid)
                {
                    Type genericInfoType = typeof(InspectorValuePropertyInfo<,>).MakeGenericType(member.DeclaringType, propInfo.PropertyType);
                    result = (InspectorPropertyInfo)Activator.CreateInstance(genericInfoType, member, backEnd, allowEditable);
                }
            }
            else if (member is MethodInfo)
            {
                Type genericInfoType = typeof(InspectorMethodPropertyInfo<>).MakeGenericType(member.DeclaringType);
                result = (InspectorPropertyInfo)Activator.CreateInstance(genericInfoType, member);
            }

            if (result != null)
            {
                result.attributes = member.GetAttributes(true);

                var orderAttr = result.GetAttribute<PropertyOrderAttribute>();

                if (orderAttr != null)
                {
                    result.Order = orderAttr.Order;
                }
                else if (result.GetAttribute<HideInInspector>() != null)
                {
                    result.Order = int.MaxValue;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets all <see cref="InspectorPropertyInfo" />s for a given type.
        /// </summary>
        /// <param name="type">The type to get infos for.</param>
        /// <param name="includeSpeciallySerializedMembers">if set to <c>true</c> members that are serialized by Odin will be included.</param>
        /// <exception cref="System.ArgumentNullException">type is null</exception>
        public static InspectorPropertyInfo[] Get(Type type, bool includeSpeciallySerializedMembers)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            InspectorPropertyInfo[] result;

            if (PropertyInfoCache.TryGetInnerValue(type, includeSpeciallySerializedMembers, out result) == false)
            {
                result = CreateInspectorProperties(type, includeSpeciallySerializedMembers);
                PropertyInfoCache.AddInner(type, includeSpeciallySerializedMembers, result);
            }

            return result;
        }

        private static int GetMemberCategoryOrder(MemberInfo member)
        {
            if (member == null) return 0; // This tends to happen for aliased Unity properties - pretend they're a field
            if (member is FieldInfo) return 0;
            if (member is PropertyInfo) return 1;
            if (member is MethodInfo) return 2;
            return 3;
        }

        private static InspectorPropertyInfo[] CreateInspectorProperties(Type type, bool includeSpeciallySerializedMembers)
        {
            List<InspectorPropertyInfo> rootProperties = new List<InspectorPropertyInfo>();

            var assemblyFlag = AssemblyUtilities.GetAssemblyTypeFlag(type.Assembly);

            bool isUnityType = assemblyFlag == AssemblyTypeFlags.UnityEditorTypes || assemblyFlag == AssemblyTypeFlags.UnityTypes;

            if (isUnityType
                && !NeverProcessUnityPropertiesFor.Contains(type)
                && !type.ImplementsOpenGenericClass(typeof(SyncList<>))
                && !typeof(UnityAction).IsAssignableFrom(type)
                && !type.ImplementsOpenGenericClass(typeof(UnityAction<>))
                && !type.ImplementsOpenGenericClass(typeof(UnityAction<,>))
                && !type.ImplementsOpenGenericClass(typeof(UnityAction<,,>))
                && !type.ImplementsOpenGenericClass(typeof(UnityAction<,,,>)))
            {
                // It's a Unity type - we do weird stuff for those
                PopulateUnityProperties(type, rootProperties);
            }
            else
            {
                PopulateMemberInspectorProperties(type, includeSpeciallySerializedMembers, rootProperties);
            }

            Dictionary<InspectorPropertyInfo, float> memberOrder = new Dictionary<InspectorPropertyInfo, float>();

            //// Sort all members by order and then by category, and then by their original order
            rootProperties = rootProperties.OrderBy(n => n.Order)
                                           .ThenBy(n => GetMemberCategoryOrder(n.MemberInfo))
                                           //.ThenBy(n => memberOrder[n])
                                           .ToList();

            for (int i = 0; i < rootProperties.Count; i++)
            {
                memberOrder.Add(rootProperties[i], i);
            }

            BuildPropertyGroups(rootProperties, memberOrder, includeSpeciallySerializedMembers);

            return rootProperties.OrderBy(n =>
                                  {
                                      if (n is InspectorPropertyGroupInfo && n.Order == 0)
                                      {
                                          return FindFirstMemberOfGroup(n as InspectorPropertyGroupInfo).Order;
                                      }

                                      return n.Order;
                                  })
                                 .ThenBy(n => memberOrder[n])
                                 .ThenBy(n => GetMemberCategoryOrder(n.MemberInfo))
                                 //.Examine(n => Debug.Log("ROOT: " + n + " --- (" + n.Order + ", " + memberOrder[n] + ", " + GetMemberCategoryOrder(n.MemberInfo) + ")"))
                                 .ToArray();
        }

        private static InspectorPropertyInfo FindFirstMemberOfGroup(InspectorPropertyGroupInfo groupInfo)
        {
            for (int i = 0; i < groupInfo.GroupInfos.Length; i++)
            {
                var info = groupInfo.GroupInfos[i];

                if (info.PropertyType == PropertyType.Group)
                {
                    var result = FindFirstMemberOfGroup(info as InspectorPropertyGroupInfo);

                    if (result != null)
                    {
                        return result;
                    }
                }
                else
                {
                    return info;
                }
            }

            return null;
        }

        private struct GroupAttributeInfo
        {
            public InspectorPropertyInfo Member;
            public PropertyGroupAttribute Attribute;
            public bool Exclude;
        }

        private class GroupData
        {
            public string Name;
            public string ID;
            public GroupData Parent;
            public PropertyGroupAttribute ConsolidatedAttribute;
            public List<GroupAttributeInfo> Attributes = new List<GroupAttributeInfo>();
            public readonly List<GroupData> ChildGroups = new List<GroupData>();
        }

        private static void BuildPropertyGroups(List<InspectorPropertyInfo> rootProperties, Dictionary<InspectorPropertyInfo, float> memberOrder, bool includeSpeciallySerializedMembers)
        {
            Dictionary<string, GroupData> groupTree = new Dictionary<string, GroupData>();

            // Build group tree
            rootProperties.ForEach(member => member.GetAttributes<PropertyGroupAttribute>().ForEach(attr => RegisterGroupAttribute(member, attr, groupTree)));

            // Validate group tree, cull invalid groups and consolidate group attributes
            groupTree = groupTree.Where(n => ProcessGroups(n.Value, groupTree))
                                 .ToDictionary(n => n.Key, n => n.Value);

            // Create groups from group tree
            var groups = groupTree.Values.Select(n => new { Data = n, Group = CreatePropertyGroups(n, memberOrder, includeSpeciallySerializedMembers) }).ToList();

            Dictionary<InspectorPropertyInfo, InspectorPropertyGroupInfo> removedMembers = new Dictionary<InspectorPropertyInfo, InspectorPropertyGroupInfo>();

            // Replace root level members with groups
            foreach (var group in groups)
            {
                var members = RecurseGroupMembers(group.Data)
                                .OrderBy(n => memberOrder[n]);

                var firstMember = members.First();
                var index = rootProperties.IndexOf(firstMember);

                // Check for aliasing
                {
                    string finalGroupName = "#" + group.Data.Name;

                    var hiddenPropertyIndex = rootProperties.FindIndex(n => n.PropertyName == finalGroupName);

                    if (hiddenPropertyIndex >= 0)
                    {
                        var hiddenProperty = rootProperties[hiddenPropertyIndex];

                        // We need to alias either a group or a member
                        InspectorPropertyInfo newAliasForHiddenProperty;

                        if (TryHidePropertyWithGroup(hiddenProperty, group.Group, includeSpeciallySerializedMembers, out newAliasForHiddenProperty))
                        {
                            rootProperties[hiddenPropertyIndex] = newAliasForHiddenProperty;
                            removedMembers[hiddenProperty] = group.Group;
                            memberOrder[newAliasForHiddenProperty] = memberOrder[hiddenProperty];
                        }
                    }
                }

                if (index >= 0)
                {
                    //Debug.Log("REPLACE " + firstMember.PropertyName + " WITH GROUP " + group.Data.ID);

                    removedMembers.Add(rootProperties[index], group.Group);
                    memberOrder[group.Group] = memberOrder[rootProperties[index]];
                    rootProperties[index] = group.Group;
                }
                else
                {
                    var removedByGroup = removedMembers[firstMember];

                    index = rootProperties.IndexOf(removedByGroup);

                    rootProperties.Insert(index + 1, group.Group);
                    memberOrder[group.Group] = memberOrder[rootProperties[index]] + 0.1f;
                }
            }

            // Remove all remaining root members that are contained in any groups
            foreach (var group in groups)
            {
                var members = RecurseGroupMembers(group.Data);

                foreach (var member in members)
                {
                    if (!removedMembers.ContainsKey(member))
                    {
                        removedMembers.Add(member, group.Group);
                    }

                    rootProperties.Remove(member);
                }
            }
        }

        private static void RegisterGroupAttribute(InspectorPropertyInfo member, PropertyGroupAttribute attribute, Dictionary<string, GroupData> groupTree)
        {
            string[] path = attribute.GroupID.Split('/');

            string firstPathStep = path[0];

            GroupData currentGroup;

            if (!groupTree.TryGetValue(firstPathStep, out currentGroup))
            {
                currentGroup = new GroupData();
                currentGroup.ID = firstPathStep;
                currentGroup.Name = firstPathStep;

                groupTree.Add(firstPathStep, currentGroup);
            }

            for (int i = 1; i < path.Length; i++)
            {
                string step = path[i];

                var nextGroup = currentGroup.ChildGroups.FirstOrDefault(n => n.Name == step);

                if (nextGroup == null)
                {
                    nextGroup = new GroupData();
                    nextGroup.ID = string.Join("/", path.Take(i + 1).ToArray());
                    nextGroup.Name = step;
                    nextGroup.Parent = currentGroup;

                    currentGroup.ChildGroups.Add(nextGroup);
                }

                currentGroup = nextGroup;
            }

            var info = new GroupAttributeInfo();

            info.Member = member;
            info.Attribute = attribute;

            currentGroup.Attributes.Add(info);
        }

        private static bool ProcessGroups(GroupData groupData, Dictionary<string, GroupData> groupTree)
        {
            if (groupData.Attributes.Count == 0)
            {
                foreach (var expectingGroup in RecurseGroups(groupData).Where(n => n.Attributes.Count > 0))
                {
                    foreach (var attrInfo in expectingGroup.Attributes)
                    {
                        Debug.LogError(
                            "Group attribute '" + attrInfo.Attribute.GetType().Name +
                            "' on member '" + attrInfo.Member.PropertyName +
                            "' expected a group with the name '" + groupData.Name +
                            "' to exist in declaring type '" +
                            attrInfo.Member.MemberInfo.DeclaringType.GetNiceName() +
                            "'. Its ID was '" + expectingGroup.ID + "'."
                        );
                    }
                }

                return false;
            }

            // Consolidate the various group attributes into a single group attribute
            {
                groupData.ConsolidatedAttribute = groupData.Attributes[0].Attribute;
                Type groupAttrType = groupData.ConsolidatedAttribute.GetType();

                for (int i = 1; i < groupData.Attributes.Count; i++)
                {
                    var attrInfo = groupData.Attributes[i];

                    if (attrInfo.Attribute.GetType() != groupAttrType)
                    {
                        Debug.LogError(
                            "Cannot have group attributes of different types with the " +
                            "same group name, on the same type (or its inherited types): " +
                            "Group type mismatch: the group '" + groupData.ID
                            + "' is expecting attributes of type '" + groupAttrType.Name +
                            "', but got an attribute of type '" + attrInfo.Attribute.GetType().Name +
                            "' on the member '" + attrInfo.Member.MemberInfo.DeclaringType.GetNiceName() +
                            "." + attrInfo.Member.PropertyName + "'.");

                        groupData.Attributes.RemoveAt(i--);
                        continue;
                    }
                    else
                    {
                        // Consolidate attribute
                        try
                        {
                            groupData.ConsolidatedAttribute = groupData.ConsolidatedAttribute.Combine(attrInfo.Attribute);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }
            }

            // Add subgroups if applicable
            {
                ISubGroupProviderAttribute subGroupProvider = groupData.ConsolidatedAttribute as ISubGroupProviderAttribute;

                if (subGroupProvider != null)
                {
                    string[] groupPath = groupData.ID.Split('/');

                    Dictionary<string, PropertyGroupAttribute> subGroupPaths = new Dictionary<string, PropertyGroupAttribute>();

                    foreach (var subGroupAttribute in subGroupProvider.GetSubGroupAttributes())
                    {
                        string[] subGroupPath = subGroupAttribute.GroupID.Split('/');

                        bool valid = true;

                        if (subGroupPath.Length != groupPath.Length + 1)
                        {
                            valid = false;
                        }

                        if (valid)
                        {
                            for (int i = 0; i < groupPath.Length; i++)
                            {
                                if (subGroupPath[i] != groupPath[i])
                                {
                                    valid = false;
                                    break;
                                }
                            }
                        }

                        if (valid)
                        {
                            var subGroupData = groupData.ChildGroups.FirstOrDefault(n => n.Name == subGroupAttribute.GroupName);

                            if (subGroupData == null)
                            {
                                subGroupData = new GroupData();

                                subGroupData.ID = subGroupAttribute.GroupID;
                                subGroupData.Name = subGroupAttribute.GroupName;
                                subGroupData.Parent = groupData;

                                groupData.ChildGroups.Add(subGroupData);
                            }

                            if (!subGroupPaths.ContainsKey(subGroupAttribute.GroupID))
                            {
                                subGroupPaths.Add(subGroupAttribute.GroupID, subGroupAttribute);
                            }

                            var attrInfo = new GroupAttributeInfo();

                            attrInfo.Member = groupData.Attributes[0].Member;
                            attrInfo.Attribute = subGroupAttribute;
                            attrInfo.Exclude = true;

                            subGroupData.Attributes.Add(attrInfo);
                        }
                        else
                        {
                            Debug.LogError("Subgroup '" + subGroupAttribute.GroupID + "' of type '" + subGroupAttribute.GetType().Name + "' for group '" + groupData.ID + "' of type '" + groupData.ConsolidatedAttribute.GetType().Name + "' must have an ID that starts with '" + groupData.ID + "' and continue one path step further.");
                        }
                    }

                    for (int i = 0; i < groupData.Attributes.Count; i++)
                    {
                        var attrInfo = groupData.Attributes[i];
                        var newPath = subGroupProvider.RepathMemberAttribute(attrInfo.Attribute);

                        if (newPath != null && newPath != attrInfo.Attribute.GroupID)
                        {
                            if (!subGroupPaths.ContainsKey(newPath))
                            {
                                Debug.LogError("Member '" + attrInfo.Member.PropertyName + "' of " + groupData.ConsolidatedAttribute.GetType().Name + " group '" + groupData.ID + "' was repathed to subgroup at path '" + newPath + "', but no such subgroup was defined.");
                                continue;
                            }

                            groupData.Attributes.RemoveAt(i--);

                            attrInfo.Attribute = subGroupPaths[newPath];

                            var subGroup = groupData.ChildGroups.First(n => n.ID == newPath);
                            subGroup.Attributes.Add(attrInfo);
                        }
                    }
                }
            }

            // Recurse on children and remove invalid children
            groupData.ChildGroups.RemoveAll(child => !ProcessGroups(child, groupTree));

            // Remove duplicate group members if applicable
            {
                HashSet<string> memberNames = new HashSet<string>();

                for (int i = 0; i < groupData.Attributes.Count; i++)
                {
                    var attrInfo = groupData.Attributes[i];

                    if (attrInfo.Member.PropertyType == PropertyType.Group || attrInfo.Exclude) continue;

                    var name = attrInfo.Member.PropertyName;

                    if (!memberNames.Add(name))
                    {
                        groupData.Attributes.RemoveAt(i--);
                    }
                }
            }

            return true;
        }

        private static InspectorPropertyGroupInfo CreatePropertyGroups(GroupData groupData, Dictionary<InspectorPropertyInfo, float> memberOrder, bool includeSpeciallySerializedMembers)
        {
            List<InspectorPropertyInfo> children = new List<InspectorPropertyInfo>();

            foreach (var attrInfo in groupData.Attributes)
            {
                if (attrInfo.Exclude) continue;

                children.Add(attrInfo.Member);
            }

            // Replace members with sub groups
            foreach (var childGroupData in groupData.ChildGroups)
            {
                var childGroup = CreatePropertyGroups(childGroupData, memberOrder, includeSpeciallySerializedMembers);

                // Insert child group where the first member in said group would have been
                var members = RecurseGroupMembers(childGroupData)
                                .OrderBy(n => memberOrder[n]);

                var firstMember = members.First();
                var index = children.IndexOf(firstMember);

                if (index >= 0)
                {
                    //Debug.Log("REPLACE " + firstMember.PropertyName + " WITH GROUP " + childGroup.GetAttribute<PropertyGroupAttribute>().GroupID);

                    memberOrder[childGroup] = memberOrder[children[index]];
                    children[index] = childGroup;
                }
                else
                {
                    memberOrder[childGroup] = memberOrder[firstMember];
                    children.Insert(0, childGroup);
                }

                // Hide any aliased properties
                string finalGroupName = "#" + childGroup.PropertyName;

                for (int i = 0; i < children.Count; i++)
                {
                    var child = children[i];

                    if (child != childGroup && child.PropertyName == finalGroupName)
                    {
                        InspectorPropertyInfo newAliasForHiddenProperty;

                        if (TryHidePropertyWithGroup(child, childGroup, includeSpeciallySerializedMembers, out newAliasForHiddenProperty))
                        {
                            memberOrder[newAliasForHiddenProperty] = memberOrder[children[i]];
                            children[i] = newAliasForHiddenProperty;
                        }
                    }
                }
            }

            // Remove the rest of the members
            foreach (var childGroupData in groupData.ChildGroups)
            {
                var members = RecurseGroupMembers(childGroupData);

                foreach (var member in members)
                {
                    children.Remove(member);
                }
            }

            children = children.OrderBy(n => n.Order)
                               .ThenBy(n => memberOrder[n])
                               .ThenBy(n => GetMemberCategoryOrder(n.MemberInfo))
                               //.Examine(n => Debug.Log(groupData.ID + ": " + n + " --- (" + n.Order + ", " + memberOrder[n] + ", " + GetMemberCategoryOrder(n.MemberInfo) + ")"))
                               .ToList();

            var result = new InspectorPropertyGroupInfo("#" + groupData.Name, groupData.ConsolidatedAttribute.Order, children);
            result.attributes = new Attribute[] { groupData.ConsolidatedAttribute };
            return result;
        }

        private static bool TryHidePropertyWithGroup(InspectorPropertyInfo hidden, InspectorPropertyGroupInfo group, bool includeSpeciallySerializedMembers, out InspectorPropertyInfo newAliasForHiddenProperty)
        {
            if (hidden.PropertyType == PropertyType.Group)
            {
                var newGroupName = group.MemberInfo.DeclaringType.GetNiceName() + "." + group.PropertyName;
                var oldGroupName = hidden.MemberInfo.DeclaringType.GetNiceName() + "." + hidden.PropertyName;

                Debug.LogWarning("Property group '" + newGroupName + "' conflicts with already existing group property '" + oldGroupName + "'. Group property '" + newGroupName + "' will be removed from the property tree.");
                newAliasForHiddenProperty = null;
                return false;
            }
            else
            {
                var alias = FormatterUtilities.GetPrivateMemberAlias(hidden.MemberInfo, hidden.MemberInfo.DeclaringType.GetNiceName(), " -> ");

                var aliasName = alias.Name;
                var groupName = group.MemberInfo.DeclaringType.GetNiceName() + "." + group.PropertyName;
                var hiddenPropertyName = hidden.MemberInfo.DeclaringType.GetNiceName() + "." + hidden.PropertyName;

                if (InspectorPropertyInfo.TryCreate(alias, includeSpeciallySerializedMembers, out newAliasForHiddenProperty))
                {
                    Debug.LogWarning("Property group '" + groupName + "' hides member property '" + hiddenPropertyName + "'. Alias property '" + aliasName + "' created for member property '" + hiddenPropertyName + "'.");
                    return true;
                }
                else
                {
                    Debug.LogWarning("Property group '" + groupName + "' tries to hide member property '" + hiddenPropertyName + "', but failed to create alias property '" + aliasName + "' for member property '" + hiddenPropertyName + "'; group property '" + groupName + "' will be removed.");
                    return false;
                }
            }
        }

        private static IEnumerable<GroupData> RecurseGroups(GroupData groupData)
        {
            yield return groupData;

            foreach (var child in groupData.ChildGroups.SelectMany(n => RecurseGroups(n)))
            {
                yield return child;
            }
        }

        private static IEnumerable<InspectorPropertyInfo> RecurseGroupMembers(GroupData groupData)
        {
            foreach (var attrInfo in groupData.Attributes)
            {
                yield return attrInfo.Member;
            }

            foreach (var childGroup in groupData.ChildGroups.SelectMany(n => RecurseGroups(n)))
            {
                foreach (var attrInfo in childGroup.Attributes)
                {
                    yield return attrInfo.Member;
                }
            }
        }

        private static void PopulateUnityProperties(Type type, List<InspectorPropertyInfo> result)
        {
            // Steal the properties from Unity; we have no way of knowing what Unity is going to do with this type
            SerializedProperty prop;

            if (type.IsAbstract || type.IsInterface || type.IsArray) return;

            UnityEngine.Object toDestroy = null;

            if (typeof(Component).IsAssignableFrom(type))
            {
                GameObject go = new GameObject("temp");
                Component component;

                if (type.IsAssignableFrom(typeof(Transform)))
                {
                    component = go.transform;
                }
                else
                {
                    component = go.AddComponent(type);
                }

                SerializedObject obj = new SerializedObject(component);
                prop = obj.GetIterator();

                toDestroy = go;
            }
            else if (typeof(ScriptableObject).IsAssignableFrom(type))
            {
                ScriptableObject scriptableObject = ScriptableObject.CreateInstance(type);

                SerializedObject obj = new SerializedObject(scriptableObject);
                prop = obj.GetIterator();

                toDestroy = scriptableObject;
            }
            else if (UnityVersion.IsVersionOrGreater(2017, 1))
            {
                // Unity broke creation of emitted scriptable objects in 2017.1, but emitting
                // MonoBehaviours still works.

                GameObject go = new GameObject();
                var handle = UnityPropertyEmitter.CreateEmittedMonoBehaviourProperty("InspectorPropertyInfo_UnityPropertyExtractor", type, 1, ref go);
                prop = handle.UnityProperty;
                toDestroy = go;
            }
            else
            {
                prop = UnityPropertyEmitter.CreateEmittedScriptableObjectProperty("InspectorPropertyInfo_UnityPropertyExtractor", type, 1);

                if (prop != null)
                {
                    toDestroy = prop.serializedObject.targetObject;
                }
            }

            try
            {
                if (prop == null)
                {
                    Debug.LogWarning("Could not get serialized property for type " + type.GetNiceName() + "; this type will not be shown in the inspector.");
                    return;
                }

                //// Occasionally used debug code to inspect all serialized properties for types
                //{
                //    string path = prop.propertyPath;
                //    if (prop.Next(true))
                //    {
                //        do
                //        {
                //            Debug.Log(type.GetNiceFullName() + "." + prop.propertyPath + " - " + prop.propertyType);
                //        } while (prop.Next(true));

                //        prop = prop.serializedObject.FindProperty(path);
                //    }
                //}

                // Enter children if there are any
                if (prop.Next(true))
                {
                    var members = type.GetAllMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                      .Where(n => (n is FieldInfo || n is PropertyInfo) && !AlwaysSkipUnityPropertiesDeclaredBy.Contains(n.DeclaringType))
                                      .ToList();

                    // Iterate through children (but not sub-children)
                    do
                    {
                        if (AlwaysSkipUnityProperties.Contains(prop.name)) continue;
                        if (typeof(Component).IsAssignableFrom(type) && AlwaysSkipUnityPropertiesForComponents.Contains(prop.name)) continue;

                        string memberName = prop.name;

                        if (UnityPropertyMemberNameReplacements.ContainsKeys(type, memberName))
                        {
                            memberName = UnityPropertyMemberNameReplacements[type][memberName];
                        }

                        MemberInfo member = members.FirstOrDefault(n => n.Name == memberName || n.Name == prop.name);

                        if (member == null)
                        {
                            // Try to find a member that matches the display name
                            var propName = prop.displayName.Replace(" ", "");
                            bool changedPropName = false;

                            if (string.Equals(propName, "material", StringComparison.InvariantCultureIgnoreCase))
                            {
                                changedPropName = true;
                                propName = "sharedMaterial";
                            }
                            else if (string.Equals(propName, "mesh", StringComparison.InvariantCultureIgnoreCase))
                            {
                                changedPropName = true;
                                propName = "sharedMesh";
                            }

                            member = members.FirstOrDefault(n => string.Equals(n.Name, propName, StringComparison.InvariantCultureIgnoreCase) && prop.IsCompatibleWithType(n.GetReturnType()));

                            if (changedPropName && member == null)
                            {
                                // Try again with the old name
                                propName = prop.displayName.Replace(" ", "");
                                member = members.FirstOrDefault(n => string.Equals(n.Name, propName, StringComparison.InvariantCultureIgnoreCase) && prop.IsCompatibleWithType(n.GetReturnType()));
                            }
                        }

                        if (member == null)
                        {
                            // Now we are truly getting desperate.
                            // Look away, kids - this code is rated M for Monstrous

                            var propName = prop.displayName;
                            //string typeName = prop.GetProperTypeName();

                            var possibles = members.Where(n => (propName.Contains(n.Name, StringComparison.InvariantCultureIgnoreCase) || n.Name.Contains(propName, StringComparison.InvariantCultureIgnoreCase)) && prop.IsCompatibleWithType(n.GetReturnType())).ToList();

                            if (possibles.Count == 1)
                            {
                                // We found only one possibly compatible member
                                // It's... *probably* this one
                                member = possibles[0];
                            }
                        }

                        if (member == null)
                        {
                            // If we can alias this Unity property as a "virtual member", do that
                            var valueType = prop.GuessContainedType();

                            if (valueType != null && SerializedPropertyUtilities.CanSetGetValue(valueType))
                            {
                                result.Add(new UnityOnlyPropertyInfo(prop.name, type, valueType, prop.editable));
                                continue;
                            }
                        }

                        if (member == null)
                        {
                            Debug.LogWarning("Failed to find corresponding member for Unity property '" + prop.name + "/" + prop.displayName + "' on type " + type.GetNiceName() + ", and cannot alias a Unity property of type '" + prop.propertyType + "/" + prop.type + "'. This property will be missing in the inspector.");
                            continue;
                        }

                        // Makes things easier if we can only find the same member once
                        members.Remove(member);

                        InspectorPropertyInfo info;

                        // Add Unity's found property member as an info
                        if (TryCreate(member, SerializationBackend.Unity, prop.editable, out info))
                        {
                            // Make sure the names match - that way, we can find the property again
                            // when we create a Unity property path from the names
                            info.PropertyName = prop.name;

                            result.Add(info);
                        }
                    } while (prop.Next(false));
                }
            }
            catch (InvalidOperationException)
            {
                // Ignore; it just means we've reached the end of the property
            }
            finally
            {
                if (toDestroy != null)
                {
                    UnityEngine.Object.DestroyImmediate(toDestroy);
                }
            }
        }

        private static void PopulateMemberInspectorProperties(Type type, bool includeSpeciallySerializedMembers, List<InspectorPropertyInfo> properties)
        {
            if (type.BaseType != typeof(object) && type.BaseType != null)
            {
                PopulateMemberInspectorProperties(type.BaseType, includeSpeciallySerializedMembers, properties);
            }

            foreach (var member in type.GetMembers(Flags.InstanceAnyDeclaredOnly))
            {
                InspectorPropertyInfo info;

                if (InspectorPropertyInfo.TryCreate(member, includeSpeciallySerializedMembers, out info))
                {
                    InspectorPropertyInfo previousPropertyWithName = null;
                    int previousPropertyIndex = -1;

                    for (int j = 0; j < properties.Count; j++)
                    {
                        if (properties[j].MemberInfo.Name == info.MemberInfo.Name)
                        {
                            previousPropertyIndex = j;
                            previousPropertyWithName = properties[j];
                            break;
                        }
                    }

                    if (previousPropertyWithName != null)
                    {
                        bool createAlias = true;

                        if (previousPropertyWithName.PropertyType == PropertyType.Method && info.PropertyType == PropertyType.Method)
                        {
                            var oldMethod = (MethodInfo)previousPropertyWithName.MemberInfo;
                            var newMethod = (MethodInfo)member;

                            if (oldMethod.GetBaseDefinition() == newMethod.GetBaseDefinition())
                            {
                                // We have encountered an override of a method that is already a property
                                // This is a special case; we remove the base method property, and keep
                                // only the override method property.

                                createAlias = false;
                                properties.RemoveAt(previousPropertyIndex);
                            }
                        }

                        if (createAlias)
                        {
                            var alias = FormatterUtilities.GetPrivateMemberAlias(previousPropertyWithName.MemberInfo, previousPropertyWithName.MemberInfo.DeclaringType.GetNiceName(), " -> ");

                            var aliasName = alias.Name;
                            var hidden = info.MemberInfo.DeclaringType.GetNiceName() + "." + info.MemberInfo.Name;
                            var inherited = previousPropertyWithName.MemberInfo.DeclaringType.GetNiceName() + "." + previousPropertyWithName.MemberInfo.Name;

                            if (InspectorPropertyInfo.TryCreate(alias, includeSpeciallySerializedMembers, out previousPropertyWithName))
                            {
                                //Debug.LogWarning("The inspector property '" + hidden + "' hides inherited property '" + inherited + "'. Alias property '" + aliasName + "' created for inherited property '" + inherited + "'.");
                                properties[previousPropertyIndex] = previousPropertyWithName;
                            }
                            else
                            {
                                Debug.LogWarning("The inspector property '" + hidden + "' hides inherited property '" + inherited + "'. Failed to create alias property '" + aliasName + "' for inherited property '" + inherited + "'; removing inherited property instead.");
                                properties.RemoveAt(previousPropertyIndex);
                            }
                        }
                    }

                    properties.Add(info);
                }
            }
        }
    }
}
#endif