//-----------------------------------------------------------------------
// <copyright file="PropertyRangeAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
	using System;

	/// <summary>
	/// <para>PropertyRange attribute creates a slider control to set the value of a property to between the specified range.</para>
	/// <para>This is equivalent to Unity's Range attribute, but this attribute can be applied to both fields and property.</para>
	/// </summary>
	/// <example>The following example demonstrates how PropertyRange is used.</example>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	/// 	[PropertyRange(0, 100)]
	///		public int MyInt;
	///		
	///		[PropertyRange(-100, 100)]
	///		public float MyFloat;
	///		
	///		[PropertyRange(-100, -50)]
	///		public decimal MyDouble;
	///	}
	/// </code>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class PropertyRangeAttribute : Attribute
	{
		/// <summary>
		/// The minimum value.
		/// </summary>
		public double Min { get; private set; }

		/// <summary>
		/// The maximum value.
		/// </summary>
		public double Max { get; private set; }

		/// <summary>
		/// Creates a slider control to set the value of the property to between the specified range..
		/// </summary>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		public PropertyRangeAttribute(double min, double max)
		{
			this.Min = min < max ? min : max;
			this.Max = max > min ? max : min;
		}
	}
}
