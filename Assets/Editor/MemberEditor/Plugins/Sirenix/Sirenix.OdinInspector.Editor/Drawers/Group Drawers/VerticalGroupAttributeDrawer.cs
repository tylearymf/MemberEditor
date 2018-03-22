#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="VerticalGroupAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using UnityEngine;

    /// <summary>
    /// Drawer for the <see cref="VerticalGroupAttribute"/>
    /// </summary>
    /// <seealso cref="VerticalGroupAttribute"/>
    [OdinDrawer]
    public class VerticalGroupAttributeDrawer : OdinGroupDrawer<VerticalGroupAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyGroupLayout(InspectorProperty property, VerticalGroupAttribute attribute, GUIContent label)
        {
            GUILayout.BeginVertical();

            if (attribute.PaddingTop != 0)
            {
                GUILayout.Space(attribute.PaddingTop);
            }

            for (int i = 0; i < property.Children.Count; i++)
            {
                property.Children[i].Draw();
            }

            if (attribute.PaddingBottom != 0)
            {
                GUILayout.Space(attribute.PaddingBottom);
            }

            GUILayout.EndVertical();
        }
    }
}
#endif