#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ShowDrawerChainAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Show drawer chain attribute drawer.
    /// </summary>
    [DrawerPriority(10000, 0, 0)]
    [OdinDrawer]
    public class ShowDrawerChainAttributeDrawer : OdinAttributeDrawer<ShowDrawerChainAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, ShowDrawerChainAttribute attribute, GUIContent label)
        {
            OdinDrawer[] drawers = DrawerLocator.GetDrawersForProperty(property);

            SirenixEditorGUI.BeginBox("Drawers");
            for (int i = 0; i < drawers.Length; i++)
            {
                bool highlight = drawers[i].GetType().Assembly != typeof(ShowDrawerChainAttributeDrawer).Assembly;

                if (highlight)
                {
                    GUIHelper.PushColor(Color.green);
                }

                EditorGUILayout.LabelField(i + ": " + drawers[i].GetType().GetNiceName());
                var rect = GUILayoutUtility.GetLastRect();

                GUI.Label(rect, DrawerLocator.GetDrawerPriority(drawers[i].GetType()).ToString(), SirenixGUIStyles.RightAlignedGreyMiniLabel);

                if (highlight)
                {
                    GUIHelper.PopColor();
                }
            }
            SirenixEditorGUI.EndBox();

            this.CallNextDrawer(property, label);
        }
    }
}
#endif