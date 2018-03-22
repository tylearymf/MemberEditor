﻿//-----------------------------------------------------------------------
// <copyright file="VerticalGroupAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

	/// <summary>
	/// <para>VerticalGroup is used to gather properties together in a vertical group in the inspector.</para>
	/// <para>This doesn't do much in and of itself, but in combination with other groups, such as <see cref="HorizontalGroupAttribute"/> it can be very useful.</para>
	/// </summary>
	/// <example>
	/// <para>The following example demonstrates how VerticalGroup can be used in conjuction with <see cref="HorizontalGroupAttribute"/></para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	///	{
	///		[HorizontalGroup("Split")]
	///		[VerticalGroup("Split/Left")]
	///		public Vector3 Vector;
	///		
	///		[VerticalGroup("Split/Left")]
	///		public GameObject First;
	///		
	///		[VerticalGroup("Split/Left")]
	///		public GameObject Second;
	///		
	///		[VerticalGroup("Split/Right", PaddingTop = 18f)]
	///		public int A;
	///		
	///		[VerticalGroup("Split/Right")]
	///		public int B;
	///	}
	/// </code>
	/// </example>
	/// <seealso cref="HorizontalGroupAttribute"/>
	/// <seealso cref="BoxGroupAttribute"/>
	/// <seealso cref="TabGroupAttribute"/>
	/// <seealso cref="ToggleGroupAttribute"/>
	/// <seealso cref="ButtonGroupAttribute"/>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public class VerticalGroupAttribute : PropertyGroupAttribute
	{
		/// <summary>
		/// Space in pixels at the top of the group.
		/// </summary>
		public float PaddingTop { get; set; }

		/// <summary>
		/// Space in pixels at the bottom of the group.
		/// </summary>
		public float PaddingBottom { get; set; }
		
		/// <summary>
		/// Groups properties vertically.
		/// </summary>
		/// <param name="groupId">The group ID.</param>
		/// <param name="order">The group order.</param>
		public VerticalGroupAttribute(string groupId, int order = 0) : base(groupId, order) { }

        /// <summary>
        /// <para>Groups properties vertically.</para>
        /// <para>GroupId: _DefaultVerticalGroup</para>
        /// </summary>
        /// <param name="order">The group order.</param>
        public VerticalGroupAttribute(int order = 0)
            : this("_DefaultVerticalGroup", order)
        {

        }

		/// <summary>
		/// Combines properties that have been group vertically.
		/// </summary>
		protected override void CombineValuesWith(PropertyGroupAttribute other)
		{
			var a = other as VerticalGroupAttribute;
			if (a != null)
			{
				if (a.PaddingTop != 0)
				{
					this.PaddingTop = a.PaddingTop;
				}

				if (a.PaddingBottom != 0)
				{
					this.PaddingBottom = a.PaddingBottom;
				}
			}
		}
	}
}
