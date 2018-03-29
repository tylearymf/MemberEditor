//-----------------------------------------------------------------------
// <copyright file="PreviouslySerializedAsAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
    using System;

    /// <summary>
    /// Indicates that an instance field or auto-property was previously serialized with a different name, so that values serialized with the old name will be properly deserialized into this member.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class PreviouslySerializedAsAttribute : Attribute
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public PreviouslySerializedAsAttribute(string name)
        {
            this.Name = name;
        }
    }
}