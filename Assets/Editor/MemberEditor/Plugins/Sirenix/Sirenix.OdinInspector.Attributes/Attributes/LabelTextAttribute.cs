//-----------------------------------------------------------------------
// <copyright file="LabelTextAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// <para>LabelText is used to change the labels of properties.</para>
    /// <para>Use this if you want a different label than the name of the property.</para>
    /// </summary>
    /// <example>
    /// <para>The following example shows how LabelText is applied to a few property fields.</para>
    /// <code>
    /// public MyComponent : MonoBehaviour
    /// {
    ///		[LabelText("1")]
    ///		public int MyInt1;
    ///
    ///		[LabelText("2")]
    ///		public int MyInt2;
    ///
    ///		[LabelText("3")]
    ///		public int MyInt3;
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="TitleAttribute"/>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    [DontApplyToListElements]
    public class LabelTextAttribute : Attribute
    {
        /// <summary>
        /// Give a property a custom label.
        /// </summary>
        /// <param name="text">The new text of the label.</param>
        public LabelTextAttribute(string text)
        {
            this.Text = text;
        }

        /// <summary>
        /// The new text of the label.
        /// </summary>
        public string Text { get; private set; }
    }
}