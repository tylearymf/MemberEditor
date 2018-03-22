#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="SceneObjectsOnlyAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Draws properties marked with <see cref="SceneObjectsOnlyAttribute"/>.
    /// </summary>
    /// <seealso cref="SceneObjectsOnlyAttribute"/>
    /// <seealso cref="AssetsOnlyAttribute"/>
    [OdinDrawer]
    [DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
    public sealed class SceneObjectsOnlyAttributeDrawer<T> : OdinAttributeDrawer<SceneObjectsOnlyAttribute, T> where T : UnityEngine.Object
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<T> entry, SceneObjectsOnlyAttribute attribute, GUIContent label)
        {
            for (int i = 0; i < entry.Values.Count; i++)
            {
                var val = entry.Values[i];
                if (val != null)
                {
                    if (AssetDatabase.Contains(val))
                    {
                        string name = val.name;
                        var component = val as Component;
                        if (component != null)
                        {
                            name = "from " + component.gameObject.name;
                        }

                        SirenixEditorGUI.ErrorMessageBox(val.GetType().GetNiceName() + " " + name + " may not be an asset.");
                        break;
                    }
                }
            }

            this.CallNextDrawer(entry.Property, label);
        }
    }
}
#endif