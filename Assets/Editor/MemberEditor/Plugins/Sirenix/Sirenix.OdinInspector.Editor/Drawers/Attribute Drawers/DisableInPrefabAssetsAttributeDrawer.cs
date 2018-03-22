#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideInInlineEditorsAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    /// <summary>
    /// Draws properties marked with <see cref="DisableInPrefabAssetsAttribute"/>.
    /// </summary>
    [OdinDrawer]
    [DrawerPriority(1000, 0, 0)]
    public sealed class DisableInPrefabAssetsAttributeDrawer : OdinAttributeDrawer<DisableInPrefabAssetsAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, DisableInPrefabAssetsAttribute attribute, GUIContent label)
        {
            var unityObjectTarget = property.Tree.WeakTargets[0] as UnityEngine.Object;

            if (unityObjectTarget == null)
            {
                this.CallNextDrawer(property, label);
                return;
            }

            PropertyContext<bool> disable;

            if (property.Context.Get(this, "disable", out disable))
            {
                var type = PrefabUtility.GetPrefabType(unityObjectTarget);
                disable.Value = type == PrefabType.Prefab || type == PrefabType.ModelPrefab;
            }

            if (disable.Value)
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