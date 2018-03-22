#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinMenuTreeExtensions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Class with utility methods for <see cref="OdinMenuTree" />s and <see cref="OdinMenuItem" />s.
    /// </summary>
    /// <example>
    /// <code>
    /// OdinMenuTree tree = new OdinMenuTree();
    /// tree.AddAllAssetsAtPath("Some Menu Item", "Some Asset Path", typeof(ScriptableObject), true)
    ///     .AddThumbnailIcons();
    /// tree.AddAssetAtPath("Some Second Menu Item", "SomeAssetPath/SomeAssetFile.asset");
    /// // etc...
    /// </code>
    /// </example>
    /// <seealso cref="OdinMenuTree" />
    /// <seealso cref="OdinMenuItem" />
    /// <seealso cref="OdinMenuStyle" />
    /// <seealso cref="OdinMenuTreeSelection" />
    /// <seealso cref="OdinMenuEditorWindow" />
    public static class OdinMenuTreeExtensions
    {
        /// <summary>
        /// Adds the menu item at the specified menu item path and populates the result list with all menu items created in order to add the menuItem at the specified path.
        /// </summary>
        /// <param name="tree">The tree instance.</param>
        /// <param name="result">The result list.</param>
        /// <param name="path">The menu item path.</param>
        /// <param name="menuItem">The menu item.</param>
        public static void AddMenuItemAtPath(this OdinMenuTree tree, List<OdinMenuItem> result, string path, OdinMenuItem menuItem)
        {
            var curr = tree.Root;

            if (!string.IsNullOrEmpty(path))
            {
                path = path.Trim('/') + "/";

                var iFrom = 0;
                var iTo = 0;

                do
                {
                    iTo = path.IndexOf('/', iFrom);
                    var name = path.Substring(iFrom, iTo - iFrom);
                    var child = curr.ChildMenuItems.FirstOrDefault(x => x.Name == name);

                    if (child == null)
                    {
                        child = new OdinMenuItem(tree, name, null);
                        curr.ChildMenuItems.Add(child);
                    }

                    result.Add(child);

                    curr = child;

                    iFrom = iTo + 1;
                } while (iTo != path.Length - 1);
            }

            var oldItem = curr.ChildMenuItems.FirstOrDefault(x => x.Name == menuItem.Name);
            if (oldItem != null)
            {
                curr.ChildMenuItems.Remove(oldItem);
                menuItem.ChildMenuItems.AddRange(oldItem.ChildMenuItems);
            }

            curr.ChildMenuItems.Add(menuItem);
            result.Add(menuItem);
        }

        /// <summary>
        /// Adds the menu item at specified menu item path, and returns all menu items created in order to add the menuItem at the specified path.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="path">The menu item path.</param>
        /// <param name="menuItem">The menu item.</param>
        /// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
        public static IEnumerable<OdinMenuItem> AddMenuItemAtPath(this OdinMenuTree tree, string path, OdinMenuItem menuItem)
        {
            List<OdinMenuItem> result = new List<OdinMenuItem>(5);
            AddMenuItemAtPath(tree, result, path, menuItem);
            return result;
        }

        /// <summary>
        /// Gets the menu item at the specified path, returns null non was found.
        /// </summary>
        public static OdinMenuItem GetMenuItem(this OdinMenuTree tree, string menuPath)
        {
            var curr = tree.Root;

            if (!string.IsNullOrEmpty(menuPath))
            {
                menuPath = menuPath.Trim('/') + "/"; // Adding the '/' Makes the algorithm simpler

                var iFrom = 0;
                var iTo = 0;

                do
                {
                    iTo = menuPath.IndexOf('/', iFrom);
                    var name = menuPath.Substring(iFrom, iTo - iFrom);
                    var child = curr.ChildMenuItems.FirstOrDefault(x => x.Name == name) ?? 
                                curr.ChildMenuItems.FirstOrDefault(x => x.SmartName == name);

                    if (child == null)
                    {
                        return null;
                    }

                    curr = child;

                    iFrom = iTo + 1;
                } while (iTo != menuPath.Length - 1);
            }

            return curr;
        }

        /// <summary>
        /// Adds all asset instances from the specified path and type into a single <see cref="OdinMenuItem"/> at the specified menu item path, and returns all menu items created in order to add the menuItem at the specified path.. 
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="menuPath">The menu item path.</param>
        /// <param name="assetFolderPath">The asset folder path.</param>
        /// <param name="type">The type of objects.</param>
        /// <param name="includeSubDirectories">Whether to search for assets in subdirectories as well.</param>
        /// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
        public static IEnumerable<OdinMenuItem> AddAllAssetsAtPathCombined(this OdinMenuTree tree, string menuPath, string assetFolderPath, Type type, bool includeSubDirectories = false)
        {
            assetFolderPath = assetFolderPath.TrimEnd('/') + "/";
            if (!assetFolderPath.ToLower().StartsWith("assets/"))
            {
                assetFolderPath = "Assets/" + assetFolderPath;
            }
            assetFolderPath = assetFolderPath.TrimEnd('/') + "/";

            var assets = AssetDatabase.GetAllAssetPaths()
                .Where(x =>
                {
                    if (includeSubDirectories)
                    {
                        return x.StartsWith(assetFolderPath, StringComparison.InvariantCultureIgnoreCase);
                    }
                    return string.Compare(System.IO.Path.GetDirectoryName(x).Trim('/'), assetFolderPath.Trim('/'), true) == 0;
                })
                .Select(x =>
                {
                    UnityEngine.Object tmp = null;

                    return (Func<object>)(() =>
                    {
                        if (tmp == null)
                        {
                            tmp = AssetDatabase.LoadAssetAtPath(x, type);
                        }

                        return tmp;
                    });
                })
                .ToList();


            string path, menu;
            SplitMenuPath(menuPath, out path, out menu);

            return tree.AddMenuItemAtPath(path, new OdinMenuItem(tree, menu, assets));
        }

        /// <summary>
        /// Adds all assets at the specified path. Each asset found gets its own menu item inside the specified menu item path.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="menuPath">The menu item path.</param>
        /// <param name="assetFolderPath">The asset folder path.</param>
        /// <param name="type">The type.</param>
        /// <param name="includeSubDirectories">Whether to search for assets in subdirectories as well.</param>
        /// <param name="flattenSubDirectories">If true, sub-directories in the assetFolderPath will no longer get its own sub-menu item at the specified menu item path.</param>
        /// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
        public static IEnumerable<OdinMenuItem> AddAllAssetsAtPath(this OdinMenuTree tree, string menuPath, string assetFolderPath, Type type, bool includeSubDirectories = false, bool flattenSubDirectories = false)
        {
            assetFolderPath = assetFolderPath.TrimEnd('/') + "/";
            if (!assetFolderPath.ToLower().StartsWith("assets/"))
            {
                assetFolderPath = "Assets/" + assetFolderPath;
            }
            assetFolderPath = assetFolderPath.TrimEnd('/') + "/";

            var assets = AssetDatabase.GetAllAssetPaths()
                .Where(x =>
                {
                    if (includeSubDirectories)
                    {
                        return x.StartsWith(assetFolderPath, StringComparison.InvariantCultureIgnoreCase);
                    }
                    return string.Compare(System.IO.Path.GetDirectoryName(x).Trim('/'), assetFolderPath.Trim('/'), true) == 0;
                });

            menuPath = menuPath ?? "";
            menuPath = menuPath.TrimStart('/');

            List<OdinMenuItem> result = new List<OdinMenuItem>(5);

            foreach (var assetPath in assets)
            {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(assetPath, type);

                if (obj == null)
                {
                    continue;
                }

                var name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                var path = menuPath;

                if (flattenSubDirectories == false)
                {
                    var subPath = (System.IO.Path.GetDirectoryName(assetPath).TrimEnd('/') + "/");
                    subPath = subPath.Substring(assetFolderPath.Length);
                    if (subPath.Length != 0)
                    {
                        path = path.Trim('/') + "/" + subPath;
                    }
                }

                path = path.Trim('/') + "/" + name;
                string menu;
                SplitMenuPath(path, out path, out menu);
                tree.AddMenuItemAtPath(result, path, new OdinMenuItem(tree, menu, obj));
            }

            return result;
        }

        /// <summary>
        /// Adds all assets at the specified path. Each asset found gets its own menu item inside the specified menu item path.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="menuPath">The menu item path.</param>
        /// <param name="assetFolderPath">The asset folder path.</param>
        /// <param name="type">The type.</param>
        /// <param name="includeSubDirectories">Whether to search for assets in subdirectories as well.</param>
        /// <param name="flattenSubDirectories">If true, sub-directories in the assetFolderPath will no longer get its own sub-menu item at the specified menu item path.</param>
        /// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
        public static IEnumerable<OdinMenuItem> AddAllAssetsAtPath(this OdinMenuTree tree, string menuPath, string assetFolderPath, bool includeSubDirectories = false, bool flattenSubDirectories = false)
        {
            return AddAllAssetsAtPath(tree, menuPath, assetFolderPath, typeof(UnityEngine.Object), includeSubDirectories, flattenSubDirectories);
        }

        /// <summary>
        /// Adds the asset at the specified menu item path and returns all menu items created in order to add in order to end up at the specified menu path.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="menuItemPath">The menu item path.</param>
        /// <param name="assetPath">The asset path.</param>
        /// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
        public static IEnumerable<OdinMenuItem> AddAssetAtPath(this OdinMenuTree tree, string menuItemPath, string assetPath)
        {
            return AddAssetAtPath(tree, menuItemPath, assetPath, typeof(UnityEngine.Object));
        }

        /// <summary>
        /// Adds the asset at the specified menu item path and returns all menu items created in order to add in order to end up at the specified menu path.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="menuItemPath">The menu item path.</param>
        /// <param name="assetPath">The asset path.</param>
        /// <param name="type">The type.</param>
        /// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
        public static IEnumerable<OdinMenuItem> AddAssetAtPath(this OdinMenuTree tree, string menuItemPath, string assetPath, Type type)
        {
            if (!assetPath.StartsWith("assets/", StringComparison.InvariantCultureIgnoreCase))
            {
                assetPath = "Assets/" + assetPath;
            }

            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(assetPath, type);

            string name;
            SplitMenuPath(menuItemPath, out menuItemPath, out name);
            return tree.AddMenuItemAtPath(menuItemPath, new OdinMenuItem(tree, name, obj));
        }

        /// <summary>
        /// Sorts the entire tree of menu items recursively by name with respects to numbers.
        /// </summary>
        public static IEnumerable<OdinMenuItem> SortMenuItemsByName(this OdinMenuTree tree)
        {
            return SortMenuItemsByName(tree.EnumerateTree(true));
        }

        /// <summary>
        /// Sorts the collection of menu items recursively by name with respects to numbers.
        /// </summary>
        public static IEnumerable<OdinMenuItem> SortMenuItemsByName(this IEnumerable<OdinMenuItem> menuItems)
        {
            Comparison<OdinMenuItem> comparer = (a, b) =>
            {
                // Compare with respect to numbers.

                var x = a.SmartName;
                var y = b.SmartName;

                var withoutNumbersX = Regex.Replace(x, @"[\d-]", string.Empty);
                var withoutNumbersY = Regex.Replace(y, @"[\d-]", string.Empty);

                var result = string.Compare(withoutNumbersX, withoutNumbersY);

                if (result == 0)
                {
                    x = x.Substring(x.LastIndexOf('/') + 1);
                    y = y.Substring(y.LastIndexOf('/') + 1);
                    x = Regex.Match(x, @"\d+").Value;
                    y = Regex.Match(y, @"\d+").Value;

                    double xVal, yVal;

                    if (double.TryParse(x, out xVal) && double.TryParse(y, out yVal))
                    {
                        return xVal.CompareTo(yVal);
                    }
                }

                return string.Compare(x, y);
            };

            return menuItems.ForEach(x => x.ChildMenuItems.Sort(comparer));
        }

        /// <summary>
        /// Adds the specified object at the specified menu item path and returns all menu items created in order to add in order to end up at the specified menu path.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="menuPath">The menu path.</param>
        /// <param name="instance">The object instance.</param>
        /// <param name="forceShowOdinSerializedMembers">Set this to true if you want Odin serialzied members such as dictionaries and generics to be shown as well.</param>
        /// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
        public static IEnumerable<OdinMenuItem> AddObjectAtPath(this OdinMenuTree tree, string menuPath, object instance, bool forceShowOdinSerializedMembers = false)
        {
            string name;
            SplitMenuPath(menuPath, out menuPath, out name);

            if (forceShowOdinSerializedMembers && !(instance as UnityEngine.Object))
            {
                return tree.AddMenuItemAtPath(menuPath, new OdinMenuItem(tree, name, new SerializedValueWrapper(instance)));
            }
            else
            {
                return tree.AddMenuItemAtPath(menuPath, new OdinMenuItem(tree, name, instance));
            }
        }

        /// <summary>
        /// Assigns the specified icon to all menu items in the collection.
        /// </summary>
        public static IEnumerable<OdinMenuItem> AddIcon(this IEnumerable<OdinMenuItem> menuItems, EditorIcon icon)
        {
            menuItems.AddIcon(icon.Highlighted, icon.Raw);

            return menuItems;
        }

        /// <summary>
        /// Assigns the specified icon to all menu items in the collection.
        /// </summary>
        public static IEnumerable<OdinMenuItem> AddIcon(this IEnumerable<OdinMenuItem> menuItems, Texture icon)
        {
            var last = menuItems.LastOrDefault();
            if (last != null)
            {
                last.Icon = icon;
            }

            return menuItems;
        }

        /// <summary>
        /// Assigns the specified icon to all menu items in the collection.
        /// </summary>
        public static IEnumerable<OdinMenuItem> AddIcon(this IEnumerable<OdinMenuItem> menuItems, Texture icon, Texture iconSelected)
        {
            var last = menuItems.LastOrDefault();
            if (last != null)
            {
                last.Icon = icon;
                last.IconSelected = iconSelected;
            }

            return menuItems;
        }

        /// <summary>
        /// Assigns the asset mini thumbnail as an icon to all menu items in the collection. If the menu items object is null then a Unity folder icon is assigned.
        /// </summary>
        public static IEnumerable<OdinMenuItem> AddThumbnailIcons(this IEnumerable<OdinMenuItem> menuItems, bool preferAssetPreviewAsIcon = false)
        {
            foreach (var item in menuItems)
            {
                var unityObject = item.ObjectInstance as UnityEngine.Object;

                if (unityObject)
                {
                    item.Icon = GUIHelper.GetAssetThumbnail(unityObject, item.GetType(), preferAssetPreviewAsIcon);
                }
                else if (item.ObjectInstance == null)
                {
                    item.Icon = EditorIcons.UnityFolderIcon;
                }
            }

            return menuItems;
        }

        private static void SplitMenuPath(string menuPath, out string path, out string name)
        {
            menuPath = menuPath.Trim('/');
            var i = menuPath.LastIndexOf('/');

            if (i == -1)
            {
                path = "";
                name = menuPath;
            }
            else
            {
                path = menuPath.Substring(0, i);
                name = menuPath.Substring(i + 1);
            }
        }

        private static bool ReplaceDollarSignWithAssetName(ref string menuItem, string name)
        {
            if (menuItem == null)
            {
                return false;
            }

            if (menuItem == "$")
            {
                menuItem = name;
            }

            if (menuItem.StartsWith("$/"))
            {
                menuItem = name + menuItem.Substring(2);
            }

            if (menuItem.EndsWith("/$"))
            {
                menuItem = menuItem.Substring(0, menuItem.Length - 1) + name;
            }

            if (menuItem.Contains("/$/"))
            {
                menuItem = menuItem.Replace("/$/", "/" + name + "/");
                return true;
            }

            return false;
        }

        [ShowOdinSerializedPropertiesInInspector]
        private class SerializedValueWrapper
        {
            private object instance;

            [HideLabel, ShowInInspector, HideReferenceObjectPicker]
            public object Instance
            {
                get { return instance; }
                set { }
            }

            public SerializedValueWrapper(object obj)
            {
                this.instance = obj;
            }
        }
    }
}

#endif