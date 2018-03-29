#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="HorizontalGroupAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Utilities;
    using Utilities.Editor;
    using UnityEngine;
    using System.Linq;
    using System;
    using UnityEditor;

    /// <summary>
    /// Drawer for the <see cref="HorizontalGroupAttribute"/>
    /// </summary>
    /// <seealso cref="HorizontalGroupAttribute"/>
    [OdinDrawer]
    public class HorizontalGroupAttributeDrawer : OdinGroupDrawer<HorizontalGroupAttribute>
    {
        private class Context
        {
            public float[] Widths;
            public float[] MinWidths;
            public float[] MaxWidths;
            public float[] LabelWidths;
            public float TotalWidth;
            public Vector2[] Margins;
            public Vector2[] Paddings;
            public StringMemberHelper TitleHelper;
            public int ContainsPercentageWidth = 0;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyGroupLayout(InspectorProperty property, HorizontalGroupAttribute attribute, GUIContent label)
        {
            var context = property.Context.Get(this, "Context", (Context)null);

            if (context.Value == null)
            {
                context.Value = new Context();
                if (attribute.Title != null)
                {
                    context.Value.TitleHelper = new StringMemberHelper(property.ParentType, attribute.Title);
                }

                context.Value.Widths = new float[property.Children.Count];
                context.Value.MinWidths = new float[property.Children.Count];
                context.Value.MaxWidths = new float[property.Children.Count];
                context.Value.LabelWidths = new float[property.Children.Count];
                context.Value.Margins = new Vector2[property.Children.Count];
                context.Value.Paddings = new Vector2[property.Children.Count];

                float percentageAllocated = 0;
                for (int i = 0; i < property.Children.Count; i++)
                {
                    var child = property.Children[i];
                    var attr = child.Children.Recurse()
                                .Append(child)
                                .SelectMany(a => a.Info.GetAttributes<HorizontalGroupAttribute>())
                                .FirstOrDefault(x => x.GroupID == attribute.GroupID);

                    if (attr == null)
                    {
                        context.Value.Widths[i] = -1;
                    }
                    else
                    {
                        context.Value.Widths[i] = attr.Width;
                        context.Value.MinWidths[i] = attr.MinWidth;
                        context.Value.MaxWidths[i] = attr.MaxWidth;
                        context.Value.LabelWidths[i] = attr.LabelWidth;

                        if (attr.Width > 0 && attr.Width < 1)
                        {
                            context.Value.ContainsPercentageWidth++;
                            percentageAllocated += attr.Width;

                            // If we allocate 100% there is no way to resize the window down.
                            // In those cases, we convert the attribute to adjust itself automatically and Unity will ensure that 
                            // that it reaches the 100% for us.
                            if (percentageAllocated >= 0.97)
                            {
                                percentageAllocated -= attr.Width;
                                context.Value.Widths[i] = 0;
                                attr.Width = 0;
                            }
                        }

                        if (attr.MinWidth > 0 && attr.MinWidth <= 1)
                        {
                            context.Value.ContainsPercentageWidth++;

                            percentageAllocated += attr.MinWidth;
                            // Same thing for MinWidth.
                            if (percentageAllocated >= 0.97)
                            {
                                percentageAllocated -= attr.MinWidth;
                                context.Value.MinWidths[i] = 0;
                                attr.MinWidth = 0;
                            }
                        }

                        context.Value.Margins[i] = new Vector2(attr.MarginLeft, attr.MarginRight);
                        context.Value.Paddings[i] = new Vector2(attr.PaddingLeft, attr.PaddingRight);
                    }
                }
            }

            if (context.Value.TitleHelper != null)
            {
                if (context.Value.TitleHelper.ErrorMessage != null)
                {
                    SirenixEditorGUI.ErrorMessageBox(context.Value.TitleHelper.ErrorMessage);
                }
                else
                {
                    SirenixEditorGUI.Title(context.Value.TitleHelper.GetString(property), null, TextAlignment.Left, false);
                }
            }

            SirenixEditorGUI.BeginIndentedHorizontal(GUILayoutOptions.ExpandWidth(false));
            {
                if (Event.current.type == EventType.Repaint)
                {
                    var newWidth = GUIHelper.GetCurrentLayoutRect().width;

                    if (context.Value.TotalWidth != newWidth)
                    {
                        GUIHelper.RequestRepaint();
                    }

                    context.Value.TotalWidth = newWidth;
                }

                for (int i = 0; i < property.Children.Count; i++)
                {
                    float width, minWidth, maxWidth;
                    Vector2 padding, margin;

                    if (context.Value.ContainsPercentageWidth > 1 && context.Value.TotalWidth == 0)
                    {
                        width = 20; // Start small and expand next frame. Instead of starting to big and slowly getting smaller.
                        minWidth = 0;
                        maxWidth = 0;
                        padding = new Vector2();
                        margin = new Vector2();
                    }
                    else
                    {
                        width = context.Value.Widths[i];
                        minWidth = context.Value.MinWidths[i];
                        maxWidth = context.Value.MaxWidths[i];
                        margin = context.Value.Margins[i];
                        padding = context.Value.Paddings[i];

                        if (padding.x > 0 && padding.x <= 1) padding.x = padding.x * context.Value.TotalWidth;
                        if (padding.y > 0 && padding.y <= 1) padding.y = padding.y * context.Value.TotalWidth;
                        if (margin.x > 0 && margin.x <= 1) margin.x = margin.x * context.Value.TotalWidth;
                        if (margin.y > 0 && margin.y <= 1) margin.y = margin.y * context.Value.TotalWidth;


                        if (width <= 1) width = width * context.Value.TotalWidth;

                        width -= padding.x + padding.y;

                        if (minWidth > 0)
                        {
                            if (minWidth <= 1) minWidth = minWidth * context.Value.TotalWidth;
                            minWidth -= padding.x + padding.y;
                        }

                        if (maxWidth > 0)
                        {
                            if (maxWidth <= 1) maxWidth = maxWidth * context.Value.TotalWidth;
                            maxWidth -= padding.x + padding.y;
                        }
                    }

                    GUILayoutOptions.GUILayoutOptionsInstance options = null;

                    if (minWidth > 0) options = GUILayoutOptions.MinWidth(minWidth);
                    if (maxWidth > 0) options = options == null ? GUILayoutOptions.MaxWidth(maxWidth) : options.MaxWidth(maxWidth);
                    if (options == null) options = GUILayoutOptions.Width(width < 0 ? 0 : width);

                    var prevFieldWidth = EditorGUIUtility.fieldWidth;
                    EditorGUIUtility.fieldWidth = 40;
                    GUILayout.Space(margin.x + padding.x);
                    GUILayout.BeginVertical(options);
                    GUILayout.Space(0);
                    if (attribute.LabelWidth > 0) { GUIHelper.PushLabelWidth(attribute.LabelWidth); }
                    InspectorUtilities.DrawProperty(property.Children[i], property.Children[i].Label);
                    if (attribute.LabelWidth > 0) { GUIHelper.PopLabelWidth(); }
                    GUILayout.Space(0);
                    GUILayout.EndVertical();
                    EditorGUIUtility.fieldWidth = prevFieldWidth;
                    GUILayout.Space(margin.y + padding.y);
                }
            }
            SirenixEditorGUI.EndIndentedHorizontal();
        }
    }
}
#endif