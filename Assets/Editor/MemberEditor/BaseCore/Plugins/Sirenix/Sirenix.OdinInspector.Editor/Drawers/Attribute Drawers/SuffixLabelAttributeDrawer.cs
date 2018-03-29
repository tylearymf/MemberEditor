#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SuffixLabelAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
	using UnityEngine;
	using Sirenix.OdinInspector.Editor;
	using Sirenix.Utilities.Editor;
	using Sirenix.Utilities;

	/// <summary>
	/// Draws properties marked with <see cref="SuffixLabelAttribute"/>.
	/// </summary>
	/// <seealso cref="LabelTextAttribute"/>
	/// <seealso cref="PropertyTooltipAttribute"/>
	/// <seealso cref="InlineButtonAttribute"/>
	/// <seealso cref="CustomValueDrawerAttribute"/>
	[OdinDrawer]
	[AllowGUIEnabledForReadonly]
	[DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
	public sealed class SuffixLabelAttributeDrawer : OdinAttributeDrawer<SuffixLabelAttribute>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(InspectorProperty property, SuffixLabelAttribute attribute, GUIContent label)
		{
			PropertyContext<StringMemberHelper> context;
			if (property.Context.Get(this, "SuffixContext", out context))
			{
				context.Value = new StringMemberHelper(property.FindParent(PropertyValueCategory.Member, true).ParentType, attribute.Label);
			}

			if (attribute.Overlay)
			{
				this.CallNextDrawer(property, label);
				GUIHelper.PushGUIEnabled(true);
				GUI.Label(GUILayoutUtility.GetLastRect().HorizontalPadding(0, 8), context.Value.GetString(property), SirenixGUIStyles.RightAlignedGreyMiniLabel);
				GUIHelper.PopGUIEnabled();
			}
			else
			{
				GUILayout.BeginHorizontal();
				GUILayout.BeginVertical();
				this.CallNextDrawer(property, label);
				GUILayout.EndVertical();
				GUIHelper.PushGUIEnabled(true);
				GUILayout.Label(context.Value.GetString(property), SirenixGUIStyles.RightAlignedGreyMiniLabel, GUILayoutOptions.ExpandWidth(false));
				GUIHelper.PopGUIEnabled();
				GUILayout.EndHorizontal();
			}
		}
	}
}
#endif