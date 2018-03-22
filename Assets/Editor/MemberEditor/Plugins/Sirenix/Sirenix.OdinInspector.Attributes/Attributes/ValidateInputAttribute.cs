//-----------------------------------------------------------------------
// <copyright file="ValidateInputAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// <para>ValidateInput is used on any property, and allows to validate input from inspector.</para>
    /// <para>Use this to enforce correct values.</para>
    /// </summary>
    /// <remarks>
    /// <note type="note">ValidateInput refuses invalid values.</note>
    /// <note type="note">ValidateInput only works in the editor. Values changed through scripting will not be validated.</note>
    /// </remarks>
    /// <example>
    /// <para>The following examples shows how a speed value can be forced to be above 0.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///		[ValidateInput("ValidateInput")]
    ///		public float Speed;
    ///
    ///		// Specify custom output message and message type.
    ///		[ValidateInput("ValidateInput", "Health must be more than 0!", InfoMessageType.Warning)]
    ///		public float Health;
    ///
    ///		private bool ValidateInput(float property)
    ///		{
    ///			return property > 0f;
    ///		}
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// <para>The following example shows how a static function could also be used.</para>
    /// <code>
    /// public class MyComponent : MonoBehaviour
    /// {
    ///		[ValidateInput("StaticValidateFunction")]
    ///		public int MyInt;
    ///
    ///		private static bool StaticValidateFunction(int property)
    ///		{
    ///			return property != 0;
    ///		}
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="InfoBoxAttribute"/>
    /// <seealso cref="RequiredAttribute"/>
	[DontApplyToListElements]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class ValidateInputAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateInputAttribute"/> class.
        /// </summary>
        /// <param name="memberName">Name of callback function to validate input. The function must have at least one parameter of the same type as the property.</param>
        /// <param name="defaultMessage">Default message for invalid values.</param>
        /// <param name="messageType">Type of the message.</param>
        public ValidateInputAttribute(string memberName, string defaultMessage = null, InfoMessageType messageType = InfoMessageType.Error)
        {
            this.MemberName = memberName;
            this.DefaultMessage = defaultMessage;
            this.MessageType = messageType;
            this.IncludeChildren = true;
        }

        /// <summary>
        /// Obsolete. Rejecting invalid input is no longer supported. Use the other constructors instead.
        /// </summary>
        [Obsolete("Rejecting invalid input is no longer supported. Use the other constructor instead.", false)]
        public ValidateInputAttribute(string memberName, string message, InfoMessageType messageType, bool rejectedInvalidInput)
        {
            this.MemberName = memberName;
            this.DefaultMessage = message;
            this.MessageType = messageType;
            this.IncludeChildren = true;
        }

        /// <summary>
        /// Default message for invalid values.
        /// </summary>
        public string DefaultMessage { get; private set; }

        /// <summary>
        /// Name of callback function to validate input. The function must have at least one parameter of the same type as the property.
        /// </summary>
        public string MemberName { get; private set; }

        /// <summary>
        /// The type of the message.
        /// </summary>
        public InfoMessageType MessageType { get; private set; }

        /// <summary>
        /// Whether to also trigger validation when changes to child values happen. This is true by default.
        /// </summary>
        public bool IncludeChildren { get; set; }
    }
}