#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MultiLineDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws string properties marked with <see cref="MultilineAttribute"/>.
	/// This drawer only works for string fields, unlike <see cref="MultiLinePropertyAttributeDrawer"/>.
    /// </summary>
	/// <seealso cref="MultilineAttribute"/>
	/// <seealso cref="MultiLineAttributeDrawer"/>
	/// <seealso cref="DisplayAsStringAttribute"/>
	/// <seealso cref="InfoBoxAttribute"/>
	/// <seealso cref="DetailedInfoBoxAttribute"/>
    [OdinDrawer]
    public sealed class MultiLineAttributeDrawer : OdinAttributeDrawer<MultilineAttribute, string>
    {
        /// <summary>
        /// GUI call type.
        /// </summary>
        protected override GUICallType GUICallType { get { return GUICallType.Rect; } }

        /// <summary>
        /// Gets the height of the property.
        /// </summary>
        protected override float GetRectHeight(IPropertyValueEntry<string> entry, MultilineAttribute attribute, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * attribute.lines;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyRect(Rect position, IPropertyValueEntry<string> entry, MultilineAttribute attribute, GUIContent label)
        {
            position.height -= 2;
            if (label == null)
            {
                entry.SmartValue = EditorGUI.TextArea(position, entry.SmartValue, EditorStyles.textArea);
            }
            else
            {
                var controlID = GUIUtility.GetControlID(label, FocusType.Keyboard, position);
                var areaPosition = EditorGUI.PrefixLabel(position, controlID, label, EditorStyles.label);
                entry.SmartValue = EditorGUI.TextArea(areaPosition, entry.SmartValue, EditorStyles.textArea);
            }
        }
    }
}
#endif