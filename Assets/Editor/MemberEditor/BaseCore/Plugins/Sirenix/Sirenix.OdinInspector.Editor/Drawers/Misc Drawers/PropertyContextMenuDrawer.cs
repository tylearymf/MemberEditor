#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyContextMenuDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Serialization;
    using Sirenix.Utilities.Editor;
    using Sirenix.Utilities;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    internal static class PropertyContextMenuDrawerStaticInfo
    {
        public static InspectorProperty HighlightedProperty = null;
    }

    /// <summary>
    /// Opens a context menu for any given property on right click. The context menu is populated by all relevant drawers that implements <see cref="IDefinesGenericMenuItems"/>.
    /// </summary>
    /// <seealso cref="IDefinesGenericMenuItems"/>
    [OdinDrawer]
    [DrawerPriority(95, 0, 0)]
    public sealed class PropertyContextMenuDrawer<T> : OdinValueDrawer<T>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, GUIContent label)
        {
            this.CallNextDrawer(entry, label);

            var property = entry.Property;

            var genericMenu = property.Context.Get(this, "drawContextMenu", (GenericMenu)null);
            var overrideRect = property.Context.GetGlobal("overrideRect", (Rect?)null);

            var rect = overrideRect.Value.HasValue ? overrideRect.Value.Value : property.LastDrawnValueRect;

            var prev = GUI.enabled;
            GUI.enabled = true;
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && rect.Contains(Event.current.mousePosition))
            {
                bool enableContextMenu = true;

                var disableAttr = property.Info.GetAttribute<DisableContextMenuAttribute>();

                if (disableAttr != null)
                {
                    if (entry.ValueCategory == PropertyValueCategory.Member)
                    {
                        enableContextMenu = !disableAttr.DisableForMember;
                    }
                    else
                    {
                        enableContextMenu = !disableAttr.DisableForCollectionElements;
                    }
                }

                if (enableContextMenu)
                {
                    Event.current.Use();

                    GenericMenu menu = new GenericMenu();
                    GUIHelper.RemoveFocusControl();

                    PropertyContextMenuDrawerStaticInfo.HighlightedProperty = property;
                    PopulateGenericMenu(property, menu);

                    property.PopulateGenericMenu(menu);
                    if (menu.GetItemCount() != 0)
                    {
                        genericMenu.Value = menu;
                    }
                }
            }
            GUI.enabled = prev;

            if (PropertyContextMenuDrawerStaticInfo.HighlightedProperty != null && Event.current.rawType == EventType.ContextClick || Event.current.rawType == EventType.MouseDown)
            {
                PropertyContextMenuDrawerStaticInfo.HighlightedProperty = null;
            }

            if (PropertyContextMenuDrawerStaticInfo.HighlightedProperty == property && Event.current.type == EventType.Repaint)
            {
                rect.width = 3;
                rect.x -= 4;
                SirenixEditorGUI.DrawSolidRect(rect, SirenixGUIStyles.HighlightedTextColor);
            }

            if (genericMenu.Value != null && Event.current.type == EventType.Repaint)
            {
                genericMenu.Value.ShowAsContext();
                genericMenu.Value = null;
            }
        }

        private class CopiedItem
        {
            private readonly CopyModes copyMode;
            private readonly object obj;

            public CopiedItem(object obj, CopyModes copyMode)
            {
                this.obj = obj;
                this.copyMode = copyMode;
            }

            public object GetObject()
            {
                return this.obj;
            }

            public CopyModes GetCopyMode()
            {
                return this.copyMode;
            }
        }

        private static void PopulateChangedFromPrefabContext(InspectorProperty property, GenericMenu genericMenu)
        {
            var entry = property.ValueEntry;

            if (entry != null)
            {
                InspectorProperty prefabProperty = null;

                if (property.Tree.PrefabPropertyTree != null)
                {
                    prefabProperty = property.Tree.PrefabPropertyTree.GetPropertyAtPath(property.Path);
                }

                bool active = prefabProperty != null;

                if (entry.ValueChangedFromPrefab)
                {
                    if (active)
                    {
                        genericMenu.AddItem(new GUIContent("Revert to prefab value"), false, () =>
                        {
                            for (int i = 0; i < entry.ValueCount; i++)
                            {
                                property.Tree.RemovePrefabModification(property, i, PrefabModificationType.Value);
                            }

                            if (property.Tree.UnitySerializedObject != null)
                            {
                                property.Tree.UnitySerializedObject.Update();
                            }
                        });
                    }
                    else
                    {
                        genericMenu.AddDisabledItem(new GUIContent("Revert to prefab value (Does not exist on prefab)"));
                    }
                }

                if (entry.ListLengthChangedFromPrefab)
                {
                    if (active)
                    {
                        genericMenu.AddItem(new GUIContent("Revert to prefab list length"), false, () =>
                        {
                            for (int i = 0; i < entry.ValueCount; i++)
                            {
                                property.Tree.RemovePrefabModification(property, i, PrefabModificationType.ListLength);
                            }

                            property.Children.Update();

                            if (property.Tree.UnitySerializedObject != null)
                            {
                                property.Tree.UnitySerializedObject.Update();
                            }
                        });
                    }
                    else
                    {
                        genericMenu.AddDisabledItem(new GUIContent("Revert to prefab list length (Does not exist on prefab)"));
                    }
                }

                if (entry.DictionaryChangedFromPrefab)
                {
                    if (active)
                    {
                        genericMenu.AddItem(new GUIContent("Revert dictionary changes to prefab value"), false, () =>
                        {
                            for (int i = 0; i < entry.ValueCount; i++)
                            {
                                property.Tree.RemovePrefabModification(property, i, PrefabModificationType.Dictionary);
                            }

                            property.Children.Update();

                            if (property.Tree.UnitySerializedObject != null)
                            {
                                property.Tree.UnitySerializedObject.Update();
                            }
                        });
                    }
                    else
                    {
                        genericMenu.AddDisabledItem(new GUIContent("Revert dictionary changes to prefab value (Does not exist on prefab)"));
                    }
                }
            }
        }

        private void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            PopulateChangedFromPrefabContext(property, genericMenu);

            if (genericMenu.GetItemCount() > 0)
            {
                genericMenu.AddSeparator("");
            }
            var objs = property.ValueEntry.WeakValues.FilterCast<object>().Where(x => x != null).ToArray();
            var valueToCopy = (objs == null || objs.Length == 0) ? null : (objs.Length == 1 ? objs[0] : objs);
            bool isUnityObject = property.ValueEntry.BaseValueType.InheritsFrom(typeof(UnityEngine.Object));
            bool hasValue = valueToCopy != null;
            bool canPaste = Clipboard.CanPaste(property.ValueEntry.BaseValueType);
            bool isEditable = property.ValueEntry.IsEditable;
            bool isNullable =
                (property.ValueEntry.BaseValueType.IsClass || property.ValueEntry.BaseValueType.IsInterface) &&
                property.Info.PropertyType == PropertyType.ReferenceType &&
                (property.ValueEntry.SerializationBackend != SerializationBackend.Unity || isUnityObject);

            //if (canPaste && property.ValueEntry.SerializationBackend != SerializationBackend.Unity && Clipboard.CurrentCopyMode == CopyModes.CopyReference)
            //{
            //    canPaste = false;
            //}

            if (canPaste && isEditable)
            {
                genericMenu.AddItem(new GUIContent("Paste"), false, () =>
                {
                    property.Tree.DelayActionUntilRepaint(() =>
                    {
                        for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                        {
                            property.ValueEntry.WeakValues[i] = Clipboard.Paste();
                        }
                        // Apply happens after the action is invoked in repaint
                        //property.ValueEntry.ApplyChanges();
                        GUIHelper.RequestRepaint();
                    });
                });
            }
            else
            {
                genericMenu.AddDisabledItem(new GUIContent("Paste"));
            }

            if (hasValue)
            {
                if (isUnityObject)
                {
                    genericMenu.AddItem(new GUIContent("Copy"), false, () => Clipboard.Copy(valueToCopy, CopyModes.CopyReference));
                }
                else if (property.ValueEntry.TypeOfValue.IsNullableType() == false)
                {
                    genericMenu.AddItem(new GUIContent("Copy"), false, () => Clipboard.Copy(valueToCopy, CopyModes.CopyReference));
                }
                else if (property.ValueEntry.SerializationBackend == SerializationBackend.Unity)
                {
                    genericMenu.AddItem(new GUIContent("Copy"), false, () => Clipboard.Copy(valueToCopy, CopyModes.DeepCopy));
                }
                else
                {
                    genericMenu.AddItem(new GUIContent("Copy"), false, () => Clipboard.Copy(valueToCopy, CopyModes.DeepCopy));
                    genericMenu.AddItem(new GUIContent("Copy Special/Deep Copy (default)"), false, () => Clipboard.Copy(valueToCopy, CopyModes.DeepCopy));
                    genericMenu.AddItem(new GUIContent("Copy Special/Shallow Copy"), false, () => Clipboard.Copy(valueToCopy, CopyModes.ShallowCopy));
                    genericMenu.AddItem(new GUIContent("Copy Special/Copy Reference"), false, () => Clipboard.Copy(valueToCopy, CopyModes.CopyReference));
                }
            }
            else
            {
                genericMenu.AddDisabledItem(new GUIContent("Copy"));
            }

            if (isNullable)
            {
                genericMenu.AddSeparator("");

                if (hasValue && isEditable)
                {
                    genericMenu.AddItem(new GUIContent("Set To Null"), false, () =>
                    {
                        property.Tree.DelayActionUntilRepaint(() =>
                        {
                            for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                            {
                                property.ValueEntry.WeakValues[i] = null;
                            }
                            // Apply happens after the action is invoked in repaint
                            //property.ValueEntry.ApplyChanges();
                            GUIHelper.RequestRepaint();
                        });
                    });
                }
                else
                {
                    genericMenu.AddDisabledItem(new GUIContent("Set To Null"));
                }
            }
        }
    }
}
#endif