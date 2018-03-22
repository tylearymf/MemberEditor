#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SpaceAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="SpaceAttribute"/>.
    /// </summary>
    /// <seealso cref="SpaceAttribute"/>
    [OdinDrawer]
    [DrawerPriority(1.1, 0, 0)]
    public sealed class SpaceAttributeDrawer : OdinAttributeDrawer<SpaceAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, SpaceAttribute attribute, GUIContent label)
        {
            // Don't show space for collection elements. We have to do this on the drawer level as it's a Unity attribute.
            if (property.Info.PropertyType == PropertyType.Method || (property.ValueEntry != null && property.ValueEntry.ValueCategory == PropertyValueCategory.Member))
            {
                if (attribute.height == 0)
                {
                    EditorGUILayout.Space();
                }
                else
                {
                    GUILayout.Space(attribute.height);
                }
            }

            this.CallNextDrawer(property, label);
        }
    }
}
#endif