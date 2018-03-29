//-----------------------------------------------------------------------
// <copyright file="EnableIfAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// <para>EnableIf is used on any property, and can enable or disable the property in the inspector.</para>
    /// <para>Use this to enable properties when they are relevant.</para>
    /// </summary>
	/// <example>
    /// <para>The following example shows how a property can be enabled by the state of a field.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
	/// {
	///		public bool EnableProperty;
	///
	///		[EnableIf("EnableProperty")]
	///		public int MyInt;
    ///		
    ///	    public SomeEnum SomeEnumField;
    ///		
    ///		[EnableIf("SomeEnumField", SomeEnum.SomeEnumMember)]
    ///		public string SomeString;
	/// }
    /// </code>
    /// </example>
	/// <example>
    /// <para>The following examples show how a property can be enabled by a function.</para>
    /// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///		[EnableIf("MyEnableFunction")]
	///		public int MyInt;
	///
	///		private bool MyEnableFunction()
	///		{
	///			// ...
	///		}
	/// }
    /// </code>
    /// </example>
	/// <seealso cref="DisableIfAttribute"/>
	/// <seealso cref="ShowIfAttribute"/>
	/// <seealso cref="HideIfAttribute"/>
	/// <seealso cref="DisableInEditorModeAttribute"/>
	/// <seealso cref="DisableInPlayModeAttribute"/>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class EnableIfAttribute : Attribute
    {
        /// <summary>
        /// The name of a bool member field, property or method.
        /// </summary>
        public string MemberName { get; private set; }

        /// <summary>
        /// The optional member value.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Enables a property in the inspector, based on the state of a member.
        /// </summary>
        /// <param name="memberName">Name of member bool field, property, or method.</param>
        public EnableIfAttribute(string memberName)
        {
            this.MemberName = memberName;
        }

        /// <summary>
        /// Enables a property in the inspector, if the specified member returns the specified value.
        /// </summary>
        /// <param name="memberName">Name of member to check value of.</param>
        /// <param name="optionalValue">Value to check against.</param>
        public EnableIfAttribute(string memberName, object optionalValue)
        {
            this.MemberName = memberName;
            this.Value = optionalValue;
        }
    }
}