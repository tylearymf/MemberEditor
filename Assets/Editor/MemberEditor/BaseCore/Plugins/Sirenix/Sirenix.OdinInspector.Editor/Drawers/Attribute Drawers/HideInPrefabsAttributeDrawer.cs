#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideInPrefabsAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="HideInPrefabsAttribute"/>.
    /// </summary>
    /// <seealso cref="HideIfAttribute"/>
    /// <seealso cref="EnableIfAttribute"/>
    /// <seealso cref="DisableInEditorModeAttribute"/>
    [OdinDrawer]
    [DrawerPriority(1000, 0, 0)]
    public sealed class HideInPrefabsAttributeDrawer : OdinAttributeDrawer<HideInPrefabsAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, HideInPrefabsAttribute attribute, GUIContent label)
        {
            var unityObjectTarget = property.Tree.WeakTargets[0] as UnityEngine.Object;

            if (unityObjectTarget == null)
            {
                this.CallNextDrawer(property, label);
                return;
            }

            PropertyContext<bool> hide;

            if (property.Context.Get(this, "hide", out hide))
            {
                var type = PrefabUtility.GetPrefabType(unityObjectTarget);
                hide.Value =
                    type == PrefabType.Prefab ||
                    type == PrefabType.PrefabInstance ||
                    type == PrefabType.ModelPrefab ||
                    type == PrefabType.ModelPrefabInstance;
            }

            if (hide.Value == false)
            {
                this.CallNextDrawer(property, label);
            }
        }
    }
}

#endif