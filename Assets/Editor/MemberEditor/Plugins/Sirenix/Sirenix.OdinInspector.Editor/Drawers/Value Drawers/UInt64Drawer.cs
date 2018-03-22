#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UInt64Drawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
	using Sirenix.Utilities.Editor;
	using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Ulong property drawer.
    /// </summary>
    [OdinDrawer]
    public sealed class UInt64Drawer : OdinValueDrawer<ulong>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<ulong> entry, GUIContent label)
        {
			long value = SirenixEditorFields.LongField(label, (long)entry.SmartValue);

            if (value < 0)
            {
                value = 0;
            }

            entry.SmartValue = (ulong)value;
        }
    }
}
#endif