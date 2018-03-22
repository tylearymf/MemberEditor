﻿//-----------------------------------------------------------------------
// <copyright file="ButtonGroupAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// <para>ButtonGroup is used on any instance function, and adds buttons to the inspector organized into horizontal groups.</para>
    /// <para>Use this to organize multiple button in a tiddy horizontal group.</para>
    /// </summary>
    /// <example>
    /// <para>The following example shows how ButtonGroup is used to organize two buttons into one group.</para>
    /// <code>
    ///	public class MyComponent : MonoBehaviour
    ///	{
    ///		[ButtonGroup("MyGroup")]
    ///		private void A()
    ///		{
    ///			// ..
    ///		}
    ///
    ///		[ButtonGroup("MyGroup")]
    ///		private void B()
    ///		{
    ///			// ..
    ///		}
    ///	}
    /// </code>
    /// </example>
    /// <example>
    /// <para>The following example shows how ButtonGroup can be used to create multiple groups of buttons.</para>
    /// <code>
    ///	public class MyComponent : MonoBehaviour
    ///	{
    ///		[ButtonGroup("First")]
    ///		private void A()
    ///		{ }
    ///
    ///		[ButtonGroup("First")]
    ///		private void B()
    ///		{ }
    ///
    ///		[ButtonGroup("")]
    ///		private void One()
    ///		{ }
    ///
    ///		[ButtonGroup("")]
    ///		private void Two()
    ///		{ }
    ///
    ///		[ButtonGroup("")]
    ///		private void Three()
    ///		{ }
    ///	}
    /// </code>
    /// </example>
    /// <seealso cref="ButtonAttribute"/>
	/// <seealso cref="InlineButtonAttribute"/>
    /// <seealso cref="BoxGroupAttribute"/>
    /// <seealso cref="FoldoutGroupAttribute"/>
    /// <seealso cref="HorizontalGroupAttribute"/>
    /// <seealso cref="TabGroupAttribute"/>
    /// <seealso cref="ToggleGroupAttribute"/>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ButtonGroupAttribute : PropertyGroupAttribute
    {
        /// <summary>
        /// Organizes the button into the specified button group.
        /// </summary>
        /// <param name="group">The group to organize the button into.</param>
        /// <param name="order">The order of the group in the inspector..</param>
        public ButtonGroupAttribute(string group = "_DefaultGroup", int order = 0)
            : base(group, order)
        { }
    }
}