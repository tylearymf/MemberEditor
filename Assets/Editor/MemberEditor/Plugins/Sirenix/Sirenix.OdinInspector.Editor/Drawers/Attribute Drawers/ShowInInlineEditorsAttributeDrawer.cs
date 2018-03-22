#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ShowInInlineEditorsAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="ShowInInlineEditorsAttribute"/>.
    /// </summary>
    [OdinDrawer]
    [DrawerPriority(1000, 0, 0)]
    public sealed class ShowInInlineEditorsAttributeDrawer : OdinAttributeDrawer<ShowInInlineEditorsAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, ShowInInlineEditorsAttribute attribute, GUIContent label)
        {
            if (InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth == 0)
                return;

            this.CallNextDrawer(property, label);
        }
    }
}
#endif