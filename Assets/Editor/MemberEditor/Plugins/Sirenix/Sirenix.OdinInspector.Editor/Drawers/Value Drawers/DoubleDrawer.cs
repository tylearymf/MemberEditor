#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DoubleDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
	using Sirenix.Utilities.Editor;
	using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Double property drawer.
    /// </summary>
    [OdinDrawer]
    public sealed class DoubleDrawer : OdinValueDrawer<double>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<double> entry, GUIContent label)
        {
			entry.SmartValue = SirenixEditorFields.DoubleField(label, entry.SmartValue);
        }
    }
}
#endif