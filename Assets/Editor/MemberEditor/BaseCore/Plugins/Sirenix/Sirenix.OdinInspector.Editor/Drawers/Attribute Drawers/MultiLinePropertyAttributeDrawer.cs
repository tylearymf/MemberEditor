#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MultiLinePropertyDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws string properties marked with <see cref="MultiLinePropertyAttribute"/>.
	/// This drawer works for both string field and properties, unlike <see cref="MultiLineAttributeDrawer"/>.
    /// </summary>
	/// <seealso cref="MultiLinePropertyAttribute"/>
	/// <seealso cref="MultilineAttribute"/>
	/// <seealso cref="DisplayAsStringAttribute"/>
	/// <seealso cref="InfoBoxAttribute"/>
	/// <seealso cref="DetailedInfoBoxAttribute"/>
    [OdinDrawer]
    public sealed class MultiLinePropertyAttributeDrawer : OdinAttributeDrawer<MultiLinePropertyAttribute, string>
    {
        /// <summary>
        /// GUI call type.
        /// </summary>
        protected override GUICallType GUICallType { get { return GUICallType.Rect; } }

        /// <summary>
        /// Gets the height of the property in the inspector.
        /// </summary>
        protected override float GetRectHeight(IPropertyValueEntry<string> entry, MultiLinePropertyAttribute attribute, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * attribute.Lines;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyRect(Rect position, IPropertyValueEntry<string> entry, MultiLinePropertyAttribute attribute, GUIContent label)
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