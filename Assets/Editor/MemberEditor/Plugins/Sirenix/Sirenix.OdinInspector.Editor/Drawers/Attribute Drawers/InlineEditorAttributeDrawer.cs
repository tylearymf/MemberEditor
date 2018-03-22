#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InlineAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using System;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// Static GUI information reguarding the InlineEditor attribute.
    /// </summary>
    public static class InlineEditorAttributeDrawer
    {
        /// <summary>
        /// Gets a value indicating how many InlineEditors we are currently in.
        /// </summary>
        public static int CurrentInlineEditorDrawDepth { get; internal set; }
    }

    /// <summary>
    /// Draws properties marked with <see cref="InlineEditorAttribute"/>.
    /// </summary>
	/// <seealso cref="InlineEditorAttribute"/>
	/// <seealso cref="DrawWithUnityAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0, 0, 3000)]
    public class InlineEditorAttributeDrawer<T> : OdinAttributeDrawer<InlineEditorAttribute, T> where T : UnityEngine.Object
    {
        private class InlineAttributeContext
        {
            public LocalPersistentContext<bool> Toggled;
            public Editor Editor;
            public Editor PreviewEditor;
            public Rect InlineEditorRect;
            public UnityEngine.Object Target;
            public bool DrawHeader;
            public bool DrawPreview;
            public bool DrawGUI;
            public float MaxHeight;
            public Vector2 ScrollPos;

            public void DestroyEditors()
            {
                if (this.PreviewEditor != this.Editor && this.PreviewEditor != null)
                {
                    UnityEngine.Object.DestroyImmediate(this.PreviewEditor);
                    this.PreviewEditor = null;
                }
                
                if (this.Editor != null)
                {
                    UnityEngine.Object.DestroyImmediate(this.Editor);
                    this.Editor = null;
                }

                Selection.selectionChanged -= this.DestroyEditors;
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, InlineEditorAttribute attribute, GUIContent label)
        {
            var context = entry.Property.Context.Get(this, "InlineAttributeConfig", (InlineAttributeContext)null);
            if (context.Value == null)
            {
                context.Value = new InlineAttributeContext();
                context.Value.Toggled = entry.Context.GetPersistent<bool>(this, "Toggled", attribute.Expanded && InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth == 0);
                context.Value.MaxHeight = attribute.MaxHeight;
                Selection.selectionChanged += context.Value.DestroyEditors;
            }

            var unityObj = (UnityEngine.Object)entry.WeakSmartValue;

            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginBoxHeader();
            {
                GUIHelper.PushGUIEnabled(GUI.enabled && entry.IsEditable);

                bool makeRoomForFoldoutIfNoLabel = false;

                if (entry.ValueState != PropertyValueState.NullReference && label != null)
                {
                    label = GUIHelper.TempContent("   " + label.text, label.image, label.tooltip);
                }
                else if (label == null)
                {
                    makeRoomForFoldoutIfNoLabel = true;
                }
                if (makeRoomForFoldoutIfNoLabel)
                {
                    EditorGUI.indentLevel++;
                }

                GUIUtility.GetControlID(999, FocusType.Passive);

                var prev = EditorGUI.showMixedValue;

                if (entry.ValueState == PropertyValueState.ReferenceValueConflict)
                {
                    EditorGUI.showMixedValue = true;
                }

                bool changed = false;
                EditorGUI.BeginChangeCheck();

                var newValue = SirenixEditorFields.UnityObjectField(
                    label,
                    unityObj,
                    entry.BaseValueType,
                    entry.Property.Info.GetAttribute<AssetsOnlyAttribute>() == null);

                if (EditorGUI.EndChangeCheck())
                {
                    changed = true;
                }

                EditorGUI.showMixedValue = prev;

                if (makeRoomForFoldoutIfNoLabel)
                {
                    EditorGUI.indentLevel--;
                }

                if (entry.ValueState != PropertyValueState.NullReference)
                {
                    var rect = GUILayoutUtility.GetLastRect();
                    context.Value.Toggled.Value = SirenixEditorGUI.Foldout(rect, context.Value.Toggled.Value, GUIContent.none);
                }

                GUIHelper.PopGUIEnabled();

                if (newValue != unityObj || changed)
                {
                    entry.Property.Tree.DelayActionUntilRepaint(() =>
                    {
                        for (int i = 0; i < entry.ValueCount; i++)
                        {
                            entry.WeakValues[i] = newValue;
                        }
                    });
                }

                if (newValue == null && context.Value.Editor != null)
                {
                    context.Value.DestroyEditors();
                }
            }
            SirenixEditorGUI.EndBoxHeader();
            if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(entry.Property, this), context.Value.Toggled.Value))
            {
                if (context.Value.MaxHeight != 0)
                {
                    context.Value.ScrollPos = EditorGUILayout.BeginScrollView(context.Value.ScrollPos, GUILayoutOptions.MaxHeight(200));
                }
                if (entry.ValueState == PropertyValueState.ReferenceValueConflict || entry.ValueState == PropertyValueState.ReferencePathConflict)
                {
                    SirenixEditorGUI.InfoMessageBox("Multi object editing is currently not supported for inline editors.");
                }
                else
                {
                    bool createNewEditor = unityObj != null && (context.Value.Editor == null || context.Value.Target != unityObj || context.Value.Target == null);
                    if (createNewEditor)
                    {
                        context.Value.Target = unityObj;

                        bool isGameObject = unityObj as GameObject;

                        context.Value.DrawHeader = isGameObject ? attribute.DrawHeader : attribute.DrawHeader;
                        context.Value.DrawGUI = isGameObject ? false : attribute.DrawGUI;
                        context.Value.DrawPreview = attribute.DrawPreview || isGameObject && attribute.DrawGUI;

                        if (context.Value.Editor != null)
                        {
                            context.Value.DestroyEditors();
                        }

                        context.Value.Editor = Editor.CreateEditor(context.Value.Target);

                        var component = context.Value.Target as Component;
                        if (component != null)
                        {
                            context.Value.PreviewEditor = Editor.CreateEditor(component.gameObject);
                        }
                        else
                        {
                            context.Value.PreviewEditor = context.Value.Editor;
                        }

                        var materialEditor = context.Value.Editor as MaterialEditor;
                        if (materialEditor != null)
                        {
                            typeof(MaterialEditor).GetProperty("forceVisible", Flags.AllMembers).SetValue(materialEditor, true, null);
                        }
                    }

                    if (context.Value.Editor != null && context.Value.Editor.SafeIsUnityNull() == false)
                    {
                        SaveLayoutSettings();
                        InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth++;
                        try
                        {
                            bool split = context.Value.DrawGUI && context.Value.DrawPreview;
                            if (split)
                            {
                                GUILayout.BeginHorizontal();
                                if (Event.current.type == EventType.Repaint)
                                {
                                    context.Value.InlineEditorRect = GUIHelper.GetCurrentLayoutRect();
                                }
                                GUILayout.BeginVertical();
                            }

                            // Brace for impact
                            if (context.Value.DrawHeader)
                            {
                                var tmp = Event.current.rawType;
                                EditorGUILayout.BeginFadeGroup(0.9999f); // This one fixes some layout issues for reasons beyond me, but locks the input.
                                Event.current.type = tmp; // Lets undo that shall we?
                                GUILayout.Space(0); // Yeah i know. But it removes some unwanted top padding.
                                context.Value.Editor.DrawHeader();
                                GUILayout.Space(1); // This adds the the 1 pixel border clipped from the fade group.
                                EditorGUILayout.EndFadeGroup();
                            }
                            else
                            {
                                // Many of unity editors will not work if the header is not drawn.
                                // So lets draw it anyway. -_-
                                GUIHelper.BeginDrawToNothing();
                                context.Value.Editor.DrawHeader();
                                GUIHelper.EndDrawToNothing();
                            }

                            if (context.Value.DrawGUI)
                            {
                                var prev = GeneralDrawerConfig.Instance.ShowMonoScriptInEditor;
                                try
                                {
                                    GeneralDrawerConfig.Instance.ShowMonoScriptInEditor = false;
                                    GUIHelper.PushHierarchyMode(true);
                                    context.Value.Editor.OnInspectorGUI();
                                    GUIHelper.PushHierarchyMode(false);
                                }
                                finally
                                {
                                    GeneralDrawerConfig.Instance.ShowMonoScriptInEditor = prev;
                                }
                            }

                            if (split)
                            {
                                GUILayout.EndVertical();
                            }

                            if (context.Value.DrawPreview && context.Value.PreviewEditor.HasPreviewGUI())
                            {
                                Rect tmpRect;

                                var size = split ? attribute.PreviewWidth : attribute.PreviewHeight;

                                if (split)
                                {
                                    tmpRect = GUILayoutUtility.GetRect(size + 15, size, GUILayoutOptions.Width(size).Height(size));
                                }
                                else
                                {
                                    tmpRect = GUILayoutUtility.GetRect(0, size, GUILayoutOptions.Height(size).ExpandWidth(true));
                                }

                                if (!split && Event.current.type == EventType.Repaint || context.Value.InlineEditorRect.height < 1)
                                {
                                    context.Value.InlineEditorRect = tmpRect;
                                }

                                var rect = context.Value.InlineEditorRect;
                                if (split)
                                {
                                    rect.xMin += rect.width - size;
                                }
                                rect.height = Mathf.Clamp(rect.height, 30, 1000);
                                rect.width = Mathf.Clamp(rect.width, 30, 1000);
                                var tmp = GUI.enabled;
                                GUI.enabled = true;
                                context.Value.PreviewEditor.DrawPreview(rect);
                                GUI.enabled = tmp;
                            }

                            if (split)
                            {
                                GUILayout.EndHorizontal();
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex is ExitGUIException || ex.InnerException is ExitGUIException)
                            {
                                throw ex;
                            }
                            else
                            {
                                Debug.LogException(ex);
                            }
                        }
                        finally
                        {
                            InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth--;
                            RestoreLayout();
                        }
                    }
                }
                if (context.Value.MaxHeight != 0)
                {
                    EditorGUILayout.EndScrollView();
                }
            }
            else
            {
                if (context.Value.Editor != null)
                {
                    context.Value.DestroyEditors();
                    context.Value.Editor = null;
                }
            }
            SirenixEditorGUI.EndFadeGroup();
            SirenixEditorGUI.EndBox();
        }

        private static Stack<LayoutSettings> layoutSettingsStack = new Stack<LayoutSettings>();

        private static void SaveLayoutSettings()
        {
            layoutSettingsStack.Push(new LayoutSettings()
            {
                Skin = GUI.skin,
                Color = GUI.color,
                ContentColor = GUI.contentColor,
                BackgroundColor = GUI.backgroundColor,
                Enabled = GUI.enabled,
                Changed = GUI.changed,
                IndentLevel = EditorGUI.indentLevel,
                FieldWidth = EditorGUIUtility.fieldWidth,
                LabelWidth = GUIHelper.ActualLabelWidth,
                HierarchyMode = EditorGUIUtility.hierarchyMode,
                WideMode = EditorGUIUtility.wideMode,
            });
        }

        private static void RestoreLayout()
        {
            var settings = layoutSettingsStack.Pop();

            GUI.skin = settings.Skin;
            GUI.color = settings.Color;
            GUI.contentColor = settings.ContentColor;
            GUI.backgroundColor = settings.BackgroundColor;
            GUI.enabled = settings.Enabled;
            GUI.changed = settings.Changed;
            EditorGUI.indentLevel = settings.IndentLevel;
            EditorGUIUtility.fieldWidth = settings.FieldWidth;
            EditorGUIUtility.labelWidth = settings.LabelWidth;
            EditorGUIUtility.hierarchyMode = settings.HierarchyMode;
            EditorGUIUtility.wideMode = settings.WideMode;
        }

        private struct LayoutSettings
        {
            public GUISkin Skin;
            public Color Color;
            public Color ContentColor;
            public Color BackgroundColor;
            public bool Enabled;
            public bool Changed;
            public int IndentLevel;
            public float FieldWidth;
            public float LabelWidth;
            public bool HierarchyMode;
            public bool WideMode;
        }
    }
}
#endif