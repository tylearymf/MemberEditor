#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DisableIfAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="DisableIfAttribute"/>.
    /// </summary>
    /// <seealso cref="DisableIfAttribute"/>
    /// <seealso cref="EnableIfAttribute"/>
    /// <seealso cref="DisableInEditorModeAttribute"/>
    /// <seealso cref="DisableInPlayModeAttribute"/>
    [OdinDrawer]
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public class DisableIfAttributeDrawer : OdinAttributeDrawer<DisableIfAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, DisableIfAttribute attribute, GUIContent label)
        {
            if (GUI.enabled == false)
            {
                this.CallNextDrawer(property, label);
                return;
            }

            bool result;
            string errorMessage;

            IfAttributesHelper.HandleIfAttributesCondition(this, property, attribute.MemberName, attribute.Value, out result, out errorMessage);

            if (errorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(errorMessage);
                this.CallNextDrawer(property, label);
            }
            else if (result)
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