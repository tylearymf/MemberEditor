#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DisableInPlayModeAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <seealso cref="DisableInPlayModeAttribute"/>.
    /// </summary>
	/// <seealso cref="DisableInPlayModeAttribute"/>
	/// <seealso cref="DisableInEditorModeAttribute"/>
	/// <seealso cref="DisableIfAttribute"/>
	/// <seealso cref="EnableIfAttribute"/>
    [OdinDrawer]
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public class DisableInPlayModeAttributeDrawer : OdinAttributeDrawer<DisableInPlayModeAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
		protected override void DrawPropertyLayout(InspectorProperty property, DisableInPlayModeAttribute attribute, GUIContent label)
        {
            GUIHelper.PushGUIEnabled(!Application.isPlaying && GUI.enabled);
            this.CallNextDrawer(property, label);
            GUIHelper.PopGUIEnabled();
        }
    }
}
#endif