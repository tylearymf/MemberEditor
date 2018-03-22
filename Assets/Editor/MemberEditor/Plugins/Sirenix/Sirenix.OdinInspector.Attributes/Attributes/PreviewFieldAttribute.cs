//-----------------------------------------------------------------------
// <copyright file="PreviewFieldAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// <para>
    /// Draws a square ObjectField which renders a preview for UnityEngine.Object types.
    /// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values. 
    /// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
    /// </para>
    /// <para>
    /// These object fields can also be selectively enabled and customized globally from the Odin preferences window.
    /// </para>
    /// </summary>
    /// <example>
    /// <para>The following example shows how PreviewField is applied to a few property fields.</para>
    /// <code>
    /// public MyComponent : MonoBehaviour
    /// {
    ///		[PreviewField]
    ///		public UnityEngine.Object SomeObject;
    ///		
    ///		[PreviewField]
    ///		public Texture SomeTexture;
    ///
    ///		[HorizontalGroup, HideLabel, PreviewField(30)]
    ///		public Material A, B, C, D, F;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="TitleAttribute"/>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    [DontApplyToListElements]
    public class PreviewFieldAttribute : Attribute
    {
        /// <summary>
        /// Draws a square object field which renders a preview for UnityEngine.Object type objects.
        /// </summary>
        /// <param name="height">The height of the preview field.</param>
        public PreviewFieldAttribute(float height)
        {
            this.Height = height;
        }

        /// <summary>
        /// Draws a square object field which renders a preview for UnityEngine.Object type objects.
        /// </summary>
        /// <param name="height">The height of the preview field.</param>
        /// <param name="alignment">The alignment of the preview field.</param>
        public PreviewFieldAttribute(float height, ObjectFieldAlignment alignment)
        {
            this.Height = height;
            this.Alignment = alignment;
            this.AlignmentHasValue = true;
        }

        /// <summary>
        /// Draws a square object field which renders a preview for UnityEngine.Object type objects.
        /// </summary>
        /// <param name="alignment">The alignment of the preview field.</param>
        public PreviewFieldAttribute(ObjectFieldAlignment alignment)
        {
            this.Alignment = alignment;
            this.AlignmentHasValue = true;
        }

        /// <summary>
        /// Draws a square object field which renders a preview for UnityEngine.Object type objects.
        /// </summary>
        public PreviewFieldAttribute()
        {
            this.Height = 0;
        }

        /// <summary>
        /// The height of the object field
        /// </summary>
        public float Height { get; private set; }

        /// <summary>
        /// Left aligned.
        /// </summary>
        public ObjectFieldAlignment Alignment { get; private set; }

        /// <summary>
        /// Whether an alignment value is specified.
        /// </summary>
        public bool AlignmentHasValue { get; private set; }
    }
}