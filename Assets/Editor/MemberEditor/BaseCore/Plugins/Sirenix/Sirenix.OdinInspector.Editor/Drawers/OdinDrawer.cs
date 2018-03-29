#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorPropertyDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;
    using Utilities;
    using Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// <para>
    /// Base class for all Odin drawers. In order to create your own custom drawers you need to derive from one of the following drawers:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="OdinAttributeDrawer{TAttribute}"/></item>
    /// <item><see cref="OdinAttributeDrawer{TAttribute, TValue}"/></item>
    /// <item><see cref="OdinValueDrawer{T}"/></item>
    /// <item><see cref="OdinGroupDrawer{TGroupAttribute}"/></item>
    /// </list>
    /// <para>Remember to provide your custom drawer with an <see cref="Sirenix.OdinInspector.Editor.OdinDrawerAttribute"/> in order for it to be located by the <see cref="DrawerLocator"/>.</para>
    /// <para>Drawers require a <see cref="PropertyTree"/> context, and are instantiated automatically by the <see cref="DrawerLocator"/>.</para>
    /// <para>Odin supports the use of GUILayout and takes care of undo for you. It also takes care of multi-selection in many simple cases. Checkout the manual for more information.</para>
    /// </summary>
    /// <seealso cref="OdinAttributeDrawer{TAttribute}"/>
    /// <seealso cref="OdinAttributeDrawer{TAttribute, TValue}"/>
    /// <seealso cref="OdinValueDrawer{T}"/>
    /// <seealso cref="OdinGroupDrawer{TGroupAttribute}"/>
    /// <seealso cref="InspectorProperty"/>
    /// <seealso cref="Sirenix.OdinInspector.Editor.OdinDrawerAttribute"/>
    /// <seealso cref="DrawerPriorityAttribute"/>
    /// <seealso cref="DrawerLocator"/>
    /// <seealso cref="InspectorUtilities"/>
    /// <seealso cref="PropertyTree"/>
    /// <seealso cref="Sirenix.Utilities.Editor.GUIHelper"/>
    /// <seealso cref="Sirenix.Utilities.Editor.SirenixEditorGUI"/>
    public abstract class OdinDrawer
    {
        private bool odinDrawerAttributeFetched;
        private OdinDrawerAttribute odinDrawerAttribute;
        private bool initializedGuiEnabledForReadOnly;
        private bool guiEnabledForReadOnly;

        /// <summary>
        /// If true, not-editable properties will not have its GUI being disabled as otherwise would be the case.
        /// This is useful if you want some GUI to be enabled regardless of whether a property is read-only or not.
        /// This value is true when an <see cref="AllowGUIEnabledForReadonlyAttribute"/> is defined on the drawer class itself.
        /// </summary>
        protected bool AutoSetGUIEnabled
        {
            get
            {
                if (!this.initializedGuiEnabledForReadOnly)
                {
                    this.guiEnabledForReadOnly = this.GetType().IsDefined<AllowGUIEnabledForReadonlyAttribute>(inherit: true);
                    this.initializedGuiEnabledForReadOnly = true;
                }

                return this.guiEnabledForReadOnly;
            }
        }

        /// <summary>
        /// Gets the OdinDrawerAttribute defined on the class. This returns null, if no <see cref="OdinInspector.Editor.OdinDrawerAttribute"/> is defined.
        /// </summary>
        public OdinDrawerAttribute OdinDrawerAttribute
        {
            get
            {
                if (!this.odinDrawerAttributeFetched)
                {
                    this.odinDrawerAttribute = this.GetType().GetAttribute<OdinDrawerAttribute>();
                    this.odinDrawerAttributeFetched = true;
                }

                return this.odinDrawerAttribute;
            }
        }

        /// <summary>
        /// <para>Override this method in order to define custom type constraints to specify whether or not a type should be drawn by the drawer.</para>
        /// <para>Note that Odin's <see cref="DrawerLocator" /> has full support for generic class constraints, so most often you can get away with not overriding CanDrawTypeFilter.</para>
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// Returns true by default, unless overridden.
        /// </returns>
        public virtual bool CanDrawTypeFilter(Type type)
        {
            return true;
        }

        /// <summary>
        /// Draws the property using the default label found in <see cref="InspectorProperty" />
        /// This method also disables the GUI if the property is read-only and <see cref="AutoSetGUIEnabled" /> is false.
        /// </summary>
        /// <param name="property">The property.</param>
        public void DrawProperty(InspectorProperty property)
        {
            this.DrawProperty(property, property.Label);
        }

        /// <summary>
        /// Draws the property with a custom label.
        /// This method also disables the GUI if the property is read-only and <see cref="AutoSetGUIEnabled" /> is false.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="label">The label. Null is allow if you wish no label should be drawn.</param>
        public void DrawProperty(InspectorProperty property, GUIContent label)
        {
            bool enabledForReadOnly = property.ValueEntry != null ? this.AutoSetGUIEnabled : true;

            if (!enabledForReadOnly)
            {
                GUIHelper.PushGUIEnabled(GUI.enabled && property.ValueEntry.IsEditable);
            }

            this.DrawPropertyImplementation(property, label);

            if (!enabledForReadOnly)
            {
                GUIHelper.PopGUIEnabled();
            }
        }

        /// <summary>
        /// Draws the actual property. This method is called by this.DrawProperty(...)
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="label">The label. This can be null, so make sure your drawer supports that.</param>
        protected abstract void DrawPropertyImplementation(InspectorProperty property, GUIContent label);

        /// <summary>
        /// <para>Calls the next drawer in the drawer chain.</para>
        /// <para>
        /// In Odin, multiple drawers are used to draw any given property. This method calls
        /// the next drawer in the drawer chain provided by the <see cref="DrawerLocator" />.
        /// The order of the drawer chain is defined using the <see cref="DrawerPriorityAttribute" />.
        /// </para>
        /// </summary>
        /// <param name="entry">The entry with the property to draw.</param>
        /// <param name="label">The label. Null is allow if you wish no label should be drawn.</param>
        /// <returns>Returns true, if a next drawer was called, otherwise a warning message is shown in the inspector and false is returned.</returns>
        protected bool CallNextDrawer(IPropertyValueEntry entry, GUIContent label)
        {
            return this.CallNextDrawer(entry.Property, label);
        }

        /// <summary>
        /// <para>Calls the next drawer in the drawer chain.</para>
        /// <para>
        /// Odin supports multiple drawers being used to draw any given property. This method calls
        /// the next drawer in the drawer chain provided by the <see cref="DrawerLocator" />.
        /// The order of the drawer chain is defined using the <see cref="DrawerPriorityAttribute" />.
        /// </para>
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="label">The label. Null is allowed if you wish no label to be drawn.</param>
        /// <returns>Returns true, if a next drawer was called, otherwise a warning message is shown in the inspector and false is returned.</returns>
        protected bool CallNextDrawer(InspectorProperty property, GUIContent label)
        {
            var nextDrawer = DrawerLocator.GetNextDrawer(this, property);

            if (nextDrawer != null)
            {
                property.IncrementDrawerChainIndex();
                nextDrawer.DrawProperty(property, label);
                return true;
            }
            else if (property.ValueEntry != null)
            {
                GUILayout.BeginHorizontal();
                {
                    if (label != null)
                    {
                        EditorGUILayout.PrefixLabel(label);
                    }
                    SirenixEditorGUI.WarningMessageBox("There is no custom drawer defined for type '" + property.ValueEntry.TypeOfValue.GetNiceName() + "', and the type has no members to draw.");
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                {
                    if (label != null)
                    {
                        EditorGUILayout.PrefixLabel(label);
                    }
                    SirenixEditorGUI.WarningMessageBox("There is no drawer defined for property " + property.NiceName + " of type " + property.Info.PropertyType + ".");
                }
                GUILayout.EndHorizontal();
            }

            return false;
        }
    }
}
#endif