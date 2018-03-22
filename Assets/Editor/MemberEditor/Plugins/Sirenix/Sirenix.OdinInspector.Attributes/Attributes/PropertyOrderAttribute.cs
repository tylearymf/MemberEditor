//-----------------------------------------------------------------------
// <copyright file="PropertyOrderAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
	/// <para>PropertyOrder is used on any property, and allows for ordering of properties.</para>
	/// <para>Use this to define in which order your properties are shown.</para>
    /// </summary>
	/// <remarks>
	/// <para>Lower order values will be drawn before higher values.</para>
    /// <note type="note">There is unfortunately no way of ensuring that properties are in the same order, as they appear in your class. PropertyOrder overcomes this.</note>
    /// </remarks>
	/// <example>
	/// <para>The following example shows how PropertyOrder is used to order properties in the inspector.</para>
    /// <code>
	///	public class MyComponent : MonoBehaviour
	///	{
	///		[PropertyOrder(1)]
	///		public int MySecondProperty;
	///	
	///		[PropertyOrder(-1)]
	///		public int MyFirstProperty;
	///	}
	/// </code>
    /// </example>    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class PropertyOrderAttribute : Attribute
    {
		/// <summary>
		/// The order for the property.
		/// </summary>
		public int Order { get; private set; }

		/// <summary>
		/// Defines a custom order for the property.
		/// </summary>
		/// <param name="order">The order for the property.</param>
		public PropertyOrderAttribute(int order)
        {
            this.Order = order;
        }
    }
}