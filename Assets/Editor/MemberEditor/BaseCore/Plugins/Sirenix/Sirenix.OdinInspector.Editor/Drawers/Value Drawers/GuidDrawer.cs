#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GuidDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.Utilities.Editor;
    using System;
    using UnityEngine;

    /// <summary>
    /// Int property drawer.
    /// </summary>
    [OdinDrawer]
    public sealed class GuidDrawer : OdinValueDrawer<Guid>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<Guid> entry, GUIContent label)
        {
            entry.SmartValue = SirenixEditorFields.GuidField(label, entry.SmartValue);
        }
    }
}
#endif