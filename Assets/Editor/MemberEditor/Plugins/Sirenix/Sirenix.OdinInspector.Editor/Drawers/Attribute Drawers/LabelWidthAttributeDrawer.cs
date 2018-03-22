#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="LabelTextAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Utilities.Editor;
    using UnityEngine;
    using UnityEditor;

    /// <summary>
    /// Draws properties marked with the <see cref="LabelWidthAttribute"/>.
    /// </summary>
    /// <seealso cref="LabelTextAttribute"/>
    /// <seealso cref="HideLabelAttribute"/>
    /// <seealso cref="LabelWidthAttribute"/>
    /// <seealso cref="TooltipAttribute"/>
    /// <seealso cref="TitleAttribute"/>
    /// <seealso cref="HeaderAttribute"/>
    /// <seealso cref="GUIColorAttribute"/>
    [OdinDrawer]
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public sealed class LabelWidthAttributeDrawer : OdinAttributeDrawer<LabelWidthAttribute>
    {
        /// <summary>
        /// Draws the attribute.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, LabelWidthAttribute attribute, GUIContent label)
        {
            if (attribute.Width < 0)
            {
                GUIHelper.PushLabelWidth(EditorGUIUtility.labelWidth + attribute.Width);
            }
            else
            {
                GUIHelper.PushLabelWidth(attribute.Width);
            }

            this.CallNextDrawer(property, label);
            GUIHelper.PopLabelWidth();
        }
    }
}
#endif