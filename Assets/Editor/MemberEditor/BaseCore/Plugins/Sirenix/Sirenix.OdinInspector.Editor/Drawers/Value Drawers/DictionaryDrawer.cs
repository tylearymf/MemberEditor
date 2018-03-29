#if UNITY_EDITOR
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Sirenix.Serialization;

    /// <summary>
    /// Property drawer for <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    [OdinDrawer]
    public class DictionaryDrawer<TDictionary, TKey, TValue> : OdinValueDrawer<TDictionary> where TDictionary : IDictionary<TKey, TValue>
    {
        private const string CHANGE_ID = "DICTIONARY_DRAWER";
        private static readonly bool KeyIsValueType = typeof(TKey).IsValueType;
        private static GUIStyle addKeyPaddingStyle;

        private static GUIStyle AddKeyPaddingStyle
        {
            get
            {
                if (addKeyPaddingStyle == null)
                {
                    addKeyPaddingStyle = new GUIStyle("CN Box")
                    {
                        overflow = new RectOffset(0, 0, 1, 0),
                        fixedHeight = 0,
                        stretchHeight = false,
                        padding = new RectOffset(10, 10, 10, 10)
                    };
                }

                return addKeyPaddingStyle;
            }
        }

        [ShowOdinSerializedPropertiesInInspector]
        private class TempKeyValue
        {
            [ShowInInspector]
            public TKey Key;

            [ShowInInspector]
            public TValue Value;
        }

        private class Context
        {
            public GUIPagingHelper Paging = new GUIPagingHelper();
            public GeneralDrawerConfig Config;
            public LocalPersistentContext<bool> Toggled;
            public float KeyWidthOffset;
            public bool ShowAddKeyGUI = false;
            public bool? NewKewIsValid;
            public string NewKeyErrorMessage;
            public TKey NewKey;
            public TValue NewValue;
            public DictionaryHandler<TDictionary, TKey, TValue> DictionaryHandler;
            public GUIContent Label;
            public DictionaryDrawerSettings AttrSettings;
            public bool DisableAddKey;

            public TempKeyValue TempKeyValue;
            public IPropertyValueEntry<TKey> TempKeyEntry;
            public IPropertyValueEntry<TValue> TempValueEntry;

            public GUIStyle ListItemStyle = new GUIStyle(GUIStyle.none)
            {
                padding = new RectOffset(7, 20, 3, 3)
            };
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<TDictionary> entry, GUIContent label)
        {
            var context = entry.Property.Context.Get(this, "context", (Context)null);
            if (context.Value == null)
            {
                context.Value = new Context();
                context.Value.Toggled = entry.Context.GetPersistent(this, "Toggled", GeneralDrawerConfig.Instance.OpenListsByDefault);
                context.Value.KeyWidthOffset = 130;
                context.Value.Label = label ?? new GUIContent(typeof(TDictionary).GetNiceName());
                context.Value.AttrSettings = entry.Property.Info.GetAttribute<DictionaryDrawerSettings>() ?? new DictionaryDrawerSettings();
                context.Value.DisableAddKey = entry.Property.Tree.HasPrefabs && !entry.GetDictionaryHandler().SupportsPrefabModifications;

                if (!context.Value.DisableAddKey)
                {
                    context.Value.TempKeyValue = new TempKeyValue();

                    var tree = PropertyTree.Create(context.Value.TempKeyValue);
                    tree.UpdateTree();

                    context.Value.TempKeyEntry = (IPropertyValueEntry<TKey>)tree.GetPropertyAtPath("Key").ValueEntry;
                    context.Value.TempValueEntry = (IPropertyValueEntry<TValue>)tree.GetPropertyAtPath("Value").ValueEntry;
                }
            }

            context.Value.DictionaryHandler = (DictionaryHandler<TDictionary, TKey, TValue>)entry.GetDictionaryHandler();
            context.Value.Config = GeneralDrawerConfig.Instance;
            context.Value.Paging.NumberOfItemsPerPage = context.Value.Config.NumberOfItemsPrPage;
            context.Value.ListItemStyle.padding.right = !entry.IsEditable || context.Value.AttrSettings.IsReadOnly ? 4 : 20;

            //if (!IsSupportedKeyType)
            //{
            //    var message = entry.Property.Context.Get(this, "error_message", (string)null);
            //    var detailedMessage = entry.Property.Context.Get(this, "error_message_detailed", (string)null);
            //    var folded = entry.Property.Context.Get(this, "error_message_folded", true);

            //    if (message.Value == null)
            //    {
            //        string str = "";

            //        if (label != null)
            //        {
            //            str += label.text + "\n\n";
            //        }

            //        str += "The dictionary key type '" + typeof(TKey).GetNiceFullName() + "' is not supported in prefab instances. Expand this box to see which key types are supported.";

            //        message.Value = str;
            //    }

            //    if (detailedMessage.Value == null)
            //    {
            //        var sb = new StringBuilder("The following key types are supported:");

            //        sb.AppendLine()
            //          .AppendLine();

            //        foreach (var type in DictionaryKeyUtility.GetPersistentPathKeyTypes())
            //        {
            //            sb.AppendLine(type.GetNiceName());
            //        }

            //        sb.AppendLine("Enums of any type");

            //        detailedMessage.Value = sb.ToString();
            //    }

            //    folded.Value = SirenixEditorGUI.DetailedMessageBox(message.Value, detailedMessage.Value, MessageType.Error, folded.Value);

            //    return;
            //}

            SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
            {
                context.Value.Paging.Update(elementCount: entry.Property.Children.Count);
                this.DrawToolbar(entry, context.Value);
                context.Value.Paging.Update(elementCount: entry.Property.Children.Count);

                if (!context.Value.DisableAddKey && context.Value.AttrSettings.IsReadOnly == false)
                {
                    this.DrawAddKey(entry, context.Value);
                }

                float t;
                GUIHelper.BeginLayoutMeasuring();
                if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(entry.Property, this), context.Value.Toggled.Value, out t))
                {
                    var rect = SirenixEditorGUI.BeginVerticalList(false);
                    if (context.Value.AttrSettings.DisplayMode == DictionaryDisplayOptions.OneLine)
                    {
                        var maxWidth = rect.width - 90;
                        rect.xMin = context.Value.KeyWidthOffset + 22;
                        rect.xMax = rect.xMin + 10;
                        context.Value.KeyWidthOffset = context.Value.KeyWidthOffset + SirenixEditorGUI.SlideRect(rect).x;

                        if (Event.current.type == EventType.Repaint)
                        {
                            context.Value.KeyWidthOffset = Mathf.Clamp(context.Value.KeyWidthOffset, 90, maxWidth);
                        }

                        if (context.Value.Paging.ElementCount != 0)
                        {
                            var headerRect = SirenixEditorGUI.BeginListItem(false);
                            {
                                GUILayout.Space(14);
                                if (Event.current.type == EventType.Repaint)
                                {
                                    GUI.Label(headerRect.SetWidth(context.Value.KeyWidthOffset), context.Value.AttrSettings.KeyLabel, SirenixGUIStyles.LabelCentered);
                                    GUI.Label(headerRect.AddXMin(context.Value.KeyWidthOffset), context.Value.AttrSettings.ValueLabel, SirenixGUIStyles.LabelCentered);
                                    SirenixEditorGUI.DrawSolidRect(headerRect.AlignBottom(1), SirenixGUIStyles.BorderColor);
                                }
                            }
                            SirenixEditorGUI.EndListItem();
                        }
                    }

                    this.DrawElements(entry, label, context.Value);
                    SirenixEditorGUI.EndVerticalList();
                }
                SirenixEditorGUI.EndFadeGroup();

                // Draw borders
                var outerRect = GUIHelper.EndLayoutMeasuring();
                if (t > 0.01f && Event.current.type == EventType.Repaint)
                {
                    Color col = SirenixGUIStyles.BorderColor;
                    outerRect.yMin -= 1;
                    SirenixEditorGUI.DrawBorders(outerRect, 1, col);
                    col.a *= t;
                    if (context.Value.AttrSettings.DisplayMode == DictionaryDisplayOptions.OneLine)
                    {
                        // Draw Slide Rect Border
                        outerRect.width = 1;
                        outerRect.x += context.Value.KeyWidthOffset + 13;
                        SirenixEditorGUI.DrawSolidRect(outerRect, col);
                    }
                }
            }
            SirenixEditorGUI.EndIndentedVertical();
        }

        private void DrawAddKey(IPropertyValueEntry<TDictionary> entry, Context context)
        {
            if (entry.IsEditable == false || context.AttrSettings.IsReadOnly)
            {
                return;
            }

            if (SirenixEditorGUI.BeginFadeGroup(context, context.ShowAddKeyGUI))
            {
                GUILayout.BeginVertical(AddKeyPaddingStyle);
                {
                    if (typeof(TKey) == typeof(string) && context.NewKey == null)
                    {
                        context.NewKey = (TKey)(object)"";
                        context.NewKewIsValid = null;
                    }

                    if (context.NewKewIsValid == null)
                    {
                        context.NewKewIsValid = CheckKeyIsValid(entry, context.NewKey, out context.NewKeyErrorMessage);
                    }

                    InspectorUtilities.BeginDrawPropertyTree(context.TempKeyEntry.Property.Tree, false);

                    // Key
                    {
                        //context.TempKeyValue.key = context.NewKey;
                        context.TempKeyEntry.Property.Update();

                        EditorGUI.BeginChangeCheck();

                        context.TempKeyEntry.Property.Draw();

                        bool changed1 = EditorGUI.EndChangeCheck();
                        bool changed2 = context.TempKeyEntry.ApplyChanges();

                        if (changed1 || changed2)
                        {
                            context.NewKey = context.TempKeyValue.Key;
                            EditorApplication.delayCall += () => context.NewKewIsValid = null;
                            GUIHelper.RequestRepaint();
                        }
                    }

                    // Value
                    {
                        //context.TempKeyValue.value = context.NewValue;
                        context.TempValueEntry.Property.Update();
                        context.TempValueEntry.Property.Draw();
                        context.TempValueEntry.ApplyChanges();
                        context.NewValue = context.TempKeyValue.Value;
                    }

                    context.TempKeyEntry.Property.Tree.InvokeDelayedActions();
                    var changed = context.TempKeyEntry.Property.Tree.ApplyChanges();

                    if (changed)
                    {
                        context.NewKey = context.TempKeyValue.Key;
                        EditorApplication.delayCall += () => context.NewKewIsValid = null;
                        GUIHelper.RequestRepaint();
                    }

                    InspectorUtilities.EndDrawPropertyTree(context.TempKeyEntry.Property.Tree);

                    GUIHelper.PushGUIEnabled(GUI.enabled && context.NewKewIsValid.Value);
                    if (GUILayout.Button(context.NewKewIsValid.Value ? "Add" : context.NewKeyErrorMessage))
                    {
                        context.DictionaryHandler.SetValue(context.NewKey, context.NewValue);
                        EditorApplication.delayCall += () => context.NewKewIsValid = null;
                        GUIHelper.RequestRepaint();

                        entry.Property.Tree.DelayActionUntilRepaint(() =>
                        {
                            context.NewValue = default(TValue);
                            context.TempKeyValue.Value = default(TValue);
                            context.TempValueEntry.Update();
                        });
                    }
                    GUIHelper.PopGUIEnabled();
                }
                GUILayout.EndVertical();
            }
            SirenixEditorGUI.EndFadeGroup();
        }

        private void DrawToolbar(IPropertyValueEntry<TDictionary> entry, Context context)
        {
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                if (entry.ListLengthChangedFromPrefab) GUIHelper.PushIsBoldLabel(true);

                if (context.Config.HideFoldoutWhileEmpty && context.Paging.ElementCount == 0)
                {
                    GUILayout.Label(context.Label, GUILayoutOptions.ExpandWidth(false));
                }
                else
                {
                    context.Toggled.Value = SirenixEditorGUI.Foldout(context.Toggled.Value, context.Label);
                }

                if (entry.ListLengthChangedFromPrefab) GUIHelper.PopIsBoldLabel();

                GUILayout.FlexibleSpace();

                // Item Count
                if (context.Config.ShowItemCount)
                {
                    if (entry.ValueState == PropertyValueState.CollectionLengthConflict)
                    {
                        int min = entry.Values.Min(x => x.Count);
                        int max = entry.Values.Max(x => x.Count);
                        GUILayout.Label(min + " / " + max + " items", EditorStyles.centeredGreyMiniLabel);
                    }
                    else
                    {
                        GUILayout.Label(context.Paging.ElementCount == 0 ? "Empty" : context.Paging.ElementCount + " items", EditorStyles.centeredGreyMiniLabel);
                    }
                }

                bool hidePaging =
                        context.Config.HidePagingWhileCollapsed && context.Toggled.Value == false ||
                        context.Config.HidePagingWhileOnlyOnePage && context.Paging.PageCount == 1;

                if (!hidePaging)
                {
                    var wasEnabled = GUI.enabled;
                    bool pagingIsRelevant = context.Paging.IsEnabled && context.Paging.PageCount != 1;

                    GUI.enabled = wasEnabled && pagingIsRelevant && !context.Paging.IsOnFirstPage;
                    if (SirenixEditorGUI.ToolbarButton(EditorIcons.ArrowLeft, true))
                    {
                        if (Event.current.button == 0)
                        {
                            context.Paging.CurrentPage--;
                        }
                        else
                        {
                            context.Paging.CurrentPage = 0;
                        }
                    }

                    GUI.enabled = wasEnabled && pagingIsRelevant;
                    var width = GUILayoutOptions.Width(10 + context.Paging.PageCount.ToString().Length * 10);
                    context.Paging.CurrentPage = EditorGUILayout.IntField(context.Paging.CurrentPage + 1, width) - 1;
                    GUILayout.Label(GUIHelper.TempContent("/ " + context.Paging.PageCount));

                    GUI.enabled = wasEnabled && pagingIsRelevant && !context.Paging.IsOnLastPage;
                    if (SirenixEditorGUI.ToolbarButton(EditorIcons.ArrowRight, true))
                    {
                        if (Event.current.button == 0)
                        {
                            context.Paging.CurrentPage++;
                        }
                        else
                        {
                            context.Paging.CurrentPage = context.Paging.PageCount - 1;
                        }
                    }

                    GUI.enabled = wasEnabled && context.Paging.PageCount != 1;
                    if (context.Config.ShowExpandButton)
                    {
                        if (SirenixEditorGUI.ToolbarButton(context.Paging.IsEnabled ? EditorIcons.ArrowDown : EditorIcons.ArrowUp, true))
                        {
                            context.Paging.IsEnabled = !context.Paging.IsEnabled;
                        }
                    }
                    GUI.enabled = wasEnabled;
                }
                if (!context.DisableAddKey && context.AttrSettings.IsReadOnly != true)
                {
                    if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
                    {
                        context.ShowAddKeyGUI = !context.ShowAddKeyGUI;
                    }
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        private static GUIStyle oneLineMargin;

        private static GUIStyle OneLineMargin
        {
            get
            {
                if (oneLineMargin == null)
                {
                    oneLineMargin = new GUIStyle() { margin = new RectOffset(8, 0, 0, 0) };
                }
                return oneLineMargin;
            }
        }

        private static GUIStyle headerMargin;

        private static GUIStyle HeaderMargin
        {
            get
            {
                if (headerMargin == null)
                {
                    headerMargin = new GUIStyle() { margin = new RectOffset(40, 0, 0, 0) };
                }
                return headerMargin;
            }
        }

        private void DrawElements(IPropertyValueEntry<TDictionary> entry, GUIContent label, Context context)
        {
            for (int i = context.Paging.StartIndex; i < context.Paging.EndIndex; i++)
            {
                var keyValuePairProperty = entry.Property.Children[i];
                var keyValuePairEntry = (PropertyDictionaryElementValueEntry<TDictionary, TKey, TValue>)keyValuePairProperty.BaseValueEntry;

                Rect rect = SirenixEditorGUI.BeginListItem(false, context.ListItemStyle);
                {
                    if (context.AttrSettings.DisplayMode != DictionaryDisplayOptions.OneLine)
                    {
                        bool defaultExpanded;
                        switch (context.AttrSettings.DisplayMode)
                        {
                            case DictionaryDisplayOptions.CollapsedFoldout:
                                defaultExpanded = false;
                                break;

                            case DictionaryDisplayOptions.ExpandedFoldout:
                                defaultExpanded = true;
                                break;

                            default:
                                defaultExpanded = SirenixEditorGUI.ExpandFoldoutByDefault;
                                break;
                        }
                        var isExpanded = keyValuePairProperty.Context.Get(this, "Expanded", defaultExpanded);

                        SirenixEditorGUI.BeginBox();
                        SirenixEditorGUI.BeginBoxHeader();
                        {
                            if (keyValuePairEntry.HasTempInvalidKey)
                            {
                                GUIHelper.PushColor(Color.red);
                            }
                            var btnRect = GUIHelper.GetCurrentLayoutRect().AlignLeft(HeaderMargin.margin.left);
                            btnRect.y += 1;
                            GUILayout.BeginVertical(HeaderMargin);
                            GUIHelper.PushIsDrawingDictionaryKey(true);

                            GUIHelper.PushLabelWidth(10);

                            InspectorUtilities.DrawProperty(keyValuePairProperty.Children[0], null);

                            GUIHelper.PopLabelWidth();

                            GUIHelper.PopIsDrawingDictionaryKey();
                            GUILayout.EndVertical();
                            if (keyValuePairEntry.HasTempInvalidKey)
                            {
                                GUIHelper.PopColor();
                            }
                            isExpanded.Value = SirenixEditorGUI.Foldout(btnRect, isExpanded.Value, GUIHelper.TempContent("Key"));
                        }
                        SirenixEditorGUI.EndBoxHeader();

                        if (SirenixEditorGUI.BeginFadeGroup(isExpanded, isExpanded.Value))
                        {
                            InspectorUtilities.DrawProperty(keyValuePairProperty.Children[1], null);
                        }
                        SirenixEditorGUI.EndFadeGroup();

                        SirenixEditorGUI.EndBox();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.BeginVertical(GUILayoutOptions.Width(context.KeyWidthOffset));
                        {
                            var keyProperty = keyValuePairProperty.Children[0];

                            if (keyValuePairEntry.HasTempInvalidKey)
                            {
                                GUIHelper.PushColor(Color.red);
                            }

                            if (context.AttrSettings.IsReadOnly) GUIHelper.PushGUIEnabled(false);

                            GUIHelper.PushIsDrawingDictionaryKey(true);
                            GUIHelper.PushLabelWidth(10);

                            InspectorUtilities.DrawProperty(keyProperty, null);

                            GUIHelper.PopLabelWidth();
                            GUIHelper.PopIsDrawingDictionaryKey();

                            if (context.AttrSettings.IsReadOnly) GUIHelper.PopGUIEnabled();

                            if (keyValuePairEntry.HasTempInvalidKey)
                            {
                                GUIHelper.PopColor();
                            }
                        }
                        GUILayout.EndVertical();
                        GUILayout.BeginVertical(OneLineMargin);
                        {
                            var valueEntry = keyValuePairProperty.Children[1];
                            var tmp = GUIHelper.ActualLabelWidth;
                            EditorGUIUtility.labelWidth = 150;
                            InspectorUtilities.DrawProperty(valueEntry, null);
                            EditorGUIUtility.labelWidth = tmp;
                        }
                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();
                    }

                    if (entry.IsEditable && !context.AttrSettings.IsReadOnly && SirenixEditorGUI.IconButton(new Rect(rect.xMax - 24 + 5, rect.y + 2 + ((int)rect.height - 23) / 2, 14, 14), EditorIcons.X))
                    {
                        context.DictionaryHandler.Remove(context.DictionaryHandler.GetKey(0, i));
                        EditorApplication.delayCall += () => context.NewKewIsValid = null;
                        GUIHelper.RequestRepaint();
                    }
                }
                SirenixEditorGUI.EndListItem();
            }

            if (context.Paging.IsOnLastPage && entry.ValueState == PropertyValueState.CollectionLengthConflict)
            {
                SirenixEditorGUI.BeginListItem(false);
                GUILayout.Label(GUIHelper.TempContent("------"), EditorStyles.centeredGreyMiniLabel);
                SirenixEditorGUI.EndListItem();
            }
        }

        private static bool CheckKeyIsValid(IPropertyValueEntry<TDictionary> entry, TKey key, out string errorMessage)
        {
            if (!KeyIsValueType && object.ReferenceEquals(key, null))
            {
                errorMessage = "Key cannot be null.";
                return false;
            }

            var keyStr = DictionaryKeyUtility.GetDictionaryKeyString(key);

            if (entry.Property.Children[keyStr] == null)
            {
                errorMessage = "";
                return true;
            }
            else
            {
                errorMessage = "An item with the same key already exists.";
                return false;
            }
        }

        //void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        //{
        //    if (property.ValueEntry.WeakSmartValue == null)
        //    {
        //        return;
        //    }
        //    if (property.ValueEntry.IsEditable && property.Children.Count > 0)
        //    {
        //        genericMenu.AddItem(new GUIContent("Clear Dictionary"), false, () =>
        //        {
        //            property.ValueEntry.GetDictionaryHandler().Clear();
        //            var context = property.Context.Get(this, "context", (Context)null);
        //            if (context.Value != null)
        //            {
        //                EditorApplication.delayCall += () => context.Value.NewKewIsValid = null;
        //            }
        //        });
        //    }
        //    else
        //    {
        //        genericMenu.AddDisabledItem(new GUIContent("Clear Dictionary"));
        //    }
        //}
    }
}
#endif