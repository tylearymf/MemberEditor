//-----------------------------------------------------------------------
// <copyright file="InlinePropertyAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public class InlinePropertyAttribute : Attribute
    {
        /// <summary>
        /// Specify a label width for all child properties.
        /// </summary>
        public int LabelWidth { get; set; }
    }
}