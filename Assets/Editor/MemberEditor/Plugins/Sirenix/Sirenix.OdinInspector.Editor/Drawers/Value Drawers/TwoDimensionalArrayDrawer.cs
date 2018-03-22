#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ListDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Base class for two-dimentional array drawers.
    /// </summary>
    public abstract class TwoDimensionalArrayDrawer<TArray, TElement> : OdinValueDrawer<TArray> where TArray : IList
    {
#pragma warning disable 1591 // Missing XML comment for publicly visible type or member

        protected internal class Context
        {
            public int RowCount;
            public int ColCount;
            public GUITable Table;
            public TElement[,] Value;
            public int DraggingRow = -1;
            public int DraggingCol = -1;
            public string ErrorMessage;
            public TableMatrixAttribute Attribute;
            public Func<Rect, TElement, TElement> DrawElement;
            public StringMemberHelper HorizontalTitleGetter;
            public StringMemberHelper VerticalTitleGetter;
            public Vector2 dragStartPos;
            public bool IsDraggingColumn;
            public int ColumnDragFrom;
            public int ColumnDragTo;
            public bool IsDraggingRow;
            public int RowDragFrom;
            public int RowDragTo;
        }

#pragma warning restore 1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// <para>Override this method in order to define custom type constraints to specify whether or not a type should be drawn by the drawer.</para>
        /// <para>Note that Odin's <see cref="DrawerLocator" /> has full support for generic class constraints, so most often you can get away with not overriding CanDrawTypeFilter.</para>
        /// </summary>
        public override bool CanDrawTypeFilter(Type type)
        {
            return type.IsArray && type.GetArrayRank() == 2 && type.GetElementType() == typeof(TElement);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected virtual TableMatrixAttribute GetDefaultTableMatrixAttributeSettings()
        {
            return new TableMatrixAttribute();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected TableMatrixAttribute TableMatrixAttribute { get; private set; }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<TArray> entry, GUIContent label)
        {
            TElement[,] value = entry.Values[0] as TElement[,];
            bool rowLengthConflic = false;
            bool colLengthConflic = false;
            int colCount = value.GetLength(0);
            int rowCount = value.GetLength(1);
            for (int i = 1; i < entry.Values.Count; i++)
            {
                var arr = entry.Values[i] as TElement[,];
                colLengthConflic = colLengthConflic || arr.GetLength(0) != colCount;
                rowLengthConflic = rowLengthConflic || arr.GetLength(1) != rowCount;
                colCount = Mathf.Min(colCount, arr.GetLength(0));
                rowCount = Mathf.Min(rowCount, arr.GetLength(1));
            }

            var context = entry.Context.Get(this, "context", (Context)null);
            if (context.Value == null || colCount != context.Value.ColCount || rowCount != context.Value.RowCount)
            {
                context.Value = new Context();
                context.Value.Value = value;
                context.Value.ColCount = colCount;
                context.Value.RowCount = rowCount;
                context.Value.Attribute = entry.Property.Info.GetAttribute<TableMatrixAttribute>() ?? this.GetDefaultTableMatrixAttributeSettings();

                if (context.Value.Attribute.DrawElementMethod != null)
                {
                    string error;
                    var drawElementMethod = entry.ParentType.FindMember()
                        .IsMethod()
                        .IsStatic()
                        .HasReturnType<TElement>()
                        .IsNamed(context.Value.Attribute.DrawElementMethod)
                        .HasParameters<Rect, TElement>()
                        .GetMember<MethodInfo>(out error);

                    if (error != null)
                    {
                        context.Value.ErrorMessage += error + "\n\n";
                    }
                    else
                    {
                        context.Value.DrawElement = (Func<Rect, TElement, TElement>)Delegate.CreateDelegate(typeof(Func<Rect, TElement, TElement>), drawElementMethod);
                    }
                }

                context.Value.HorizontalTitleGetter = new StringMemberHelper(entry.ParentType, context.Value.Attribute.HorizontalTitle);
                context.Value.VerticalTitleGetter = new StringMemberHelper(entry.ParentType, context.Value.Attribute.VerticalTitle);

                context.Value.Table = GUITable.Create(
                    Mathf.Max(colCount, 1) + (colLengthConflic ? 1 : 0), Mathf.Max(rowCount, 1) + (rowLengthConflic ? 1 : 0),
                    (rect, x, y) => this.DrawElement(rect, entry, context.Value, x, y),
                    context.Value.HorizontalTitleGetter.GetString(entry),
                    context.Value.Attribute.HideColumnIndices ? (Action<Rect, int>)null : (rect, x) => this.DrawColumn(rect, entry, context.Value, x),
                    context.Value.VerticalTitleGetter.GetString(entry),
                    context.Value.Attribute.HideRowIndices ? (Action<Rect, int>)null : (rect, y) => this.DrawRows(rect, entry, context.Value, y),
                    context.Value.Attribute.ResizableColumns
                );

                if (context.Value.Attribute.RowHeight != 0)
                {
                    for (int y = 0; y < context.Value.RowCount; y++)
                    {
                        int _y = context.Value.Table.RowCount - 1 - y;

                        for (int x = 0; x < context.Value.Table.ColumnCount; x++)
                        {
                            var cell = context.Value.Table[x, _y];
                            if (cell != null)
                            {
                                cell.Height = context.Value.Attribute.RowHeight;
                            }
                        }
                    }
                }

                if (colLengthConflic)
                {
                    context.Value.Table[context.Value.Table.ColumnCount - 1, 1].Width = 15;
                }

                if (colLengthConflic)
                {
                    for (int x = 0; x < context.Value.Table.ColumnCount; x++)
                    {
                        context.Value.Table[x, context.Value.Table.RowCount - 1].Height = 15;
                    }
                }
            }


            if (context.Value.Attribute.SquareCells)
            {
                SetSquareRowHeights(context);
            }

            this.TableMatrixAttribute = context.Value.Attribute;

            context.Value.Value = value;
            var prev = EditorGUI.showMixedValue;

            this.OnBeforeDrawTable(entry, context.Value, label);

            if (context.Value.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(context.Value.ErrorMessage);
            }
            else
            {
                try
                {
                    context.Value.Table.DrawTable();
                    GUILayout.Space(3);
                }
                catch (ExitGUIException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            EditorGUI.showMixedValue = prev;
        }

        private static void SetSquareRowHeights(PropertyContext<Context> context)
        {
            if (context.Value.ColCount > 0 && context.Value.RowCount > 0)
            {
                var lastCell = context.Value.Table[context.Value.ColCount - 1, context.Value.RowCount - 1];
                if (lastCell != null && Mathf.Abs(lastCell.Rect.height - lastCell.Rect.width) > 0)
                {
                    for (int y = 0; y < context.Value.RowCount; y++)
                    {
                        int _y = context.Value.Table.RowCount - 1 - y;

                        for (int x = 0; x < context.Value.Table.ColumnCount; x++)
                        {
                            var cell = context.Value.Table[x, _y];
                            if (cell != null)
                            {
                                cell.Height = lastCell.Rect.width;
                            }
                        }
                    }
                    context.Value.Table.ReCalculateSizes();
                    GUIHelper.RequestRepaint();
                }
            }
        }

        /// <summary>
        /// This method gets called from DrawPropertyLayout right before the table and error message is drawn.
        /// </summary>
        protected internal virtual void OnBeforeDrawTable(IPropertyValueEntry<TArray> entry, Context value, GUIContent label)
        {
        }

        private void DrawRows(Rect rect, IPropertyValueEntry<TArray> entry, Context context, int rowIndex)
        {
            if (rowIndex < context.RowCount)
            {
                GUI.Label(rect, rowIndex.ToString(), SirenixGUIStyles.LabelCentered);

                // Handle Row dragging.
                if (!context.Attribute.IsReadOnly)
                {
                    var id = GUIUtility.GetControlID(FocusType.Passive);
                    if (GUI.enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
                    {
                        GUIHelper.RemoveFocusControl();
                        GUIUtility.hotControl = id;
                        EditorGUIUtility.SetWantsMouseJumping(1);
                        Event.current.Use();
                        context.RowDragFrom = rowIndex;
                        context.RowDragTo = rowIndex;
                        context.dragStartPos = Event.current.mousePosition;
                    }
                    else if (GUIUtility.hotControl == id)
                    {
                        if ((context.dragStartPos - Event.current.mousePosition).sqrMagnitude > 5 * 5)
                        {
                            context.IsDraggingRow = true;
                        }
                        if (Event.current.type == EventType.MouseDrag)
                        {
                            Event.current.Use();
                        }
                        else if (Event.current.type == EventType.MouseUp)
                        {
                            GUIUtility.hotControl = 0;
                            EditorGUIUtility.SetWantsMouseJumping(0);
                            Event.current.Use();
                            context.IsDraggingRow = false;

                            ApplyArrayModifications(entry, arr => MultiDimArrayUtilities.MoveRow(arr, context.RowDragFrom, context.RowDragTo));
                        }
                    }

                    if (context.IsDraggingRow && Event.current.type == EventType.Repaint)
                    {
                        float mouseY = Event.current.mousePosition.y;
                        if (mouseY > rect.y - 1 && mouseY < rect.y + rect.height + 1)
                        {
                            Rect arrowRect;
                            if (mouseY > rect.y + rect.height * 0.5f)
                            {
                                arrowRect = rect.AlignBottom(16);
                                arrowRect.width = 16;
                                arrowRect.y += 8;
                                arrowRect.x -= 13;
                                context.RowDragTo = rowIndex;
                            }
                            else
                            {
                                arrowRect = rect.AlignTop(16);
                                arrowRect.width = 16;
                                arrowRect.y -= 8;
                                arrowRect.x -= 13;
                                context.RowDragTo = rowIndex - 1;
                            }
                            entry.Property.Tree.DelayActionUntilRepaint(() =>
                            {
                                //GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
                                GUI.DrawTexture(arrowRect, EditorIcons.ArrowRight.Active);
                                //GL.sRGBWrite = false;

                                var lineRect = arrowRect;
                                lineRect.y = lineRect.center.y - 2 + 1;
                                lineRect.height = 3;
                                lineRect.x += 14;
                                lineRect.xMax = context.Table.TableRect.xMax;
                                EditorGUI.DrawRect(lineRect, new Color(0, 0, 0, 0.6f));
                            });
                        }

                        if (rowIndex == context.RowCount - 1)
                        {
                            entry.Property.Tree.DelayActionUntilRepaint(() =>
                            {
                                var cell = context.Table[context.Table.ColumnCount - 1, context.Table.RowCount - context.RowCount + context.RowDragFrom];
                                var rowRect = cell.Rect;
                                rowRect.xMin = rect.xMin;
                                SirenixEditorGUI.DrawSolidRect(rowRect, new Color(0, 0, 0, 0.2f));
                            });
                        }
                    }
                }
            }
            else
            {
                GUI.Label(rect, "...", EditorStyles.centeredGreyMiniLabel);
            }

            if (!context.Attribute.IsReadOnly && Event.current.type == EventType.MouseDown && Event.current.button == 1 && rect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Insert 1 above"), false, () => ApplyArrayModifications(entry, arr => MultiDimArrayUtilities.InsertOneRowAbove(arr, rowIndex)));
                menu.AddItem(new GUIContent("Insert 1 below"), false, () => ApplyArrayModifications(entry, arr => MultiDimArrayUtilities.InsertOneRowBelow(arr, rowIndex)));
                menu.AddItem(new GUIContent("Duplicate"), false, () => ApplyArrayModifications(entry, arr => MultiDimArrayUtilities.DuplicateRow(arr, rowIndex)));
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, () => ApplyArrayModifications(entry, arr => MultiDimArrayUtilities.DeleteRow(arr, rowIndex)));
                menu.ShowAsContext();
            }
        }

        private void DrawColumn(Rect rect, IPropertyValueEntry<TArray> entry, Context context, int columnIndex)
        {
            if (columnIndex < context.ColCount)
            {
                GUI.Label(rect, columnIndex.ToString(), SirenixGUIStyles.LabelCentered);

                // Handle Column dragging.
                if (!context.Attribute.IsReadOnly)
                {
                    var id = GUIUtility.GetControlID(FocusType.Passive);
                    if (GUI.enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
                    {
                        GUIHelper.RemoveFocusControl();
                        GUIUtility.hotControl = id;
                        EditorGUIUtility.SetWantsMouseJumping(1);
                        Event.current.Use();
                        context.ColumnDragFrom = columnIndex;
                        context.ColumnDragTo = columnIndex;
                        context.dragStartPos = Event.current.mousePosition;
                    }
                    else if (GUIUtility.hotControl == id)
                    {
                        if ((context.dragStartPos - Event.current.mousePosition).sqrMagnitude > 5 * 5)
                        {
                            context.IsDraggingColumn = true;
                        }
                        if (Event.current.type == EventType.MouseDrag)
                        {
                            Event.current.Use();
                        }
                        else if (Event.current.type == EventType.MouseUp)
                        {
                            GUIUtility.hotControl = 0;
                            EditorGUIUtility.SetWantsMouseJumping(0);
                            Event.current.Use();
                            context.IsDraggingColumn = false;

                            ApplyArrayModifications(entry, arr => MultiDimArrayUtilities.MoveColumn(arr, context.ColumnDragFrom, context.ColumnDragTo));
                        }
                    }

                    if (context.IsDraggingColumn && Event.current.type == EventType.Repaint)
                    {
                        float mouseX = Event.current.mousePosition.x;
                        if (mouseX > rect.x - 1 && mouseX < rect.x + rect.width + 1)
                        {
                            Rect arrowRect;
                            if (mouseX > rect.x + rect.width * 0.5f)
                            {
                                arrowRect = rect.AlignRight(16);
                                arrowRect.height = 16;
                                arrowRect.y -= 13;
                                arrowRect.x += 8;
                                context.ColumnDragTo = columnIndex;
                            }
                            else
                            {
                                arrowRect = rect.AlignLeft(16);
                                arrowRect.height = 16;
                                arrowRect.y -= 13;
                                arrowRect.x -= 8;
                                context.ColumnDragTo = columnIndex - 1;
                            }

                            entry.Property.Tree.DelayActionUntilRepaint(() =>
                            {
                                //GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
                                GUI.DrawTexture(arrowRect, EditorIcons.ArrowDown.Active);
                                //GL.sRGBWrite = false;

                                var lineRect = arrowRect;
                                lineRect.x = lineRect.center.x - 2 + 1;
                                lineRect.width = 3;
                                lineRect.y += 14;
                                lineRect.yMax = context.Table.TableRect.yMax;
                                EditorGUI.DrawRect(lineRect, new Color(0, 0, 0, 0.6f));
                            });
                        }

                        if (columnIndex == context.ColCount - 1)
                        {
                            entry.Property.Tree.DelayActionUntilRepaint(() =>
                            {
                                var cell = context.Table[context.Table.ColumnCount - context.ColCount + context.ColumnDragFrom, context.Table.RowCount - 1];
                                var rowRect = cell.Rect;
                                rowRect.yMin = rect.yMin;
                                SirenixEditorGUI.DrawSolidRect(rowRect, new Color(0, 0, 0, 0.2f));
                            });
                        }
                    }
                }
            }
            else
            {
                GUI.Label(rect, "-", EditorStyles.centeredGreyMiniLabel);
            }

            if (!context.Attribute.IsReadOnly && Event.current.type == EventType.MouseDown && Event.current.button == 1 && rect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Insert 1 left"), false, () => ApplyArrayModifications(entry, arr => MultiDimArrayUtilities.InsertOneColumnLeft(arr, columnIndex)));
                menu.AddItem(new GUIContent("Insert 1 right"), false, () => ApplyArrayModifications(entry, arr => MultiDimArrayUtilities.InsertOneColumnRight(arr, columnIndex)));
                menu.AddItem(new GUIContent("Duplicate"), false, () => ApplyArrayModifications(entry, arr => MultiDimArrayUtilities.DuplicateColumn(arr, columnIndex)));
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, () => ApplyArrayModifications(entry, arr => MultiDimArrayUtilities.DeleteColumn(arr, columnIndex)));
                menu.ShowAsContext();
            }
        }

        private void ApplyArrayModifications(IPropertyValueEntry<TArray> entry, Func<TElement[,], TElement[,]> modification)
        {
            for (int i = 0; i < entry.Values.Count; i++)
            {
                int localI = i;
                var newArr = modification((entry.Values[localI] as TElement[,]));
                entry.Property.Tree.DelayActionUntilRepaint(() =>
                {
                    entry.Values[localI] = (TArray)(object)newArr;
                });
            }
        }

        private void DrawElement(Rect rect, IPropertyValueEntry<TArray> entry, Context context, int x, int y)
        {
            if (x < context.ColCount && y < context.RowCount)
            {
                bool showMixedValue = false;
                if (entry.Values.Count != 1)
                {
                    for (int i = 1; i < entry.Values.Count; i++)
                    {
                        var a = (entry.Values[i] as TElement[,])[x, y];
                        var b = (entry.Values[i - 1] as TElement[,])[x, y];

                        if (!CompareElement(a, b))
                        {
                            showMixedValue = true;
                            break;
                        }
                    }
                }

                EditorGUI.showMixedValue = showMixedValue;
                EditorGUI.BeginChangeCheck();
                var prevValue = context.Value[x, y];
                TElement value;

                if (context.DrawElement != null)
                {
                    value = context.DrawElement(rect, prevValue);
                }
                else
                {
                    value = DrawElement(rect, prevValue);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0; i < entry.Values.Count; i++)
                    {
                        (entry.Values[i] as TElement[,])[x, y] = value;
                    }

                    entry.Values.ForceMarkDirty();
                }
            }
        }

        /// <summary>
        /// Compares the element.
        /// </summary>
        protected virtual bool CompareElement(TElement a, TElement b)
        {
            return EqualityComparer<TElement>.Default.Equals(a, b);
        }

        /// <summary>
        /// Draws a table cell element.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="value">The input value.</param>
        /// <returns>The output value.</returns>
        protected abstract TElement DrawElement(Rect rect, TElement value);
    }

    internal static class TwoDimensionalEnumArrayDrawerLocator
    {
        [InitializeOnLoadMethod]
        private static void RegisterDrawer()
        {
            DrawerLocator.RegisterCustomDrawerLocator(new CustomDrawerLocator((valueType, attributeType) =>
            {
                if (attributeType != null)
                {
                    return null;
                }

                if (valueType == null)
                {
                    return null;
                }

                if (valueType.IsArray && valueType.GetArrayRank() == 2)
                {
                    Type drawerType;
                    if (valueType.GetElementType().IsEnum)
                    {
                        drawerType = typeof(TwoDimensionalEnumArrayDrawer<,>)
                            .MakeGenericType(valueType, valueType.GetElementType());
                    }
                    else if (valueType.GetElementType().InheritsFrom(typeof(UnityEngine.Object)))
                    {
                        drawerType = typeof(TwoDimensionalUnityObjectArrayDrawer<,>)
                            .MakeGenericType(valueType, valueType.GetElementType());
                    }
                    else
                    {
                        drawerType = typeof(TwoDimensionalGenericArrayDrawer<,>)
                            .MakeGenericType(valueType, valueType.GetElementType());
                    }

                    return new DrawerInfo(drawerType, valueType, null);
                }

                return null;
            }));
        }
    }

    [OdinDrawer]
    [DrawerPriority(0, 0, 0.9)]
    internal class TwoDimensionalGenericArrayDrawer<TArray, TElement> : TwoDimensionalArrayDrawer<TArray, TElement>
        where TArray : IList
    {
        private static string drawElementErrorMessage =
            "Odin doesn't know how to draw a table matrix for this particular type. Make a custom DrawElementMethod via the TableMatrix attribute like so:" + "\n" +
            "" + "\n" +
            "[TableMatrix(DrawElementMethod = \"DrawElement\")]" + "\n" +
            "public " + typeof(TElement).GetNiceName() + "[,] myTable" + "\n" +
            "" + "\n" +
            "static " + typeof(TElement).GetNiceName() + " DrawElement(Rect rect, " + typeof(TElement).GetNiceName() + " value)" + "\n" +
            "{" + "\n" +
            "   // Draw and modify the value in the rect provided using classes such as:" + "\n" +
            "   // GUI, EditorGUI, SirenixEditorFields and SirenixEditorGUI." + "\n" +
            "   return newValue;" + "\n" +
            "}";

        protected internal override void OnBeforeDrawTable(IPropertyValueEntry<TArray> entry, Context context, GUIContent label)
        {
            if (context.DrawElement == null && context.ErrorMessage == null)
            {
                context.ErrorMessage = drawElementErrorMessage;
            }
        }

        protected override void DrawPropertyLayout(IPropertyValueEntry<TArray> entry, GUIContent label)
        {
            base.DrawPropertyLayout(entry, label);
        }

        /// <summary>
        /// Draws the element.
        /// </summary>
        protected override TElement DrawElement(Rect rect, TElement value)
        {
            return value;
        }
    }

    [OdinDrawer]
    internal class TwoDimensionalUnityObjectArrayDrawer<TArray, TElement> : TwoDimensionalArrayDrawer<TArray, TElement>
        where TArray : IList
        where TElement : UnityEngine.Object
    {
        protected override TElement DrawElement(Rect rect, TElement value)
        {
            bool ediable = !this.TableMatrixAttribute.IsReadOnly;
            value = SirenixEditorFields.PreviewObjectField(rect, value, false, ediable, ediable);
            return value;
        }

        protected override bool CompareElement(TElement a, TElement b)
        {
            return a == b;
        }
    }

    [OdinDrawer]
    internal class TwoDimensionalEnumArrayDrawer<TArray, TElement> : TwoDimensionalArrayDrawer<TArray, TElement>
        where TArray : IList
    {
        protected override TElement DrawElement(Rect rect, TElement value)
        {
            return (TElement)(object)SirenixEditorFields.EnumDropdown(rect.Padding(4), null, (Enum)(object)value, null);
        }
    }

    [OdinDrawer]
    internal class TwoDimensionalAnimationCurveArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, AnimationCurve> where TArray : IList
    {
        protected override AnimationCurve DrawElement(Rect rect, AnimationCurve value)
        {
            if (value == null)
            {
                if (GUI.Button(rect.Padding(2), "Null - Create Animation Curve", EditorStyles.objectField))
                {
                    value = new AnimationCurve();
                }
                return value;
            }

            return EditorGUI.CurveField(rect.Padding(2), value);
        }
    }

    [OdinDrawer]
    internal class TwoDimensionalGuidArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Guid> where TArray : IList
    {
        protected override Guid DrawElement(Rect rect, Guid value)
        {
            return SirenixEditorFields.GuidField(rect.Padding(2), value);
        }
    }

    [OdinDrawer]
    internal class TwoDimensionalLayerMaskArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, LayerMask> where TArray : IList
    {
        protected override LayerMask DrawElement(Rect rect, LayerMask value)
        {
            return SirenixEditorFields.LayerMaskField(rect.Padding(2), value);
        }
    }

    [OdinDrawer]
    internal class TwoDimensionalStringArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, string> where TArray : IList
    {
        private static GUIStyle style = null;

        protected override string DrawElement(Rect rect, string value)
        {
            if (style == null)
            {
                style = new GUIStyle(EditorStyles.textField);
                style.alignment = TextAnchor.MiddleCenter;
            }

            return EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width + 1, rect.height + 1), value, style);
        }
    }

    [OdinDrawer]
    internal class TwoDimensionalBoolArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, bool> where TArray : IList
    {
        protected override bool DrawElement(Rect rect, bool value)
        {
            if (Event.current.type == EventType.Repaint)
            {
                return EditorGUI.Toggle(rect.AlignCenter(16, 16), value);
            }
            else
            {
                return EditorGUI.Toggle(rect, value);
            }
        }
    }

    [OdinDrawer]
    internal class TwoDimensionalIntArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, int> where TArray : IList
    {
        protected override int DrawElement(Rect rect, int value)
        {
            return SirenixEditorFields.IntField(rect.Padding(2), value);
        }
    }

    [OdinDrawer]
    internal class TwoDimensionalLongArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, long> where TArray : IList
    {
        protected override long DrawElement(Rect rect, long value)
        {
            return SirenixEditorFields.LongField(rect.Padding(2), value);
        }
    }

    [OdinDrawer]
    internal class TwoDimensionalFloatArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, float> where TArray : IList
    {
        protected override float DrawElement(Rect rect, float value)
        {
            return SirenixEditorFields.FloatField(rect.Padding(2), value);
        }
    }

    [OdinDrawer]
    internal class TwoDimensionalDoubleArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, double> where TArray : IList
    {
        protected override double DrawElement(Rect rect, double value)
        {
            return SirenixEditorFields.DoubleField(rect.Padding(2), value);
        }
    }

    [OdinDrawer]
    internal class TwoDimensionalDecimalArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, decimal> where TArray : IList
    {
        protected override decimal DrawElement(Rect rect, decimal value)
        {
            return SirenixEditorFields.DecimalField(rect.Padding(2), value);
        }
    }

    [OdinDrawer]
    internal class TwoDimensionalVector2ArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Vector2> where TArray : IList
    {
        protected override Vector2 DrawElement(Rect rect, Vector2 value)
        {
            return SirenixEditorFields.Vector2Field(rect.Padding(2), value);
        }
    }

    [OdinDrawer]
    internal class TwoDimensionalVector3ArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Vector3> where TArray : IList
    {
        protected override Vector3 DrawElement(Rect rect, Vector3 value)
        {
            return SirenixEditorFields.Vector3Field(rect.Padding(2), value);
        }
    }

    [OdinDrawer]
    internal class TwoDimensionalVector4ArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Vector4> where TArray : IList
    {
        protected override Vector4 DrawElement(Rect rect, Vector4 value)
        {
            return SirenixEditorFields.Vector4Field(rect.Padding(2), value);
        }
    }

    [OdinDrawer]
    internal class TwoDimensionalColorArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Color> where TArray : IList
    {
        protected override Color DrawElement(Rect rect, Color value)
        {
            return SirenixEditorFields.ColorField(rect.Padding(2), value);
        }
    }

    [OdinDrawer]
    internal class TwoDimensionalQuaternionArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Quaternion> where TArray : IList
    {
        protected override Quaternion DrawElement(Rect rect, Quaternion value)
        {
            return SirenixEditorFields.RotationField(rect.Padding(2), value, QuaternionDrawMode.Eulers);
        }
    }
}
#endif