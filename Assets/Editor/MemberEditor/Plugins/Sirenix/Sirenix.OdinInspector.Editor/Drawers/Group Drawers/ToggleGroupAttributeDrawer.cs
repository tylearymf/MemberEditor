#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ToggleGroupAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using System;
    using System.Reflection;
    using Utilities.Editor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Draws all properties grouped together with the <see cref="ToggleGroupAttribute"/>
    /// </summary>
    /// <seealso cref="ToggleGroupAttribute"/>
    [OdinDrawer]
    public class ToggleGroupAttributeDrawer : OdinGroupDrawer<ToggleGroupAttribute>
    {
        private class ToggleGroupConfig
        {
            public StringMemberHelper TitleHelper;
            public LocalPersistentContext<bool> IsVisible;
            public string ErrorMessage;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyGroupLayout(InspectorProperty property, ToggleGroupAttribute attribute, GUIContent label)
        {
            var toggleProperty = property.Children.Get(attribute.ToggleMemberName);

            var context = property.Context.Get<ToggleGroupConfig>(this, "ToggleGroupConfig", (ToggleGroupConfig)null);

            if (context.Value == null)
            {
                context.Value = new ToggleGroupConfig();
                context.Value.IsVisible = property.Context.GetPersistent<bool>(this, "IsVisible", false);
                if (toggleProperty == null)
                {
                    context.Value.ErrorMessage = "No property or field named " + attribute.ToggleMemberName + " found. Make sure the property is part of the inspector and the group.";
                }
                else
                {
                    context.Value.TitleHelper = new StringMemberHelper(property.ParentType, attribute.ToggleGroupTitle, ref context.Value.ErrorMessage);
                }
            }

            if (context.Value.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(context.Value.ErrorMessage);
            }
            else
            {
                PropertyContext<string> openGroup = null;

                if (attribute.CollapseOthersOnExpand)
                {
                    if (property.Parent == null)
                    {
                        //openGroup = GUIHelper.GetTemporaryContext<PropertyContext<string>>(property.Tree);
                        openGroup = property.Context.Get<string>(this, "OpenGroup", (string)null);
                    }
                    else
                    {
                        var parent = (property.Parent.ValueEntry == null || property.Parent.ValueEntry.ValueCategory == PropertyValueCategory.Member) ? property.Parent : property.Parent.Parent;
                        openGroup = parent.Context.GetGlobal<string>("OpenFoldoutToggleGroup", (string)null);
                    }

                    if (openGroup.Value != null && openGroup.Value != property.Path)
                    {
                        context.Value.IsVisible.Value = false;
                    }
                }

                bool isEnabled = (bool)toggleProperty.ValueEntry.WeakSmartValue;

                string title = context.Value.TitleHelper.GetString(property) ?? attribute.GroupName;

                bool prev = context.Value.IsVisible.Value;
                bool visibleBuffer = context.Value.IsVisible.Value;
                if (SirenixEditorGUI.BeginToggleGroup(UniqueDrawerKey.Create(property, this), ref isEnabled, ref visibleBuffer, title))
                {
                    for (int i = 0; i < property.Children.Count; i++)
                    {
                        var child = property.Children[i];
                        if (child != toggleProperty)
                        {
                            InspectorUtilities.DrawProperty(child);
                        }
                    }
                }
                else
                {
                    // OnValueChanged is not fired if property is not drawn.
                    GUIHelper.BeginDrawToNothing();
                    InspectorUtilities.DrawProperty(toggleProperty);
                    GUIHelper.EndDrawToNothing();
                }
                SirenixEditorGUI.EndToggleGroup();

                context.Value.IsVisible.Value = visibleBuffer;
                if (openGroup != null && prev != context.Value.IsVisible.Value && context.Value.IsVisible.Value)
                {
                    openGroup.Value = property.Path;
                }

                toggleProperty.ValueEntry.WeakSmartValue = isEnabled;

                // Why is this here? Commenting this out for now
                //toggleProperty.ValueEntry.ApplyChanges();
            }
        }
    }
}
#endif