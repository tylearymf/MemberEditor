//-----------------------------------------------------------------------
// <copyright file="TableListAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// TODO: Document
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class TableListAttribute : Attribute
    {
        /// <summary>
        /// TODO: Document
        /// </summary>
        public int NumberOfItemsPerPage { get; set; }
    }
}