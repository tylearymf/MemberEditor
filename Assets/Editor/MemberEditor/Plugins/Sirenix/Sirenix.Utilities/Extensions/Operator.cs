//-----------------------------------------------------------------------
// <copyright file="Operator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities
{
    /// <summary>
    /// Determents the type of operator.
    /// </summary>
    /// <seealso cref="TypeExtensions" />
    public enum Operator
    {
        /// <summary>
        /// The == operator.
        /// </summary>
        Equality,

        /// <summary>
        /// The != operator.
        /// </summary>
        Inequality,

        /// <summary>
        /// The + operator.
        /// </summary>
        Addition,

        /// <summary>
        /// The - operator.
        /// </summary>
        Subtraction,

        /// <summary>
        /// The &lt; operator.
        /// </summary>
        LessThan,

        /// <summary>
        /// The &gt; operator.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// The &lt;= operator.
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        /// The &gt;= operator.
        /// </summary>
        GreaterThanOrEqual
    }
}