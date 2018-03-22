#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="CompositeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using Sirenix.Utilities;

    /// <summary>
    /// Drawer for composite properties.
    /// </summary>
    [DrawerPriority(0, 0, 0)]
    public sealed class CompositeDrawer : OdinDrawer
    {
        private class Context
        {
            public LocalPersistentContext<bool> IsVisisble;
            public InlinePropertyAttribute IsInlineProperty;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyImplementation(InspectorProperty property, GUIContent label)
        {
            if (label == null)
            {
                SirenixEditorGUI.BeginIndentedVertical();
                for (int i = 0; i < property.Children.Count; i++)
                {
                    InspectorUtilities.DrawProperty(property.Children[i]);
                }
                SirenixEditorGUI.EndIndentedVertical();
            }
            else
            {
                Context context;
                if (property.Context.Get<Context>(this, "context", out context))
                {
                    context.IsVisisble = property.Context.GetPersistent(this, "IsVisible", SirenixEditorGUI.ExpandFoldoutByDefault);
                    context.IsInlineProperty =
                        property.ValueEntry.TypeOfValue.GetAttribute<InlinePropertyAttribute>() ??
                        property.Info.GetAttribute<InlinePropertyAttribute>();
                }

                if (context.IsInlineProperty != null)
                {
                    var outerRect = EditorGUILayout.BeginHorizontal();
                    {
                        if (Event.current.type == EventType.Repaint)
                        {
                            outerRect.y += 1;
                            EditorGUI.PrefixLabel(outerRect, label);
                        }

                        GUILayout.Space(EditorGUIUtility.labelWidth);
                        GUILayout.BeginVertical();
                        {
                            if (context.IsInlineProperty.LabelWidth > 0) GUIHelper.PushLabelWidth(context.IsInlineProperty.LabelWidth);
                            GUIHelper.PushIndentLevel(0);
                            for (int i = 0; i < property.Children.Count; i++)
                            {
                                InspectorUtilities.DrawProperty(property.Children[i]);
                            }
                            GUIHelper.PopIndentLevel();
                            if (context.IsInlineProperty.LabelWidth > 0) GUIHelper.PopLabelWidth();

                        }
                        GUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    context.IsVisisble.Value = SirenixEditorGUI.Foldout(context.IsVisisble.Value, label);
                    if (SirenixEditorGUI.BeginFadeGroup(context, context.IsVisisble.Value))
                    {
                        EditorGUI.indentLevel++;
                        for (int i = 0; i < property.Children.Count; i++)
                        {
                            InspectorUtilities.DrawProperty(property.Children[i]);
                        }
                        EditorGUI.indentLevel--;
                    }
                    SirenixEditorGUI.EndFadeGroup();
                }
            }
        }
    }
}
#endif