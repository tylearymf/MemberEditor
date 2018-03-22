#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="OdinDrawer.cs" company="Sirenix IVS">
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
    /// Base class for all value drawers. Use this class to create your own custom drawers for any specific type.
    /// </para>
    ///
    /// <para>
    /// Remember to provide your custom drawer with an <see cref="Sirenix.OdinInspector.Editor.OdinDrawerAttribute"/>
    /// in order for it to be located by the <see cref="DrawerLocator"/>.
    /// </para>
    ///
    /// <para>
    /// Odin supports the use of GUILayout and takes care of undo for you. It also takes care of multi-selection
    /// in many simple cases. Checkout the manual for more information on handling multi-selection.
    /// </para>
    /// </summary>
    ///
    /// <remarks>
    /// Checkout the manual for more information.
    /// </remarks>
    ///
    /// <example>
    /// <code>
    /// public class MyCustomBaseType
    /// {
    ///
    /// }
    ///
    /// public class MyCustomType : MyCustomBaseType
    /// {
    ///
    /// }
    ///
    /// // Remember to wrap your custom attribute drawer within a #if UNITY_EDITOR condition, or locate the file inside an Editor folder.
    /// [OdinDrawer]
    /// public sealed class MyCustomBaseTypeDrawer&lt;T&gt; : OdinValueDrawer&lt;T&gt; where T : MyCustomBaseType
    /// {
    ///     protected override void DrawPropertyLayout(IPropertyValueEntry&lt;T&gt; entry, GUIContent label)
    ///     {
    ///         T value = entry.SmartValue;
    ///         // Draw your custom drawer here using GUILayout and EditorGUILAyout.
    ///     }
    /// }
    ///
    /// // Usage:
    /// // Both values will be drawn using the MyCustomBaseTypeDrawer
    /// public class MyComponent : SerializedMonoBehaviour
    /// {
    ///     public MyCustomBaseType A;
    ///
    ///     public MyCustomType B;
    /// }
    /// </code>
    /// </example>
    ///
    /// <example>
    /// <para>
    /// Odin uses multiple drawers to draw any given property, and the order in which these drawers are
    /// called are defined using the <see cref="DrawerPriorityAttribute"/>.
    /// Your custom drawer injects itself into this chain of drawers based on its <see cref="DrawerPriorityAttribute"/>.
    /// If no <see cref="DrawerPriorityAttribute"/> is defined, a priority is generated automatically based on the type of the drawer.
    /// Each drawer can ether choose to draw the property or not, or pass on the responsibility to the
    /// next drawer by calling CallNextDrawer(). An example of this is provided in the documentation for <see cref="OdinAttributeDrawer{TAttribute, TValue}"/>.
    /// </para>
    ///
    /// <para>
    /// This means that there is no guarantee that your drawer will be called, sins other drawers
    /// could have a higher priority than yours and choose not to call CallNextDrawer().
    /// </para>
    ///
    /// <para>
    /// To avoid this, you can tell Odin, that your drawer is a PrependDecorator or an AppendDecorator drawer (see <see cref="OdinDrawerBehaviour"/>) as shown in the example shows below.
    /// Prepend and append decorators are always drawn and are also ordered by the <see cref="OdinDrawerBehaviour"/>.
    /// </para>
    ///
    /// <para>
    /// Note that Odin's <see cref="DrawerLocator" /> have full support for generic class constraints,
    /// and if that is not enough, you can also add additional type constraints by overriding CanDrawTypeFilter(Type type).
    /// </para>
    ///
    /// <para>
    /// Also note that all custom property drawers needs to handle cases where the label provided by the DrawPropertyLayout is null,
    /// otherwise exceptions will be thrown when in cases where the label is hidden. For instance when [HideLabel] is used, or the property is drawn within a list where labels are also not shown.
    /// </para>
    ///
    /// <code>
    /// // [OdinDrawer(OdinDrawerBehaviour.DrawProperty)] // default
    /// // [OdinDrawer(OdinDrawerBehaviour.AppendDecorator)]
    /// [OdinDrawer(OdinDrawerBehaviour.PrependDecorator)]
    /// [DrawerPriority(DrawerPriorityLevel.AttributePriority)]
    /// public sealed class MyCustomTypeDrawer&lt;T&gt; : OdinValueDrawer&lt;T&gt; where T : MyCustomType
    /// {
    ///     public override bool CanDrawTypeFilter(Type type)
    ///     {
    ///         return type != typeof(SomeType);
    ///     }
    ///
    ///     protected override void DrawPropertyLayout(IPropertyValueEntry&lt;T&gt; entry, GUIContent label)
    ///     {
    ///         T value = entry.SmartValue;
    ///         // Draw property here.
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="OdinAttributeDrawer{TAttribute, TValue}"/>
    /// <seealso cref="OdinAttributeDrawer{TAttribute}"/>
    /// <seealso cref="OdinValueDrawer{T}"/>
    /// <seealso cref="OdinGroupDrawer{TGroupAttribute}"/>
    /// <seealso cref="OdinDrawer"/>
    /// <seealso cref="InspectorProperty"/>
    /// <seealso cref="OdinDrawerAttribute"/>
    /// <seealso cref="DrawerPriorityAttribute"/>
    /// <seealso cref="DrawerLocator"/>
    /// <seealso cref="InspectorUtilities"/>
    /// <seealso cref="PropertyTree"/>
    /// <seealso cref="Sirenix.Utilities.Editor.GUIHelper"/>
    /// <seealso cref="Sirenix.Utilities.Editor.SirenixEditorGUI"/>
    public abstract class OdinValueDrawer<T> : OdinDrawer
    {
        /// <summary>
        /// Drawing properties using GUICallType.GUILayout and overriding DrawPropertyLayout is the default behavior.
        /// But you can also draw the property the "good" old Unity way, by overriding and implementing
        /// GetRectHeight and DrawPropertyRect. Just make sure to override GUICallType as well and return GUICallType.Rect
        /// </summary>
        protected virtual GUICallType GUICallType { get { return GUICallType.GUILayout; } }

        /// <summary>
        /// Draws the actual property. This method is called by this.DrawProperty(...)
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="label">The label. This can be null, so make sure your drawer supports that.</param>
        /// <exception cref="System.ArgumentNullException">property</exception>
        /// <exception cref="System.ArgumentException">The given property to draw at path '" + property.Path + "' does not have a value of required type " + typeof(IPropertyValueEntry).GetNiceName() + ".</exception>
        protected sealed override void DrawPropertyImplementation(InspectorProperty property, GUIContent label)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            IPropertyValueEntry<T> castEntry = property.ValueEntry as IPropertyValueEntry<T>;

            if (castEntry == null)
            {
                property.Update();
                castEntry = property.ValueEntry as IPropertyValueEntry<T>;
            }

            if (castEntry == null)
            {
                throw new ArgumentException("The given property to draw at path '" + property.Path + "' does not have a value of required type " + typeof(IPropertyValueEntry<T>).GetNiceName() + ".");
            }

            this.DrawProperty(castEntry, label);
        }

        private void DrawProperty(IPropertyValueEntry<T> entry, GUIContent label)
        {
            switch (this.GUICallType)
            {
                case GUICallType.GUILayout:
                    {
                        this.DrawPropertyLayout(entry, label);
                    }
                    break;

                case GUICallType.Rect:
                    {
                        float height = this.GetRectHeight(entry, label);
                        Rect position = EditorGUILayout.GetControlRect(false, height);
                        this.DrawPropertyRect(position, entry, label);
                    }
                    break;

                default:
                    throw new NotImplementedException(this.GUICallType.ToString() + " has not been implemented.");
            }
        }

        /// <summary>
        /// Draws the property group with GUILayout support. This method is called from DrawPropertyImplementation if the GUICallType is set to GUILayout, which is the default.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="label">The label. This can be null, so make sure your drawer supports that.</param>
        protected virtual void DrawPropertyLayout(IPropertyValueEntry<T> entry, GUIContent label)
        {
            if (label != null)
            {
                EditorGUILayout.LabelField(label, new GUIContent("The DrawPropertyLayout method has not been implemented for the drawer of type '" + this.GetType().GetNiceName() + "'."));
            }
            else
            {
                EditorGUILayout.LabelField(new GUIContent("The DrawPropertyLayout method has not been implemented for the drawer of type '" + this.GetType().GetNiceName() + "'."));
            }
        }

        /// <summary>
        /// Draws the property in the Rect provided. This method does not support the GUILayout, and is only called by DrawPropertyImplementation if the GUICallType is set to Rect, which is not the default.
        /// If the GUICallType is set to Rect, both GetRectHeight and DrawPropertyRect needs to be implemented.
        /// If the GUICallType is set to GUILayout, implementing DrawPropertyLayout will suffice.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="label">The label. This can be null, so make sure your drawer supports that.</param>
        protected virtual void DrawPropertyRect(Rect position, IPropertyValueEntry<T> entry, GUIContent label)
        {
            if (label != null)
            {
                EditorGUI.LabelField(position, label, new GUIContent("The DrawPropertyRect method has not been implemented for the drawer of type '" + this.GetType().GetNiceName() + "'."));
            }
            else
            {
                EditorGUI.LabelField(position, new GUIContent("The DrawPropertyRect method has not been implemented for the drawer of type '" + this.GetType().GetNiceName() + "'."));
            }
        }

        /// <summary>
        /// Return the GUI height of the property. This method is called by DrawPropertyImplementation if the GUICallType is set to Rect, which is not the default.
        /// If the GUICallType is set to Rect, both GetRectHeight and DrawPropertyRect needs to be implemented.
        /// If the GUICallType is set to GUILayout, implementing DrawPropertyLayout will suffice.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="label">The label. This can be null, so make sure your drawer supports that.</param>
        /// <returns>Returns EditorGUIUtility.singleLineHeight by default.</returns>
        protected virtual float GetRectHeight(IPropertyValueEntry<T> entry, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
#endif