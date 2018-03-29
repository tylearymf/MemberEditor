//-----------------------------------------------------------------------
// <copyright file="SerializationOptions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace Sirenix.Serialization
{
    /// <summary>
    /// Not yet documented.
    /// </summary>
    public class SerializationAbortException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SerializationAbortException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public SerializationAbortException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}