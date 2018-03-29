#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideInInlineEditorsAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="HideInInlineEditorsAttribute"/>.
    /// </summary>
    [OdinDrawer]
    [DrawerPriority(1000, 0, 0)]
    public sealed class HideInInlineEditorsAttributeDrawer : OdinAttributeDrawer<HideInInlineEditorsAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, HideInInlineEditorsAttribute attribute, GUIContent label)
        {
            if (InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth > 0)
                return;

            this.CallNextDrawer(property, label);
        }
    }
}

#endif