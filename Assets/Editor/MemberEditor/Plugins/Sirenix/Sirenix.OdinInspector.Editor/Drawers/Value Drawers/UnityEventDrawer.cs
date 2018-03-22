#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UnityEventDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Events;
    using System;
    using System.Collections;
    using System.Reflection;
    using Sirenix.Utilities;

    /// <summary>
    /// Unity event drawer.
    /// </summary>
    [OdinDrawer]
    [DrawerPriority(0, 0, 1)]
    public sealed class UnityEventDrawer<T> : UnityPropertyDrawer<UnityEditorInternal.UnityEventDrawer, T> where T : UnityEventBase
    {
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, GUIContent label)
        {
            var eventDrawer = entry.Context.Get(this, "event_drawer", (UnityEditorInternal.UnityEventDrawer)null);

            if (eventDrawer.Value == null)
            {
                eventDrawer.Value = new UnityEditorInternal.UnityEventDrawer();
                this.drawer = eventDrawer.Value;

                if (UnityPropertyHandlerUtility.IsAvailable)
                {
                    this.propertyHandler = UnityPropertyHandlerUtility.CreatePropertyHandler(this.drawer);
                }
            }

            FieldInfo fieldInfo;
            SerializedProperty unityProperty = entry.Property.Tree.GetUnityPropertyForPath(entry.Property.Path, out fieldInfo);

            if (unityProperty == null)
            {
                if (UnityVersion.IsVersionOrGreater(2017, 1))
                {
                    this.CallNextDrawer(entry, label);
                    return;
                }
                else if (!typeof(T).IsDefined<SerializableAttribute>())
                {
                    SirenixEditorGUI.ErrorMessageBox("You have likely forgotten to mark your custom UnityEvent class '" + typeof(T).GetNiceName() + "' with the [Serializable] attribute! Could not get a Unity SerializedProperty for the property '" + entry.Property.NiceName + "' of type '" + entry.TypeOfValue.GetNiceName() + "' at path '" + entry.Property.Path + "'.");
                    return;
                }
            }

            base.DrawPropertyLayout(entry, label);
        }

        //private static readonly FieldInfo InternalFieldInfoFieldInfo = typeof(TDrawer).GetField("m_FieldInfo", Flags.InstanceAnyVisibility);
        //private static readonly ValueSetter<TDrawer, FieldInfo> SetFieldInfo;

        ////private static readonly Action<SerializedProperty> ResetUnityEventDrawerState;

        ////static UnityEventDrawer()
        ////{
        ////    try
        ////    {
        ////        var unityEventDrawer = typeof(Editor).Assembly.GetType("UnityEditorInternal.UnityEventDrawer");
        ////        var scriptAttributeUtility = typeof(Editor).Assembly.GetType("UnityEditor.ScriptAttributeUtility");
        ////        var propertyHandler = typeof(Editor).Assembly.GetType("UnityEditor.PropertyHandler");

        ////        var scriptAttributeUtility_getHandlerMethod = scriptAttributeUtility.GetMethod("GetHandler", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        ////        var propertyHandler_propertyDrawerField = propertyHandler.GetField("m_PropertyDrawer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        ////        var unityEventDrawer_statesField = unityEventDrawer.GetField("m_States", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        ////        if (scriptAttributeUtility_getHandlerMethod == null
        ////            || propertyHandler_propertyDrawerField == null
        ////            || unityEventDrawer_statesField == null)
        ////        {
        ////            throw new Exception();
        ////        }

        ////        ResetUnityEventDrawerState = (prop) =>
        ////        {
        ////            object handler = scriptAttributeUtility_getHandlerMethod.Invoke(null, new object[] { prop });
        ////            object drawer = propertyHandler_propertyDrawerField.GetValue(handler);

        ////            if (drawer != null && drawer.GetType().Name == "UnityEventDrawer")
        ////            {
        ////                IDictionary states = (IDictionary)unityEventDrawer_statesField.GetValue(drawer);
        ////                states.Remove(prop.propertyPath);
        ////            }
        ////        };
        ////    }
        ////    catch
        ////    {
        ////        Debug.LogWarning("Could not fetch internal Unity classes and members required for UnityEventDrawer to fix an internal Unity caching bug that causes havoc with UnityEvents rendered in InlineEditors that have been expanded and closed several times. Internal Unity members or types must have changed in the current version of Unity.");
        ////    }
        ////}

        ///// <summary>
        ///// Draws the property.
        ///// </summary>
        //protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, GUIContent label)
        //{
        //    var unityProperty = entry.Property.Tree.GetUnityPropertyForPath(entry.Property.Path);

        //    //if (unityProperty == null)
        //    //{
        //    //    SirenixEditorGUI.ErrorMessageBox("Could not create an alias UnityEditor.SerializedProperty for the property '" + entry.Property.Name + "'.");
        //    //    return;
        //    //}

        //    if (unityProperty == null/* || unityProperty.serializedObject.targetObject is EmittedScriptableObject<T> || entry.Property.Tree.UnitySerializedObject == null/* || (typeof(Component).IsAssignableFrom(entry.Property.Tree.UnitySerializedObject.targetObject.GetType()) == false)*/)
        //    {
        //        SirenixEditorGUI.WarningMessageBox("Cannot properly draw UnityEvents for properties that are not directly serialized by Unity from a component. To get the classic Unity event appearance, please turn " + entry.Property.Name + " into a public field, or a private field with the [SerializedField] attribute on, and ensure that it is defined on a component.");
        //        this.CallNextDrawer(entry.Property, label);
        //    }
        //    else
        //    {
        //        var unityDrawer = entry.Property.Context.Get(this, "unity_drawer", (UnityEditorInternal.UnityEventDrawer)null);

        //        if (unityDrawer.Value == null)
        //        {
        //            unityDrawer.Value = new UnityEditorInternal.UnityEventDrawer();
        //        }

        //        if (unityProperty.serializedObject.targetObject is EmittedScriptableObject)
        //        {
        //        }

        //        float height = unityDrawer.Value.GetPropertyHeight(unityProperty, label);
        //        Rect position = EditorGUILayout.GetControlRect(false, height);
        //        unityDrawer.Value.OnGUI(position, unityProperty, label);

        //        if (unityProperty.serializedObject.targetObject is EmittedScriptableObject)
        //        {
        //        }
        //    }
        //}
    }
}
#endif