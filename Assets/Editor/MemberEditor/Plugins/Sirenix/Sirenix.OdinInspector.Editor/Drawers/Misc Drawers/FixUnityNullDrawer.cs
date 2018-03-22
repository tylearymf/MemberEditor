#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="NullReferenceDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.Serialization;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Runtime.Serialization;
    using UnityEditor;
    using UnityEngine;

    [OdinDrawer]
    [DrawerPriority(10, 0, 0)]
    internal sealed class FixUnityNullDrawer<T> : OdinValueDrawer<T> where T : class
    {
        public override bool CanDrawTypeFilter(Type type)
        {
            return !typeof(UnityEngine.Object).IsAssignableFrom(typeof(T));
        }

        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, GUIContent label)
        {
            bool valueNeedsFixing = entry.ValueState == PropertyValueState.NullReference &&
                                    entry.SerializationBackend == SerializationBackend.Unity;

            if (valueNeedsFixing)
            {
                bool possibleRecursion = false;

                var prop = entry.Property.Parent;

                while (prop != null)
                {
                    if (prop.ValueEntry != null && (prop.ValueEntry.TypeOfValue == typeof(T) || prop.ValueEntry.BaseValueType == typeof(T)))
                    {
                        // We have a possible recursion
                        possibleRecursion = true;
                        break;
                    }

                    prop = prop.Parent;
                }

                if (possibleRecursion)
                {
                    SirenixEditorGUI.ErrorMessageBox("Possible Unity serialization recursion detected; cutting off drawing pre-emptively.");
                    return; // Get out of here
                }

                // If no recursion, fix value in layout
                if (Event.current.type == EventType.Layout)
                {
                    for (int i = 0; i < entry.ValueCount; i++)
                    {
                        object value = UnitySerializationUtility.CreateDefaultUnityInitializedObject(typeof(T));
                        entry.WeakValues.ForceSetValue(i, value);
                    }

                    entry.ApplyChanges();

                    var tree = entry.Property.Tree;

                    if (tree.UnitySerializedObject != null)
                    {
                        tree.UnitySerializedObject.ApplyModifiedPropertiesWithoutUndo();
                        Undo.RecordObjects(tree.UnitySerializedObject.targetObjects, "Odin inspector value changed");
                    }

                    entry.Property.Update(true);
                }
            }

            this.CallNextDrawer(entry, label);
        }
    }
}
#endif