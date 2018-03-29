#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UnityPropertyHandlerUtility.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using Utilities;
    using System;

    internal static class UnityPropertyHandlerUtility
    {
        private const string ScriptAttributeUtilityName = "UnityEditor.ScriptAttributeUtility";
        private const string PropertyHandlerCacheName = "UnityEditor.PropertyHandlerCache";
        private const string PropertyHandlerName = "UnityEditor.PropertyHandler";

        private const string ScriptAttributeUtility_PropertyHandlerCacheName = "propertyHandlerCache";
        private const string PropertyHandlerCache_SetHandlerName = "SetHandler";
        private const string PropertyHandler_OnGUIName = "OnGUI";
        private const string PropertyHandler_PropertyDrawerName = "m_PropertyDrawer";

        // Different name so as not to potentially collide with other delegates
        private delegate void FiveArgAction<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

        private static readonly Func<object> ScriptAttributeUtility_GetPropertyHandlerCache;
        private static readonly Action<object, SerializedProperty, object> PropertyHandlerCache_SetHandler;
        private static readonly Func<object> PropertyHandler_Create;
        private static readonly FiveArgAction<object, Rect, SerializedProperty, GUIContent, bool> PropertyHandler_OnGUI;
        private static readonly Action<object, PropertyDrawer> PropertyHandler_SetPropertyDrawer;

        private static readonly Type ScriptAttributeUtility = typeof(Editor).Assembly.GetType(ScriptAttributeUtilityName);
        private static readonly Type PropertyHandlerCache = typeof(Editor).Assembly.GetType(PropertyHandlerCacheName);
        private static readonly Type PropertyHandler = typeof(Editor).Assembly.GetType(PropertyHandlerName);

        static UnityPropertyHandlerUtility()
        {
            if (ScriptAttributeUtility == null)
            {
                CouldNotFindTypeError(ScriptAttributeUtilityName);
                return;
            }

            if (PropertyHandlerCache == null)
            {
                CouldNotFindTypeError(PropertyHandlerCacheName);
                return;
            }

            if (PropertyHandler == null)
            {
                CouldNotFindTypeError(PropertyHandlerName);
                return;
            }

            var propertyHandlerCacheProperty = ScriptAttributeUtility.GetProperty(ScriptAttributeUtility_PropertyHandlerCacheName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            var setHandlerMethod = PropertyHandlerCache.GetMethod(PropertyHandlerCache_SetHandlerName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var onGUIMethod = PropertyHandler.GetMethod(PropertyHandler_OnGUIName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var drawerField = PropertyHandler.GetField(PropertyHandler_PropertyDrawerName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (propertyHandlerCacheProperty == null)
            {
                CouldNotFindMemberError(ScriptAttributeUtility, ScriptAttributeUtility_PropertyHandlerCacheName);
                return;
            }

            if (setHandlerMethod == null)
            {
                CouldNotFindMemberError(PropertyHandlerCache, PropertyHandlerCache_SetHandlerName);
                return;
            }

            if (onGUIMethod == null)
            {
                CouldNotFindMemberError(PropertyHandler, PropertyHandler_OnGUIName);
                return;
            }

            if (drawerField == null)
            {
                CouldNotFindMemberError(PropertyHandler, PropertyHandler_PropertyDrawerName);
                return;
            }

            ScriptAttributeUtility_GetPropertyHandlerCache = () => propertyHandlerCacheProperty.GetValue(null, null);
            PropertyHandlerCache_SetHandler = (instance, property, handler) => setHandlerMethod.Invoke(instance, new object[] { property, handler });
            PropertyHandler_Create = () => Activator.CreateInstance(PropertyHandler);
            PropertyHandler_OnGUI = (instance, rect, property, label, includeChildren) => onGUIMethod.Invoke(instance, new object[] { rect, property, label, includeChildren });
            PropertyHandler_SetPropertyDrawer = (instance, drawer) => drawerField.SetValue(instance, drawer);

            IsAvailable = true;
        }

        private static void CouldNotFindTypeError(string typeName)
        {
            Debug.LogError("Could not find the internal Unity type '" + typeName + "'; cannot correctly set internal Unity state for drawing of custom Unity property drawers - drawers which call EditorGUI.PropertyField or EditorGUILayout.PropertyField will be drawn partially twice.");
        }

        private static void CouldNotFindMemberError(Type type, string memberName)
        {
            Debug.LogError("Could not find the member '" + memberName + "' on internal Unity type '" + type.GetNiceFullName() + "'; cannot correctly set internal Unity state for drawing of custom Unity property drawers - drawers which call EditorGUI.PropertyField or EditorGUILayout.PropertyField will be drawn partially twice.");
        }

        public static bool IsAvailable { get; private set; }

        public static void PropertyHandlerOnGUI(object handler, Rect rect, SerializedProperty property, GUIContent label, bool includeChildren)
        {
            if (!IsAvailable)
            {
                return;
            }

            var cache = ScriptAttributeUtility_GetPropertyHandlerCache();
            PropertyHandlerCache_SetHandler(cache, property, handler);
            PropertyHandler_OnGUI(handler, rect, property, label, includeChildren);
        }

        public static object CreatePropertyHandler(PropertyDrawer drawer)
        {
            if (!IsAvailable)
            {
                return null;
            }

            object handler = PropertyHandler_Create();
            PropertyHandler_SetPropertyDrawer(handler, drawer);
            return handler;
        }
    }
}
#endif