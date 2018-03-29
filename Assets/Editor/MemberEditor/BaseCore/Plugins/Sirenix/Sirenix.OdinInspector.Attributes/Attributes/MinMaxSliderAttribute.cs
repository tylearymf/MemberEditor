//-----------------------------------------------------------------------
// <copyright file="MinMaxSliderAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// <para>Draw a special slider the user can use to specify a range between a min and a max value.</para>
    /// <para>Uses a Vector2 where x is min and y is max.</para>
    /// </summary>
    /// <example>
	/// <para>The following example shows how MinMaxSlider is used.</para>
    /// <code>
    /// public class Player : MonoBehaviour
    /// {
    ///		[MinMaxSlider(4, 5)]
    ///		public Vector2 SpawnRadius;
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class MinMaxSliderAttribute : Attribute
    {
        /// <summary>
        /// The min value for the slider.
        /// </summary>
        public float MinValue { get; private set; }

        /// <summary>
        /// The max value for the slider.
        /// </summary>
        public float MaxValue { get; private set; }

		/// <summary>
		/// Draw float fields for min and max value.
		/// </summary>
		public bool ShowFields { get; private set; }

        /// <summary>
        /// Draws a min-max slider in the inspector. X will be set to min, and Y will be set to max.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The max value.</param>
        /// <param name="showFields">If <c>true</c> number fields will drawn next to the MinMaxSlider.</param>
        public MinMaxSliderAttribute(float minValue, float maxValue, bool showFields = false)
        {
            this.MinValue = minValue;
            this.MaxValue = maxValue;
			this.ShowFields = showFields;
        }
    }
}