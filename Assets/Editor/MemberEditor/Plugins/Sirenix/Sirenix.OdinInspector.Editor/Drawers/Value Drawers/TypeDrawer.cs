#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="TypeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Type property drawer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [OdinDrawer]
    [DrawerPriority(0, 0, 2001)]
    public class TypeDrawer<T> : OdinValueDrawer<T> where T : Type
    {
        private static readonly TwoWaySerializationBinder Binder = new DefaultSerializationBinder();

        private class Context
        {
            public string typeNameTemp;
            public bool isValid = true;
            public string uniqueControlName;
            public bool wasFocusedControl;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, GUIContent label)
        {
            Context context;
            if (entry.Context.Get(this, "context", out context))
            {
                context.uniqueControlName = Guid.NewGuid().ToString();
                context.typeNameTemp = entry.SmartValue == null ? "" : Binder.BindToName(entry.SmartValue);
            }

            EditorGUI.BeginChangeCheck();

            if (!context.isValid)
            {
                GUIHelper.PushColor(Color.red);
            }

            GUI.SetNextControlName(context.uniqueControlName);

            context.typeNameTemp = SirenixEditorFields.TextField(label, context.typeNameTemp);

            if (!context.isValid)
            {
                GUIHelper.PopColor();
            }

            bool isFocused = GUI.GetNameOfFocusedControl() == context.uniqueControlName;
            bool defocused = false;

            if (isFocused != context.wasFocusedControl)
            {
                defocused = !isFocused;
                context.wasFocusedControl = isFocused;
            }

            if (EditorGUI.EndChangeCheck())
            {
                if (string.IsNullOrEmpty(context.typeNameTemp.Trim()))
                {
                    // String is empty
                    entry.SmartValue = null;
                    context.isValid = true;
                }
                else
                {
                    Type type = Binder.BindToType(context.typeNameTemp);

                    if (type == null)
                    {
                        type = AssemblyUtilities.GetTypeByCachedFullName(context.typeNameTemp);
                    }

                    if (type == null)
                    {
                        context.isValid = false;
                    }
                    else
                    {
                        // Use WeakSmartValue in case of a different Type-derived instance showing up somehow, so we don't get cast errors
                        entry.WeakSmartValue = type;
                        context.isValid = true;
                    }
                }
            }

            if (defocused)
            {
                // Ensure we show the full type name when the control is defocused
                context.typeNameTemp = entry.SmartValue == null ? "" : Binder.BindToName(entry.SmartValue);
                context.isValid = true;
            }
        }
    }
}
#endif