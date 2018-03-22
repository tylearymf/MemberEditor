#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="RectDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
	using UnityEngine;
	using UnityEditor;
	using Sirenix.OdinInspector.Editor;
	using Sirenix.Utilities.Editor;

    /// <summary>
    /// Rect property drawer.
    /// </summary>
    [OdinDrawer]
	public class RectDrawer : OdinValueDrawer<Rect>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(IPropertyValueEntry<Rect> entry, GUIContent label)
		{
			if (label == null)
			{
				this.DrawValues(entry);
			}
			else
			{
				var isVisible = entry.Property.Context.GetPersistent<bool>(this, "IsVisible", GeneralDrawerConfig.Instance.ExpandFoldoutByDefault);

				isVisible.Value = SirenixEditorGUI.Foldout(isVisible.Value, label);
				if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(entry, this), isVisible.Value))
				{
					EditorGUI.indentLevel++;
					this.DrawValues(entry);
					EditorGUI.indentLevel--;
				}
				SirenixEditorGUI.EndFadeGroup();
			}

		}

		private void DrawValues(IPropertyValueEntry<Rect> entry)
		{
			var value = entry.SmartValue;

			// Position
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUI.BeginChangeCheck();
				value.position = SirenixEditorFields.VectorPrefixLabel("Position", value.position);
				if (EditorGUI.EndChangeCheck())
				{
					entry.SmartValue = value;
				}

				GUIHelper.PushIndentLevel(0);
				GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
				entry.Property.Children[0].Draw(GUIHelper.TempContent("X"));
				entry.Property.Children[1].Draw(GUIHelper.TempContent("Y"));
				GUIHelper.PopLabelWidth();
				GUIHelper.PopIndentLevel();
			}
			EditorGUILayout.EndHorizontal();

			// Size
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUI.BeginChangeCheck();
				value.size = SirenixEditorFields.VectorPrefixLabel("Size", value.size);
				if (EditorGUI.EndChangeCheck())
				{
					entry.SmartValue = value;
				}

				GUIHelper.PushIndentLevel(0);
				GUIHelper.PushLabelWidth(SirenixEditorFields.SingleLetterStructLabelWidth);
				entry.Property.Children[2].Draw(GUIHelper.TempContent("X"));
				entry.Property.Children[3].Draw(GUIHelper.TempContent("Y"));
				GUIHelper.PopLabelWidth();
				GUIHelper.PopIndentLevel();
			}
			EditorGUILayout.EndHorizontal();

		}
	}
}
#endif