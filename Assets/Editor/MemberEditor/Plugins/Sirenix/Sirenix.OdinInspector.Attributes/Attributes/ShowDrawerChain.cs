//-----------------------------------------------------------------------
// <copyright file="ShowDrawerChainAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// <para>
    /// ShowDrawerChain lists all prepend, append and value drawers being used in the inspector.
    /// This is great in situations where you want to debug, and want to know which drawers might be involved in drawing the property.</para>
    /// </summary>
    /// <example>
    /// <code>
    ///	public class MyComponent : MonoBehaviour
    ///	{
    ///		[ShowDrawerChain]
    ///		public int IndentedInt;
    ///	}
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class ShowDrawerChainAttribute : Attribute
    {
    }
}