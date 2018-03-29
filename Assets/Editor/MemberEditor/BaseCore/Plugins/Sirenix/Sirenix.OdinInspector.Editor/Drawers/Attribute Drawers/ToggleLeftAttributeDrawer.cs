#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ToggleLeftAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="ToggleLeftAttribute"/>.
    /// </summary>
    /// <seealso cref="ToggleLeftAttribute"/>
    [OdinDrawer]
    public sealed class ToggleLeftAttributeDrawer : OdinAttributeDrawer<ToggleLeftAttribute, bool>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<bool> entry, ToggleLeftAttribute attribute, GUIContent label)
        {
            entry.SmartValue = label == null ?
               EditorGUILayout.ToggleLeft(GUIContent.none, entry.SmartValue) :
               EditorGUILayout.ToggleLeft(label, entry.SmartValue);
        }
    }
}
#endif