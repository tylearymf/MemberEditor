//-----------------------------------------------------------------------
// <copyright file="HideIfAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// <para>HideIf is used on any property and can hide the property in the inspector.</para>
    /// <para>Use this to hide irrelevant properties based on the current state of the object.</para>
    /// </summary>
    /// <example>
    /// <para>This example shows a component with fields hidden by the state of another field.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///		public bool HideProperties;
    ///
    ///		[HideIf("HideProperties")]
    ///		public int MyInt;
    ///
    ///		[HideIf("HideProperties", false)]
    ///		public string MyString;
    ///
    ///	    public SomeEnum SomeEnumField;
    ///
    ///		[HideIf("SomeEnumField", SomeEnum.SomeEnumMember)]
    ///		public string SomeString;
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// <para>This example shows a component with a field that is hidden when the game object is inactive.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///		[HideIf("MyVisibleFunction")]
    ///		public int MyHideableField;
    ///
    ///		private bool MyVisibleFunction()
    ///		{
    ///			return !this.gameObject.activeInHierarchy;
    ///		}
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="EnableIfAttribute"/>
    /// <seealso cref="DisableIfAttribute"/>
    /// <seealso cref="ShowIfAttribute"/>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [DontApplyToListElements]
    public sealed class HideIfAttribute : Attribute
    {
        /// <summary>
        /// Name of a bool field, property or function to show or hide the property.
        /// </summary>
        public string MemberName { get; private set; }

        /// <summary>
        /// The optional member value.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Whether or not to slide the property in and out when the state changes.
        /// </summary>
        public bool Animate { get; private set; }

        /// <summary>
        /// Hides a property in the inspector, if the specified member returns true.
        /// </summary>
        /// <param name="memberName">Name of a bool field, property or function to show or hide the property.</param>
        /// <param name="animate">Whether or not to slide the property in and out when the state changes.</param>
        public HideIfAttribute(string memberName, bool animate = true)
        {
            this.MemberName = memberName;
            this.Animate = animate;
        }

        /// <summary>
        /// Hides a property in the inspector, if the specified member returns the specified value.
        /// </summary>
        /// <param name="memberName">Name of member to check the value of.</param>
        /// <param name="optionalValue">The value to check for.</param>
        /// <param name="animate">Whether or not to slide the property in and out when the state changes.</param>
        public HideIfAttribute(string memberName, object optionalValue, bool animate = true)
        {
            this.MemberName = memberName;
            this.Value = optionalValue;
            this.Animate = animate;
        }
    }
}