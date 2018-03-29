#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="ListDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using UnityEngine;
    using System.Collections.Generic;
    using Sirenix.Utilities.Editor;
    using System;
    using Sirenix.Utilities;
    using UnityEditor;
    using System.Linq;
    using Sirenix.Serialization;

    /// <summary>
    /// Attribute drawer for the TableList attribute
    /// </summary>
    /// <seealso cref="Sirenix.OdinInspector.TableListAttribute" />
    [OdinDrawer]
    public class TableListAttributeDrawer : OdinAttributeDrawer<TableListAttribute>
    {
        private const string CHANGE_ID = "TableList_DRAWER";

        /// <summary>
        /// <para>Only draw for IList&lt;&gt; types.</para>
        /// </summary>
        public override bool CanDrawTypeFilter(Type type)
        {
            return type.ImplementsOpenGenericInterface(typeof(IList<>));
        }

        private class Context
        {
            public GUIPagingHelper Paging;
            public bool SwitchView;
            public GUITable Table;
            public PropertyListValueEntryChanger ListChanger;
            public ObjectPicker ObjectPicker;
            public LocalPersistentContext<bool> DrawList;
            public bool WasUpdated = false;
            private bool update;

            private IEnumerable<InspectorProperty> EnumerateGroupMembers(InspectorProperty groupProperty)
            {
                for (int i = 0; i < groupProperty.Children.Count; i++)
                {
                    var info = groupProperty.Children[i].Info;
                    if (info.PropertyType != PropertyType.Group)
                    {
                        yield return groupProperty.Children[i];
                    }
                    else
                    {
                        foreach (var item in EnumerateGroupMembers(groupProperty.Children[i]))
                        {
                            yield return item;
                        }
                    }
                }
            }

            public void UpdateTable(InspectorProperty property, TableListAttribute attribute, string label)
            {
                if (this.Table == null)
                {
                    this.update = true;
                }

                if (this.SwitchView && Event.current.type == EventType.Layout)
                {
                    this.DrawList.Value = !this.DrawList.Value;
                    this.SwitchView = false;
                    if (!this.DrawList.Value)
                    {
                        this.update = true;
                    }
                }

                if (this.Paging.ElementCount != property.Children.Count)
                {
                    this.update = true;
                }

                this.Paging.Update(property.Children.Count);

                if (!this.update)
                {
                    return;
                }

                this.update = false;
                this.WasUpdated = true;

                HashSet<string> seenColumns = new HashSet<string>();
                List<InspectorProperty> columnProperties = new List<InspectorProperty>();

                for (int i = this.Paging.StartIndex; i < this.Paging.EndIndex; i++)
                {
                    var listItem = property.Children[i];

                    if (listItem.Children.Count != 0)
                    {
                        for (int j = 0; j < listItem.Children.Count; j++)
                        {
                            var child = listItem.Children[j];
                            if (seenColumns.Add(child.Name))
                            {
                                columnProperties.Add(child);
                            }
                        }
                    }
                }

                columnProperties.Sort((a, b) => (a.Info.Order + a.Index * 0.01f).CompareTo(b.Info.Order + b.Index * 0.01f));

                GUITableColumn[] columns = new GUITableColumn[columnProperties.Count + 1];

                for (int i = 0; i < columnProperties.Count; i++)
                {
                    int columnIndex = i;
                    var p = columnProperties[i];

                    TableColumnWidthAttribute attr = null;

                    if (p.Info.PropertyType == PropertyType.Group)
                    {
                        attr = EnumerateGroupMembers(p)
                            .Select(c => c.Info.GetAttribute<TableColumnWidthAttribute>())
                            .FirstOrDefault(x => x != null);
                    }
                    else
                    {
                        attr = p.Info.GetAttribute<TableColumnWidthAttribute>();
                    }

                    float width = 0;

                    if (attr != null)
                    {
                        width = attr.Width;
                    }

                    columns[i] = new GUITableColumn()
                    {
                        OnGUI = (rect, index) =>
                        {
                            var listItem = property.Children[index + this.Paging.StartIndex];
                            var listItemElement = listItem.Children[p.Name];
                            if (listItemElement != null)
                            {
                                rect.x = (int)rect.x; // Fixes text flickering
                                GUILayout.BeginArea(rect);
                                var height = EditorGUILayout.BeginVertical(SirenixGUIStyles.OdinEditorWrapper).height + 3;
                                var labelWidth = rect.width * 0.3f;
                                GUIHelper.PushLabelWidth(labelWidth);
                                listItemElement.Draw(null);
                                GUIHelper.PopLabelWidth();
                                EditorGUILayout.EndVertical();

                                if (Event.current.type == EventType.Repaint)
                                {
                                    var cell = this.Table[columnIndex, index + 2];
                                    if (cell.Height != height)
                                    {
                                        cell.Height = height;
                                        this.Table.MarkDirty();
                                    }
                                }

                                GUILayout.EndArea();
                            }
                        },
                        Width = width,
                        ColumnTitle = p.Label == null ? p.NiceName : p.Label.text
                    };
                }

                columns[columnProperties.Count] = new GUITableColumn()
                {
                    OnGUI = (rect, index) =>
                    {
                        if (Event.current.type == EventType.Repaint)
                        {
                            rect = rect.AlignCenter(14, 14);
                        }
                        if (SirenixEditorGUI.IconButton(rect, EditorIcons.X))
                        {
                            this.ListChanger.RemoveListElementAt(this.Paging.StartIndex + index, CHANGE_ID);
                        }
                    },
                    ColumnTitle = "",
                    Width = 20,
                    Resizable = false
                };

                this.Table = GUITable.Create(this.Paging.EndIndex - this.Paging.StartIndex, label, columns);

                this.Table[0, 0].OnGUI = rect =>
                {
                    var fullRect = rect;

                    rect = rect.AlignRight(20);
                    SirenixEditorGUI.DrawBorders(rect, 1, 0, 0, 0);
                    if (SirenixEditorGUI.IconButton(rect.AlignCenter(19, 19), EditorIcons.Plus))
                    {
                        if (this.ListChanger.ElementType.InheritsFrom<UnityEngine.Object>() && Event.current.modifiers == EventModifiers.Control)
                        {
                            this.ListChanger.AddListElement(new object[this.ListChanger.ValueCount], "Add Unity Null Value");
                        }
                        else
                        {
                            this.ObjectPicker.ShowObjectPicker(
                                property.Info.GetAttribute<AssetsOnlyAttribute>() == null,
                                rect,
                                property.ValueEntry.SerializationBackend == SerializationBackend.Unity);
                        }
                    }

                    rect.x -= 24;
                    rect.width = 24;
                    SirenixEditorGUI.DrawBorders(rect, 1, 0, 0, 0);
                    if (SirenixEditorGUI.IconButton(rect.AlignCenter(23, 23), EditorIcons.List))
                    {
                        this.SwitchView = true;
                    }

                    rect.x -= 24;
                    rect.width = 24;
                    SirenixEditorGUI.DrawBorders(rect, 1, 0, 0, 0);
                    if (SirenixEditorGUI.IconButton(rect.AlignCenter(20, 20), EditorIcons.Refresh))
                    {
                        this.update = true;
                    }

                    if (this.Paging.PageCount > 1)
                    {
                        rect.x -= 24;
                        rect.width = 24;
                        SirenixEditorGUI.DrawBorders(rect, 1, 0, 0, 0);

                        if (this.Paging.IsOnLastPage) GUIHelper.PushGUIEnabled(false);

                        if (SirenixEditorGUI.IconButton(rect.AlignCenter(20, 20), EditorIcons.TriangleRight))
                        {
                            property.Tree.DelayActionUntilRepaint(() =>
                            {
                                this.Paging.CurrentPage++;
                                this.update = true;
                            });
                        }

                        if (this.Paging.IsOnLastPage) GUIHelper.PopGUIEnabled();

                        rect.x -= 27;
                        rect.width = 27;
                        SirenixEditorGUI.DrawBorders(rect, 1, 0, 0, 0);
                        GUI.Label(rect.AlignMiddle(16), " / " + this.Paging.PageCount);

                        rect.x -= 35;
                        rect.width = 35;
                        SirenixEditorGUI.DrawBorders(rect, 1, 0, 0, 0);

                        EditorGUI.BeginChangeCheck();
                        this.Paging.CurrentPage = EditorGUI.IntField(rect.HorizontalPadding(4).AlignMiddle(16), this.Paging.CurrentPage + 1) - 1;
                        if (EditorGUI.EndChangeCheck())
                        {
                            this.update = true;
                        }

                        if (this.Paging.IsOnFirstPage) GUIHelper.PushGUIEnabled(false);

                        rect.x -= 24;
                        rect.width = 24;
                        SirenixEditorGUI.DrawBorders(rect, 1, 0, 0, 0);
                        if (SirenixEditorGUI.IconButton(rect.AlignCenter(20, 20), EditorIcons.TriangleLeft))
                        {
                            property.Tree.DelayActionUntilRepaint(() =>
                            {
                                this.Paging.CurrentPage--;
                                this.update = true;
                            });
                        }

                        if (this.Paging.IsOnFirstPage) GUIHelper.PopGUIEnabled();
                    }

                    fullRect.xMax = rect.xMin;

                    GUI.Label(fullRect, this.Paging.ElementCount + " items", SirenixGUIStyles.RightAlignedGreyMiniLabel);
                    GUI.Label(fullRect, label, SirenixGUIStyles.LabelCentered);
                };
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, TableListAttribute attribute, GUIContent label)
        {
            var context = property.Context.Get(this, "Context", (Context)null);
            if (context.Value == null)
            {
                context.Value = new Context();
                context.Value.ListChanger = property.ValueEntry.GetListValueEntryChanger();
                context.Value.Paging = new GUIPagingHelper();
                context.Value.Paging.NumberOfItemsPerPage = attribute.NumberOfItemsPerPage == 0 ? GeneralDrawerConfig.Instance.NumberOfItemsPrPage : attribute.NumberOfItemsPerPage;

                property.Context.GetPersistent(this, "DrawList", out context.Value.DrawList);
            }

            context.Value.ObjectPicker = ObjectPicker.GetObjectPicker(UniqueDrawerKey.Create(property, this), context.Value.ListChanger.ElementType);
            context.Value.UpdateTable(property, attribute, label == null ? string.Empty : label.text); // @todo @fix passing null to update table for label causes OutOfRangeExceptions to be thrown.

            if (context.Value.DrawList.Value)
            {
                if (GUILayout.Button("Show table"))
                {
                    context.Value.SwitchView = true;
                }

                this.CallNextDrawer(property, label);
            }
            else
            {
                if (context.Value.Table != null)
                {
                    if (context.Value.WasUpdated && Event.current.type == EventType.Repaint)
                    {
                        // Everything is messed up the first frame. Lets not show that.
                        GUIHelper.PushColor(new Color(0, 0, 0, 0));
                    }

                    context.Value.Table.DrawTable();

                    if (context.Value.WasUpdated && Event.current.type == EventType.Repaint)
                    {
                        context.Value.WasUpdated = false;
                        GUIHelper.PopColor();
                    }
                }

                if (context.Value.ObjectPicker.IsReadyToClaim && Event.current.type == EventType.Repaint)
                {
                    var value = context.Value.ObjectPicker.ClaimObject();
                    object[] values = new object[context.Value.ListChanger.ValueCount];
                    values[0] = value;
                    for (int j = 1; j < values.Length; j++)
                    {
                        values[j] = SerializationUtility.CreateCopy(value);
                    }
                    context.Value.ListChanger.AddListElement(values, CHANGE_ID);
                }
            }

            GUILayout.Space(3);
        }
    }
}
#endif