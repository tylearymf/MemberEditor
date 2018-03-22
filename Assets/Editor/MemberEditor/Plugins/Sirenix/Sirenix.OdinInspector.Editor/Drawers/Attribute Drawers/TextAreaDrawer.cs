#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TextAreaAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// TextArea attribute drawer.
    /// </summary>
    [OdinDrawer]
    public class TextAreaAttributeDrawer : OdinAttributeDrawer<TextAreaAttribute, string>
    {
        /// <summary>
        /// Drawing properties using GUICallType.GUILayout and overriding DrawPropertyLayout is the default behavior.
        /// But you can also draw the property the "good" old Unity way, by overriding and implementing
        /// GetRectHeight and DrawPropertyRect. Just make sure to override GUICallType as well and return GUICallType.Rect
        /// </summary>
        protected override GUICallType GUICallType { get { return GUICallType.Rect; } }

        private delegate string ScrollableTextAreaInternalDelegate(Rect position, string text, ref Vector2 scrollPosition, GUIStyle style);

        private static readonly ScrollableTextAreaInternalDelegate EditorGUI_ScrollableTextAreaInternal;

        static TextAreaAttributeDrawer()
        {
            var method = typeof(EditorGUI).GetMethod("ScrollableTextAreaInternal", Flags.StaticAnyVisibility);

            if (method != null)
            {
                EditorGUI_ScrollableTextAreaInternal = (ScrollableTextAreaInternalDelegate)Delegate.CreateDelegate(typeof(ScrollableTextAreaInternalDelegate), method);
            }
        }

        /// <summary>
        /// Draws the property in the Rect provided. This method does not support the GUILayout, and is only called by DrawPropertyImplementation if the GUICallType is set to Rect, which is not the default.
        /// If the GUICallType is set to Rect, both GetRectHeight and DrawPropertyRect needs to be implemented.
        /// If the GUICallType is set to GUILayout, implementing DrawPropertyLayout will suffice.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="entry">The value entry.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="label">The label. This can be null, so make sure your drawer supports that.</param>
        protected override void DrawPropertyRect(Rect position, IPropertyValueEntry<string> entry, TextAreaAttribute attribute, GUIContent label)
        {
            if (EditorGUI_ScrollableTextAreaInternal == null)
            {
                EditorGUI.LabelField(position, label, GUIHelper.TempContent("Cannot draw TextArea because Unity's internal API has changed."));
                return;
            }

            var scrollPosition = entry.Context.Get(this, "scroll_position", Vector2.zero);

            if (label != null)
            {
                Rect labelPosition = position;
                labelPosition.height = 16f;
                position.yMin += labelPosition.height;
                EditorGUI.HandlePrefixLabel(position, labelPosition, label);
            }

            entry.SmartValue = EditorGUI_ScrollableTextAreaInternal(position, entry.SmartValue, ref scrollPosition.Value, EditorStyles.textArea);
        }

        protected override float GetRectHeight(IPropertyValueEntry<string> entry, TextAreaAttribute attribute, GUIContent label)
        {
            // @todo: should this be here?
            TextAreaAttribute textAreaAttribute = attribute;
            string stringValue = entry.SmartValue;
            float num = EditorStyles.textArea.CalcHeight(GUIHelper.TempContent(stringValue), GUIHelper.ContextWidth);
            int num2 = Mathf.CeilToInt(num / 13f);
            num2 = Mathf.Clamp(num2, textAreaAttribute.minLines, textAreaAttribute.maxLines);
            return 32f + (float)((num2 - 1) * 13);
        }
    }
}
#endif