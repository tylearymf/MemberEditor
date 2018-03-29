//-----------------------------------------------------------------------
// <copyright file="MultiLinePropertyAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
	/// <para>MultiLineProperty is used on any string property</para>
	/// <para>Use this to allow users to edit strings in a multi line textbox.</para>
    /// </summary>
	/// <remarks>
    /// <para>MultiLineProperty is similar to Unity's <see cref="UnityEngine.MultilineAttribute"/> but can be applied to both fields and properties.</para>
    /// </remarks>
	/// <example>
	/// <para>The following example shows how MultiLineProperty is applied to properties.</para>
    /// <code>
	///	public class MyComponent : MonoBehaviour
	///	{
	///		[MultiLineProperty]
	///		public string MyString;
	///	
	///		[ShowInInspector, MultiLineProperty(10)]
	///		public string PropertyString { get; set; }
	///	}
	/// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class MultiLinePropertyAttribute : Attribute
    {
		/// <summary>
		/// The number of lines for the text box.
		/// </summary>
		public int Lines { get; private set; }

		/// <summary>
		/// Makes a multiline textbox for editing strings.
		/// </summary>
		/// <param name="lines">The number of lines for the text box.</param>
		public MultiLinePropertyAttribute(int lines = 3)
        {
            this.Lines = Math.Max(1, lines);
        }
    }
}