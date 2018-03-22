#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UnityDecoratorAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.Utilities.Editor;
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Draws all Unity DecoratorDrawers within prepend attribute drawers within Odin.
    /// </summary>
    [DrawerPriority(0, 1, 0)]
    public sealed class UnityDecoratorAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint> : OdinAttributeDrawer<TAttribute>
        where TDrawer : UnityEditor.DecoratorDrawer, new()
        where TAttribute : TAttributeConstraint
        where TAttributeConstraint : PropertyAttribute
    {
        private static readonly FieldInfo InternalAttributeFieldInfo = typeof(TDrawer).GetField("m_Attribute", Flags.InstanceAnyVisibility);
        private static readonly ValueSetter<TDrawer, Attribute> SetAttribute;

        private TDrawer drawer = new TDrawer();

        /// <summary>
        /// Initializes the <see cref="UnityDecoratorAttributeDrawer{TDrawer, TAttribute}"/> class.
        /// </summary>
        static UnityDecoratorAttributeDrawer()
        {
            if (InternalAttributeFieldInfo == null)
            {
                Debug.LogError("Could not find the internal Unity field 'DecoratorDrawer.m_Attribute'; UnityDecoratorDrawer alias '" + typeof(UnityDecoratorAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint>).GetNiceName() + "' has been disabled.");
            }
            else
            {
                SetAttribute = EmitUtilities.CreateInstanceFieldSetter<TDrawer, Attribute>(InternalAttributeFieldInfo);
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(InspectorProperty property, TAttribute attribute, GUIContent label)
        {
            if (SetAttribute == null)
            {
                SirenixEditorGUI.ErrorMessageBox("Could not find the internal Unity field 'DecoratorDrawer.m_Attribute'; UnityDecoratorDrawer alias '" + typeof(UnityDecoratorAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint>).GetNiceName() + "' has been disabled.");
                return;
            }

            SetAttribute(ref this.drawer, attribute);

            float height = this.drawer.GetHeight();
            var position = EditorGUILayout.GetControlRect(false, height);

            this.drawer.OnGUI(position);
            this.CallNextDrawer(property, label);
        }
    }
}
#endif