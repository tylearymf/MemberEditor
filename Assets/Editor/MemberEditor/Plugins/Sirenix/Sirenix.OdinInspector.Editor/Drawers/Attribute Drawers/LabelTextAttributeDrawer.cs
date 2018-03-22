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
    /// Draws properties marked with <see cref="LabelTextAttribute"/>.
    /// Creates a new GUIContent, with the provided label text, before calling further down in the drawer chain.
    /// </summary>
    /// <seealso cref="LabelTextAttribute"/>
    /// <seealso cref="HideLabelAttribute"/>
    /// <seealso cref="TooltipAttribute"/>
    /// <seealso cref="LabelWidthAttribute"/>
    /// <seealso cref="TitleAttribute"/>
    /// <seealso cref="HeaderAttribute"/>
    /// <seealso cref="GUIColorAttribute"/>
    [OdinDrawer]
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public sealed class LabelTextAttributeDrawer : OdinAttributeDrawer<LabelTextAttribute>
    {
        /// <summary>
        /// Draws the attribute.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, LabelTextAttribute attribute, GUIContent label)
        {
            var context = property.Context.Get<StringMemberHelper>(this, "StringContext", (StringMemberHelper)null);
            if (context.Value == null)
            {
                context.Value = new StringMemberHelper(property.ParentType, attribute.Text);
            }

            if (context.Value.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(context.Value.ErrorMessage);
            }

            if (label == null)
            {
                property.Label = null;
            }
            else
            {
                property.Label = label;
                property.Label.text = context.Value.GetString(property);
            }

            this.CallNextDrawer(property, property.Label);
        }
    }
}
#endif