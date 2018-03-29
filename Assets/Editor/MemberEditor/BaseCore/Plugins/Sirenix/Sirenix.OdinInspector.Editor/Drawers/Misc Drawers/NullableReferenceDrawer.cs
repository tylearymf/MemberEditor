#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="NullReferenceDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using System;
    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using Sirenix.Utilities;

    /// <summary>
    /// Draws all nullable reference types, with an object field.
    /// </summary>
    [OdinDrawer]
    [AllowGUIEnabledForReadonly]
    [DrawerPriority(0, 0, 2000)]
    public sealed class NullableReferenceDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems where T : class
    {
        public override bool CanDrawTypeFilter(Type type)
        {
            return type != typeof(string) && !typeof(UnityEngine.Object).IsAssignableFrom(type);
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, GUIContent label)
        {
            var isToggled = entry.Property.Context.GetPersistent<bool>(this, "Toggled", SirenixEditorGUI.ExpandFoldoutByDefault);

            if (entry.ValueState == PropertyValueState.NullReference)
            {
                GUIHelper.PushGUIEnabled(GUI.enabled && entry.IsEditable);

                try
                {
                    if (typeof(UnityEngine.Object).IsAssignableFrom(entry.TypeOfValue))
                    {
                        entry.WeakSmartValue = label == null ?
                            EditorGUILayout.ObjectField((UnityEngine.Object)entry.WeakSmartValue, entry.TypeOfValue, entry.Property.Info.GetAttribute<AssetsOnlyAttribute>() == null) :
                            EditorGUILayout.ObjectField(label, (UnityEngine.Object)entry.WeakSmartValue, entry.TypeOfValue, entry.Property.Info.GetAttribute<AssetsOnlyAttribute>() == null);
                    }
                    else
                    {
                        if (entry.SerializationBackend == SerializationBackend.Unity && entry.IsEditable && Event.current.type == EventType.Layout)
                        {
                            Debug.LogError("Unity-backed value is null. This should already be fixed by the FixUnityNullDrawer!");
                        }
                        else
                        {
                            bool drawWithBox = ShouldDrawReferenceObjectPicker(entry);
                            bool contextValue = isToggled.Value;

                            if (drawWithBox)
                            {
                                SirenixEditorGUI.BeginBox();
                                SirenixEditorGUI.BeginBoxHeader();
                                {
                                    DrawObjectField(entry, label, ref contextValue);
                                }
                                SirenixEditorGUI.EndBoxHeader();
                                SirenixEditorGUI.EndBox();
                            }
                            else
                            {
                                DrawObjectField(entry, label, ref contextValue, false);
                            }

                            isToggled.Value = contextValue;
                        }
                    }
                }
                finally
                {
                    GUIHelper.PopGUIEnabled();
                }
            }
            else
            {
                if (ShouldDrawReferenceObjectPicker(entry))
                {
                    SirenixEditorGUI.BeginBox();
                    SirenixEditorGUI.BeginBoxHeader();
                    {
                        GUIHelper.PushGUIEnabled(GUI.enabled && entry.IsEditable);
                        bool contextValue = isToggled.Value;
                        DrawObjectField(entry, label, ref contextValue);
                        isToggled.Value = contextValue;
                        GUIHelper.PopGUIEnabled();
                    }
                    SirenixEditorGUI.EndBoxHeader();
                    if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(entry, this), isToggled.Value))
                    {
                        this.CallNextDrawer(entry.Property, null);
                    }
                    SirenixEditorGUI.EndFadeGroup();
                    SirenixEditorGUI.EndBox();
                }
                else
                {
                    this.CallNextDrawer(entry.Property, label);
                }
            }

            var objectPicker = ObjectPicker.GetObjectPicker(entry, entry.BaseValueType);
            if (objectPicker.IsReadyToClaim)
            {
                var obj = objectPicker.ClaimObject();
                entry.Property.Tree.DelayActionUntilRepaint(() => entry.WeakSmartValue = obj);
            }
        }

        private static void DrawObjectField(IPropertyValueEntry<T> entry, GUIContent label, ref bool isToggled, bool showToggle = true)
        {
            var prev = EditorGUI.showMixedValue;

            if (entry.ValueState == PropertyValueState.ReferenceValueConflict)
            {
                EditorGUI.showMixedValue = true;
            }

            object newValue;
            GUI.changed = false;
            if (showToggle == false)
            {
#pragma warning disable 0618 // Type or member is obsolete
                newValue = SirenixEditorGUI.ObjectField(entry, entry.BaseValueType, label, entry.SmartValue, entry.Property.Info.GetAttribute<AssetsOnlyAttribute>() == null);
#pragma warning restore 0618 // Type or member is obsolete
            }
            else if (label == null)
            {
                EditorGUI.indentLevel++;
#pragma warning disable 0618 // Type or member is obsolete
                newValue = SirenixEditorGUI.ObjectField(entry, entry.BaseValueType, label, entry.SmartValue, entry.Property.Info.GetAttribute<AssetsOnlyAttribute>() == null);
#pragma warning restore 0618 // Type or member is obsolete
                EditorGUI.indentLevel--;
            }
            else
            {
                newValue = SirenixEditorFields.PolymorphicObjectField(GUIHelper.TempContent("   " + label.text, label.tooltip), entry.SmartValue, entry.BaseValueType, entry.Property.Info.GetAttribute<AssetsOnlyAttribute>() == null);
            }
            if (GUI.changed)
            {
                //entry.WeakSmartValue = newValue;
                entry.Property.Tree.DelayActionUntilRepaint(() => entry.WeakSmartValue = newValue);
            }
            if (showToggle)
            {
                isToggled = SirenixEditorGUI.Foldout(GUILayoutUtility.GetLastRect(), isToggled, GUIContent.none);
            }

            EditorGUI.showMixedValue = prev;
        }

        private static bool ShouldDrawReferenceObjectPicker(IPropertyValueEntry<T> entry)
        {
            return entry.SerializationBackend != SerializationBackend.Unity
                && !entry.BaseValueType.IsValueType
                && entry.BaseValueType != typeof(string)
                && !entry.ValueIsWeakList
                && !entry.BaseValueType.IsArray
                && entry.IsEditable
                && (!(typeof(UnityEngine.Object).IsAssignableFrom(entry.TypeOfValue) && !entry.BaseValueType.IsInterface))
                && !entry.BaseValueType.InheritsFrom(typeof(System.Collections.IDictionary))
                && entry.Property.Info.GetAttribute<HideReferenceObjectPickerAttribute>() == null;
        }

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            var entry = property.ValueEntry as IPropertyValueEntry<T>;
            bool isChangeable = property.ValueEntry.SerializationBackend != SerializationBackend.Unity
                && !entry.BaseValueType.IsValueType
                && entry.BaseValueType != typeof(string);

            if (isChangeable)
            {
                if (entry.IsEditable)
                {
                    var objectPicker = ObjectPicker.GetObjectPicker(entry, entry.BaseValueType);
                    var rect = entry.Property.LastDrawnValueRect;
                    rect.position = GUIUtility.GUIToScreenPoint(rect.position);
                    rect.height = 20;
                    genericMenu.AddItem(new GUIContent("Change Type"), false, () =>
                    {
                        objectPicker.ShowObjectPicker(false, rect);
                    });
                }
                else
                {
                    genericMenu.AddDisabledItem(new GUIContent("Change Type"));
                }
            }
        }
    }
}
#endif