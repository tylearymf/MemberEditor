#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AssetListAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Utilities;
    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using System.Reflection;
    using System.Collections;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    [OdinDrawer]
    [DrawerPriority(0, 0, 3001)]
    public class AssetListAttributeOnSingleObjectDrawer<TElement> : OdinAttributeDrawer<AssetListAttribute, TElement> where TElement : UnityEngine.Object
    {
        private class CurrentContext
        {
            public List<UnityEngine.Object> AvailableAsset = new List<UnityEngine.Object>();

            public AssetListAttribute Attribute;
            public string[] Tags;
            public string[] LayerNames;
            public DirectoryInfo AssetsFolderLocation;
            public string PrettyPath;
            public Func<TElement, bool> StaticCustomIncludeMethod;
            public Func<object, TElement, bool> InstanceCustomIncludeMethod;
            public string ErrorMessage;
            public InspectorProperty Property;
            public bool IsPopulated = false;
            public double MaxSearchDurationPrFrameInMS = 1;
            public int NumberOfResultsToSearch = 0;
            public int TotalSearchCount = 0;
            public int CurrentSearchingIndex = 0;
            private IEnumerator pupulateListRotine;

            private IEnumerator PupulateListRotine()
            {
                while (true)
                {
                    if (this.IsPopulated)
                    {
                        yield return null;
                        continue;
                    }

                    HashSet<UnityEngine.Object> seenObjects = new HashSet<UnityEngine.Object>();
                    int[] layers = this.LayerNames != null ? this.LayerNames.Select(l => LayerMask.NameToLayer(l)).ToArray() : null;

                    this.AvailableAsset.Clear();

                    IEnumerable<AssetUtilities.AssetSearchResult> allAssets;
                    if (this.PrettyPath == null)
                    {
                        allAssets = AssetUtilities.GetAllAssetsOfTypeWithProgress(this.Property.ValueEntry.BaseValueType, null);
                    }
                    else
                    {
                        allAssets = AssetUtilities.GetAllAssetsOfTypeWithProgress(this.Property.ValueEntry.BaseValueType, "Assets/" + this.PrettyPath.TrimStart('/'));
                    }

                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    foreach (var p in allAssets)
                    {
                        if (sw.Elapsed.TotalMilliseconds > this.MaxSearchDurationPrFrameInMS)
                        {
                            this.NumberOfResultsToSearch = p.NumberOfResults;
                            this.CurrentSearchingIndex = p.CurrentIndex;

                            GUIHelper.RequestRepaint();
                            yield return null;
                            sw.Reset();
                            sw.Start();
                        }

                        var asset = p.Asset;

                        if (asset != null && seenObjects.Add(asset))
                        {
                            var go = asset as Component != null ? (asset as Component).gameObject : asset as GameObject == null ? null : asset as GameObject;

                            var assetName = go == null ? asset.name : go.name;

                            if (this.Attribute.AssetNamePrefix != null && assetName.StartsWith(this.Attribute.AssetNamePrefix, StringComparison.InvariantCultureIgnoreCase) == false)
                            {
                                continue;
                            }

                            if (this.AssetsFolderLocation != null)
                            {
                                var path = new DirectoryInfo(Path.GetDirectoryName(Application.dataPath + "/" + AssetDatabase.GetAssetPath(asset)));
                                if (this.AssetsFolderLocation.HasSubDirectory(path) == false)
                                {
                                    continue;
                                }
                            }

                            if (this.LayerNames != null && go == null || this.Tags != null && go == null)
                            {
                                continue;
                            }

                            if (go != null && this.Tags != null && !this.Tags.Contains(go.tag))
                            {
                                continue;
                            }

                            if (go != null && this.LayerNames != null && !layers.Contains(go.layer))
                            {
                                continue;
                            }

                            if (
                                this.StaticCustomIncludeMethod != null && !this.StaticCustomIncludeMethod(asset as TElement) ||
                                this.InstanceCustomIncludeMethod != null && !this.InstanceCustomIncludeMethod(this.Property.ParentValues[0], asset as TElement))
                            {
                                continue;
                            }

                            this.AvailableAsset.Add(asset);
                        }
                    }

                    this.IsPopulated = true;
                    GUIHelper.RequestRepaint();
                    yield return null;
                }
            }

            public void EnsureListPopulation()
            {
                if (Event.current.type == EventType.Layout)
                {
                    if (this.pupulateListRotine == null)
                    {
                        this.pupulateListRotine = this.PupulateListRotine();
                    }

                    this.pupulateListRotine.MoveNext();
                }
            }
        }

        private static GUIStyle padding;

        private static GUIStyle Padding
        {
            get
            {
                if (padding == null)
                {
                    padding = new GUIStyle() { padding = new RectOffset(5, 5, 3, 3) };
                }
                return padding;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<TElement> entry, AssetListAttribute attribute, GUIContent label)
        {
            var config = entry.Property.Context.Get(this, "Test", (CurrentContext)null);
            if (config.Value == null)
            {
                config.Value = new CurrentContext();
                config.Value.Attribute = attribute;
                config.Value.Tags = attribute.Tags != null ? attribute.Tags.Trim().Split(',').Select(i => i.Trim()).ToArray() : null;
                config.Value.LayerNames = attribute.LayerNames != null ? attribute.LayerNames.Trim().Split(',').Select(i => i.Trim()).ToArray() : null;
                config.Value.Property = entry.Property;
                if (attribute.Path != null)
                {
                    var path = attribute.Path.Trim('/', ' ');
                    path = "Assets/" + path + "/";
                    path = Application.dataPath + "/" + path;

                    config.Value.AssetsFolderLocation = new DirectoryInfo(path);

                    path = attribute.Path.TrimStart('/').TrimEnd('/');
                    config.Value.PrettyPath = "/" + path.TrimStart('/');
                }

                if (attribute.CustomFilterMethod != null)
                {
                    MethodInfo methodInfo;
                    string error;
                    if (MemberFinder.Start(entry.ParentType)
                        .IsMethod()
                        .IsNamed(attribute.CustomFilterMethod)
                        .HasReturnType<bool>()
                        .HasParameters<TElement>()
                        .TryGetMember<MethodInfo>(out methodInfo, out error))
                    {
                        if (methodInfo.IsStatic)
                        {
                            config.Value.StaticCustomIncludeMethod = (Func<TElement, bool>)Delegate.CreateDelegate(typeof(Func<TElement, bool>), methodInfo, true);
                        }
                        else
                        {
                            config.Value.InstanceCustomIncludeMethod = EmitUtilities.CreateWeakInstanceMethodCaller<bool, TElement>(methodInfo);
                        }
                    }

                    config.Value.ErrorMessage = error;
                }

                if (config.Value.ErrorMessage != null)
                {
                    // We can get away with lag on load.
                    config.Value.MaxSearchDurationPrFrameInMS = 20;
                    config.Value.EnsureListPopulation();
                    config.Value.MaxSearchDurationPrFrameInMS = 1;
                }
            }

            var currentValue = (UnityEngine.Object)entry.WeakSmartValue;

            if (config.Value.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(config.Value.ErrorMessage);
            }
            else
            {
                config.Value.EnsureListPopulation();
            }

            SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
            {
                SirenixEditorGUI.BeginHorizontalToolbar();
                if (label != null)
                {
                    GUILayout.Label(label);
                }

                GUILayout.FlexibleSpace();
                if (config.Value.PrettyPath != null)
                {
                    GUILayout.Label(config.Value.PrettyPath, SirenixGUIStyles.RightAlignedGreyMiniLabel);
                    SirenixEditorGUI.VerticalLineSeparator();
                }

                if (config.Value.IsPopulated)
                {
                    GUILayout.Label(config.Value.AvailableAsset.Count + " items", SirenixGUIStyles.RightAlignedGreyMiniLabel);
                    GUIHelper.PushGUIEnabled(GUI.enabled && (config.Value.AvailableAsset.Count > 0 && config.Value.ErrorMessage == null));
                }
                else
                {
                    GUILayout.Label("Scanning " + config.Value.CurrentSearchingIndex + " / " + config.Value.NumberOfResultsToSearch, SirenixGUIStyles.RightAlignedGreyMiniLabel);
                    GUIHelper.PushGUIEnabled(false);
                }

                SirenixEditorGUI.VerticalLineSeparator();

                bool drawConflict = entry.Property.ParentValues.Count > 1;
                if (drawConflict == false)
                {
                    var index = config.Value.AvailableAsset.IndexOf(currentValue) + 1;
                    if (index > 0)
                    {
                        GUILayout.Label(index.ToString(), SirenixGUIStyles.RightAlignedGreyMiniLabel);
                    }
                    else
                    {
                        drawConflict = true;
                    }
                }

                if (drawConflict)
                {
                    GUILayout.Label("-", SirenixGUIStyles.RightAlignedGreyMiniLabel);
                }

                if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleLeft) && config.Value.IsPopulated)
                {
                    var index = config.Value.AvailableAsset.IndexOf(currentValue) - 1;
                    index = index < 0 ? config.Value.AvailableAsset.Count - 1 : index;
                    entry.WeakSmartValue = config.Value.AvailableAsset[index];
                }

                if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleDown) && config.Value.IsPopulated)
                {
                    GenericMenu m = new GenericMenu();
                    var selected = currentValue;
                    int itemsPrPage = 40;
                    bool showPages = config.Value.AvailableAsset.Count > 50;
                    string page = "";
                    int selectedPage = (config.Value.AvailableAsset.IndexOf(entry.WeakSmartValue as UnityEngine.Object) / itemsPrPage);
                    for (int i = 0; i < config.Value.AvailableAsset.Count; i++)
                    {
                        var obj = config.Value.AvailableAsset[i];
                        if (obj != null)
                        {
                            var path = AssetDatabase.GetAssetPath(obj);
                            var name = string.IsNullOrEmpty(path) ? obj.name : path.Substring(7).Replace("/", "\\");
                            var localEntry = entry;

                            if (showPages)
                            {
                                var p = (i / itemsPrPage);
                                page = (p * itemsPrPage) + " - " + Mathf.Min(((p + 1) * itemsPrPage), config.Value.AvailableAsset.Count - 1);
                                if (selectedPage == p)
                                {
                                    page += " (contains selected)";
                                }
                                page += "/";
                            }

                            m.AddItem(new GUIContent(page + name), obj == selected, () =>
                           {
                               localEntry.Property.Tree.DelayActionUntilRepaint(() => localEntry.WeakSmartValue = obj);
                           });
                        }
                    }
                    m.ShowAsContext();
                }

                if (SirenixEditorGUI.ToolbarButton(EditorIcons.TriangleRight) && config.Value.IsPopulated)
                {
                    var index = config.Value.AvailableAsset.IndexOf(currentValue) + 1;
                    entry.WeakSmartValue = config.Value.AvailableAsset[index % config.Value.AvailableAsset.Count];
                }

                GUIHelper.PopGUIEnabled();

                SirenixEditorGUI.EndHorizontalToolbar();
                SirenixEditorGUI.BeginVerticalList();
                SirenixEditorGUI.BeginListItem(false, padding);
                this.CallNextDrawer(entry.Property, null);
                SirenixEditorGUI.EndListItem();
                SirenixEditorGUI.EndVerticalList();
            }
            SirenixEditorGUI.EndIndentedVertical();
        }
    }
}
#endif