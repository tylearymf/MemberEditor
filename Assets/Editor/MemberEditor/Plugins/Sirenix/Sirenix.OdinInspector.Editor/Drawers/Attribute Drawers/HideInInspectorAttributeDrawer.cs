#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideInInspectorAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="HideInInspector"/>
    /// </summary>
    /// <seealso cref="HideInInspector"/>
    /// <seealso cref="ShowIfAttribute"/>
    /// <seealso cref="HideIfAttribute"/>
    /// <seealso cref="ReadOnlyAttribute"/>
    /// <seealso cref="EnableIfAttribute"/>
    /// <seealso cref="DisableIfAttribute"/>
    /// <seealso cref="DisableInEditorModeAttribute"/>
    /// <seealso cref="DisableInPlayModeAttribute"/>
    [OdinDrawer]
    [DrawerPriority(1000, 0, 0)]
    public sealed class HideInInspectorAttributeDrawer : OdinAttributeDrawer<HideInInspector>
    {
        /// <summary>
        /// Does not draw the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, HideInInspector attribute, GUIContent label)
        {
            // Draw if we are a collection element
            if (property.ValueEntry != null && property.ValueEntry.ValueCategory != PropertyValueCategory.Member)
            {
                this.CallNextDrawer(property, label);
                return;
            }

            // Draw if we are in a reference
            var isInReference = property.Context.GetGlobal("is_in_reference", false);

            if (isInReference.Value)
            {
                this.CallNextDrawer(property, label);
            }
        }
    }
}
#endif