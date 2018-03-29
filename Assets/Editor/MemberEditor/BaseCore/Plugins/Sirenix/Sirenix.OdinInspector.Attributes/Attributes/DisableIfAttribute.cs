//-----------------------------------------------------------------------
// <copyright file="DisableIfAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// <para>DisableIf is used on any property, and can disable or enable the property in the inspector.</para>
    /// <para>Use this to disable properties when they are irrelevant.</para>
    /// </summary>
    /// <example>
    /// <para>The following example shows how a property can be disabled by the state of a field.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///		public bool DisableProperty;
    ///
    ///		[DisableIf("DisableProperty")]
    ///		public int MyInt;
    ///		
    ///	    public SomeEnum SomeEnumField;
    ///		
    ///		[DisableIf("SomeEnumField", SomeEnum.SomeEnumMember)]
    ///		public string SomeString;
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// <para>The following examples show how a property can be disabled by a function.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///		[EnableIf("MyDisableFunction")]
    ///		public int MyInt;
    ///
    ///		private bool MyDisableFunction()
    ///		{
    ///			// ...
    ///		}
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="EnableIfAttribute"/>
    /// <seealso cref="ShowIfAttribute"/>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class DisableIfAttribute : Attribute
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
        /// Disables a property in the inspector, based on the state of a member.
        /// </summary>
        /// <param name="memberName">Name of member bool field, property, or method.</param>
        public DisableIfAttribute(string memberName)
        {
            this.MemberName = memberName;
        }

        /// <summary>
        /// Disables a property in the inspector, if the specified member returns the specified value.
        /// </summary>
        /// <param name="memberName">Name of member to check value of.</param>
        /// <param name="optionalValue">Value to check against.</param>
        public DisableIfAttribute(string memberName, object optionalValue)
        {
            this.MemberName = memberName;
            this.Value = optionalValue;
        }
    }
}