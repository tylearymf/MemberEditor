#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ByteDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
	using Sirenix.Utilities.Editor;
	using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Byte property drawer.
    /// </summary>
    [OdinDrawer]
    public sealed class ByteDrawer : OdinValueDrawer<byte>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<byte> entry, GUIContent label)
        {
			int value = SirenixEditorFields.IntField(label, entry.SmartValue);

            if (value < byte.MinValue)
            {
                value = byte.MinValue;
            }
            else if (value > byte.MaxValue)
            {
                value = byte.MaxValue;
            }

            entry.SmartValue = (byte)value;
        }
    }
}
#endif