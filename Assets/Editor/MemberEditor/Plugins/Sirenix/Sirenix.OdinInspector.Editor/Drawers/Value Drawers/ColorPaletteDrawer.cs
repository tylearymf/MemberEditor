#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ColorPaletteDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using System.Collections.Generic;
    using Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Color palette property drawer.
    /// </summary>
    [OdinDrawer]
    internal sealed class ColorPaletteDrawer : OdinValueDrawer<ColorPalette>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<ColorPalette> entry, GUIContent label)
        {
            var isEditing = entry.Property.Context.Get(this, "isEditing", false);
            entry.SmartValue.Name = entry.SmartValue.Name ?? "Palette Name";

            SirenixEditorGUI.BeginBox();
            {
                SirenixEditorGUI.BeginBoxHeader();
                {
                    GUILayout.Label(entry.SmartValue.Name);
                    GUILayout.FlexibleSpace();
                    if (SirenixEditorGUI.IconButton(EditorIcons.Pen))
                    {
                        isEditing.Value = !isEditing.Value;
                    }
                }
                SirenixEditorGUI.EndBoxHeader();

                if (entry.SmartValue.Colors == null)
                {
                    entry.SmartValue.Colors = new List<Color>();
                }

                if (SirenixEditorGUI.BeginFadeGroup(entry.SmartValue, entry, isEditing.Value))
                {
                    this.CallNextDrawer(entry.Property, null);
                }
                SirenixEditorGUI.EndFadeGroup();

                if (SirenixEditorGUI.BeginFadeGroup(entry.SmartValue, entry.SmartValue, isEditing.Value == false))
                {
                    Color col = default(Color);

                    var stretch = ColorPaletteManager.Instance.StretchPalette;
                    var size = ColorPaletteManager.Instance.SwatchSize;
                    var margin = ColorPaletteManager.Instance.SwatchSpacing;
                    ColorPaletteAttributeDrawer.DrawColorPaletteColorPicker(entry, entry.SmartValue, ref col, entry.SmartValue.ShowAlpha, stretch, size, 20, margin);
                }
                SirenixEditorGUI.EndFadeGroup();
            }
            SirenixEditorGUI.EndBox();
        }
    }
}
#endif