#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HeaderDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="HeaderAttribute"/>.
    /// </summary>
	/// <seealso cref="HeaderAttribute"/>
	/// <seealso cref="TitleAttribute"/>
	/// <seealso cref="HideLabelAttribute"/>
	/// <seealso cref="LabelTextAttribute"/>
	/// <seealso cref="SpaceAttribute"/>
    [OdinDrawer]
    [DrawerPriority(1, 0, 0)]
    public sealed class HeaderAttributeDrawer : OdinAttributeDrawer<HeaderAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, HeaderAttribute attribute, GUIContent label)
        {
            // Don't draw for collection elements
            if (property.ValueEntry != null)
            {
                var category = property.ValueEntry.ValueCategory;

                if (category != PropertyValueCategory.Member)
                {
                    this.CallNextDrawer(property, label);
                    return;
                }
            }

            if (property != property.Tree.GetRootProperty(0))
            {
                EditorGUILayout.Space();
            }

            var headerContext = property.Context.Get<StringMemberHelper>(this, "Header", (StringMemberHelper)null);
            if (headerContext.Value == null)
            {
                headerContext.Value = new StringMemberHelper(property.ParentType, attribute.header);
            }

            EditorGUILayout.LabelField(headerContext.Value.GetString(property), EditorStyles.boldLabel);
            this.CallNextDrawer(property, label);
        }
    }
}
#endif