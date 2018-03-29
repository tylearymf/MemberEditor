#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="StringDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// String property drawer.
    /// </summary>
    [OdinDrawer]
    public sealed class StringDrawer : OdinValueDrawer<string>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<string> entry, GUIContent label)
        {
            entry.SmartValue = label == null ?
                EditorGUILayout.TextField(entry.SmartValue, EditorStyles.textField) :
                EditorGUILayout.TextField(label, entry.SmartValue, EditorStyles.textField);
        }
    }
}
#endif