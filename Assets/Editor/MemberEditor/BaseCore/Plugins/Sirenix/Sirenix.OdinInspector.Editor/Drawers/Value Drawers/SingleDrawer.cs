#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SingleDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Float property drawer.
    /// </summary>
    [OdinDrawer]
    public sealed class SingleDrawer : OdinValueDrawer<float>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<float> entry, GUIContent label)
        {
            entry.SmartValue = SirenixEditorFields.FloatField(label, entry.SmartValue, GUILayoutOptions.MinWidth(0));
        }
    }
}
#endif