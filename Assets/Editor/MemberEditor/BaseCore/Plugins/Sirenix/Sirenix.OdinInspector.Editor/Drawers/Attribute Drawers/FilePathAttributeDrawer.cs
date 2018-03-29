#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="FilePathAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    [OdinDrawer]
    public sealed class FilePathAttributeDrawer : OdinAttributeDrawer<FilePathAttribute, string>, IDefinesGenericMenuItems
    {
        private class FilePathContext
        {
            public string ErrorMessage;
            public StringMemberHelper Parent;
            public StringMemberHelper Extensions;
            public bool Exists;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<string> entry, FilePathAttribute attribute, GUIContent label)
        {
            // Create a property context for parent path and extensions options.
            InspectorProperty parentProperty = entry.Property.FindParent(PropertyValueCategory.Member, true);
            PropertyContext<FilePathContext> context;
            if (entry.Context.Get(this, "FilePathContext", out context))
            {
                context.Value = new FilePathContext();
                context.Value.Parent = new StringMemberHelper(parentProperty.ParentType, attribute.ParentFolder, ref context.Value.ErrorMessage);
                context.Value.Extensions = new StringMemberHelper(parentProperty.ParentType, attribute.Extensions, ref context.Value.ErrorMessage);
                context.Value.Exists = PathExists(entry.SmartValue, context.Value.Parent.GetString(entry));
            }

            // Display evt. errors in creating property context.
            if (context.Value.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(context.Value.ErrorMessage);
            }

            // Display required path error, if enabled.
            if (attribute.RequireValidPath && context.Value.Exists == false)
            {
                SirenixEditorGUI.ErrorMessageBox("The path is invalid.");
            }

            // Draw the field.
            EditorGUI.BeginChangeCheck();
            entry.SmartValue = SirenixEditorFields.FilePathField(label, entry.SmartValue, context.Value.Parent.GetString(parentProperty), context.Value.Extensions.GetString(entry), attribute.AbsolutePath, attribute.UseBackslashes);

            // Update exists check.
            if (EditorGUI.EndChangeCheck() && attribute.RequireValidPath)
            {
                context.Value.Exists = PathExists(entry.SmartValue, context.Value.Parent.GetString(entry));
                GUI.changed = true;
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

            return File.Exists(path);
        }

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            var parentProperty = property.FindParent(PropertyValueCategory.Member, true);
            IPropertyValueEntry<string> entry = (IPropertyValueEntry<string>)property.ValueEntry;
            string parent = entry.Context.Get<FilePathContext>(this, "FilePathContext", (FilePathContext)null).Value.Parent.GetString(parentProperty);

            if (genericMenu.GetItemCount() > 0)
            {
                genericMenu.AddSeparator("");
            }

            string path = entry.SmartValue;

            // Create an absolute path from the current value.
            if (path.IsNullOrWhitespace() == false)
            {
                if (Path.IsPathRooted(path) == false)
                {
                    if (parent.IsNullOrWhitespace() == false)
                    {
                        path = Path.Combine(parent, path);
                    }

                    path = Path.GetFullPath(path);
                }

            }
            else if (parent.IsNullOrWhitespace() == false)
            {
                // Use the parent path instead.
                path = Path.GetFullPath(parent);
            }
            else
            {
                // Default to Unity project.
                path = Path.GetDirectoryName(Application.dataPath); 
            }

            // Find first existing directory.
            if (path.IsNullOrWhitespace() == false)
            {
                while (path.IsNullOrWhitespace() == false && Directory.Exists(path) == false)
                {
                    path = Path.GetDirectoryName(path);
                }
            }

            // Show in explorer
            if (path.IsNullOrWhitespace() == false)
            {
                genericMenu.AddItem(new GUIContent("Show in explorer"), false, () => System.Diagnostics.Process.Start(path));
            }
            else
            {
                genericMenu.AddDisabledItem(new GUIContent("Show in explorer"));
            }
        }
    }
}
#endif