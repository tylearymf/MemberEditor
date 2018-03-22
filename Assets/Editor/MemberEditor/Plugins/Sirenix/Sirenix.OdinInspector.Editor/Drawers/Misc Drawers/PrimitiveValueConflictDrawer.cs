#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PrimitiveValueConflictDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Evaluates all strings, enums and primitive types and ensures EditorGUI.showMixedValue is true if there are any value conflicts in the current selection.
    /// </summary>
    [OdinDrawer]
    [DrawerPriority(0.5, 0, 0)]
    [AllowGUIEnabledForReadonly]
    public sealed class PrimitiveValueConflictDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems
    {
        /// <summary>
        /// Sets the drawer to only be evaluated on primitive types, strings and enums.
        /// </summary>
        public override bool CanDrawTypeFilter(Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type.IsEnum || type.IsValueType;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, GUIContent label)
        {
            // showMixedValue will not be applied to all child properties.
            if (entry.ValueState == PropertyValueState.PrimitiveValueConflict)
            {
                GUI.changed = false;

                EditorGUI.showMixedValue = true;
                this.CallNextDrawer(entry.Property, label);

                if (GUI.changed)
                {
                    // Just to be sure, force the change for all targets
                    for (int i = 0; i < entry.ValueCount; i++)
                    {
                        entry.Values[i] = entry.SmartValue;
                    }
                }
            }
            else
            {
                EditorGUI.showMixedValue = false;
                this.CallNextDrawer(entry.Property, label);
            }
        }

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            if (property.ValueEntry.ValueState == PropertyValueState.PrimitiveValueConflict)
            {
                var tree = property.Tree;

                if (typeof(UnityEngine.Object).IsAssignableFrom(tree.TargetType))
                {
                    for (int i = 0; i < tree.WeakTargets.Count; i++)
                    {
                        object value = property.ValueEntry.WeakValues[i];
                        string valueString = value == null ? "null" : value.ToString();
                        string contentString = "Resolve value conflict with.../" + ((UnityEngine.Object)tree.WeakTargets[i]).name + " (" + valueString + ")";

                        genericMenu.AddItem(new GUIContent(contentString), false, () =>
                        {
                            property.Tree.DelayActionUntilRepaint(() =>
                            {
                                for (int j = 0; j < property.ValueEntry.WeakValues.Count; j++)
                                {
                                    property.ValueEntry.WeakValues[j] = value;
                                }
                            });
                        });
                    }
                }
            }
        }
    }
}
#endif