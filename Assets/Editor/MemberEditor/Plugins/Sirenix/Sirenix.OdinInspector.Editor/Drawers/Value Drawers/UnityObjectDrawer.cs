#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UnityObjectDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Utilities.Editor;
    using UnityEngine;
    using UnityEditor;

    /// <summary>
    /// Unity object drawer.
    /// </summary>
    [OdinDrawer]
    [DrawerPriority(0, 0, 0.25)] // Set priority so that vanilla Unity CustomPropertyDrawers can draw UnityObject types by default
    public sealed class UnityObjectDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems
        where T : UnityEngine.Object
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            var drawAsPreview = entry.Context.Get(this, "drawPreview", (bool?)null);
            if (drawAsPreview.Value == null)
            {
                var flags = GeneralDrawerConfig.Instance.SquareUnityObjectEnableFor;

                drawAsPreview.Value = (int)flags != 0 && (
                    (flags & GeneralDrawerConfig.UnityObjectType.Components) != 0 && typeof(Component).IsAssignableFrom(typeof(T)) ||
                    (flags & GeneralDrawerConfig.UnityObjectType.GameObjects) != 0 && typeof(GameObject).IsAssignableFrom(typeof(T)) ||
                    (flags & GeneralDrawerConfig.UnityObjectType.Materials) != 0 && typeof(Material).IsAssignableFrom(typeof(T)) ||
                    (flags & GeneralDrawerConfig.UnityObjectType.Sprites) != 0 && typeof(Sprite).IsAssignableFrom(typeof(T)) ||
                    (flags & GeneralDrawerConfig.UnityObjectType.Textures) != 0 && typeof(Texture).IsAssignableFrom(typeof(T)));

                if (!drawAsPreview.Value.Value && (flags & GeneralDrawerConfig.UnityObjectType.Others) != 0)
                {
                    bool isOther =
                        !typeof(Component).IsAssignableFrom(typeof(T)) &&
                        !typeof(GameObject).IsAssignableFrom(typeof(T)) &&
                        !typeof(Material).IsAssignableFrom(typeof(T)) &&
                        !typeof(Sprite).IsAssignableFrom(typeof(T)) &&
                        !typeof(Texture).IsAssignableFrom(typeof(T));

                    if (isOther)
                    {
                        drawAsPreview.Value = true;
                    }
                }
            }

            if (!drawAsPreview.Value.Value)
            {
                entry.WeakSmartValue = SirenixEditorFields.UnityObjectField(
                    label,
                    entry.WeakSmartValue as UnityEngine.Object,
                    entry.BaseValueType,
                    entry.Property.Info.GetAttribute<AssetsOnlyAttribute>() == null);
            }
            else
            {
                entry.WeakSmartValue = SirenixEditorFields.UnityPreviewObjectField(
                    label,
                    entry.WeakSmartValue as UnityEngine.Object,
                    entry.BaseValueType,
                    entry.Property.Info.GetAttribute<AssetsOnlyAttribute>() == null,
                    GeneralDrawerConfig.Instance.SquareUnityObjectFieldHeight,
                    GeneralDrawerConfig.Instance.SquareUnityObjectAlignment);
            }

            if (EditorGUI.EndChangeCheck())
            {
                entry.Values.ForceMarkDirty();
            }
        }

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            var unityObj = property.ValueEntry.WeakSmartValue as UnityEngine.Object;
            if (unityObj)
            {
                genericMenu.AddItem(new GUIContent("Open in new inspector"), false, () =>
                {
                    GUIHelper.OpenInspectorWindow(unityObj);
                });
            }
            else
            {
                genericMenu.AddDisabledItem(new GUIContent("Open in new inspector"));
            }
        }
    }
}
#endif