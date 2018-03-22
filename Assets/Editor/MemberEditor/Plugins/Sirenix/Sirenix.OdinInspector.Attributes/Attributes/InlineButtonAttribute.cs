//-----------------------------------------------------------------------
// <copyright file="InlineButtonAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
	using System;

	/// <summary>
	/// <para>The inline button adds a button to the end of a property.</para>
	/// </summary>
	/// <remarks>
	/// <note type="note">Due to a bug, multiple inline buttons are currently not supported.</note>
	/// </remarks>
	/// <example>
	/// <para>The following examples demonstrates how InlineButton can be used.</para>
	/// <code>
	///	public class MyComponent : MonoBehaviour
	///	{
	///		// Adds a button to the end of the A property.
	///		[InlineButton("MyFunction")]
	///		public int A;
	///		
	///		// This is example demonstrates how you can change the label of the button.
	///		// InlineButton also supports refering to string members with $.
	///		[InlineButton("MyFunction", "Button")]
	///		public int B;
	///		
	/// 	private void MyFunction()
	///		{
	///			// ...
	///		}
	///	}
	/// </code>
	/// </example>
	/// <seealso cref="ButtonAttribute"/>
	/// <seealso cref="ButtonGroupAttribute"/>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
	public sealed class InlineButtonAttribute : Attribute
	{
		/// <summary>
		/// Name of member method to call when the button is clicked.
		/// </summary>
		public string MemberMethod { get; private set; }

		/// <summary>
		/// Optional label of the button.
		/// </summary>
		public string Label { get; private set; }

		/// <summary>
		/// Draws a button to the right of the property.
		/// </summary>
		/// <param name="memberMethod">Name of member method to call when the button is clicked.</param>
		/// <param name="label">Optional label of the button.</param>
		public InlineButtonAttribute(string memberMethod, string label = null)
		{
			this.MemberMethod = memberMethod;
			this.Label = label;
		}
	}
}