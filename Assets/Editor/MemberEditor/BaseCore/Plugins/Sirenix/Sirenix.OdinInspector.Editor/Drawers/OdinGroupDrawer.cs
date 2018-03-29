#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="InspectorPropertyGroupDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// <para>
    /// Base class for all group drawers. Use this class to create your own custom group drawers. OdinGroupDrawer are used to group multiple properties together using an attribute.
    /// </para>
    ///
    /// <para>
    /// Note that all box group attributes needs to inherit from the <see cref="PropertyGroupAttribute"/>
    /// </para>
    ///
    /// <para>
    /// Remember to provide your custom drawer with an <see cref="Sirenix.OdinInspector.Editor.OdinDrawerAttribute"/>
    /// in order for it to be located by the <see cref="DrawerLocator"/>.
    /// </para>
    ///
    /// </summary>
    ///
    /// <remarks>
    /// Checkout the manual for more information.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    /// public class MyBoxGroupAttribute : PropertyGroupAttribute
    /// {
    ///     public MyBoxGroupAttribute(string group, int order = 0) : base(group, order)
    ///     {
    ///     }
    /// }
    ///
    /// // Remember to wrap your custom group drawer within a #if UNITY_EDITOR condition, or locate the file inside an Editor folder.
    /// [OdinDrawer]
    /// public class BoxGroupAttributeDrawer : OdinGroupDrawer&lt;MyBoxGroupAttribute&gt;
    /// {
    ///     protected override void DrawPropertyGroupLayout(InspectorProperty property, MyBoxGroupAttribute attribute, GUIContent label)
    ///     {
    ///         GUILayout.BeginVertical("box");
    ///         for (int i = 0; i &lt; property.Children.Count; i++)
    ///         {
    ///             InspectorUtilities.DrawProperty(property.Children[i]);
    ///         }
    ///         GUILayout.EndVertical();
    ///     }
    /// }
    ///
    /// // Usage:
    /// public class MyComponent : MonoBehaviour
    /// {
    ///     [MyBoxGroup("MyGroup")]
    ///     public int A;
    ///
    ///     [MyBoxGroup("MyGroup")]
    ///     public int B;
    ///
    ///     [MyBoxGroup("MyGroup")]
    ///     public int C;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="OdinAttributeDrawer{TAttribute}"/>
    /// <seealso cref="OdinAttributeDrawer{TAttribute, TValue}"/>
    /// <seealso cref="OdinValueDrawer{T}"/>
    /// <seealso cref="OdinDrawer"/>
    /// <seealso cref="InspectorProperty"/>
    /// <seealso cref="OdinDrawerAttribute"/>
    /// <seealso cref="DrawerPriorityAttribute"/>
    /// <seealso cref="DrawerLocator"/>
    /// <seealso cref="InspectorUtilities"/>
    /// <seealso cref="PropertyTree"/>
    /// <seealso cref="Sirenix.Utilities.Editor.GUIHelper"/>
    /// <seealso cref="Sirenix.Utilities.Editor.SirenixEditorGUI"/>
    public abstract class OdinGroupDrawer<TGroupAttribute> : OdinDrawer where TGroupAttribute : PropertyGroupAttribute
    {
        /// <summary>
        /// Drawing properties using GUICallType.GUILayout and overriding DrawPropertyGroupLayout is the default behavior.
        /// But you can also draw the property group the "good" old Unity way, by overriding and implementing
        /// GetRectHeight and DrawPropertyGroupRect. Just make sure to override GUICallType as well and return GUICallType.Rect
        /// </summary>
        protected virtual GUICallType GUICallType { get { return GUICallType.GUILayout; } }

        /// <summary>
        /// Draws the actual property. This method is called by this.DrawProperty(...)
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="label">The label. This can be null, so make sure your drawer supports that.</param>
        /// <exception cref="System.NotImplementedException">Case GUILayout is neither Rect of GUILayout.</exception>
        protected sealed override void DrawPropertyImplementation(InspectorProperty property, GUIContent label)
        {
            if (property.Info.PropertyType != PropertyType.Group)
            {
                GUILayout.Label(label, "Property " + property.Name + " is not a property group and cannot be drawn with the drawer " + this.GetType().GetNiceName() + ".");
            }

            TGroupAttribute attribute = property.Info.GetAttribute<TGroupAttribute>();

            if (attribute == null)
            {
                GUILayout.Label(label, "Property group " + property.Name + " does not have an attribute of the required type " + typeof(TGroupAttribute).GetNiceName() + ".");
                return;
            }

            switch (this.GUICallType)
            {
                case GUICallType.GUILayout:
                    this.DrawPropertyGroupLayout(property, attribute, label);
                    break;

                case GUICallType.Rect:
                    float height = this.GetRectHeight(property, attribute, label);
                    Rect position = EditorGUILayout.GetControlRect(false, height);
                    this.DrawPropertyGroupRect(position, property, attribute, label);
                    break;

                default:
                    throw new NotImplementedException(this.GUICallType.ToString());
            }
        }

        /// <summary>
        /// Return the GUI height of the property. This method is called by DrawPropertyImplementation if the GUICallType is set to Rect, which is not the default.
        /// If the GUICallType is set to Rect, both GetRectHeight and DrawPropertyGroupRect needs to be implemented.
        /// If the GUICallType is set to GUILayout, implementing DrawPropertyGroupLayout will suffice.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="label">The label. This can be null, so make sure your drawer supports that.</param>
        /// <returns>Returns EditorGUIUtility.singleLineHeight by default.</returns>
        protected virtual float GetRectHeight(InspectorProperty property, TGroupAttribute attribute, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        /// <summary>
        /// Draws the property group in the Rect provided. This method does not support the GUILayout, and is only called by DrawPropertyImplementation if the GUICallType is set to Rect, which is not the default.
        /// If the GUICallType is set to Rect, both GetRectHeight and DrawPropertyGroupRect needs to be implemented.
        /// If the GUICallType is set to GUILayout, implementing DrawPropertyGroupLayout will suffice.
        /// </summary>
        /// <param name="position">The position rect.</param>
        /// <param name="property">The property.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="label">The label. This can be null, so make sure your drawer supports that.</param>
        protected virtual void DrawPropertyGroupRect(Rect position, InspectorProperty property, TGroupAttribute attribute, GUIContent label)
        {
            EditorGUI.LabelField(position, label, "The DrawPropertyGroupRect method has not been implemented for the drawer of type '" + this.GetType().GetNiceName() + "'.");
        }

        /// <summary>
        /// Draws the property group with GUILayout support. This method is called from DrawPropertyImplementation if the GUICallType is set to GUILayout, which is the default.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="label">The label. This can be null, so make sure your drawer supports that.</param>
        protected virtual void DrawPropertyGroupLayout(InspectorProperty property, TGroupAttribute attribute, GUIContent label)
        {
            EditorGUILayout.LabelField(label, "The DrawPropertyGroupLayout method has not been implemented for the drawer of type '" + this.GetType().GetNiceName() + "'.");
        }
    }
}
#endif