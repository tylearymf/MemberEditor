#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DrawerLocator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using Serialization;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Utilities;
    using UnityEditor;
    using UnityEngine;
    using Drawers;
    using Utilities.Editor;
    using UnityEngine.Assertions;

    /// <summary>
    /// <para>Utility class for locating and sorting property drawers for the inspector.</para>
    /// <para>See Odin manual section 'Drawers in Depth' for details on how the DrawerLocator determines which drawers to use.</para>
    /// </summary>
    [InitializeOnLoad]
    public static class DrawerLocator
    {
        private static readonly FieldInfo CustomPropertyDrawerTypeField = typeof(CustomPropertyDrawer).GetField("m_Type", Flags.InstanceAnyVisibility);
        private static readonly FieldInfo CustomPropertyDrawerUseForChildrenField = typeof(CustomPropertyDrawer).GetField("m_UseForChildren", Flags.InstanceAnyVisibility);

        // Engage the cache! Fire all dictionaries! Leave no hash alive!

        private static readonly Dictionary<Type, List<DrawerInfo>> AttributeDrawerMap = new Dictionary<Type, List<DrawerInfo>>();

        // Attribute types for outer keys, value types for inner keys

        private static readonly DoubleLookupDictionary<Type, Type, OdinDrawer[]> AttributeDrawerCache = new DoubleLookupDictionary<Type, Type, OdinDrawer[]>();
        private static readonly DoubleLookupDictionary<MemberInfo, Type, OdinDrawer[]> ListElementDrawerCache = new DoubleLookupDictionary<MemberInfo, Type, OdinDrawer[]>();
        private static readonly DoubleLookupDictionary<MemberInfo, Type, OdinDrawer[]> MemberDrawerCache = new DoubleLookupDictionary<MemberInfo, Type, OdinDrawer[]>();
        private static readonly Dictionary<Type, OdinDrawer[]> PropertyGroupDrawers = new Dictionary<Type, OdinDrawer[]>();

        private static readonly Dictionary<Type, OdinDrawer[]> ValueDrawerCache = new Dictionary<Type, OdinDrawer[]>();
        private static readonly Dictionary<MemberInfo, OdinDrawer[]> MethodPropertyDrawerCache = new Dictionary<MemberInfo, OdinDrawer[]>();

        private static readonly Dictionary<Type, DrawerPriority> DrawerTypePriorityLookup = new Dictionary<Type, DrawerPriority>();
        private static readonly Dictionary<Type, OdinDrawer> InstantiatedDrawers = new Dictionary<Type, OdinDrawer>();

        private static readonly List<CustomDrawerLocator> CustomDrawerLocators = new List<CustomDrawerLocator>();

        /// <summary>
        /// Odin has its own implementations for these attribute drawers; never use Unity's.
        /// </summary>
        private static readonly HashSet<string> ExcludeUnityDrawers = new HashSet<string>()
        {
            "HeaderDrawer",
            "DelayedDrawer",
            "MultilineDrawer",
            "RangeDrawer",
            "SpaceDrawer",
            "TextAreaDrawer",
            "ColorUsageDrawer"
        };

        private static readonly List<DrawerInfo> GroupDrawerInfos;
        private static readonly List<DrawerInfo> PropertyDrawerInfos;
        private static readonly List<DrawerInfo> AttributeDrawerInfos;

        private static readonly CompositeDrawer CompositeDrawer = new CompositeDrawer();

        private static readonly MemberInfo NoMemberInfo = typeof(DrawerLocator).GetField("NoMemberInfo", BindingFlags.NonPublic | BindingFlags.Static);

        static DrawerLocator()
        {
            // This method is *very* expensive performance-wise and generates lots of garbage due to liberal use of LINQ for readability.
            // This is acceptable, as it only runs once per AppDomain reload, and only ever in the editor.

            //
            // First, get all relevant types
            //

            var allTypes = AssemblyUtilities.GetTypes(AssemblyTypeFlags.CustomTypes | AssemblyTypeFlags.UnityEditorTypes)
                            .Where(type => type.IsAbstract == false && type.IsClass && (typeof(OdinDrawer).IsAssignableFrom(type) || (typeof(GUIDrawer).IsAssignableFrom(type) && (!(type.Namespace ?? "").StartsWith("Unity", StringComparison.InvariantCulture) || !ExcludeUnityDrawers.Contains(type.Name)))))
                            .ToArray();

            //
            // Find all regular Unity property and decorator drawers and create alias drawers for them
            //

            IEnumerable<Type> unityPropertyDrawers;
            IEnumerable<Type> unityPropertyAttributeDrawers;
            IEnumerable<Type> unityDecoratorDrawers;

            if (DrawerLocator.CustomPropertyDrawerTypeField != null && DrawerLocator.CustomPropertyDrawerUseForChildrenField != null)
            {
                unityPropertyDrawers =
                    allTypes.Where(type => type.IsGenericTypeDefinition == false && typeof(PropertyDrawer).IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null)
                            .SelectMany(type => type.GetCustomAttributes<CustomPropertyDrawer>(true).Select(attr => new { Type = type, Attribute = attr }))
                            .Where(n =>
                            {
                                if (n.Attribute != null)
                                {
                                    var drawnType = CustomPropertyDrawerTypeField.GetValue(n.Attribute) as Type;

                                    if (drawnType != null && !typeof(PropertyAttribute).IsAssignableFrom(drawnType) && UnitySerializationUtility.GuessIfUnityWillSerialize(drawnType))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            })
                            .Select(n =>
                            {
                                var drawnType = (Type)CustomPropertyDrawerTypeField.GetValue(n.Attribute);

                                if (drawnType.IsAbstract || (bool)DrawerLocator.CustomPropertyDrawerUseForChildrenField.GetValue(n.Attribute))
                                {
                                    var tArg = typeof(AbstractTypeUnityPropertyDrawer<,,>).GetGenericArguments()[2];
                                    return typeof(AbstractTypeUnityPropertyDrawer<,,>).MakeGenericType(n.Type, drawnType, tArg);
                                }
                                else
                                {
                                    return typeof(UnityPropertyDrawer<,>).MakeGenericType(n.Type, drawnType);
                                }
                            });

                unityPropertyAttributeDrawers =
                    allTypes.Where(type => type.IsGenericTypeDefinition == false && typeof(PropertyDrawer).IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null)
                            .SelectMany(type => type.GetCustomAttributes<CustomPropertyDrawer>(true).Select(attr => new { Type = type, Attribute = attr }))
                            .Where(n =>
                            {
                                if (n.Attribute != null)
                                {
                                    var drawnType = CustomPropertyDrawerTypeField.GetValue(n.Attribute) as Type;

                                    if (drawnType != null && typeof(PropertyAttribute).IsAssignableFrom(drawnType))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            })
                            .Select(n =>
                            {
                                Type drawnType = (Type)CustomPropertyDrawerTypeField.GetValue(n.Attribute);

                                if ((bool)DrawerLocator.CustomPropertyDrawerUseForChildrenField.GetValue(n.Attribute))
                                {
                                    var tAttributeArgParam = typeof(UnityPropertyAttributeDrawer<,,>).GetGenericArguments()[1];
                                    return typeof(UnityPropertyAttributeDrawer<,,>).MakeGenericType(n.Type, tAttributeArgParam, drawnType);
                                }
                                else
                                {
                                    return typeof(UnityPropertyAttributeDrawer<,,>).MakeGenericType(n.Type, drawnType, typeof(PropertyAttribute));
                                }
                            });

                unityDecoratorDrawers =
                    allTypes.Where(type => type.IsGenericTypeDefinition == false && typeof(DecoratorDrawer).IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null)
                            .Select(type => new { Type = type, Attribute = type.GetCustomAttribute<CustomPropertyDrawer>(true) })
                            .Where(n => n.Attribute != null)
                            .Select(n => new { Type = n.Type, Attribute = n.Attribute, DrawnType = CustomPropertyDrawerTypeField.GetValue(n.Attribute) as Type })
                            .Where(n => n.DrawnType != null && typeof(PropertyAttribute).IsAssignableFrom(n.DrawnType))
                            .Select(n =>
                            {
                                if ((bool)DrawerLocator.CustomPropertyDrawerUseForChildrenField.GetValue(n.Attribute))
                                {
                                    var tAttributeArgParam = typeof(UnityDecoratorAttributeDrawer<,,>).GetGenericArguments()[1];
                                    return typeof(UnityDecoratorAttributeDrawer<,,>).MakeGenericType(n.Type, tAttributeArgParam, n.DrawnType);
                                }
                                else
                                {
                                    return typeof(UnityDecoratorAttributeDrawer<,,>).MakeGenericType(n.Type, n.DrawnType, typeof(PropertyAttribute));
                                }
                            });
            }
            else
            {
                Debug.LogWarning("Could not find internal fields 'm_Type' and/or 'm_UseForChildren' in type CustomPropertyDrawer; Unity PropertyDrawers and DecoratorDrawers have been disabled in the inspector.");
                unityPropertyDrawers = Enumerable.Empty<Type>();
                unityPropertyAttributeDrawers = Enumerable.Empty<Type>();
                unityDecoratorDrawers = Enumerable.Empty<Type>();
            }

            //
            // Find, group and sort all defined property drawer types
            //

            DrawerLocator.PropertyDrawerInfos =
                allTypes.Where(type => type.ImplementsOpenGenericClass(typeof(OdinValueDrawer<>)) && type.GetConstructor(Type.EmptyTypes) != null && type.IsDefined<OdinDrawerAttribute>(false))
                        .Append(unityPropertyDrawers)
                        .Select(drawerType =>
                        {
                            Type drawnType;

                            if (!DrawerIsValid(drawerType, out drawnType) || drawnType == null)
                            {
                                return null;
                            }

                            return new DrawerInfo(drawerType, drawnType, null, drawerType.GetAttribute<OdinDrawerAttribute>(false));
                        })
                        .Where(info => info != null)
                        .Distinct()
                        .OrderByDescending(info => info.Priority)
                        .ToList();

            DrawerLocator.AttributeDrawerInfos =
                allTypes.Where(type => type.ImplementsOpenGenericClass(typeof(OdinAttributeDrawer<>)) && type.GetConstructor(Type.EmptyTypes) != null && type.IsDefined<OdinDrawerAttribute>(false))
                        .Append(unityDecoratorDrawers)
                        .Append(unityPropertyAttributeDrawers)
                        .Select(type =>
                        {
                            if (type.ImplementsOpenGenericClass(typeof(OdinAttributeDrawer<,>)))
                            {
                                Type drawnType;

                                if (!DrawerIsValid(type, out drawnType))
                                {
                                    return null;
                                }

                                Type[] args = type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinAttributeDrawer<,>));
                                return new DrawerInfo(type, drawnType, args[0], type.GetAttribute<OdinDrawerAttribute>(false));
                            }
                            else
                            {
                                return new DrawerInfo(type, null, type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinAttributeDrawer<>))[0], type.GetAttribute<OdinDrawerAttribute>(false));
                            }
                        })
                        .Where(info => info != null)
                        .Distinct()
                        .OrderByDescending(info => info.Priority)
                        .ToList();

            DrawerLocator.GroupDrawerInfos =
                allTypes.Where(type => type.IsGenericTypeDefinition == false && type.IsGenericType == false && type.ImplementsOpenGenericClass(typeof(OdinGroupDrawer<>)) && type.GetConstructor(Type.EmptyTypes) != null)
                        .Select(type => new DrawerInfo(type, null, type.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinGroupDrawer<>))[0], type.GetAttribute<OdinDrawerAttribute>(false)))
                        .Where(info => info.OdinDrawerAttribute != null)
                        .Distinct()
                        .OrderByDescending(info => info.Priority)
                        .ToList();

            //
            // Register found drawers in various dictionaries
            //

            for (int i = 0; i < DrawerLocator.PropertyDrawerInfos.Count; i++)
            {
                var info = DrawerLocator.PropertyDrawerInfos[i];
                DrawerLocator.DrawerTypePriorityLookup[info.DrawerType] = info.Priority;

                //Debug.Log("Found value drawer: " + info);
            }

            for (int i = 0; i < DrawerLocator.AttributeDrawerInfos.Count; i++)
            {
                var info = DrawerLocator.AttributeDrawerInfos[i];
                DrawerLocator.DrawerTypePriorityLookup[info.DrawerType] = info.Priority;

                //Debug.Log("Found attribute drawer: " + info);

                // Also register in attribute value drawer maps

                if (info.DrawnValueType != null)
                {
                    List<DrawerInfo> list;

                    if (!DrawerLocator.AttributeDrawerMap.TryGetValue(info.DrawnAttributeType, out list))
                    {
                        list = new List<DrawerInfo>();
                        DrawerLocator.AttributeDrawerMap.Add(info.DrawnAttributeType, list);
                    }

                    list.Add(info);
                }
            }
        }

        /// <summary>
        /// <para>
        /// Registers a custom drawer locator. Note that since drawers are, for now, globally and statically cached,
        /// if this is registered late then it will not factor into drawer selection.
        /// </para>
        /// <para>
        /// It is advisable to register custom drawer locators using [<see cref="InitializeOnLoad"/>].
        /// </para>
        /// </summary>
        internal static void RegisterCustomDrawerLocator(CustomDrawerLocator customLocator)
        {
            Assert.IsNotNull(customLocator);
            CustomDrawerLocators.Add(customLocator);
        }

        /// <summary>
        /// Get all drawer infos for group drawers.
        /// </summary>
        public static IEnumerable<DrawerInfo> GetGroupDrawerInfos()
        {
            foreach (var info in GroupDrawerInfos)
            {
                yield return info;
            }
        }

        /// <summary>
        /// Get all drawer infos for attribute drawers.
        /// </summary>
        public static IEnumerable<DrawerInfo> GetAttributeDrawerInfos()
        {
            foreach (var info in AttributeDrawerInfos)
            {
                yield return info;
            }
        }

        /// <summary>
        /// Get all drawer infos for property value drawers.
        /// </summary>
        public static IEnumerable<DrawerInfo> GetPropertyDrawerInfos()
        {
            foreach (var info in PropertyDrawerInfos)
            {
                yield return info;
            }
        }

        /// <summary>
        /// Get all drawer infos.
        /// </summary>
        public static IEnumerable<DrawerInfo> GetAllDrawerInfos()
        {
            foreach (var info in PropertyDrawerInfos)
            {
                yield return info;
            }

            foreach (var info in AttributeDrawerInfos)
            {
                yield return info;
            }

            foreach (var info in GroupDrawerInfos)
            {
                yield return info;
            }
        }

        /// <summary>
        /// Gets all attribute drawers for a given attribute and value type.
        /// </summary>
        public static OdinDrawer[] GetAttributeDrawers(Type attributeType, Type valueType = null, bool forceUniqueDrawerInstances = false)
        {
            OdinDrawer[] result;

            if (valueType == null)
            {
                valueType = typeof(NoType);
            }

            if (forceUniqueDrawerInstances)
            {
                result = GetAllDrawers(valueType, attributeType, AttributeDrawerInfos, forceUniqueDrawerInstances: true);
            }
            else if (!AttributeDrawerCache.TryGetInnerValue(attributeType, valueType, out result))
            {
                result = GetAllDrawers(valueType, attributeType, AttributeDrawerInfos);
                AttributeDrawerCache.AddInner(attributeType, valueType, result);
            }

            return result;
        }

        /// <summary>
        /// Gets the priority of a given drawer type.
        /// </summary>
        public static DrawerPriority GetDrawerPriority(Type drawerType)
        {
            DrawerPriority result;

            if (DrawerTypePriorityLookup.TryGetValue(drawerType, out result))
            {
                return result;
            }
            else if (drawerType.IsGenericType && DrawerTypePriorityLookup.TryGetValue(drawerType.GetGenericTypeDefinition(), out result))
            {
                return result;
            }

            bool hasResult = false;

            if (!hasResult)
            {
                for (int i = 0; i < PropertyDrawerInfos.Count; i++)
                {
                    if (PropertyDrawerInfos[i].DrawerType == drawerType)
                    {
                        result = PropertyDrawerInfos[i].Priority;
                        hasResult = true;
                        break;
                    }
                }
            }

            if (!hasResult)
            {
                if (drawerType == typeof(InvalidTypeForAttributeDrawer))
                {
                    result = typeof(InvalidTypeForAttributeDrawer).GetAttribute<DrawerPriorityAttribute>().Priority;
                    hasResult = true;
                }
            }

            if (!hasResult)
            {
                for (int i = 0; i < AttributeDrawerInfos.Count; i++)
                {
                    if (AttributeDrawerInfos[i].DrawerType == drawerType)
                    {
                        result = AttributeDrawerInfos[i].Priority;
                        hasResult = true;
                        break;
                    }
                }
            }

            if (!hasResult)
            {
                for (int i = 0; i < GroupDrawerInfos.Count; i++)
                {
                    if (GroupDrawerInfos[i].DrawerType == drawerType)
                    {
                        result = GroupDrawerInfos[i].Priority;
                        hasResult = true;
                        break;
                    }
                }
            }

            {
                DrawerPriorityAttribute attr = drawerType.GetAttribute<DrawerPriorityAttribute>();

                if (attr != null)
                {
                    result = attr.Priority;
                    hasResult = true;
                }
            }

            if (!hasResult)
            {
                result = DrawerPriority.ValuePriority;
            }

            DrawerTypePriorityLookup.Add(drawerType, result);

            return result;
        }

        /// <summary>
        /// Gets all method drawers for a given declaring type.
        /// </summary>
        public static OdinDrawer[] GetDrawersForInstanceMethod(MemberInfo memberInfo)
        {
            if (memberInfo == null)
            {
                throw new ArgumentNullException("memberInfo");
            }

            OdinDrawer[] result;

            if (!MethodPropertyDrawerCache.TryGetValue(memberInfo, out result))
            {
                result = GetDrawersForMemberInfo(memberInfo);
                MethodPropertyDrawerCache.Add(memberInfo, result);
            }

            return result;
        }

        /// <summary>
        /// Get all primary value drawers for a given member info with a given contained value type.
        /// </summary>
        public static OdinDrawer[] GetDrawersForMemberInfo(MemberInfo memberInfo, Type containedValueType = null, bool isListElement = false)
        {
            if (containedValueType == null)
            {
                containedValueType = typeof(NoType);
            }

            if (memberInfo == null)
            {
                // This happens for Unity aliased properties
                memberInfo = NoMemberInfo;
            }

            OdinDrawer[] result;

            var cache = isListElement ? DrawerLocator.ListElementDrawerCache : DrawerLocator.MemberDrawerCache;

            if (!cache.TryGetInnerValue(memberInfo, containedValueType, out result))
            {
                List<OdinDrawer> resultList = new List<OdinDrawer>();

                // Find all possible attribute drawers
                foreach (var attribute in memberInfo.GetAttributes<Attribute>(true))
                {
                    var attrType = attribute.GetType();

                    if (isListElement && (attrType.IsDefined<DontApplyToListElementsAttribute>(true) || attrType.GetType() == typeof(UnityEngine.SpaceAttribute)))
                    {
                        continue;
                    }

                    var attrDrawers = DrawerLocator.GetAttributeDrawers(attrType, containedValueType, true);

                    if (attrDrawers.Length > 0)
                    {
                        resultList.AddRange(attrDrawers);
                    }
                    else if (!isListElement && CanShowInvalidAttributeErrorForMember(memberInfo, containedValueType, attrType))
                    {
                        List<DrawerInfo> validDrawers = new List<DrawerInfo>();

                        if (DrawerLocator.AttributeDrawerMap.ContainsKey(attrType))
                        {
                            validDrawers.AddRange(DrawerLocator.AttributeDrawerMap[attrType]);
                        }

                        if (validDrawers.Count > 0)
                        {
                            // Inject a drawer to state that this drawer-relevant attribute is invalid for the current value type
                            resultList.Add(new InvalidTypeForAttributeDrawer(memberInfo.GetNiceName(), attrType, containedValueType, validDrawers));
                        }
                    }
                }

                if (memberInfo is MethodInfo)
                {
                    // Add the standard method drawer
                    var drawer = GetDrawer(typeof(MethodPropertyDrawer<>).MakeGenericType(memberInfo.DeclaringType), false);
                    resultList.Add(drawer);
                }
                else
                {
                    // Add the standard value drawers
                    resultList.AddRange(DrawerLocator.GetValueDrawers(containedValueType));
                }

                // Sort all drawers by priority, and then order them by their original added order
                Dictionary<OdinDrawer, int> originalIndices = new Dictionary<OdinDrawer, int>(resultList.Count);

                for (int i = 0; i < resultList.Count; i++)
                {
                    originalIndices[resultList[i]] = i;
                }

                result = resultList.OrderByDescending(n => GetDrawerPriority(n.GetType()))
                                   .ThenBy(n => originalIndices[n]).ToArray();

                cache.AddInner(memberInfo, containedValueType, result);
            }

            return result;
        }

        /// <summary>
        /// Gets all drawers for a given property.
        /// </summary>
        public static OdinDrawer[] GetDrawersForProperty(InspectorProperty property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            MemberInfo memberInfo = property.Info.MemberInfo;

            if (property.Info.PropertyType == PropertyType.Method)
            {
                return GetDrawersForInstanceMethod(property.Info.MemberInfo);
            }
            else if (property.Info.PropertyType == PropertyType.Group)
            {
                PropertyGroupAttribute attr = property.Info.GetAttribute<PropertyGroupAttribute>();

                if (attr != null)
                {
                    Type attrType = attr.GetType();

                    return GetPropertyGroupDrawers(attrType);
                }
                else
                {
                    throw new InvalidOperationException("Property group property had no property group attribute!");
                }
            }
            else
            {
                bool isListElement = property.ParentValueProperty != null && property.ParentValueProperty.Info == property.Info;
                return DrawerLocator.GetDrawersForMemberInfo(memberInfo, property.ValueEntry.TypeOfValue, isListElement);
            }
        }

        /// <summary>
        /// Gets the next drawer for a given drawer and a given property, if any exists.
        /// </summary>
        public static OdinDrawer GetNextDrawer(OdinDrawer drawer, InspectorProperty property)
        {
            OdinDrawer[] drawers = GetDrawersForProperty(property);

            OdinDrawer next = null;

            for (int i = 0; i < drawers.Length; i++)
            {
                if (object.ReferenceEquals(drawers[i], drawer) && i + 1 < drawers.Length)
                {
                    next = drawers[i + 1];
                    break;
                }
            }

            return next;
        }

        /// <summary>
        /// Gets all primary drawers for a given property group attribute type.
        /// </summary>
        public static OdinDrawer[] GetPropertyGroupDrawers(Type attributeType)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }

            OdinDrawer[] result;

            if (!PropertyGroupDrawers.TryGetValue(attributeType, out result))
            {
                result = GetAllDrawers(null, attributeType, GroupDrawerInfos);
                PropertyGroupDrawers.Add(attributeType, result);
            }

            return result;
        }

        /// <summary>
        /// Gets all drawers for a given value type.
        /// </summary>
        public static OdinDrawer[] GetValueDrawers(Type valueType)
        {
            OdinDrawer[] result;

            if (!ValueDrawerCache.TryGetValue(valueType, out result))
            {
                result = GetAllDrawers(valueType, null, PropertyDrawerInfos);
                ValueDrawerCache.Add(valueType, result);
            }

            return result;
        }

        private static bool CanShowInvalidAttributeErrorForMember(MemberInfo memberInfo, Type containedValueType, Type attrType)
        {
            if (memberInfo is MethodInfo)
            {
                return false;
            }

            var memberValueType = memberInfo.GetReturnType();

            if (containedValueType == typeof(NoType) || memberValueType == typeof(object) || memberInfo.IsDefined<SuppressInvalidAttributeErrorAttribute>())
            {
                return false;
            }

            var drawers = DrawerLocator.GetAttributeDrawers(attrType, containedValueType);

            if (drawers.Length > 0)
            {
                return false;
            }

            if (memberValueType.ImplementsOpenGenericInterface(typeof(IList<>)))
            {
                Type listElementType = memberValueType.GetArgumentsOfInheritedOpenGenericInterface(typeof(IList<>))[0];

                if (listElementType == typeof(object))
                {
                    return false;
                }

                drawers = DrawerLocator.GetAttributeDrawers(attrType, listElementType);

                if (drawers.Length > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool DrawerIsUnityAlias(Type drawerType)
        {
            if (!drawerType.IsGenericType || drawerType.IsGenericTypeDefinition)
                return false;

            var definition = drawerType.GetGenericTypeDefinition();

            return definition == typeof(UnityPropertyDrawer<,>)
                || definition == typeof(UnityPropertyAttributeDrawer<,,>)
                || definition == typeof(UnityDecoratorAttributeDrawer<,,>);
        }

        private static bool DrawerIsValid(Type drawerType, out Type drawnType)
        {
            drawnType = null;

            if (drawerType.ImplementsOpenGenericClass(typeof(OdinValueDrawer<>)))
            {
                drawnType = drawerType.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinValueDrawer<>))[0];
            }
            else if (drawerType.ImplementsOpenGenericClass(typeof(OdinAttributeDrawer<,>)))
            {
                drawnType = drawerType.GetArgumentsOfInheritedOpenGenericClass(typeof(OdinAttributeDrawer<,>))[1];
            }

            if (drawnType == null)
            {
                return true;
            }

            if (drawnType.IsGenericTypeDefinition)
            {
                // Unity properties support this
                if (DrawerIsUnityAlias(drawerType))
                {
                    return true;
                }
                else
                {
                    Debug.LogError("Invalid drawer declaration '" + drawerType.GetNiceName() + "'; you cannot use generic type definition '" + drawnType.GetNiceName() + "' as the drawn value argument for a drawer that is not generic. Use a generic parameter as the drawn argument, and add a constraint for type '" + drawnType.GetNiceName() + "' instead.");
                    return false;
                }
            }

            if (drawerType.IsGenericTypeDefinition && !drawnType.IsGenericParameter)
            {
                if (drawerType.IsNested && drawerType.DeclaringType.IsGenericType)
                {
                    var parentArgs = drawerType.DeclaringType.GetGenericArguments();
                    var drawerArgs = drawerType.GetGenericArguments();
                    var valueArgs = drawnType.GetGenericArguments();

                    bool valid = parentArgs.Length == drawerArgs.Length && parentArgs.Length == valueArgs.Length;

                    // The length comparison should actually be good enough to check the condition
                    // We just compare the names out of due diligence
                    if (valid)
                    {
                        for (int i = 0; i < parentArgs.Length; i++)
                        {
                            if (parentArgs[i].Name != drawerArgs[i].Name || parentArgs[i].Name != valueArgs[i].Name)
                            {
                                valid = false;
                                break;
                            }
                        }
                    }

                    if (!valid)
                    {
                        Debug.LogError("Invalid drawer declaration '" + drawerType.GetNiceName() + "'; you cannot declare drawers nested inside generic types unless the following conditions are true: 1) the nested drawer itself is not generic, 2) the nested drawer must draw a type that is nested within the same type as the nested drawer, 3) the drawn type must not be generic.");
                        return false;
                    }
                }
                else if (drawnType.IsGenericType && drawnType.GenericArgumentsContainsTypes(drawerType.GetGenericArguments().Where(n => n.IsGenericParameter).ToArray()))
                {
                    return true;
                }
                else
                {
                    Debug.LogError("Invalid drawer declaration '" + drawerType.GetNiceName() + "'; you cannot declare a generic drawer without passing a generic parameter or a generic type definition containing all the drawer's generic paremeters as the drawn value. You passed '" + drawnType.GetNiceName() + "'.");
                    return false;
                }
            }

            if (!drawnType.IsGenericParameter)
            {
                if (drawnType.IsInterface)
                {
                    Debug.LogError("Invalid drawer declaration '" + drawerType.GetNiceName() + "'; you cannot use an interface '" + drawnType.GetNiceName() + "' as the drawn value type. Use a generic drawer with a constraint for that interface instead.");
                    return false;
                }
                else if (drawnType.IsAbstract)
                {
                    Debug.LogError("Invalid drawer declaration '" + drawerType.GetNiceName() + "'; you cannot use an abstract class '" + drawnType.GetNiceName() + "' as the drawn value type. Use a generic drawer with a constraint for that abstract class instead.");
                    return false;
                }
            }

            return true;
        }

        private static OdinDrawer[] GetAllDrawers(Type valueType, Type attributeType, List<DrawerInfo> drawers, bool forceUniqueDrawerInstances = false)
        {
            List<DrawerInfo> results = new List<DrawerInfo>();

            if (valueType == typeof(NoType))
            {
                valueType = null;
            }

            if (valueType != null)
            {
                // First, look for drawers which draw this exact type and/or attribute
                results.AddRange(GetExactTypeDrawerInfos(valueType, attributeType, drawers));

                // Then look for generic drawers where this type can fulfill the generic constraints
                results.AddRange(GetGenericTypeDrawerInfos(valueType, attributeType, drawers));
            }

            // Look for exact omni attribute drawers
            results.AddRange(GetExactTypeDrawerInfos(null, attributeType, drawers));

            // Look for generic omni attribute drawers
            results.AddRange(GetGenericTypeDrawerInfos(null, attributeType, drawers));

            foreach (var locator in CustomDrawerLocators)
            {
                results.AddRange(locator.GetDrawers(valueType, attributeType));
            }

            return results.Distinct()
                          .OrderByDescending(info => info.Priority)
                          .ThenBy(info => info.DrawerType.Name)
                          .Select(info =>
                          {
                              try
                              {
                                  return GetDrawer(info.DrawerType, forceUniqueDrawerInstances);
                              }
                              catch (Exception ex)
                              {
                                  Debug.Log("Encountered the following exception when trying to instantiate a drawer of type " + info.DrawerType.GetNiceName());
                                  Debug.LogException(ex);
                                  return null;
                              }
                          })
                          .Where(drawer => drawer != null && (valueType == null || drawer.CanDrawTypeFilter(valueType)))
                          .Append(() =>
                          {
                              // Add composite drawer last, if it can be added - it is the final fallback for types with sub properties
                              if (valueType != null && attributeType == null && !typeof(UnityEngine.Object).IsAssignableFrom(valueType) && InspectorPropertyInfo.Get(valueType, true).Length > 0)
                              {
                                  return CompositeDrawer;
                              }

                              return null;
                          })
                          .Where(drawer => drawer != null)
                          .ToArray();
        }

        private static OdinDrawer GetDrawer(Type drawerType, bool forceUniqueDrawerInstance)
        {
            OdinDrawer result;

            if (forceUniqueDrawerInstance)
            {
                result = (OdinDrawer)Activator.CreateInstance(drawerType);
            }
            else if (!InstantiatedDrawers.TryGetValue(drawerType, out result))
            {
                result = (OdinDrawer)Activator.CreateInstance(drawerType);
                InstantiatedDrawers[drawerType] = result;
            }

            return result;
        }

        private static IEnumerable<DrawerInfo> GetExactTypeDrawerInfos(Type valueType, Type attributeType, IEnumerable<DrawerInfo> drawers)
        {
            return drawers.Where(info => info.DrawnAttributeType == attributeType && info.DrawnValueType == valueType);
        }

        private static IEnumerable<DrawerInfo> GetGenericTypeDrawerInfos(Type valueType, Type attributeType, IEnumerable<DrawerInfo> drawers)
        {
            return drawers.Select(info =>
            {
                if (valueType != null)
                {
                    if (info.DrawnAttributeType == attributeType && info.DrawnValueType != null && (info.DrawnValueType.IsGenericParameter || info.DrawnValueType.IsGenericType))
                    {
                        Type[] genericArgs;

                        if (DrawerIsUnityAlias(info.DrawerType))
                        {
                            if (valueType.IsGenericType && info.DrawnValueType.IsGenericTypeDefinition && valueType.GetGenericTypeDefinition() == info.DrawnValueType.GetGenericTypeDefinition())
                            {
                                var args = info.DrawerType.GetGenericArguments();

                                // Replace generic type definition argument with actual value
                                for (int i = 0; i < args.Length; i++)
                                {
                                    if (args[i] == info.DrawnValueType)
                                    {
                                        args[i] = valueType;
                                    }
                                }

                                return new DrawerInfo(info.DrawerType.GetGenericTypeDefinition().MakeGenericType(args), valueType, info.DrawnAttributeType, info.OdinDrawerAttribute);
                            }
                        }
                        else if (info.DrawnValueType.IsGenericParameter && info.DrawerType.TryInferGenericParameters(out genericArgs, valueType))
                        {
                            var definition = info.DrawerType;

                            if (!definition.IsGenericTypeDefinition)
                            {
                                definition = definition.GetGenericTypeDefinition();
                            }

                            return new DrawerInfo(definition.MakeGenericType(genericArgs), valueType, info.DrawnAttributeType, info.OdinDrawerAttribute);
                        }
                        else if (valueType.IsGenericType && info.DrawnValueType.IsGenericType && valueType.GetGenericTypeDefinition() == info.DrawnValueType.GetGenericTypeDefinition() && info.DrawerType.AreGenericConstraintsSatisfiedBy(valueType.GetGenericArguments()))
                        {
                            return new DrawerInfo(info.DrawerType.MakeGenericType(valueType.GetGenericArguments()), valueType, info.DrawnAttributeType, info.OdinDrawerAttribute);
                        }
                        // Enter special case for drawers nested in generic types, that are drawing types also nested in the same generic types
                        else if (info.DrawerType.IsNested && valueType.IsNested)
                        {
                            genericArgs = valueType.GetGenericArguments();

                            if (info.DrawerType.AreGenericConstraintsSatisfiedBy(genericArgs))
                            {
                                return new DrawerInfo(info.DrawerType.MakeGenericType(genericArgs), valueType, info.DrawnAttributeType, info.OdinDrawerAttribute);
                            }
                        }
                    }

                    if (info.DrawnValueType != null && info.DrawnValueType.IsGenericParameter && info.DrawerType.IsGenericType && !info.DrawerType.IsFullyConstructedGenericType()) // Fallback catch-all for all generic drawers for values
                    {
                        Type[] genericArgs;

                        if (info.DrawnAttributeType != null && !info.DrawnAttributeType.IsGenericParameter && info.DrawnAttributeType != attributeType)
                        {
                            return null;
                        }

                        if ((valueType != null && attributeType != null && info.DrawerType.TryInferGenericParameters(out genericArgs, valueType, attributeType))
                        || (valueType != null && attributeType == null && info.DrawerType.TryInferGenericParameters(out genericArgs, valueType)))
                        {
                            return new DrawerInfo(info.DrawerType.GetGenericTypeDefinition().MakeGenericType(genericArgs), valueType, attributeType, info.OdinDrawerAttribute);
                        }
                    }
                }
                else if (attributeType != null && info.DrawnValueType == null && info.DrawnAttributeType != null && info.DrawnAttributeType.IsGenericParameter && info.DrawerType.IsGenericType && !info.DrawerType.IsFullyConstructedGenericType())
                {
                    Type[] genericArgs;

                    if (info.DrawerType.TryInferGenericParameters(out genericArgs, attributeType))
                    {
                        info.DrawerType.TryInferGenericParameters(out genericArgs, attributeType);

                        return new DrawerInfo(info.DrawerType.GetGenericTypeDefinition().MakeGenericType(genericArgs), valueType, attributeType, info.OdinDrawerAttribute);
                    }
                }

                return null;
            })
            .Where(info => info != null);
        }

        private static class NoType
        {
        }

        [DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
        private class InvalidTypeForAttributeDrawer : OdinDrawer
        {
            private string errorMessage;
            private string validTypeMessage;
            private List<Type> validTypes;

            public InvalidTypeForAttributeDrawer(string propertyName, Type attributeType, Type valueType, List<DrawerInfo> validDrawers)
            {
                this.validTypes = validDrawers.Where(n => n.DrawnValueType != null)
                                              .Select(n => n.DrawnValueType)
                                              .Distinct()
                                              .ToList();

                var sb = new StringBuilder("Attribute '")
                                .Append(attributeType.GetNiceName())
                                .Append("' cannot be put on property '")
                                .Append(propertyName)
                                .Append("' of type '")
                                .Append(valueType.GetNiceName());

                this.errorMessage = sb.ToString();
                sb.Length = 0;

                sb.AppendLine("The following types are valid:");
                sb.AppendLine();

                for (int i = 0; i < this.validTypes.Count; i++)
                {
                    var type = this.validTypes[i];
                    sb.Append(type.GetNiceName());

                    if (type.IsGenericParameter)
                    {
                        sb.Append(" ")
                          .Append(type.GetGenericParameterConstraintsString(useFullTypeNames: true));
                    }

                    sb.AppendLine();
                }

                sb.Append("IList<T> where T is any of the above types");

                this.validTypeMessage = sb.ToString();
            }

            protected override void DrawPropertyImplementation(InspectorProperty property, GUIContent label)
            {
                var isFolded = property.Context.Get<bool>(this, "IsFolded", true);
                isFolded.Value = SirenixEditorGUI.DetailedMessageBox(this.errorMessage, this.validTypeMessage, MessageType.Error, isFolded.Value);

                this.CallNextDrawer(property, label);
            }
        }
    }
}
#endif