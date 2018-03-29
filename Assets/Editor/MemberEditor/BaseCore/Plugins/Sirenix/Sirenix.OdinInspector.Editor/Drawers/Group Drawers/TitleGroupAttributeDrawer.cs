#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TitleGroupAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
	using Sirenix.Utilities.Editor;
	using UnityEditor;
	using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="TitleGroupAttribute"/>.
    /// </summary>
    /// <seealso cref="TitleGroupAttribute"/>
    /// <seealso cref="TitleAttribute"/>
    [OdinDrawer]
	public sealed class TitleGroupAttributeDrawer : OdinGroupDrawer<TitleGroupAttribute>
	{
		private class TitleContext
		{
			public StringMemberHelper TitleHelper;
			public StringMemberHelper SubtitleHelper;
		}

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyGroupLayout(InspectorProperty property, TitleGroupAttribute attribute, GUIContent label)
		{
			PropertyContext<TitleContext> context;
			if (property.Context.Get(this, "Title", out context))
			{
				context.Value = new TitleContext()
				{
					TitleHelper = new StringMemberHelper(property.ParentType, attribute.GroupName),
					SubtitleHelper = new StringMemberHelper(property.ParentType, attribute.Subtitle),
				};
			}

            if (property != property.Tree.GetRootProperty(0))
            {
                EditorGUILayout.Space();
            }

            SirenixEditorGUI.Title(context.Value.TitleHelper.GetString(property), context.Value.SubtitleHelper.GetString(property), (TextAlignment)(int)attribute.Alignment, attribute.HorizontalLine, attribute.BoldTitle);

			GUIHelper.PushIndentLevel(EditorGUI.indentLevel + (attribute.Indent ? 1 : 0));
			for (int i = 0; i < property.Children.Count; i++)
			{
				property.Children[i].Draw();
			}
			GUIHelper.PopIndentLevel();
		}
	}
}
#endif