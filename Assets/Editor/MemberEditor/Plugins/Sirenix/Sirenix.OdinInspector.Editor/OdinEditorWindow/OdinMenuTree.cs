#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinMenuTree.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;
    using Sirenix.Utilities;
    using UnityEngine;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEditor;
    using Sirenix.Utilities.Editor;
    using System.Text.RegularExpressions;
    using System.Collections;

    /// <summary>
    /// OdinMenuTree provides a tree of <see cref="OdinMenuItem"/>s, and helps with selection, inserting menu items into the tree, and can handle keyboard navigation for you.
    /// </summary>
    /// <example>
    /// <code>
    /// OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: true)
    /// {
    ///     { "Home",                           this,                           EditorIcons.House       },
    ///     { "Odin Settings",                  null,                           EditorIcons.SettingsCog },
    ///     { "Odin Settings/Color Palettes",   ColorPaletteManager.Instance,   EditorIcons.EyeDropper  },
    ///     { "Odin Settings/AOT Generation",   AOTGenerationConfig.Instance,   EditorIcons.SmartPhone  },
    ///     { "Camera current",                 Camera.current                                          },
    ///     { "Some Class",                     this.someData                                           }
    /// };
    /// 
    /// tree.AddAllAssetsAtPath("Some Menu Item", "Some Asset Path", typeof(ScriptableObject), true)
    ///     .AddThumbnailIcons();
    /// 
    /// tree.AddAssetAtPath("Some Second Menu Item", "SomeAssetPath/SomeAssetFile.asset");
    /// 
    /// var customMenuItem = new OdinMenuItem(tree, "Menu Style", tree.DefaultMenuStyle);
    /// tree.MenuItems.Insert(2, customMenuItem);
    /// 
    /// tree.Add("Menu/Items/Are/Created/As/Needed", new GUIContent());
    /// tree.Add("Menu/Items/Are/Created", new GUIContent("And can be overridden"));
    /// </code>
    /// OdinMenuTrees are typically used with <see cref="OdinMenuEditorWindow"/>s but is made to work perfectly fine on its own for other use cases.
    /// OdinMenuItems can be inherited and and customized to fit your needs.
    /// <code>
    /// // Draw stuff
    /// someTree.DrawMenuTree();
    /// // Draw stuff
    /// someTree.HandleKeybaordMenuNavigation();
    /// </code>
    /// </example>
    /// <seealso cref="OdinMenuItem" />
    /// <seealso cref="OdinMenuStyle" />
    /// <seealso cref="OdinMenuTreeSelection" />
    /// <seealso cref="OdinMenuTreeExtensions" />
    /// <seealso cref="OdinMenuEditorWindow" />
    public class OdinMenuTree : IEnumerable
    {
        private readonly OdinMenuItem root;
        private readonly OdinMenuTreeSelection selection;
        private OdinMenuStyle defaultMenuStyle;

        /// <summary>
        /// Gets the selection.
        /// </summary>
        public OdinMenuTreeSelection Selection
        {
            get { return this.selection; }
        }

        /// <summary>
        /// Gets the root menu items.
        /// </summary>
        /// <value>
        /// The menu items.
        /// </value>
        public List<OdinMenuItem> MenuItems
        {
            get { return this.root.ChildMenuItems; }
        }

        internal OdinMenuItem Root
        {
            get { return this.root; }
        }

        /// <summary>
        /// Adds a menu item with the specified object instance at the the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="instance">The instance.</param>
        public void Add(string path, object instance)
        {
            this.AddObjectAtPath(path, instance);
        }

        /// <summary>
        /// Adds a menu item with the specified object instance and icon at the the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="instance">The object instance.</param>
        /// <param name="icon">The icon.</param>
        public void Add(string path, object instance, Texture icon)
        {
            this.AddObjectAtPath(path, instance).AddIcon(icon);
        }

        /// <summary>
        /// Adds a menu item with the specified object instance and icon at the the specified path.
        /// </summary>
        /// <param name="path">The menu item path.</param>
        /// <param name="instance">The object instance.</param>
        /// <param name="icon">The icon.</param>
        public void Add(string path, object instance, EditorIcon icon)
        {
            this.AddObjectAtPath(path, instance).AddIcon(icon);
        }

        /// <summary>
        /// Gets or sets the default menu item style.
        /// </summary>
        /// <value>
        /// The default menu style.
        /// </value>
        public OdinMenuStyle DefaultMenuStyle
        {
            get { return this.defaultMenuStyle; }
            set { this.defaultMenuStyle = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OdinMenuTree"/> class.
        /// </summary>
        /// <param name="supportsMultiSelect">if set to <c>true</c> [supports multi select].</param>
        public OdinMenuTree(bool supportsMultiSelect)
            : this(supportsMultiSelect, new OdinMenuStyle())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OdinMenuTree"/> class.
        /// </summary>
        /// <param name="supportsMultiSelect">if set to <c>true</c> [supports multi select].</param>
        /// <param name="defaultMenuStyle">The default menu item style.</param>
        public OdinMenuTree(bool supportsMultiSelect, OdinMenuStyle defaultMenuStyle)
        {
            this.defaultMenuStyle = defaultMenuStyle;
            this.selection = new OdinMenuTreeSelection(supportsMultiSelect);
            this.root = new OdinMenuItem(this, "root", null);
        }

        /// <summary>
        /// Enumerates the tree with a DFS.
        /// </summary>
        /// <param name="includeRootNode">if set to <c>true</c> then the invisible root menu item is included.</param>
        public IEnumerable<OdinMenuItem> EnumerateTree(bool includeRootNode = false)
        {
            return this.root.GetChildMenuItemsRecursive(includeRootNode);
        }

        /// <summary>
        /// Draws the menu tree recursively.
        /// </summary>
        public void DrawMenuTree()
        {
            foreach (var item in this.MenuItems)
            {
                item.DrawMenuItems(0);
            }
        }

        /// <summary>
        /// Updates the menu tree. This method is usually called automatically when needed.
        /// </summary>
        public void UpdateMenuTree()
        {
            this.root.UpdateMenuTreeRecursive(true);
        }

        /// <summary>
        /// Handles the keybaord menu navigation. Call this at the end of your GUI scope, to prevent the menu tree from stealing input events from text fields and such.
        /// </summary>
        /// <returns>Returns true, if anything was changed via the keyboard.</returns>
        public bool HandleKeybaordMenuNavigation()
        {
            if (Event.current.type != EventType.keyDown)
            {
                return false;
            }

            GUIHelper.RequestRepaint();

            if (this.Selection.Count == 0)
            {
                OdinMenuItem next = null;
                if (Event.current.keyCode == KeyCode.DownArrow)
                {
                    next = this.MenuItems.FirstOrDefault();
                }
                else if (Event.current.keyCode == KeyCode.UpArrow)
                {
                    next = this.MenuItems.LastOrDefault();
                }
                else if (Event.current.keyCode == KeyCode.LeftAlt)
                {
                    next = this.MenuItems.FirstOrDefault();
                }
                else if (Event.current.keyCode == KeyCode.RightAlt)
                {
                    next = this.MenuItems.FirstOrDefault();
                }

                if (next != null)
                {
                    Event.current.Use();
                    next.Select();
                    return true;
                }
            }
            else
            {
                if (Event.current.keyCode == KeyCode.RightArrow)
                {
                    foreach (var curr in this.Selection.ToList())
                    {
                        curr.Toggled = true;

                        if ((Event.current.modifiers & EventModifiers.Alt) != 0)
                        {
                            foreach (var item in curr.GetChildMenuItemsRecursive(false))
                            {
                                item.Toggled = curr.Toggled;
                            }
                        }
                    }

                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.LeftArrow)
                {
                    foreach (var curr in this.Selection.ToList())
                    {
                        if (curr.Parent != null && (!curr.ChildMenuItems.Any() || curr.Parent.Toggled == false))
                        {
                            curr.Parent.Select();
                            return true;
                        }
                        else
                        {
                            curr.Toggled = false;

                            if ((Event.current.modifiers & EventModifiers.Alt) != 0)
                            {
                                foreach (var item in curr.GetChildMenuItemsRecursive(false))
                                {
                                    item.Toggled = curr.Toggled;
                                }
                            }
                        }
                    }

                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.UpArrow)
                {
                    if ((Event.current.modifiers & EventModifiers.Shift) != 0)
                    {
                        var last = this.Selection.Last();
                        var prev = last.PrevVisualMenuItem;

                        if (prev != null)
                        {
                            if (prev.IsSelected)
                            {
                                last.Deselect();
                                return true;

                            }
                            else
                            {
                                prev.Select(true);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        var prev = this.Selection.Last().PrevVisualMenuItem;
                        if (prev != null)
                        {
                            prev.Select();
                            return true;
                        }
                    }

                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.DownArrow)
                {
                    if ((Event.current.modifiers & EventModifiers.Shift) != 0)
                    {
                        var last = this.Selection.Last();
                        var next = last.NextVisualMenuItem;

                        if (next != null)
                        {
                            if (next.IsSelected)
                            {
                                last.Deselect();
                                return true;
                            }
                            else
                            {
                                next.Select(true);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        var next = this.Selection.Last().NextVisualMenuItem;
                        if (next != null)
                        {
                            next.Select();
                            return true;
                        }
                    }

                    Event.current.Use();
                }
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.MenuItems.GetEnumerator();
        }
    }
}
#endif