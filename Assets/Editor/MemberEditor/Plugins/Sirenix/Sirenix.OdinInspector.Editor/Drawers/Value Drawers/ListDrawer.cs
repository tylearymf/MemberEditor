#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ListDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using System.Globalization;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Utilities;
    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using Sirenix.Serialization;

    internal static class ListDrawerStaticInfo
    {
        public static InspectorProperty CurrentDraggingPropertyInfo;
        public static DelayedGUIDrawer DelayedGUIDrawer = new DelayedGUIDrawer();
    }

    /// <summary>
    /// Property drawer for <see cref="IList{T}"/>.
    /// </summary>
    [OdinDrawer]
    [AllowGUIEnabledForReadonly]
    public sealed class ListDrawer<TList, TElement> : OdinValueDrawer<TList>, IDefinesGenericMenuItems where TList : IList<TElement>
    {
        private const string CHANGE_ID = "LIST_DRAWER";

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            if (property.ValueEntry.WeakSmartValue == null)
            {
                return;
            }

            bool isReadOnly = false;
            if (property.ValueEntry.TypeOfValue.IsArray == false)
            {
                for (int i = 0; i < property.ValueEntry.ValueCount; i++)
                {
                    var list = (property.ValueEntry.WeakValues[i] as IList<TElement>);
                    if (list != null && list.IsReadOnly)
                    {
                        isReadOnly = true;
                        break;
                    }
                }
            }

            var config = property.Info.GetAttribute<ListDrawerSettingsAttribute>();
            bool isEditable = isReadOnly == false && property.ValueEntry.IsEditable && (config == null || (!config.IsReadOnlyHasValue) || (config.IsReadOnlyHasValue && config.IsReadOnly == false));
            bool pasteElement = isEditable && Clipboard.CanPaste<TElement>();
            bool clearList = isEditable && property.Children.Count > 0;

            //if (genericMenu.GetItemCount() > 0 && (pasteElement || clearList))
            //{
            //    genericMenu.AddSeparator(null);
            //}

            if (pasteElement)
            {
                genericMenu.AddItem(new GUIContent("Paste Element"), false, () =>
                {
                    property.ValueEntry.GetListValueEntryChanger().AddListElement(new object[] { Clipboard.Paste<TElement>() }, "PasteItems");
                    GUIHelper.RequestRepaint();
                });
            }

            if (clearList)
            {
                genericMenu.AddItem(new GUIContent("Clear List"), false, () =>
                {
                    property.ValueEntry.GetListValueEntryChanger().ClearList(CHANGE_ID);
                    GUIHelper.RequestRepaint();
                });
            }
            else
            {
                genericMenu.AddDisabledItem(new GUIContent("Clear List"));
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<TList> entry, GUIContent label)
        {
            var property = entry.Property;
            var infoContext = property.Context.Get(this, "Context", (ListDrawerConfigInfo)null);
            var info = infoContext.Value;

            bool isReadOnly = false;
            if (entry.TypeOfValue.IsArray == false)
            {
                for (int i = 0; i < entry.ValueCount; i++)
                {
                    if ((entry.WeakValues[i] as IList<TElement>).IsReadOnly)
                    {
                        isReadOnly = true;
                        break;
                    }
                }
            }

            if (info == null)
            {
                var customListDrawerOptions = property.Info.GetAttribute<ListDrawerSettingsAttribute>() ?? new ListDrawerSettingsAttribute();
                isReadOnly = entry.IsEditable == false || isReadOnly || customListDrawerOptions.IsReadOnlyHasValue && customListDrawerOptions.IsReadOnly;

                info = infoContext.Value = new ListDrawerConfigInfo()
                {
                    StartIndex = 0,
                    Toggled = entry.Context.GetPersistent<bool>(this, "ListDrawerToggled", customListDrawerOptions.ExpandedHasValue ? customListDrawerOptions.Expanded : GeneralDrawerConfig.Instance.OpenListsByDefault),
                    RemoveAt = -1,
                    label = new GUIContent(label == null || string.IsNullOrEmpty(label.text) ? property.ValueEntry.TypeOfValue.GetNiceName() : label.text, label == null ? string.Empty : label.tooltip),
                    ShowAllWhilePageing = false,
                    EndIndex = 0,
                    CustomListDrawerOptions = customListDrawerOptions,
                    IsReadOnly = isReadOnly,
                    Draggable = !isReadOnly && (!customListDrawerOptions.IsReadOnlyHasValue)
                };

                info.listConfig = GeneralDrawerConfig.Instance;
                info.property = property;

                if (customListDrawerOptions.DraggableHasValue && !customListDrawerOptions.DraggableItems)
                {
                    info.Draggable = false;
                }

                if (info.CustomListDrawerOptions.OnBeginListElementGUI != null)
                {
                    string errorMessage;
                    MemberInfo memberInfo = property.ParentType
                        .FindMember()
                        .IsMethod()
                        .IsNamed(info.CustomListDrawerOptions.OnBeginListElementGUI)
                        .HasParameters<int>()
                        .ReturnsVoid()
                        .GetMember<MethodInfo>(out errorMessage);

                    if (memberInfo == null || errorMessage != null)
                    {
                        Debug.LogError(errorMessage ?? "There should really be an error message here.");
                    }
                    else
                    {
                        info.OnBeginListElementGUI = EmitUtilities.CreateWeakInstanceMethodCaller<int>(memberInfo as MethodInfo);
                    }
                }

                if (info.CustomListDrawerOptions.OnEndListElementGUI != null)
                {
                    string errorMessage;
                    MemberInfo memberInfo = property.ParentType
                        .FindMember()
                        .IsMethod()
                        .IsNamed(info.CustomListDrawerOptions.OnEndListElementGUI)
                        .HasParameters<int>()
                        .ReturnsVoid()
                        .GetMember<MethodInfo>(out errorMessage);

                    if (memberInfo == null || errorMessage != null)
                    {
                        Debug.LogError(errorMessage ?? "There should really be an error message here.");
                    }
                    else
                    {
                        info.OnEndListElementGUI = EmitUtilities.CreateWeakInstanceMethodCaller<int>(memberInfo as MethodInfo);
                    }
                }

                if (info.CustomListDrawerOptions.OnTitleBarGUI != null)
                {
                    string errorMessage;
                    MemberInfo memberInfo = property.ParentType
                        .FindMember()
                        .IsMethod()
                        .IsNamed(info.CustomListDrawerOptions.OnTitleBarGUI)
                        .HasNoParameters()
                        .ReturnsVoid()
                        .GetMember<MethodInfo>(out errorMessage);

                    if (memberInfo == null || errorMessage != null)
                    {
                        Debug.LogError(errorMessage ?? "There should really be an error message here.");
                    }
                    else
                    {
                        info.OnTitleBarGUI = EmitUtilities.CreateWeakInstanceMethodCaller(memberInfo as MethodInfo);
                    }
                }

                if (info.CustomListDrawerOptions.ListElementLabelName != null)
                {
                    string errorMessage;
                    MemberInfo memberInfo = typeof(TElement)
                        .FindMember()
                        .HasNoParameters()
                        .IsNamed(info.CustomListDrawerOptions.ListElementLabelName)
                        .HasReturnType<object>(true)
                        .GetMember(out errorMessage);

                    if (memberInfo == null || errorMessage != null)
                    {
                        Debug.LogError(errorMessage ?? "There should really be an error message here.");
                    }
                    else
                    {
                        string methodSuffix = memberInfo as MethodInfo == null ? "" : "()";
                        info.GetListElementLabelText = DeepReflection.CreateWeakInstanceValueGetter(typeof(TElement), typeof(object), info.CustomListDrawerOptions.ListElementLabelName + methodSuffix);
                    }
                }
            }

            info.listConfig = GeneralDrawerConfig.Instance;
            info.property = property;

            info.ListItemStyle.padding.left = info.Draggable ? 25 : 7;
            info.ListItemStyle.padding.right = info.IsReadOnly ? 4 : 20;

            if (Event.current.type == EventType.Repaint)
            {
                info.DropZoneTopLeft = GUIUtility.GUIToScreenPoint(new Vector2(0, 0));
            }

            info.ListValueChanger = property.ValueEntry.GetListValueEntryChanger();
            info.Count = property.Children.Count;
            info.IsEmpty = property.Children.Count == 0;

            SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
            this.BeginDropZone(info);
            {
                this.DrawToolbar(info);
                if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(property, this), info.Toggled.Value))
                {
                    GUIHelper.PushLabelWidth(EditorGUIUtility.labelWidth - info.ListItemStyle.padding.left);
                    this.DrawItems(info);
                    GUIHelper.PopLabelWidth();
                }
                SirenixEditorGUI.EndFadeGroup();
            }
            this.EndDropZone(info);
            SirenixEditorGUI.EndIndentedVertical();

            if (info.RemoveAt >= 0 && Event.current.type == EventType.Repaint)
            {
                info.ListValueChanger.RemoveListElementAt(info.RemoveAt, CHANGE_ID);

                info.RemoveAt = -1;
                GUIHelper.RequestRepaint();
            }

            if (info.ObjectPicker != null && info.ObjectPicker.IsReadyToClaim && Event.current.type == EventType.Repaint)
            {
                var value = info.ObjectPicker.ClaimObject();

                if (info.JumpToNextPageOnAdd)
                {
                    info.StartIndex = int.MaxValue;
                }

                object[] values = new object[info.ListValueChanger.ValueCount];

                values[0] = value;
                for (int j = 1; j < values.Length; j++)
                {
                    values[j] = SerializationUtility.CreateCopy(value);
                }

                info.ListValueChanger.AddListElement(values, CHANGE_ID);
            }
        }

        private DropZoneHandle BeginDropZone(ListDrawerConfigInfo info)
        {
            var dropZone = DragAndDropManager.BeginDropZone(UniqueDrawerKey.Create(info.property, this), typeof(TElement), true);

            if (Event.current.type == EventType.Repaint && DragAndDropManager.IsDragInProgress)
            {
                var rect = dropZone.Rect;
                dropZone.Rect = rect;
            }

            dropZone.Enabled = info.IsReadOnly == false;
            info.DropZone = dropZone;
            return dropZone;
        }

        private static UnityEngine.Object[] HandleUnityObjectsDrop(ListDrawerConfigInfo info)
        {
            if (info.IsReadOnly) return null;

            var eventType = Event.current.type;
            if (eventType == EventType.Layout)
            {
                info.IsAboutToDroppingUnityObjects = false;
            }
            if ((eventType == EventType.DragUpdated || eventType == EventType.DragPerform) && info.DropZone.Rect.Contains(Event.current.mousePosition))
            {
                UnityEngine.Object[] objReferences = null;

                if (DragAndDrop.objectReferences.Any(n => n is TElement))
                {
                    objReferences = DragAndDrop.objectReferences.Where(x => x != null && x is TElement).Reverse().ToArray();
                }
                else if (typeof(TElement).InheritsFrom(typeof(Component)))
                {
                    objReferences = DragAndDrop.objectReferences.OfType<GameObject>().Select(x => x.GetComponent(typeof(TElement))).Where(x => x != null).Reverse().ToArray();
                }
                else if (typeof(TElement).InheritsFrom(typeof(Sprite)) && DragAndDrop.objectReferences.Any(n => n is Texture2D && AssetDatabase.Contains(n)))
                {
                    objReferences = DragAndDrop.objectReferences.OfType<Texture2D>().Select(x =>
                    {
                        var path = AssetDatabase.GetAssetPath(x);
                        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    }).Where(x => x != null).Reverse().ToArray();
                }

                bool acceptsDrag = objReferences != null && objReferences.Length > 0;

                if (acceptsDrag)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    Event.current.Use();
                    info.IsAboutToDroppingUnityObjects = true;
                    info.IsDroppingUnityObjects = info.IsAboutToDroppingUnityObjects;
                    if (eventType == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        return objReferences;
                    }
                }
            }
            if (eventType == EventType.Repaint)
            {
                info.IsDroppingUnityObjects = info.IsAboutToDroppingUnityObjects;
            }
            return null;
        }

        private void EndDropZone(ListDrawerConfigInfo info)
        {
            if (info.DropZone.IsReadyToClaim)
            {
                ListDrawerStaticInfo.CurrentDraggingPropertyInfo = null;
                object droppedObject = info.DropZone.ClaimObject();

                object[] values = new object[info.ListValueChanger.ValueCount];

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = droppedObject;
                }

                if (info.DropZone.IsCrossWindowDrag)
                {
                    // If it's a cross-window drag, the changes will for some reason be lost if we don't do this.
                    GUIHelper.RequestRepaint();
                    EditorApplication.delayCall += () =>
                    {
                        info.ListValueChanger.InsertListElementAt(Mathf.Clamp(info.InsertAt, 0, info.property.Children.Count), values, CHANGE_ID);
                    };
                }
                else
                {
                    info.ListValueChanger.InsertListElementAt(Mathf.Clamp(info.InsertAt, 0, info.property.Children.Count), values, CHANGE_ID);
                }
            }
            else
            {
                UnityEngine.Object[] droppedObjects = HandleUnityObjectsDrop(info);
                if (droppedObjects != null)
                {
                    foreach (var obj in droppedObjects)
                    {
                        object[] values = new object[info.ListValueChanger.ValueCount];

                        for (int i = 0; i < values.Length; i++)
                        {
                            values[i] = obj;
                        }

                        info.ListValueChanger.InsertListElementAt(Mathf.Clamp(info.InsertAt, 0, info.property.Children.Count), values, CHANGE_ID);
                    }
                }
            }
            DragAndDropManager.EndDropZone();
        }

        private void DrawToolbar(ListDrawerConfigInfo info)
        {
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                // Label
                if (DragAndDropManager.IsDragInProgress && info.DropZone.IsAccepted == false)
                {
                    GUIHelper.PushGUIEnabled(false);
                }

                if (info.property.ValueEntry.ListLengthChangedFromPrefab)
                {
                    GUIHelper.PushIsBoldLabel(true);
                }

                if (info.listConfig.HideFoldoutWhileEmpty && info.IsEmpty || info.CustomListDrawerOptions.Expanded)
                {
                    GUILayout.Label(info.label, GUILayoutOptions.ExpandWidth(false));
                }
                else
                {
                    info.Toggled.Value = SirenixEditorGUI.Foldout(info.Toggled.Value, info.label ?? GUIContent.none);
                }

                if (info.property.ValueEntry.ListLengthChangedFromPrefab)
                {
                    GUIHelper.PopIsBoldLabel();
                }

                if (info.CustomListDrawerOptions.Expanded)
                {
                    info.Toggled.Value = true;
                }

                if (DragAndDropManager.IsDragInProgress && info.DropZone.IsAccepted == false)
                {
                    GUIHelper.PopGUIEnabled();
                }

                GUILayout.FlexibleSpace();

                // Item Count
                if (info.CustomListDrawerOptions.ShowItemCountHasValue ? info.CustomListDrawerOptions.ShowItemCount : info.listConfig.ShowItemCount)
                {
                    if (info.property.ValueEntry.ValueState == PropertyValueState.CollectionLengthConflict)
                    {
                        int maxLength = 0;
                        for (int i = 0; i < info.property.ValueEntry.ValueCount; i++)
                        {
                            maxLength = Math.Max(maxLength, (info.property.ValueEntry.WeakValues[i] as IList<TElement>).Count);
                        }
                        GUILayout.Label(info.Count + " / " + maxLength + " items", EditorStyles.centeredGreyMiniLabel);
                    }
                    else
                    {
                        GUILayout.Label(info.IsEmpty ? "Empty" : info.Count + " items", EditorStyles.centeredGreyMiniLabel);
                    }
                }

                bool paging = info.CustomListDrawerOptions.PagingHasValue ? info.CustomListDrawerOptions.ShowPaging : true;
                bool hidePaging =
                        info.listConfig.HidePagingWhileCollapsed && info.Toggled.Value == false ||
                        info.listConfig.HidePagingWhileOnlyOnePage && info.Count <= info.NumberOfItemsPerPage;

                int numberOfItemsPrPage = Math.Max(1, info.NumberOfItemsPerPage);
                int numberOfPages = Mathf.CeilToInt(info.Count / (float)numberOfItemsPrPage);
                int pageIndex = info.Count == 0 ? 0 : (info.StartIndex / numberOfItemsPrPage) % info.Count;

                // Paging
                if (paging)
                {
                    bool disablePaging = paging && !hidePaging && (DragAndDropManager.IsDragInProgress || info.ShowAllWhilePageing || info.Toggled.Value == false);
                    if (disablePaging)
                    {
                        GUIHelper.PushGUIEnabled(false);
                    }

                    if (!hidePaging)
                    {
                        if (pageIndex == 0) { GUIHelper.PushGUIEnabled(false); }

                        if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleLeft, true))
                        {
                            if (Event.current.button == 0)
                            {
                                info.StartIndex -= numberOfItemsPrPage;
                            }
                            else
                            {
                                info.StartIndex = 0;
                            }
                        }
                        if (pageIndex == 0) { GUIHelper.PopGUIEnabled(); }

                        var userPageIndex = EditorGUILayout.IntField((numberOfPages == 0 ? 0 : (pageIndex + 1)), GUILayoutOptions.Width(10 + numberOfPages.ToString(CultureInfo.InvariantCulture).Length * 10)) - 1;
                        if (pageIndex != userPageIndex)
                        {
                            info.StartIndex = userPageIndex * numberOfItemsPrPage;
                        }

                        GUILayout.Label("/ " + numberOfPages);

                        if (pageIndex == numberOfPages - 1) { GUIHelper.PushGUIEnabled(false); }

                        if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleRight, true))
                        {
                            if (Event.current.button == 0)
                            {
                                info.StartIndex += numberOfItemsPrPage;
                            }
                            else
                            {
                                info.StartIndex = numberOfItemsPrPage * numberOfPages;
                            }
                        }
                        if (pageIndex == numberOfPages - 1) { GUIHelper.PopGUIEnabled(); }
                    }

                    pageIndex = info.Count == 0 ? 0 : (info.StartIndex / numberOfItemsPrPage) % info.Count;

                    var newStartIndex = Mathf.Clamp(pageIndex * numberOfItemsPrPage, 0, Mathf.Max(0, info.Count - 1));
                    if (newStartIndex != info.StartIndex)
                    {
                        info.StartIndex = newStartIndex;
                        var newPageIndex = info.Count == 0 ? 0 : (info.StartIndex / numberOfItemsPrPage) % info.Count;
                        if (pageIndex != newPageIndex)
                        {
                            pageIndex = newPageIndex;
                            info.StartIndex = Mathf.Clamp(pageIndex * numberOfItemsPrPage, 0, Mathf.Max(0, info.Count - 1));
                        }
                    }

                    info.EndIndex = Mathf.Min(info.StartIndex + numberOfItemsPrPage, info.Count);

                    if (disablePaging)
                    {
                        GUIHelper.PopGUIEnabled();
                    }
                }
                else
                {
                    info.StartIndex = 0;
                    info.EndIndex = info.Count;
                }

                if (paging && hidePaging == false && info.listConfig.ShowExpandButton)
                {
                    if (SirenixEditorGUI.ToolbarButton(info.ShowAllWhilePageing ? EditorIcons.TriangleUp : EditorIcons.TriangleDown, true))
                    {
                        info.ShowAllWhilePageing = !info.ShowAllWhilePageing;
                    }
                }

                // Add Button
                if (info.IsReadOnly == false && !info.CustomListDrawerOptions.HideAddButton)
                {
                    info.ObjectPicker = ObjectPicker<TElement>.GetObjectPicker(info);

                    if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
                    {
                        if (info.CustomListDrawerOptions.AlwaysAddDefaultValue)
                        {
                            var objs = new object[info.ListValueChanger.ValueCount];

                            if (info.property.ValueEntry.SerializationBackend == SerializationBackend.Unity)
                            {
                                for (int i = 0; i < objs.Length; i++)
                                {
                                    objs[i] = UnitySerializationUtility.CreateDefaultUnityInitializedObject(typeof(TElement));
                                }
                            }
                            else
                            {
                                for (int i = 0; i < objs.Length; i++)
                                {
                                    objs[i] = default(TElement);
                                }
                            }

                            info.ListValueChanger.AddListElement(objs, "Add default value");
                        }
                        else if (typeof(TElement).InheritsFrom<UnityEngine.Object>() && Event.current.modifiers == EventModifiers.Control)
                        {
                            info.ListValueChanger.AddListElement(new object[info.ListValueChanger.ValueCount], "Add Unity Null Value");
                        }
                        else
                        {
                            info.ObjectPicker.ShowObjectPicker(
                                info.property.Info.GetAttribute<AssetsOnlyAttribute>() == null,
                                GUIHelper.GetCurrentLayoutRect(),
                                info.property.ValueEntry.SerializationBackend == SerializationBackend.Unity);
                        }
                    }

                    info.JumpToNextPageOnAdd = paging && (info.Count % numberOfItemsPrPage == 0) && (pageIndex + 1 == numberOfPages);
                }

                if (info.OnTitleBarGUI != null)
                {
                    info.OnTitleBarGUI(info.property.ParentValues[0]);
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        private void DrawItems(ListDrawerConfigInfo info)
        {
            int from = 0;
            int to = info.Count;
            bool paging = info.CustomListDrawerOptions.PagingHasValue ? info.CustomListDrawerOptions.ShowPaging : true;
            if (paging && info.ShowAllWhilePageing == false)
            {
                from = Mathf.Clamp(info.StartIndex, 0, info.Count);
                to = Mathf.Clamp(info.EndIndex, 0, info.Count);
            }

            var drawEmptySpace = info.DropZone.IsBeingHovered || info.IsDroppingUnityObjects;
            float height = drawEmptySpace ? info.IsDroppingUnityObjects ? 16 : (DragAndDropManager.CurrentDraggingHandle.Rect.height - 3) : 0;
            var rect = SirenixEditorGUI.BeginVerticalList();
            {
                for (int i = 0, j = from, k = from; j < to; i++, j++)
                {
                    var dragHandle = this.BeginDragHandle(info, j, i);
                    {
                        if (drawEmptySpace)
                        {
                            var topHalf = dragHandle.Rect;
                            topHalf.height /= 2;
                            if (topHalf.Contains(info.LayoutMousePosition) || topHalf.y > info.LayoutMousePosition.y && i == 0)
                            {
                                GUILayout.Space(height);
                                drawEmptySpace = false;
                                info.InsertAt = k;
                            }
                        }

                        if (dragHandle.IsDragging == false)
                        {
                            k++;
                            this.DrawItem(info, info.property.Children[j], dragHandle, j);
                        }
                        else
                        {
                            GUILayout.Space(3);
                            ListDrawerStaticInfo.DelayedGUIDrawer.Begin(dragHandle.Rect.width, dragHandle.Rect.height, dragHandle.CurrentMethod != DragAndDropMethods.Move);
                            DragAndDropManager.AllowDrop = false;
                            this.DrawItem(info, info.property.Children[j], dragHandle, j);
                            DragAndDropManager.AllowDrop = true;
                            ListDrawerStaticInfo.DelayedGUIDrawer.End();
                            if (dragHandle.CurrentMethod != DragAndDropMethods.Move)
                            {
                                GUILayout.Space(3);
                            }
                        }

                        if (drawEmptySpace)
                        {
                            var bottomHalf = dragHandle.Rect;
                            bottomHalf.height /= 2;
                            bottomHalf.y += bottomHalf.height;

                            if (bottomHalf.Contains(info.LayoutMousePosition) || bottomHalf.yMax < info.LayoutMousePosition.y && j + 1 == to)
                            {
                                GUILayout.Space(height);
                                drawEmptySpace = false;
                                info.InsertAt = Mathf.Min(k, to);
                            }
                        }
                    }
                    this.EndDragHandle(info, i);
                }

                if (drawEmptySpace)
                {
                    GUILayout.Space(height);
                    info.InsertAt = Event.current.mousePosition.y > rect.center.y ? to : from;
                }

                if (to == info.property.Children.Count && info.property.ValueEntry.ValueState == PropertyValueState.CollectionLengthConflict)
                {
                    SirenixEditorGUI.BeginListItem(false);
                    GUILayout.Label(GUIHelper.TempContent("------"), EditorStyles.centeredGreyMiniLabel);
                    SirenixEditorGUI.EndListItem();
                }
            }
            SirenixEditorGUI.EndVerticalList();

            if (Event.current.type == EventType.Repaint)
            {
                info.LayoutMousePosition = Event.current.mousePosition;
            }
        }

        private void EndDragHandle(ListDrawerConfigInfo info, int i)
        {
            var handle = DragAndDropManager.EndDragHandle();

            if (handle.IsDragging)
            {
                info.property.Tree.DelayAction(() =>
                {
                    if (DragAndDropManager.CurrentDraggingHandle != null)
                    {
                        ListDrawerStaticInfo.DelayedGUIDrawer.Draw(Event.current.mousePosition - DragAndDropManager.CurrentDraggingHandle.MouseDownPostionOffset);
                    }
                });
            }
        }

        private DragHandle BeginDragHandle(ListDrawerConfigInfo info, int j, int i)
        {
            var child = info.property.Children[j];
            var dragHandle = DragAndDropManager.BeginDragHandle(child, (TElement)child.ValueEntry.WeakSmartValue, info.IsReadOnly ? DragAndDropMethods.Reference : DragAndDropMethods.Move);
            dragHandle.Enabled = info.Draggable;

            if (dragHandle.OnDragStarted)
            {
                ListDrawerStaticInfo.CurrentDraggingPropertyInfo = info.property.Children[j];
                dragHandle.OnDragFinnished = dropEvent =>
                {
                    if (dropEvent == DropEvents.Moved)
                    {
                        // If it's a cross-window drag, the changes will for some reason be lost if we don't do this.
                        if (dragHandle.IsCrossWindowDrag)
                        {
                            GUIHelper.RequestRepaint();
                            EditorApplication.delayCall += () =>
                            {
                                info.ListValueChanger.RemoveListElementAt(j, CHANGE_ID);
                            };
                        }
                        else
                        {
                            info.ListValueChanger.RemoveListElementAt(j, CHANGE_ID);
                        }
                    }

                    ListDrawerStaticInfo.CurrentDraggingPropertyInfo = null;
                };
            }

            return dragHandle;
        }

        private struct ListItemInfo
        {
            public float Width;
            public Rect AddBtnRect;
            public Rect DragHandleRect;
        }

        private static GUILayoutOption[] listItemOptions = GUILayoutOptions.MinHeight(25).ExpandWidth(true);

        private Rect DrawItem(ListDrawerConfigInfo info, InspectorProperty itemProperty, DragHandle dragHandle, int index = -1)
        {
            var listItemInfo = itemProperty.Context.Get<ListItemInfo>(this, "listItemInfo");

            Rect rect;
            rect = SirenixEditorGUI.BeginListItem(false, info.ListItemStyle, listItemOptions);
            {
                if (Event.current.type == EventType.Repaint && !info.IsReadOnly)
                {
                    listItemInfo.Value.Width = rect.width;
                    dragHandle.DragHandleRect = new Rect(rect.x + 4, rect.y, 20, rect.height);
                    listItemInfo.Value.DragHandleRect = new Rect(rect.x + 4, rect.y + 2 + ((int)rect.height - 23) / 2, 20, 20);
                    listItemInfo.Value.AddBtnRect = new Rect(listItemInfo.Value.DragHandleRect.x + rect.width - 22, listItemInfo.Value.DragHandleRect.y + 1, 14, 14);
                    itemProperty.Context.GetGlobal<Rect?>("overrideRect").Value = rect;
                    if (info.Draggable)
                    {
                        //GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
                        GUI.Label(listItemInfo.Value.DragHandleRect, EditorIcons.List.Inactive, GUIStyle.none);
                        //GL.sRGBWrite = false;
                    }
                }

                GUIHelper.PushHierarchyMode(false);
                GUIContent label = null;

                if (info.CustomListDrawerOptions.ShowIndexLabelsHasValue)
                {
                    if (info.CustomListDrawerOptions.ShowIndexLabels)
                    {
                        label = new GUIContent(index.ToString());
                    }
                }
                else if (info.listConfig.ShowIndexLabels)
                {
                    label = new GUIContent(index.ToString());
                }

                if (info.GetListElementLabelText != null)
                {
                    var value = itemProperty.ValueEntry.WeakSmartValue;

                    if (object.ReferenceEquals(value, null))
                    {
                        if (label == null)
                        {
                            label = new GUIContent("Null");
                        }
                        else
                        {
                            label.text += " : Null";
                        }
                    }
                    else
                    {
                        label = label ?? new GUIContent("");
                        if (label.text != "") label.text += " : ";

                        object text = info.GetListElementLabelText(value);
                        label.text += (text == null ? "" : text.ToString());
                    }
                }

                if (info.OnBeginListElementGUI != null)
                {
                    info.OnBeginListElementGUI(info.property.ParentValues[0], index);
                }
                InspectorUtilities.DrawProperty(itemProperty, label);

                if (info.OnEndListElementGUI != null)
                {
                    info.OnEndListElementGUI(info.property.ParentValues[0], index);
                }

                GUIHelper.PopHierarchyMode();

                if (info.IsReadOnly == false)
                {
                    if (SirenixEditorGUI.IconButton(listItemInfo.Value.AddBtnRect, EditorIcons.X))
                    {
                        if (index >= 0)
                        {
                            info.RemoveAt = index;
                        }
                    }
                }
            }
            SirenixEditorGUI.EndListItem();

            return rect;
        }

        private class ListDrawerConfigInfo
        {
            public PropertyListValueEntryChanger ListValueChanger;
            public bool IsEmpty;
            public ListDrawerSettingsAttribute CustomListDrawerOptions;
            public int Count;
            public LocalPersistentContext<bool> Toggled;
            public int StartIndex;
            public int EndIndex;
            public DropZoneHandle DropZone;
            public Vector2 LayoutMousePosition;
            public Vector2 DropZoneTopLeft;
            public int InsertAt;
            public int RemoveAt;
            public bool IsReadOnly;
            public bool Draggable;
            public bool ShowAllWhilePageing;
            public ObjectPicker<TElement> ObjectPicker;
            public bool JumpToNextPageOnAdd;
            public Action<object> OnTitleBarGUI;
            public GeneralDrawerConfig listConfig;
            public InspectorProperty property;
            public GUIContent label;
            public bool IsAboutToDroppingUnityObjects;
            public bool IsDroppingUnityObjects;

            public Func<object, object> GetListElementLabelText;
            public Action<object, int> OnBeginListElementGUI;
            public Action<object, int> OnEndListElementGUI;

            public int NumberOfItemsPerPage
            {
                get
                {
                    return this.CustomListDrawerOptions.NumberOfItemsPerPageHasValue ? this.CustomListDrawerOptions.NumberOfItemsPerPage : this.listConfig.NumberOfItemsPrPage;
                }
            }

            public GUIStyle ListItemStyle = new GUIStyle(GUIStyle.none)
            {
                padding = new RectOffset(25, 20, 3, 3)
            };
        }
    }
}
#endif