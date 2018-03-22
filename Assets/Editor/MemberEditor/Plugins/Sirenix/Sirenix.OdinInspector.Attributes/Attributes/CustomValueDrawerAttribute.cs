//-----------------------------------------------------------------------
// <copyright file="CustomValueDrawerAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// Instead of making a new attribute, and a new drawer, for a one-time thing, you can with this attribute, make a method that acts as a custom property drawer.
    /// These drawers will out of the box have support for undo/redo and multi-selection.
    /// </summary>
    /// <example>
    /// Usage:
    /// <code>
    /// public class CustomDrawerExamples : MonoBehaviour
    /// {
    ///     public float From = 2, To = 7;
    ///
    /// [CustomValueDrawer("MyStaticCustomDrawerStatic")]
    /// public float CustomDrawerStatic;
    ///
    /// [CustomValueDrawer("MyStaticCustomDrawerInstance")]
    /// public float CustomDrawerInstance;
    ///
    /// [CustomValueDrawer("MyStaticCustomDrawerArray")]
    /// public float[] CustomDrawerArray;
    ///
    /// #if UNITY_EDITOR
    ///
    /// private static float MyStaticCustomDrawerStatic(float value, GUIContent label)
    /// {
    ///     return EditorGUILayout.Slider(value, 0f, 10f);
    /// }
    ///
    /// private float MyStaticCustomDrawerInstance(float value, GUIContent label)
    /// {
    ///     return EditorGUILayout.Slider(value, this.From, this.To);
    /// }
    ///
    /// private float MyStaticCustomDrawerArray(float value, GUIContent label)
    /// {
    ///     return EditorGUILayout.Slider(value, this.From, this.To);
    /// }
    ///
    /// #endif
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class CustomValueDrawerAttribute : Attribute
    {
        /// <summary>
        /// Name of the custom drawer method.
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Instead of making a new attribute, and a new drawer, for a one-time thing, you can with this attribute, make a method that acts as a custom property drawer.
        /// These drawers will out of the box have support for undo/redo and multi-selection.
        /// </summary>
        /// <param name="methodName">The name of the method to draw the value.</param>
        public CustomValueDrawerAttribute(string methodName)
        {
            this.MethodName = methodName;
        }
    }
}