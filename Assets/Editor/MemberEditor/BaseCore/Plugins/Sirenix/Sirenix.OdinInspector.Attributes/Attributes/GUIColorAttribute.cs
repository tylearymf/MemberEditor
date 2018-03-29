//-----------------------------------------------------------------------
// <copyright file="GUIColorAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;
    using UnityEngine;

	/// <summary>
	/// <para>GUIColor is used on any property and changes the GUI color used to draw the property.</para>
	/// </summary>
	/// <example>
	/// <para>The following example shows how GUIColor is used on a properties to create a rainbow effect.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	///	{
	///		[HideLabel]
	///		[GUIColor(1f, 0f, 0f)]
	///		public int A;
	///	
	///		[HideLabel]
	///		[GUIColor(1f, 0.5f, 0f)]
	///		public int B;
	///	
	///		[HideLabel]
	///		[GUIColor(1f, 1f, 0f)]
	///		public int C;
	///	
	///		[HideLabel]
	///		[GUIColor(0f, 1f, 0f)]
	///		public int D;
	///	
	///		[HideLabel]
	///		[GUIColor(0f, 1f, 1f)]
	///		public int E;
	///	
	///		[HideLabel]
	///		[GUIColor(0f, 0f, 1f)]
	///		public int F;
	///		
	///		[HideLabel]
	///		[GUIColor(1f, 0f, 1f)]
	///		public int G;
	///	}
	/// </code>
	/// </example>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class GUIColorAttribute : Attribute
    {
		/// <summary>
		/// The GUI color of the property.
		/// </summary>
        public Color Color { get; private set; }

		/// <summary>
		/// Sets the GUI color for the property.
		/// </summary>
		/// <param name="r">The red channel.</param>
		/// <param name="g">The green channel.</param>
		/// <param name="b">The blue channel.</param>
		/// <param name="a">The alpha channel.</param>
        public GUIColorAttribute(float r, float g, float b, float a = 1f)
        {
            this.Color = new Color(r, g, b, a);
        }
    }
}