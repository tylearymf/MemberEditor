#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DecimalDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
	using Sirenix.Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Decimal property drawer.
    /// </summary>
    [OdinDrawer]
    public sealed class DecimalDrawer : OdinValueDrawer<decimal>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<decimal> entry, GUIContent label)
        {
			entry.SmartValue = SirenixEditorFields.DecimalField(label, entry.SmartValue);
        }
    }
}
#endif