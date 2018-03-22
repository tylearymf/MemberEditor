#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="CustomContextMenuAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using System;
    using System.Reflection;
    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using Utilities;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Adds a generic menu option to properties marked with <see cref="CustomContextMenuAttribute"/>.
    /// </summary>
    /// <seealso cref="CustomContextMenuAttribute"/>
    /// <seealso cref="DisableContextMenuAttribute"/>
    /// <seealso cref="OnInspectorGUIAttribute"/>
    [OdinDrawer]
    [DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
    public sealed class CustomContextMenuAttributeDrawer : OdinAttributeDrawer<CustomContextMenuAttribute>, IDefinesGenericMenuItems
    {
        private class ContextMenuInfo
        {
            public string ErrorMessage;
            public string Name;
            public Action<object> MethodCaller;
        }

        /// <summary>
        /// Populates the generic menu for the property.
        /// </summary>
        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            var populated = property.Context.GetGlobal("CustomContextMenu_Populated", false);

            if (populated.Value)
            {
                return;
            }
            else
            {
                populated.Value = true;
            }

            var contextMenuInfos = property.Context.GetGlobal("CustomContextMenu", (Dictionary<CustomContextMenuAttribute, ContextMenuInfo>)null);

            if (contextMenuInfos.Value != null && contextMenuInfos.Value.Count > 0)
            {
                if (genericMenu.GetItemCount() > 0)
                {
                    genericMenu.AddSeparator("");
                }

                foreach (var item in contextMenuInfos.Value.OrderBy(n => n.Key.MenuItem ?? ""))
                {
                    var info = item.Value;

                    if (info.MethodCaller == null)
                    {
                        genericMenu.AddDisabledItem(new GUIContent(item.Key.MenuItem + " (Invalid)"));
                    }
                    else
                    {
                        genericMenu.AddItem(new GUIContent(info.Name), false, () =>
                        {
                            for (int i = 0; i < property.ParentValues.Count; i++)
                            {
                                info.MethodCaller(property.ParentValues[i]);
                            }
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, CustomContextMenuAttribute attribute, GUIContent label)
        {
            var contextMenuInfos = property.Context.GetGlobal("CustomContextMenu", (Dictionary<CustomContextMenuAttribute, ContextMenuInfo>)null);
            var populated = property.Context.GetGlobal("CustomContextMenu_Populated", false);

            populated.Value = false;

            if (contextMenuInfos.Value == null)
            {
                contextMenuInfos.Value = new Dictionary<CustomContextMenuAttribute, ContextMenuInfo>();
            }

            ContextMenuInfo info;

            if (!contextMenuInfos.Value.TryGetValue(attribute, out info))
            {
                info = new ContextMenuInfo();

                var methodInfo = property.ParentType
                    .FindMember()
                    .IsMethod()
                    .IsInstance()
                    .HasNoParameters()
                    .ReturnsVoid()
                    .IsNamed(attribute.MethodName)
                    .GetMember<MethodInfo>(out info.ErrorMessage);

                if (info.ErrorMessage == null)
                {
                    info.Name = attribute.MenuItem;
                    info.MethodCaller = EmitUtilities.CreateWeakInstanceMethodCaller(methodInfo);
                }

                contextMenuInfos.Value[attribute] = info;
            }

            if (info.ErrorMessage != null)
            {
                SirenixEditorGUI.ErrorMessageBox(info.ErrorMessage);
            }

            this.CallNextDrawer(property, label);
        }
    }
}
#endif