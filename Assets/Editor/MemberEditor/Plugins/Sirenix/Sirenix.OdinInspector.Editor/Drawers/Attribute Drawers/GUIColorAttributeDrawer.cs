#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="GUIColorAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="GUIColorAttribute"/>.
	/// This drawer sets the current GUI color, before calling the next drawer in the chain.
    /// </summary>
	/// <seealso cref="GUIColorAttribute"/>
	/// <seealso cref="LabelTextAttribute"/>
	/// <seealso cref="TitleAttribute"/>
	/// <seealso cref="HeaderAttribute"/>
	/// <seealso cref="ColorPaletteAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.5, 0, 0)]
    public sealed class GUIColorAttributeDrawer : OdinAttributeDrawer<GUIColorAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, GUIColorAttribute attribute, GUIContent label)
        {
            GUIHelper.PushColor(attribute.Color);
            this.CallNextDrawer(property, label);
            GUIHelper.PopColor();
        }
    }
}
#endif