//-----------------------------------------------------------------------
// <copyright file="ToggleAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// <para>Toggle is used on any field or property, and allows to enable or disable the property in the inspector.</para>
    /// <para>Use this to create a property that can be turned off or on.</para>
    /// </summary>
    /// <remarks>
    /// <note type="note">Toggle does current not support any static members for toggling.</note>
    /// </remarks>
    /// <example>
	/// <para>The following example shows how Toggle is used to create a togglable property.</para>
    /// <code>
	/// public class MyComponent : MonoBehaviour
	///	{
	///		[Toggle("Enabled")]
	///		public MyToggleable MyToggler = new MyToggleable();
	///	}
	///
	///	public class MyToggleable
	///	{
	///		public bool Enabled;
	///
	///		public int MyValue;
	///	}
    /// </code>
    /// </example>
	/// <seealso cref="ToggleListAttribute"/>
	/// <seealso cref="ToggleGroupAttribute"/>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ToggleAttribute : Attribute
    {
        /// <summary>
        /// Create a togglable property in the inspector.
        /// </summary>
        /// <param name="toggleMemberName">Name of any bool field or property to enable or disable the object.</param>
        public ToggleAttribute(string toggleMemberName)
        {
            this.ToggleMemberName = toggleMemberName;
            this.CollapseOthersOnExpand = true;
        }

        /// <summary>
        /// Name of any bool field or property to enable or disable the object.
        /// </summary>
        public string ToggleMemberName { get; private set; }

        /// <summary>
        /// If true, all other open toggle groups will collapse once another one opens.
        /// </summary>
        public bool CollapseOthersOnExpand { get; set; }
    }
}