#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PreviewFieldAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="PreviewFieldAttribute"/> as a square ObjectField which renders a preview for UnityEngine.Object types.
    /// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values. 
    /// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
    /// </summary>
    [OdinDrawer]
    [AllowGUIEnabledForReadonly]
    public sealed class PreviewFieldAttributeDrawer<T> : OdinAttributeDrawer<PreviewFieldAttribute, T>
        where T : UnityEngine.Object
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, PreviewFieldAttribute attribute, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();

            ObjectFieldAlignment alignment;

            if (attribute.AlignmentHasValue)
            {
                alignment = (ObjectFieldAlignment)attribute.Alignment;
            }
            else
            {
                alignment = GeneralDrawerConfig.Instance.SquareUnityObjectAlignment;
            }

            entry.WeakSmartValue = SirenixEditorFields.UnityPreviewObjectField(
                label,
                entry.WeakSmartValue as UnityEngine.Object,
                entry.BaseValueType,
                entry.Property.Info.GetAttribute<AssetsOnlyAttribute>() == null,
                attribute.Height == 0 ? GeneralDrawerConfig.Instance.SquareUnityObjectFieldHeight : attribute.Height,
                alignment);

            if (EditorGUI.EndChangeCheck())
            {
                entry.Values.ForceMarkDirty();
            }
        }
    }
}
#endif