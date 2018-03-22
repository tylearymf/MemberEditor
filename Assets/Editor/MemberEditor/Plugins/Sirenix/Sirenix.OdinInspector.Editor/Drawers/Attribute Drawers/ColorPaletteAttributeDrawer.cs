#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ColorPaletteAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Utilities;
    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
	using System.Linq;

	/// <summary>
	/// Not yet documented.
	/// </summary>
	[OdinDrawer]
    [DrawerPriority(DrawerPriorityLevel.AttributePriority)]
    public sealed class ColorPaletteAttributeDrawer : OdinAttributeDrawer<ColorPaletteAttribute, Color>
    {
		private class PaletteContext
		{
			public int PaletteIndex;
			public string CurrentName;
			public LocalPersistentContext<string> PersistentName;
			public bool ShowAlpha;
			public string[] Names;
            public StringMemberHelper NameGetter;
		}

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<Color> entry, ColorPaletteAttribute attribute, GUIContent label)
        {
            SirenixEditorGUI.BeginIndentedHorizontal();
            {
                var hideLabel = label == null;
                if (hideLabel == false)
                {
                    GUILayout.Label(label, GUILayoutOptions.Width(EditorGUIUtility.labelWidth - 4).ExpandWidth(false));
                }
                else
                {
                    GUILayout.Space(5);
                }

				PropertyContext<PaletteContext> contextBuffer;
				if (entry.Context.Get(this, "ColorPalette", out contextBuffer))
				{
					contextBuffer.Value = new PaletteContext()
					{
						PaletteIndex = 0,
						CurrentName = attribute.PaletteName,
						ShowAlpha = attribute.ShowAlpha,
						Names = ColorPaletteManager.Instance.ColorPalettes.Select(x => x.Name).ToArray(),
					};

					// Get persistent context value
					if (attribute.PaletteName == null)
					{
						contextBuffer.Value.PersistentName = entry.Context.GetPersistent<string>(this, "ColorPaletteName", null);
						var list = contextBuffer.Value.Names.ToList();
						contextBuffer.Value.CurrentName = contextBuffer.Value.PersistentName.Value;

						if (contextBuffer.Value.CurrentName != null && list.Contains(contextBuffer.Value.CurrentName))
						{
							contextBuffer.Value.PaletteIndex = list.IndexOf(contextBuffer.Value.CurrentName);
						}
                    }
                    else
                    {
                        contextBuffer.Value.NameGetter = new StringMemberHelper(entry.ParentType, attribute.PaletteName);
                    }
				}

				var context = contextBuffer.Value;

                //var colorPaletDropDown = entry.Context.Get(this, "colorPalette", 0);
                //var currentName = entry.Context.Get(this, "currentName", attribute.PaletteName);
                //var showAlpha = entry.Context.Get(this, "showAlpha", attribute.ShowAlpha);
                //var names = ColorPaletteManager.Instance.GetColorPaletteNames();

                ColorPalette colorPalette;
                var rect = EditorGUILayout.BeginHorizontal();
                {
                    rect.x -= 3;
                    rect.width = 25;

                    entry.SmartValue = SirenixEditorGUI.DrawColorField(rect, entry.SmartValue, false, context.ShowAlpha);
                    bool openInEditorShown = false;
                    GUILayout.Space(28);
                    SirenixEditorGUI.BeginInlineBox();
                    {
                        if (attribute.PaletteName == null || ColorPaletteManager.Instance.ShowPaletteName)
                        {
                            SirenixEditorGUI.BeginBoxHeader();
                            {
                                if (attribute.PaletteName == null)
                                {
                                    var newValue = EditorGUILayout.Popup(context.PaletteIndex, context.Names, GUILayoutOptions.ExpandWidth(true));
                                    if (context.PaletteIndex != newValue)
                                    {
                                        context.PaletteIndex = newValue;
										context.CurrentName = context.Names[newValue];
										context.PersistentName.Value = context.CurrentName;
                                        GUIHelper.RemoveFocusControl();
                                    }
                                }
                                else
                                {
                                    GUILayout.Label(context.CurrentName);
                                    GUILayout.FlexibleSpace();
                                }
                                openInEditorShown = true;
                                if (SirenixEditorGUI.IconButton(EditorIcons.SettingsCog))
                                {
                                    ColorPaletteManager.Instance.OpenInEditor();
                                }
                            }
                            SirenixEditorGUI.EndBoxHeader();
                        }

                        if (attribute.PaletteName == null)
                        {
                            colorPalette = ColorPaletteManager.Instance.ColorPalettes.FirstOrDefault(x => x.Name == context.Names[context.PaletteIndex]);
                        }
                        else
                        {
                            colorPalette = ColorPaletteManager.Instance.ColorPalettes.FirstOrDefault(x => x.Name == context.NameGetter.GetString(entry));
                        }

                        if (colorPalette == null)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                if (attribute.PaletteName != null)
                                {
                                    if (GUILayout.Button("Create color palette: " + context.NameGetter.GetString(entry)))
                                    {
                                        ColorPaletteManager.Instance.ColorPalettes.Add(new ColorPalette() { Name = context.NameGetter.GetString(entry) });
                                        ColorPaletteManager.Instance.OpenInEditor();
                                    }
                                }
                            }
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            context.CurrentName = colorPalette.Name;
                            context.ShowAlpha = attribute.ShowAlpha && colorPalette.ShowAlpha;
                            if (openInEditorShown == false)
                            {
                                GUILayout.BeginHorizontal();
                            }
                            var color = entry.SmartValue;
                            var stretch = ColorPaletteManager.Instance.StretchPalette;
                            var size = ColorPaletteManager.Instance.SwatchSize;
                            var margin = ColorPaletteManager.Instance.SwatchSpacing;
                            if (DrawColorPaletteColorPicker(entry, colorPalette, ref color, colorPalette.ShowAlpha, stretch, size, 20, margin))
                            {
                                entry.SmartValue = color;
                                //entry.ApplyChanges();
                            }
                            if (openInEditorShown == false)
                            {
                                GUILayout.Space(4);
                                if (SirenixEditorGUI.IconButton(EditorIcons.SettingsCog))
                                {
                                    ColorPaletteManager.Instance.OpenInEditor();
                                }
                                GUILayout.EndHorizontal();
                            }
                        }
                    }
                    SirenixEditorGUI.EndInlineBox();
                }
                EditorGUILayout.EndHorizontal();
            }

            SirenixEditorGUI.EndIndentedHorizontal();
        }

        internal static bool DrawColorPaletteColorPicker(object key, ColorPalette colorPalette, ref Color color, bool drawAlpha, bool stretchPalette, float width = 20, float height = 20, float margin = 0)
        {
            bool result = false;

            var rect = SirenixEditorGUI.BeginHorizontalAutoScrollBox(key, GUILayoutOptions.ExpandWidth(true).ExpandHeight(false));
            {
                if (stretchPalette)
                {
                    rect.width -= margin * colorPalette.Colors.Count - margin;
                    width = Mathf.Max(width, rect.width / colorPalette.Colors.Count);
                }

                bool isMouseDown = Event.current.type == EventType.MouseDown;
                var innerRect = GUILayoutUtility.GetRect((width + margin) * colorPalette.Colors.Count, height, GUIStyle.none);
                float spacing = width + margin;
                var cellRect = innerRect;
                cellRect.width = width;

                for (int i = 0; i < colorPalette.Colors.Count; i++)
                {
                    cellRect.x = spacing * i;

                    if (drawAlpha)
                    {
                        EditorGUIUtility.DrawColorSwatch(cellRect, colorPalette.Colors[i]);
                    }
                    else
                    {
                        var c = colorPalette.Colors[i];
                        c.a = 1;
                        SirenixEditorGUI.DrawSolidRect(cellRect, c);
                    }

                    if (isMouseDown && cellRect.Contains(Event.current.mousePosition))
                    {
                        color = colorPalette.Colors[i];
                        result = true;
                        GUI.changed = true;
                        Event.current.Use();
                    }
                }
            }
            SirenixEditorGUI.EndHorizontalAutoScrollBox();
            return result;
        }
    }
}
#endif