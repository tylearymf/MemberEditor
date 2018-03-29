#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DragAndDropManager.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor
{
    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Drag and drop utilities for both Unity and non-unity objects.
    /// </summary>
    public static class DragAndDropUtilities
    {
        private static bool currentDragIsMove;
        private static int draggingId;
        private static bool isAccepted;
        private static object dropZoneObject;
        private static object[] dragginObjects = new object[] { };
        private static bool isDragging = false;
        private static bool isHoveringAcceptedDropZone;

        public static bool IsDragging
        {
            get
            {
                switch (Event.current.rawType)
                {
                    case EventType.MouseDown:
                    case EventType.MouseUp:
                    case EventType.MouseMove:
                        isDragging = false;
                        break;
                    case EventType.MouseDrag:
                    case EventType.DragUpdated:
                    case EventType.DragPerform:
                    case EventType.DragExited:
                        isDragging = true;
                        break;
                    default:
                        break;
                }

                return isDragging;
            }
        }

        /// <summary>
        /// Gets a more percistent id for drag and drop.
        /// </summary>
        public static int GetDragAndDropId(Rect rect)
        {
            //var pos = GUIUtility.GUIToScreenPoint(rect.position);
            //var hint = 10000 + Mathf.Abs(pos.GetHashCode());
            return GUIUtility.GetControlID(FocusType.Keyboard, rect);
        }

        /// <summary>
        /// Draws a objectpicker butter, in the given rect. This one is designed to look good on top of DrawDropZone().
        /// </summary>
        public static object ObjectPickerZone(Rect rect, object value, Type type, bool allowSceneObjects, int id)
        {
            var btnId = GUIUtility.GetControlID(FocusType.Passive);
            var objectPicker = ObjectPicker.GetObjectPicker(type.FullName + "+" + btnId, type);
            var selectRect = rect.AlignBottom(15).AlignCenter(45);
            var uObj = value as UnityEngine.Object;
            selectRect.xMin = Mathf.Max(selectRect.xMin, rect.xMin);

            var hide = IsDragging || Event.current.type == EventType.Repaint && !rect.Contains(Event.current.mousePosition);

            if (hide)
            {
                GUIHelper.PushColor(new Color(0, 0, 0, 0));
                GUIHelper.PushGUIEnabled(false);
            }

            bool hideInspectorBtn = !hide && !(uObj);

            if (hideInspectorBtn)
            {
                GUIHelper.PushGUIEnabled(false);
                GUIHelper.PushColor(new Color(0, 0, 0, 0));
            }

            var inspectBtn = rect.AlignRight(14);
            inspectBtn.height = 14;
            SirenixEditorGUI.BeginDrawOpenInspector(inspectBtn, uObj, rect);
            SirenixEditorGUI.EndDrawOpenInspector(inspectBtn, uObj);

            if (hideInspectorBtn)
            {
                GUIHelper.PopColor();
                GUIHelper.PopGUIEnabled();
            }

            if (GUI.Button(selectRect, "select", SirenixGUIStyles.TagButton))
            {
                GUIHelper.RemoveFocusControl();
                objectPicker.ShowObjectPicker(allowSceneObjects, rect, false);
                Event.current.Use();
            }

            if (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown && EditorGUIUtility.keyboardControl == id)
            {
                objectPicker.ShowObjectPicker(allowSceneObjects, rect, false);
                Event.current.Use();
            }

            if (hide)
            {
                GUIHelper.PopColor();
                GUIHelper.PopGUIEnabled();
            }

            if (objectPicker.IsReadyToClaim)
            {
                GUIHelper.RequestRepaint();
                GUI.changed = true;
                var newValue = objectPicker.ClaimObject();
                Event.current.Use();
                return newValue;
            }

            if (Event.current.keyCode == KeyCode.Delete && Event.current.type == EventType.KeyDown && EditorGUIUtility.keyboardControl == id)
            {
                Event.current.Use();
                GUI.changed = true;
                return null;
            }

            if (uObj && Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition) && EditorGUIUtility.hotControl == id && Event.current.button == 0)
            {
                EditorGUIUtility.PingObject(uObj);
            }

            return value;
        }

        /// <summary>
        /// Draws a objectpicker butter, in the given rect. This one is designed to look good on top of DrawDropZone().
        /// </summary>
        public static T ObjectPickerZone<T>(Rect rect, T value, bool allowSceneObjects, int id)
        {
            return (T)ObjectPickerZone(rect, value, typeof(T), allowSceneObjects, id);
        }

        /// <summary>
        /// Draws the graphics for a DropZone.
        /// </summary>
        public static void DrawDropZone(Rect rect, object value, GUIContent label, int id)
        {
            bool isDragging = IsDragging;

            if (Event.current.type == EventType.Repaint)
            {
                var unityObject = value as UnityEngine.Object;
                var objectFieldThumb = EditorStyles.objectFieldThumb;
                var on = GUI.enabled && isHoveringAcceptedDropZone && rect.Contains(Event.current.mousePosition) && isDragging;

                objectFieldThumb.Draw(rect, GUIContent.none, id, on);

                if (EditorGUI.showMixedValue)
                {
                    GUI.Label(rect, "—", SirenixGUIStyles.LabelCentered);
                }
                else if (unityObject)
                {
                    if (unityObject is Component)
                    {
                        unityObject = (unityObject as Component).gameObject;
                    }

                    Texture image;
                    image = AssetPreview.GetAssetPreview(unityObject);
                    if (image == null)
                    {
                        image = AssetPreview.GetMiniThumbnail(unityObject);
                    }

                    rect = rect.Padding(2);
                    float size = Mathf.Min(rect.width, rect.height);

                    EditorGUI.DrawTextureTransparent(rect.AlignCenter(size, size), image, ScaleMode.ScaleToFit);

                    if (label != null)
                    {
                        rect = rect.AlignBottom(16);
                        GUI.Label(rect, label, EditorStyles.label);
                    }
                }
            }
        }

        /// <summary>
        /// A draggable zone for both Unity and non-unity objects.
        /// </summary>
        public static object DragAndDropZone(Rect rect, object value, Type type, bool allowMove, bool allowSwap)
        {
            var id = GetDragAndDropId(rect);
            value = DropZone(rect, value, type, id);
            value = DragZone(rect, value, type, allowMove, allowSwap, id);
            return value;
        }

        /// <summary>
        /// A drop zone area for bot Unity and non-unity objects.
        /// </summary>
        public static object DropZone(Rect rect, object value, Type type, bool allowSceneObjects, int id)
        {
            if (rect.Contains(Event.current.mousePosition))
            {
                var t = Event.current.type;

                if (t == EventType.DragUpdated || t == EventType.DragPerform)
                {
                    object obj = null;

                    if (obj == null) obj = dragginObjects.Where(x => x != null && x.GetType().InheritsFrom(type)).FirstOrDefault();
                    if (obj == null) obj = DragAndDrop.objectReferences.Where(x => x != null && x.GetType().InheritsFrom(type)).FirstOrDefault();

                    if (type.InheritsFrom<Component>() || type.IsInterface)
                    {
                        if (obj == null) obj = dragginObjects.OfType<GameObject>().Where(x => x != null).Select(x => x.GetComponent(type)).Where(x => x != null).FirstOrDefault();
                        if (obj == null) obj = DragAndDrop.objectReferences.OfType<GameObject>().Where(x => x != null).Select(x => x.GetComponent(type)).Where(x => x != null).FirstOrDefault();
                    }

                    bool acceptsDrag = obj != null;

                    if (acceptsDrag && allowSceneObjects == false)
                    {
                        var uObj = (UnityEngine.Object)obj;
                        if (uObj != null)
                        {
                            if (typeof(Component).IsAssignableFrom(uObj.GetType()))
                            {
                                uObj = ((Component)uObj).gameObject;
                            }

                            acceptsDrag = EditorUtility.IsPersistent(uObj);
                        }
                    }

                    if (acceptsDrag)
                    {
                        isHoveringAcceptedDropZone = true;
                        bool move = Event.current.modifiers != EventModifiers.Control && draggingId != 0 && currentDragIsMove;
                        if (move)
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                        }
                        else
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        }

                        Event.current.Use();
                        if (t == EventType.DragPerform)
                        {
                            if (!move)
                            {
                                draggingId = 0;
                            }

                            DragAndDrop.objectReferences = new UnityEngine.Object[] { };
                            DragAndDrop.AcceptDrag();
                            GUI.changed = true;
                            GUIHelper.RemoveFocusControl();
                            dragginObjects = new object[] { };
                            currentDragIsMove = false;
                            isAccepted = true;
                            dropZoneObject = value;
                            DragAndDrop.activeControlID = 0;
                            GUIHelper.RequestRepaint();
                            return obj;
                        }
                        else
                        {
                            DragAndDrop.activeControlID = id;
                        }
                    }
                    else
                    {
                        isHoveringAcceptedDropZone = false;
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// A drop zone area for bot Unity and non-unity objects.
        /// </summary>
        public static object DropZone(Rect rect, object value, Type type, int id)
        {
            return DropZone(rect, value, type, true, id);
        }

        /// <summary>
        /// A drop zone area for bot Unity and non-unity objects.
        /// </summary>
        public static object DropZone(Rect rect, object value, Type type)
        {
            var id = GetDragAndDropId(rect);
            return DropZone(rect, value, type, id);
        }

        /// <summary>
        /// A drop zone area for bot Unity and non-unity objects.
        /// </summary>
        public static object DropZone(Rect rect, object value, Type type, bool allowSceneObjects)
        {
            var id = GetDragAndDropId(rect);
            return DropZone(rect, value, type, allowSceneObjects, id);
        }

        /// <summary>
        /// A drop zone area for bot Unity and non-unity objects.
        /// </summary>
        public static T DropZone<T>(Rect rect, T value, bool allowSceneObjects, int id)
        {
            return (T)DropZone(rect, value, typeof(T), allowSceneObjects, id);
        }

        /// <summary>
        /// A drop zone area for bot Unity and non-unity objects.
        /// </summary>
        public static T DropZone<T>(Rect rect, T value, int id)
        {
            return (T)DropZone(rect, value, typeof(T), id);
        }

        /// <summary>
        /// A drop zone area for bot Unity and non-unity objects.
        /// </summary>
        public static T DropZone<T>(Rect rect, T value, bool allowSceneObjects)
        {
            var id = GetDragAndDropId(rect);
            return (T)DropZone(rect, value, typeof(T), allowSceneObjects, id);
        }

        /// <summary>
        /// A drop zone area for bot Unity and non-unity objects.
        /// </summary>
        public static T DropZone<T>(Rect rect, T value)
        {
            var id = GetDragAndDropId(rect);
            return (T)DropZone(rect, value, typeof(T), id);
        }

        /// <summary>
        /// A draggable zone for both Unity and non-unity objects.
        /// </summary>
        public static object DragZone(Rect rect, object value, Type type, bool allowMove, bool allowSwap, int id)
        {
            if (value == null) return null;

            // Unity null
            if (typeof(UnityEngine.Object).IsAssignableFrom(value.GetType()) && !(value as UnityEngine.Object))
            {
                return value;
            }

            var t = Event.current.type;
            var isMouseOver = rect.Contains(Event.current.mousePosition);
            var unityObject = value as UnityEngine.Object;

            if (isMouseOver && t == EventType.MouseDown)
            {
                GUIHelper.RemoveFocusControl();
                GUIUtility.hotControl = id;
                GUIUtility.keyboardControl = id;
                dragginObjects = new object[] { };
                DragAndDrop.PrepareStartDrag();
                GUIHelper.RequestRepaint();
                isAccepted = false;
                dropZoneObject = null;
                draggingId = 0;
                currentDragIsMove = false;
            }

            if (isAccepted && draggingId == id)
            {
                GUIHelper.RequestRepaint();
                GUI.changed = true;
                draggingId = 0;

                // TODO: Validate drop zone object, and only return that if it's assignable from type.

                return allowMove ? (allowSwap ? dropZoneObject : null) : value;
            }

            if (GUIUtility.hotControl != id)
            {
                return value;
            }
            else if (t == EventType.MouseMove)
            {
                GUIHelper.RequestRepaint();
                draggingId = 0;
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new UnityEngine.Object[] { };
                //GUIHelper.RemoveFocusControl();
                dragginObjects = new object[] { };
                currentDragIsMove = false;
            }

            if (Event.current.type == EventType.MouseDrag && isMouseOver && (DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0))
            {
                isAccepted = false;
                dropZoneObject = null;
                draggingId = id;
                DragAndDrop.StartDrag("Movable drag");
                if (unityObject)
                {
                    DragAndDrop.objectReferences = new UnityEngine.Object[] { unityObject };
                    dragginObjects = new object[] { };
                }
                else
                {
                    DragAndDrop.objectReferences = new UnityEngine.Object[] { };
                    dragginObjects = new object[] { value };
                }

                DragAndDrop.activeControlID = 0;
                currentDragIsMove = allowMove;
                Event.current.Use();
                GUIHelper.RequestRepaint();
            }

            return value;
        }

        /// <summary>
        /// A draggable zone for both Unity and non-unity objects.
        /// </summary>
        public static object DragZone(Rect rect, object value, Type type, bool allowMove, bool allowSwap)
        {
            var id = GetDragAndDropId(rect);
            return DragZone(rect, value, type, allowMove, allowSwap, id);
        }

        /// <summary>
        /// A draggable zone for both Unity and non-unity objects.
        /// </summary>
        public static T DragZone<T>(Rect rect, T value, bool allowMove, bool allowSwap, int id)
        {
            return (T)DragZone(rect, value, typeof(T), allowMove, allowSwap, id);
        }

        /// <summary>
        /// A draggable zone for both Unity and non-unity objects.
        /// </summary>
        public static T DragZone<T>(Rect rect, T value, bool allowMove, bool allowSwap)
        {
            var id = GetDragAndDropId(rect);
            return (T)DragZone(rect, value, typeof(T), allowMove, allowSwap, id);
        }
    }
}
#endif