#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DrawWithUnityBaseDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Base class to derive from for value drawers that merely wish to cause a value to be drawn by Unity.
    /// </summary>
    public abstract class DrawWithUnityBaseDrawer<T> : OdinValueDrawer<T>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, GUIContent label)
        {
            FieldInfo fieldInfo;
            SerializedProperty unityProperty = entry.Property.Tree.GetUnityPropertyForPath(entry.Property.Path, out fieldInfo);

            if (unityProperty == null)
            {
                SirenixEditorGUI.ErrorMessageBox("Could not get a Unity SerializedProperty for the property '" + entry.Property.NiceName + "' of type '" + entry.TypeOfValue.GetNiceName() + "' at path '" + entry.Property.Path + "'.");
                return;
            }

            if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
            {
                var targetObjects = unityProperty.serializedObject.targetObjects;

                for (int i = 0; i < targetObjects.Length; i++)
                {
                    EmittedScriptableObject<T> target = (EmittedScriptableObject<T>)targetObjects[i];
                    target.SetValue(entry.Values[i]);
                }

                unityProperty.serializedObject.Update();
                unityProperty = unityProperty.serializedObject.FindProperty(unityProperty.propertyPath);
            }

            if (label == null)
            {
                label = GUIHelper.TempContent("");
            }

            EditorGUILayout.PropertyField(unityProperty, label, true);

            if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
            {
                unityProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                var targetObjects = unityProperty.serializedObject.targetObjects;

                for (int i = 0; i < targetObjects.Length; i++)
                {
                    EmittedScriptableObject<T> target = (EmittedScriptableObject<T>)targetObjects[i];
                    entry.Values[i] = target.GetValue();
                }
            }
        }
    }
}
#endif