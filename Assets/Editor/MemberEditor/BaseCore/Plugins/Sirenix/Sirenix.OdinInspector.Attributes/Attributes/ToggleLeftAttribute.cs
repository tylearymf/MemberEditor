//-----------------------------------------------------------------------
// <copyright file="ToggleLeftAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// <para>Draws the checkbox before the label instead of after.</para>
    /// </summary>
    /// <remarks>ToggleLeftAttribute can be used an all fields and properties of type boolean</remarks>
    /// <example>
    /// <code>
    ///	public class MyComponent : MonoBehaviour
    ///	{
    ///		[ToggleLeft]
    ///		public bool MyBoolean;
    ///	}
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class ToggleLeftAttribute : Attribute
    { }
}