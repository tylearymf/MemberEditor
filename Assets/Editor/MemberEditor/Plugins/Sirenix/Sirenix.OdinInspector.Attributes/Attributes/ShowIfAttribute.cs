//-----------------------------------------------------------------------
// <copyright file="ShowIfAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// <para>ShowIf is used on any property and can hide the property in the inspector.</para>
    /// <para>Use this to hide irrelevant properties based on the current state of the object.</para>
    /// </summary>
    /// <example>
    /// <para>This example shows a component with fields hidden by the state of another field.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///		public bool ShowProperties;
    ///
    ///		[ShowIf("showProperties")]
    ///		public int MyInt;
    ///
    ///		[ShowIf("showProperties", false)]
    ///		public string MyString;
    ///		
    ///	    public SomeEnum SomeEnumField;
    ///		
    ///		[ShowIf("SomeEnumField", SomeEnum.SomeEnumMember)]
    ///		public string SomeString;
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// <para>This example shows a component with a field that is hidden when the game object is inactive.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///		[ShowIf("MyVisibleFunction")]
    ///		public int MyHideableField;
    ///
    ///		private bool MyVisibleFunction()
    ///		{
    ///			return this.gameObject.activeInHierarchy;
    ///		}
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="EnableIfAttribute"/>
    /// <seealso cref="DisableIfAttribute"/>
    /// <seealso cref="HideIfAttribute"/>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [DontApplyToListElements]
    public sealed class ShowIfAttribute : Attribute
    {
        /// <summary>
        /// Name of a bool field, property or function to show or hide the property.
        /// </summary>
        public string MemberName { get; private set; }

        /// <summary>
        /// Whether or not to slide the property in and out when the state changes.
        /// </summary>
        public bool Animate { get; private set; }

        /// <summary>
        /// The optional member value.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Shows a property in the inspector, if the specified member returns true.
        /// </summary>
        /// <param name="memberName">Name of a bool field, property or function to show or hide the property.</param>
        /// <param name="animate">Whether or not to slide the property in and out when the state changes.</param>
        public ShowIfAttribute(string memberName, bool animate = true)
        {
            this.MemberName = memberName;
            this.Animate = animate;
        }

        /// <summary>
        /// Shows a property in the inspector, if the specified member returns the specified value.
        /// </summary>
        public ShowIfAttribute(string memberName, object optionalValue, bool animate = true)
        {
            this.MemberName = memberName;
            this.Value = optionalValue;
            this.Animate = animate;
        }
    }
}