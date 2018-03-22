#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ReferencePathConflictDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws properties with a <see cref="PropertyValueState.ReferencePathConflict"/> set.
    /// </summary>
    [OdinDrawer]
    [DrawerPriority(0.5, 0, 0)]
    [AllowGUIEnabledForReadonly]
    public sealed class ReferencePathConflictDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems where T : class
    {
        private static readonly bool IsUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(typeof(T));

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, GUIContent label)
        {
            if (entry.ValueState == PropertyValueState.ReferencePathConflict)
            {
                if (IsUnityObject)
                {
                    bool prev = EditorGUI.showMixedValue;
                    EditorGUI.showMixedValue = true;
                    this.CallNextDrawer(entry, label);
                    EditorGUI.showMixedValue = prev;
                }
                else
                {
                    EditorGUILayout.LabelField(label, new GUIContent("Reference path conflict... (right-click to resolve)"));
                }
            }
            else
            {
                this.CallNextDrawer(entry.Property, label);
            }
        }

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            if (property.ValueEntry.ValueState == PropertyValueState.ReferencePathConflict)
            {
                var tree = property.Tree;

                if (typeof(UnityEngine.Object).IsAssignableFrom(tree.TargetType))
                {
                    for (int i = 0; i < tree.WeakTargets.Count; i++)
                    {
                        object value = property.ValueEntry.WeakValues[i];
                        string valueString = value == null ? "null" : value.GetType().GetNiceName();
                        string path;

                        tree.ObjectIsReferenced(value, out path);

                        string contentString = "Resolve reference path conflict with.../" + ((UnityEngine.Object)tree.WeakTargets[i]).name + " -> " + path + " (" + valueString + ")";

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