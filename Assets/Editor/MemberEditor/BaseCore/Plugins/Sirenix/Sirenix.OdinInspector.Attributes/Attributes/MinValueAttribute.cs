//-----------------------------------------------------------------------
// <copyright file="MinValueAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// <para>MinValue is used on primitive fields. It caps value of the field to a minimum value.</para>
    /// <para>Use this to define a minimum value for the field.</para>
    /// </summary>
    /// <remarks>
    /// <note type="note">Note that this attribute only works in the editor! Values changed from scripting will not be capped at a minimum.</note>
    /// </remarks>
    /// <example>
    /// <para>The following example shows a player component that must have at least 1 life.</para>
    /// <code>
    /// public class Player : MonoBehaviour
    /// {
    ///		// The life value must be set to at least 1.
    ///		[MinValue(1)]
    ///		public int Life;
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// <para>The following example shows how MinValue can be combined with <see cref="MaxValueAttribute"/></para>
    /// <code>
    /// public class Health : MonoBehaviour
    /// {
    ///		// The health value must be between 0 and 100.
    ///		[MinValue(0), MaxValue(100)]
    ///		public float Health;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="MaxValueAttribute"/>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class MinValueAttribute : Attribute
    {
        /// <summary>
        /// The minimum value for the property.
        /// </summary>
        public double MinValue { get; private set; }

        /// <summary>
        /// Sets a minimum value for the property in the inspector.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        public MinValueAttribute(double minValue)
        {
            this.MinValue = minValue;
        }
    }
}