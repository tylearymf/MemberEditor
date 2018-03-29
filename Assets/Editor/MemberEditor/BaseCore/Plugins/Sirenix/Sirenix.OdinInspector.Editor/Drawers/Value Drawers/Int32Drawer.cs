#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="Int32Drawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
	using Sirenix.Utilities.Editor;
	using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Int property drawer.
    /// </summary>
    [OdinDrawer]
    public sealed class Int32Drawer : OdinValueDrawer<int>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<int> entry, GUIContent label)
        {
			entry.SmartValue = SirenixEditorFields.IntField(label, entry.SmartValue);
        }
    }
}
#endif