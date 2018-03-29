#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ReferenceDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using System;
    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using System.Reflection;

    /// <summary>
    /// Draws all reference type properties, which has already been drawn elsewhere. This drawer adds an additional foldout to prevent infinite draw depth.
    /// </summary>
    [OdinDrawer]
    [AllowGUIEnabledForReadonly]
    [DrawerPriority(90, 0, 0)]
    public sealed class ReferenceDrawer<T> : OdinValueDrawer<T> where T : class
    {
        /// <summary>
        /// Prevents the drawer from being applied to UnityEngine.Object references since they are shown as an object field, and is not drawn in-line.
        /// </summary>
        public override bool CanDrawTypeFilter(Type type)
        {
            return
                !typeof(MemberInfo).IsAssignableFrom(type) &&
                !typeof(UnityEngine.Object).IsAssignableFrom(type);
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, GUIContent label)
        {
            if (entry.ValueState == PropertyValueState.Reference)
            {
                var isToggled = entry.Context.GetPersistent(this, "is_Toggled", false);
                var targetProp = entry.Property.Tree.GetPropertyAtPath(entry.TargetReferencePath);

                SirenixEditorGUI.BeginBox();

                SirenixEditorGUI.BeginBoxHeader();
                EditorGUILayout.BeginHorizontal();

                isToggled.Value = label != null ? SirenixEditorGUI.Foldout(isToggled.Value, label)
                                                : SirenixEditorGUI.Foldout(isToggled.Value, GUIHelper.TempContent(""));

                if (targetProp.Parent == null)
                {
                    EditorGUILayout.LabelField("Reference to " + targetProp.Path, SirenixGUIStyles.RightAlignedGreyMiniLabel);
                }
                else
                {
                    EditorGUILayout.LabelField("Reference to " + targetProp.Path, SirenixGUIStyles.RightAlignedGreyMiniLabel);
                }

                EditorGUILayout.EndHorizontal();
                SirenixEditorGUI.EndBoxHeader();

                if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(entry.Property, this), isToggled.Value))
                {
                    //EditorGUI.indentLevel++;
                    //GUIHelper.PushGUIEnabled(true);

                    var isInReference = targetProp.Context.GetGlobal("is_in_reference", false);
                    bool previous = isInReference.Value;
                    isInReference.Value = true;

                    InspectorUtilities.DrawProperty(targetProp);

                    isInReference.Value = previous;

                    //GUIHelper.PopGUIEnabled();
                    //EditorGUI.indentLevel--;
                }
                SirenixEditorGUI.EndFadeGroup();
                SirenixEditorGUI.EndBox();
            }
            else
            {
                this.CallNextDrawer(entry.Property, label);
            }
        }
    }
}
#endif