#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideInInlineEditorsAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Draws all properties marked with the <see cref="DisableInInlineEditorsAttribute"/> attribute.
    /// </summary>
    [OdinDrawer]
    [DrawerPriority(1000, 0, 0)]
    public sealed class DisableInInlineEditorsAttributeDrawer : OdinAttributeDrawer<DisableInInlineEditorsAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, DisableInInlineEditorsAttribute attribute, GUIContent label)
        {
            if (InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth > 0)
            {
                GUIHelper.PushGUIEnabled(false);
                this.CallNextDrawer(property, label);
                GUIHelper.PopGUIEnabled();
            }
            else
            {

                this.CallNextDrawer(property, label);
            }
        }
    }
}

#endif