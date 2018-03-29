#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="BooleanDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.Utilities;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Bool property drawer.
    /// </summary>
    [OdinDrawer]
    public sealed class BooleanDrawer : OdinValueDrawer<bool>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<bool> entry, GUIContent label)
        {
            // EditorGUI.Toggle will always return false, regardless of the given input when EditorGUI.showMixedValue is set to true.
            // Hence BeginChangeCheck.
            EditorGUI.BeginChangeCheck();

            bool value = label == null ?
                EditorGUILayout.Toggle(entry.SmartValue, GUILayoutOptions.ExpandWidth(false).Width(22)) :
                EditorGUILayout.Toggle(label, entry.SmartValue);

            if (EditorGUI.EndChangeCheck())
            {
                entry.SmartValue = value;
            }
        }
    }
}
#endif