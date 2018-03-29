#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="IndentAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="IndentAttribute"/>.
    /// </summary>
	/// <seealso cref="IndentAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.5, 0, 0)]
    public sealed class IndentAttributeDrawer : OdinAttributeDrawer<IndentAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, IndentAttribute attribute, GUIContent label)
        {
            GUIHelper.PushIndentLevel(EditorGUI.indentLevel + attribute.IndentLevel);
            this.CallNextDrawer(property, label);
            GUIHelper.PopIndentLevel();
        }
    }
}
#endif