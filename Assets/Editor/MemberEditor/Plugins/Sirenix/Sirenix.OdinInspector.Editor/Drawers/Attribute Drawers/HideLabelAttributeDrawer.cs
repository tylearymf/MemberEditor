#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideLabelAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="HideLabelAttribute"/>.
    /// </summary>
	/// <seealso cref="HideLabelAttribute"/>
	/// <seealso cref="LabelTextAttribute"/>
	/// <seealso cref="TitleAttribute"/>
	/// <seealso cref="HeaderAttribute"/>
	/// <seealso cref="GUIColorAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.1, 0, 0)]
    public sealed class HideLabelAttributeDrawer : OdinAttributeDrawer<HideLabelAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, HideLabelAttribute attribute, GUIContent label)
        {
            // GUILayout.BeginVertical();
            this.CallNextDrawer(property, null);
            // GUILayout.EndVertical();
        }
    }
}
#endif