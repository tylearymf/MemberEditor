#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="Int16Drawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
	using Sirenix.Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Short property drawer.
    /// </summary>
    [OdinDrawer]
    public sealed class Int16Drawer : OdinValueDrawer<short>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<short> entry, GUIContent label)
        {
            int value = SirenixEditorFields.IntField(label, entry.SmartValue);

            if (value < short.MinValue)
            {
                value = short.MinValue;
            }
            else if (value > short.MaxValue)
            {
                value = short.MaxValue;
            }

            entry.SmartValue = (short)value;
        }
    }
}
#endif