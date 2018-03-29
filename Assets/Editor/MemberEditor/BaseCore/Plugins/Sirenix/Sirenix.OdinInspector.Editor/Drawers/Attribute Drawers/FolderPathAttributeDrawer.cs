#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="FolderPathAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using System.IO;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    [OdinDrawer]
    public sealed class FolderPathAttributeDrawer : OdinAttributeDrawer<FolderPathAttribute, string>, IDefinesGenericMenuItems
    {
        private class FolderPathContext
        {
            public StringMemberHelper ParentPath;
            public bool Exists;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<string> entry, FolderPathAttribute attribute, GUIContent label)
        {
            // Create a property context for the parent path.
            InspectorProperty parentProperty = entry.Property.FindParent(PropertyValueCategory.Member, true);
            PropertyContext<FolderPathContext> context;
            if (entry.Context.Get<FolderPathContext>(this, "FolderPath", out context))
            {
                context.Value = new FolderPathContext()
                {
                    ParentPath = new StringMemberHelper(parentProperty.ParentType, attribute.ParentFolder),
                };

                context.Value.Exists = PathExists(entry.SmartValue, context.Value.ParentPath.GetString(entry));
            }

            // Display evt. errors in creating context.
            if (context.Value.ParentPath.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(context.Value.ParentPath.ErrorMessage);
            }

            // Display required valid path error if enabled.
            if (attribute.RequireValidPath && context.Value.Exists == false)
            {
                SirenixEditorGUI.ErrorMessageBox("The path is invalid.");
            }

            // Draw field.
            EditorGUI.BeginChangeCheck();
            entry.SmartValue = SirenixEditorFields.FolderPathField(label, entry.SmartValue, context.Value.ParentPath.GetString(entry), attribute.AbsolutePath, attribute.UseBackslashes);

            // Update existing check
            if (EditorGUI.EndChangeCheck() && attribute.RequireValidPath)
            {
                context.Value.Exists = PathExists(entry.SmartValue, context.Value.ParentPath.GetString(entry));
            }
        }

        private bool PathExists(string path, string parent)
        {
            if (path.IsNullOrWhitespace())
            {
                return false;
            }

            if (parent.IsNullOrWhitespace() == false)
            {
                path = Path.Combine(parent, path);
            }

            return Directory.Exists(path);
        }

        /// <summary>
        /// Adds customs generic menu options.
        /// </summary>
        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            var parentProperty = property.FindParent(PropertyValueCategory.Member, true);
            IPropertyValueEntry<string> entry = (IPropertyValueEntry<string>)property.ValueEntry;
            string parent = entry.Context.Get<FolderPathContext>(this, "FolderPath", (FolderPathContext)null).Value.ParentPath.GetString(parentProperty);

            if (genericMenu.GetItemCount() > 0)
            {
                genericMenu.AddSeparator("");
            }

            bool exists = false;
            string createDirectoryPath = entry.SmartValue;

            if (createDirectoryPath.IsNullOrWhitespace() == false)
            {
                // Get the absolute path.
                if (Path.IsPathRooted(createDirectoryPath) == false)
                {
                    if (parent.IsNullOrWhitespace() == false)
                    {
                        createDirectoryPath = Path.Combine(parent, createDirectoryPath);
                    }

                    createDirectoryPath = Path.GetFullPath(createDirectoryPath);
                }

                exists = Directory.Exists(createDirectoryPath);
            }

            string showInExplorerPath = createDirectoryPath;
            if (showInExplorerPath.IsNullOrWhitespace())
            {
                if (parent.IsNullOrWhitespace() == false)
                {
                    // Use parent path instead.
                    showInExplorerPath = Path.GetFullPath(parent);
                }
                else
                {
                    // Default to Unity project path.
                    showInExplorerPath = Path.GetDirectoryName(Application.dataPath);
                }
            }

            // Find first existing path to open.
            while (showInExplorerPath.IsNullOrWhitespace() == false && Directory.Exists(showInExplorerPath) == false)
            {
                showInExplorerPath = Path.GetDirectoryName(createDirectoryPath);
            }

            // Show in explorer
            if (showInExplorerPath.IsNullOrWhitespace() == false)
            {
                genericMenu.AddItem(new GUIContent("Show in explorer"), false, () => Application.OpenURL(showInExplorerPath));
            }
            else
            {
                genericMenu.AddDisabledItem(new GUIContent("Show in explorer"));
            }

            // Create path
            if (exists || createDirectoryPath.IsNullOrWhitespace()) // Disable the create path option, if the directory already exists, or the path is invalid.
            {
                genericMenu.AddDisabledItem(new GUIContent("Create directory"));
            }
            else
            {
                genericMenu.AddItem(new GUIContent("Create directory"), false, () =>
                {
                    Directory.CreateDirectory(createDirectoryPath);
                    AssetDatabase.Refresh();
                });
            }
        }
    }
}
#endif