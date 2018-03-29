#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ShowIfAttributeDrawer.cs" company="Sirenix IVS">
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
    /// Draws properties marked with <see cref="ShowIfAttribute"/>.
    /// </summary>
    [OdinDrawer]
    [DrawerPriority(100, 0, 0)]
    public sealed class ShowIfAttributeDrawer : OdinAttributeDrawer<ShowIfAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, ShowIfAttribute attribute, GUIContent label)
        {
            bool result;
            string errorMessage;

            IfAttributesHelper.HandleIfAttributesCondition(this, property, attribute.MemberName, attribute.Value, out result, out errorMessage);

            if (errorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(errorMessage);
                this.CallNextDrawer(property, label);
            }
            else
            {
                if (attribute.Animate)
                {
                    if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(property, this), result))
                    {
                        this.CallNextDrawer(property, label);
                    }
                    SirenixEditorGUI.EndFadeGroup();
                }
                else
                {
                    if (result)
                    {
                        this.CallNextDrawer(property, label);
                    }
                }
            }
        }
    }
}
#endif