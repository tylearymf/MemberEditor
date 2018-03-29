#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AssetListAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Serialization;
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
    /// Draws properties marked with <see cref="AssetListAttribute"/>.
    /// Displays a configurable list of assets, where each item can be enabled or disabled.
    /// </summary>
    /// <seealso cref="AssetListAttribute"/>
    /// <seealso cref="AssetsOnlyAttribute"/>
    /// <seealso cref="SceneObjectsOnlyAttribute"/>
    /// <seealso cref="RequiredAttribute"/>
    /// <seealso cref="ValidateInputAttribute"/>
    [OdinDrawer]
    [DrawerPriority(DrawerPriorityLevel.AttributePriority)]
    public sealed class AssetListAttributeDrawer<TList, TElement> : OdinAttributeDrawer<AssetListAttribute, TList>, IDefinesGenericMenuItems where TList : IList<TElement> where TElement : UnityEngine.Object
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<TList> entry, AssetListAttribute attribute, GUIContent label)
        {
            var property = entry.Property;

            var assetList = property.Context.Get(this, "assetList", (AssetList)null);
            var propertyTree = property.Context.Get(this, "togglableAssetListPropertyTree", (PropertyTree)null);
            if (property.ValueEntry.WeakSmartValue == null)
            {
                return;
            }

            if (assetList.Value == null)
            {
                assetList.Value = new AssetList();
                assetList.Value.AutoPopulate = attribute.AutoPopulate;
                assetList.Value.AssetNamePrefix = attribute.AssetNamePrefix;
                assetList.Value.Tags = attribute.Tags != null ? attribute.Tags.Trim().Split(',').Select(i => i.Trim()).ToArray() : null;
                assetList.Value.LayerNames = attribute.LayerNames != null ? attribute.LayerNames.Trim().Split(',').Select(i => i.Trim()).ToArray() : null;
                assetList.Value.List = entry;
                assetList.Value.ListChanger = property.ValueEntry.GetListValueEntryChanger();
                assetList.Value.Property = entry.Property;

                if (attribute.Path != null)
                {
                    var path = attribute.Path.TrimStart('/', ' ').TrimEnd('/', ' ');
                    path = attribute.Path.Trim('/', ' ');

                    path = "Assets/" + path + "/";
                    path = Application.dataPath + "/" + path;

                    assetList.Value.AssetsFolderLocation = new DirectoryInfo(path);

                    path = attribute.Path.Trim('/', ' ');
                    assetList.Value.PrettyPath = "/" + path.TrimStart('/');
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
                            assetList.Value.StaticCustomIncludeMethod = (Func<TElement, bool>)Delegate.CreateDelegate(typeof(Func<TElement, bool>), methodInfo, true);
                        }
                        else
                        {
                            assetList.Value.InstanceCustomIncludeMethod = EmitUtilities.CreateWeakInstanceMethodCaller<bool, TElement>(methodInfo);
                        }
                    }

                    assetList.Value.ErrorMessage = error;
                }

                // We can get away with lag on load.
                assetList.Value.MaxSearchDurationPrFrameInMS = 20;
                assetList.Value.EnsureListPopulation();
                assetList.Value.MaxSearchDurationPrFrameInMS = 1;

                //assetList.Value.List = list;

                //if (propertyTree.Value == null)
                //{
                propertyTree.Value = PropertyTree.Create(assetList.Value);
                propertyTree.Value.UpdateTree();
                propertyTree.Value.GetRootProperty(0).Label = label;
                //}
            }
            else if (Event.current.type == EventType.Layout)
            {
                assetList.Value.Property = entry.Property;
                assetList.Value.EnsureListPopulation();
                assetList.Value.SetToggleValues();
            }

            if (assetList.Value.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(assetList.Value.ErrorMessage);
            }
            assetList.Value.Property = entry.Property;
            propertyTree.Value.Draw(false);

            if (Event.current.type == EventType.Used)
            {
                assetList.Value.UpdateList();
            }
        }

        /// <summary>
        /// Populates the generic menu for the property.
        /// </summary>
        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            var assetList = property.Context.Get(this, "assetList", (AssetList)null).Value;

            if (assetList == null)
            {
                return;
            }

            if (assetList.List.SmartValue.Count != assetList.ToggleableAssets.Count)
            {
                genericMenu.AddItem(new GUIContent("Include All"), false, () => { assetList.UpdateList(true); });
            }
            else
            {
                genericMenu.AddDisabledItem(new GUIContent("Include All"));
            }
        }

        [Serializable, ShowOdinSerializedPropertiesInInspector]
        private class AssetList
        {
            [HideInInspector]
            public bool AutoPopulate;

            [HideInInspector]
            public string AssetNamePrefix;

            [HideInInspector]
            public string[] LayerNames;

            [HideInInspector]
            public string[] Tags;

            [HideInInspector]
            public IPropertyValueEntry<TList> List;

            [HideInInspector]
            public PropertyListValueEntryChanger ListChanger;

            [HideInInspector]
            public DirectoryInfo AssetsFolderLocation;

            [HideInInspector]
            public string PrettyPath;

            [HideInInspector]
            public Func<TElement, bool> StaticCustomIncludeMethod;

            [HideInInspector]
            public Func<object, TElement, bool> InstanceCustomIncludeMethod;

            [HideInInspector]
            public string ErrorMessage;

            [HideInInspector]
            public InspectorProperty Property;

            public List<ToggleableAsset> ToggleableAssets
            {
                get
                {
                    return this.toggleableAssets;
                }
            }

            [SerializeField]
            [ListDrawerSettings(IsReadOnly = true, DraggableItems = false, OnTitleBarGUI = "OnListTitlebarGUI", ShowItemCount = false)]
            [DisableContextMenu(true, true)]
            [HideReferenceObjectPicker]
            private List<ToggleableAsset> toggleableAssets = new List<ToggleableAsset>();

            [SerializeField]
            [HideInInspector]
            private HashSet<TElement> toggledAssets = new HashSet<TElement>();

            [SerializeField]
            [HideInInspector]
            private Dictionary<TElement, ToggleableAsset> toggleableAssetLookup = new Dictionary<TElement, ToggleableAsset>();

            [NonSerialized]
            public bool IsPopulated = false;

            [NonSerialized]
            public double MaxSearchDurationPrFrameInMS = 1;

            [NonSerialized]
            public int NumberOfResultsToSearch = 0;

            [NonSerialized]
            public int TotalSearchCount = 0;

            [NonSerialized]
            public int CurrentSearchingIndex = 0;

            [NonSerialized]
            private IEnumerator populateListRoutine;

            private IEnumerator PopulateListRoutine()
            {
                while (true)
                {
                    if (this.IsPopulated)
                    {
                        yield return null;
                        continue;
                    }

                    HashSet<UnityEngine.Object> seenObjects = new HashSet<UnityEngine.Object>();
                    this.toggleableAssets.Clear();
                    this.toggleableAssetLookup.Clear();

                    IEnumerable<AssetUtilities.AssetSearchResult> allAssets;
                    if (this.PrettyPath == null)
                    {
                        allAssets = AssetUtilities.GetAllAssetsOfTypeWithProgress(typeof(TElement), null);
                    }
                    else
                    {
                        allAssets = AssetUtilities.GetAllAssetsOfTypeWithProgress(typeof(TElement), "Assets/" + this.PrettyPath.TrimStart('/'));
                    }

                    int[] layers = this.LayerNames != null ? this.LayerNames.Select(l => LayerMask.NameToLayer(l)).ToArray() : null;

                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    foreach (var p in allAssets)
                    {
                        if (sw.Elapsed.TotalMilliseconds > this.MaxSearchDurationPrFrameInMS)
                        {
                            this.NumberOfResultsToSearch = p.NumberOfResults;
                            this.CurrentSearchingIndex = p.CurrentIndex;

                            GUIHelper.RequestRepaint();
                            //this.SetToggleValues(startIndex);

                            yield return null;
                            sw.Reset();
                            sw.Start();
                        }

                        var asset = p.Asset;

                        if (asset != null && seenObjects.Add(asset))
                        {
                            var go = asset as Component != null ? (asset as Component).gameObject : asset as GameObject == null ? null : asset as GameObject;

                            var assetName = go == null ? asset.name : go.name;

                            if (this.AssetNamePrefix != null && assetName.StartsWith(this.AssetNamePrefix, StringComparison.InvariantCultureIgnoreCase) == false)
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

                            if (this.toggleableAssetLookup.ContainsKey(asset as TElement))
                            {
                                continue;
                            }

                            if (
                                this.StaticCustomIncludeMethod != null && !this.StaticCustomIncludeMethod(asset as TElement) ||
                                this.InstanceCustomIncludeMethod != null && !this.InstanceCustomIncludeMethod(this.Property.ParentValues[0], asset as TElement))
                            {
                                continue;
                            }

                            var toggleable = new ToggleableAsset(asset as TElement, this.AutoPopulate);

                            this.toggleableAssets.Add(toggleable);
                            this.toggleableAssetLookup.Add(asset as TElement, toggleable);
                        }
                    }

                    this.SetToggleValues();

                    this.IsPopulated = true;
                    GUIHelper.RequestRepaint();
                    yield return null;
                }
            }

            public void EnsureListPopulation()
            {
                if (Event.current.type == EventType.Layout)
                {
                    if (this.populateListRoutine == null)
                    {
                        this.populateListRoutine = this.PopulateListRoutine();
                    }

                    this.populateListRoutine.MoveNext();
                }
            }

            public void SetToggleValues(int startIndex = 0)
            {
                if (this.List.SmartValue == null)
                {
                    return;
                }

                for (int i = startIndex; i < this.toggleableAssets.Count; i++)
                {
                    if (this.toggleableAssets[i] == null || this.toggleableAssets[i].Object == null)
                    {
                        this.Rescan();
                        break;
                    }

                    this.toggleableAssets[i].Toggled = false;
                }

                for (int i = this.List.SmartValue.Count - 1; i >= startIndex; i--)
                {
                    var asset = this.List.SmartValue[i] as TElement;
                    if (asset == null)
                    {
                        this.ListChanger.RemoveListElementAt(i, "Rmoved asset from AssetList");
                    }
                    else
                    {
                        ToggleableAsset toggleable;
                        if (this.toggleableAssetLookup.TryGetValue(asset, out toggleable))
                        {
                            toggleable.Toggled = true;
                        }
                        else
                        {
                            if (this.IsPopulated)
                            {
                                this.ListChanger.RemoveListElementAt(i, "Rmoved asset from AssetList");
                            }
                        }
                    }
                }
            }

            public void Rescan()
            {
                this.IsPopulated = false;
            }

            private void OnListTitlebarGUI()
            {
                if (this.PrettyPath != null)
                {
                    GUILayout.Label(this.PrettyPath, SirenixGUIStyles.RightAlignedGreyMiniLabel);
                    SirenixEditorGUI.VerticalLineSeparator();
                }

                if (this.IsPopulated)
                {
                    GUILayout.Label(this.List.SmartValue.Count + " / " + this.toggleableAssets.Count, EditorStyles.centeredGreyMiniLabel);
                }
                else
                {
                    GUILayout.Label("Scanning " + this.CurrentSearchingIndex + " / " + this.NumberOfResultsToSearch, SirenixGUIStyles.RightAlignedGreyMiniLabel);
                }
                bool disableGUI = !this.IsPopulated;

                if (disableGUI)
                {
                    GUIHelper.PushGUIEnabled(false);
                }

                if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh) && this.IsPopulated)
                {
                    this.Rescan();
                }

                if (AssetUtilities.CanCreateNewAsset<TElement>())
                {
                    if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus) && this.IsPopulated)
                    {
                        string path = this.PrettyPath;
                        if (path == null)
                        {
                            var lastAsset = this.List.SmartValue.Count > 0 ? this.List.SmartValue[this.List.SmartValue.Count - 1] as TElement : null;
                            if (lastAsset == null)
                            {
                                var lastToggleable = this.toggleableAssets.LastOrDefault();
                                if (lastToggleable != null)
                                {
                                    lastAsset = lastToggleable.Object;
                                }
                            }
                            if (lastAsset != null)
                            {
                                path = AssetUtilities.GetAssetLocation(lastAsset);
                            }
                        }
                        AssetUtilities.CreateNewAsset<TElement>(path, null);
                        this.Rescan();
                    }
                }

                if (disableGUI)
                {
                    GUIHelper.PopGUIEnabled();
                }
            }

            public void UpdateList()
            {
                this.UpdateList(false);
            }

            public void UpdateList(bool includeAll)
            {
                if (this.List.SmartValue == null)
                {
                    return;
                }

                this.toggledAssets.Clear();
                for (int i = 0; i < this.toggleableAssets.Count; i++)
                {
                    if (includeAll || this.AutoPopulate || this.toggleableAssets[i].Toggled)
                    {
                        this.toggledAssets.Add(this.toggleableAssets[i].Object);
                    }
                }

                for (int i = this.List.SmartValue.Count - 1; i >= 0; i--)
                {
                    if (this.List.SmartValue[i] as TElement == null)
                    {
                        this.ListChanger.RemoveListElementAt(i, "Removed asset from AssetList");
                        this.Rescan();
                    }
                    else if (this.toggledAssets.Contains(this.List.SmartValue[i] as TElement) == false)
                    {
                        if (this.IsPopulated)
                        {
                            this.ListChanger.RemoveListElementAt(i, "Removed asset from AssetList");
                        }
                    }
                    else
                    {
                        this.toggledAssets.Remove(this.List.SmartValue[i] as TElement);
                    }
                }

                foreach (var asset in this.toggledAssets.GFIterator())
                {
                    this.ListChanger.AddListElement(Enumerable.Repeat(asset, this.ListChanger.ValueCount).ToArray(), "Added asset to AssetList");
                }

                this.toggledAssets.Clear();
            }
        }

        [Serializable]
        private class ToggleableAsset
        {
            [HideInInspector]
            public bool AutoToggle;

            public bool Toggled;

            public TElement Object;

            public ToggleableAsset(TElement obj, bool autoToggle)
            {
                this.AutoToggle = autoToggle;
                this.Object = obj;
            }
        }

        [OdinDrawer]
        private sealed class AssetInstanceDrawer : OdinValueDrawer<ToggleableAsset>
        {
            protected override void DrawPropertyLayout(IPropertyValueEntry<ToggleableAsset> entry, GUIContent label)
            {
                if (entry.SmartValue.AutoToggle)
                {
#pragma warning disable 0618 // Type or member is obsolete
                    SirenixEditorGUI.ObjectField(null, entry.SmartValue.Object, entry.SmartValue.Object.GetType(), false, true);
#pragma warning restore 0618 // Type or member is obsolete
                }
                else
                {
                    var rect = GUILayoutUtility.GetRect(16, 16, GUILayoutOptions.ExpandWidth(true));
                    var toggleRect = new Rect(rect.x, rect.y, 16, 16);
                    var objectFieldRect = new Rect(rect.x + 20, rect.y, rect.width - 20, 16);

                    if (Event.current.type != EventType.Repaint)
                    {
                        toggleRect.x -= 5;
                        toggleRect.y -= 5;
                        toggleRect.width += 10;
                        toggleRect.height += 10;
                    }
                    var prevChanged = GUI.changed;

                    entry.SmartValue.Toggled = GUI.Toggle(toggleRect, entry.SmartValue.Toggled, "");

                    if (prevChanged != GUI.changed)
                    {
                        entry.ApplyChanges();
                    }

                    GUIHelper.PushGUIEnabled(entry.SmartValue.Toggled);

#pragma warning disable 0618 // Type or member is obsolete
                    SirenixEditorGUI.ObjectField(objectFieldRect, null, entry.SmartValue.Object, entry.SmartValue.Object.GetType(), false, true);
#pragma warning restore 0618 // Type or member is obsolete

                    GUIHelper.PopGUIEnabled();
                }
            }
        }
    }
}
#endif