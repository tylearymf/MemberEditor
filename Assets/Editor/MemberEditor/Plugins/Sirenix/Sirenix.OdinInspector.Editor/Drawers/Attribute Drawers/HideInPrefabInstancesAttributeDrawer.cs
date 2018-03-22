#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HideInPrefabInstancesAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor.Drawers
{
    using UnityEditor;
    using UnityEngine;
    /// <summary>
    /// Draws properties marked with <see cref="HideInPrefabInstancesAttribute"/>.
    /// </summary>
    [OdinDrawer]
    [DrawerPriority(1000, 0, 0)]
    public sealed class HideInPrefabInstancesAttributeDrawer : OdinAttributeDrawer<HideInPrefabInstancesAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, HideInPrefabInstancesAttribute attribute, GUIContent label)
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
                    type == PrefabType.ModelPrefabInstance ||
                    type == PrefabType.PrefabInstance;
            }

            if (hide.Value == false)
            {
                this.CallNextDrawer(property, label);
            }
        }
    }
}

#endif