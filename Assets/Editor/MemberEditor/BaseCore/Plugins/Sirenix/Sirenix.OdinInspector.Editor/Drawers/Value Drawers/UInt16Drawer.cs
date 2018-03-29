#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UInt16Drawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
	using Sirenix.Utilities.Editor;
	using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Ushort property drawer.
    /// </summary>
    [OdinDrawer]
    public sealed class UInt16Drawer : OdinValueDrawer<ushort>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<ushort> entry, GUIContent label)
        {
			int value = SirenixEditorFields.IntField(label, entry.SmartValue);

            if (value < ushort.MinValue)
            {
                value = ushort.MinValue;
            }
            else if (value > ushort.MaxValue)
            {
                value = ushort.MaxValue;
            }

            entry.SmartValue = (ushort)value;
        }
    }
}
#endif