#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ToggleAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Utilities.Editor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Draws properties marked with <see cref="ToggleAttribute"/>.
    /// </summary>
    /// <seealso cref="ToggleAttribute"/>
    [OdinDrawer]
    public class ToggleAttributeDrawer : OdinAttributeDrawer<ToggleAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, ToggleAttribute attribute, GUIContent label)
        {
            var toggleProperty = property.Children.Get(attribute.ToggleMemberName);

            if (toggleProperty == null)
            {
                SirenixEditorGUI.ErrorMessageBox(attribute.ToggleMemberName + " is not a member of " + property.Info.MemberInfo.GetNiceName() + ".");
            }
            else if (toggleProperty.ValueEntry.TypeOfValue != typeof(bool))
            {
                SirenixEditorGUI.ErrorMessageBox(attribute.ToggleMemberName + " on " + property.Info.MemberInfo.GetNiceName() + "  must be a boolean.");
            }
            else
            {
                bool isEnabled = (bool)toggleProperty.ValueEntry.WeakSmartValue;
                var isVisible = property.Context.GetPersistent(this, "isVisible", false);

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
                        isVisible.Value = false;
                    }
                }

                bool prev = isVisible.Value;
                bool visibleBuffer = isVisible.Value;
                if (SirenixEditorGUI.BeginToggleGroup(UniqueDrawerKey.Create(property, this), ref isEnabled, ref visibleBuffer, label != null ? label.text : property.Info.MemberInfo.GetNiceName()))
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
                SirenixEditorGUI.EndToggleGroup();

                isVisible.Value = visibleBuffer;
                if (openGroup != null && prev != isVisible.Value && isVisible.Value)
                {
                    openGroup.Value = property.Path;
                }

                toggleProperty.ValueEntry.WeakSmartValue = isEnabled;
            }
        }
    }
}
#endif