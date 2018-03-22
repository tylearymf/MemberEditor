//-----------------------------------------------------------------------
// <copyright file="TableColumnWidthAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// TODO: Document
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public class TableColumnWidthAttribute : Attribute
    {
        /// <summary>
        /// TODO: Document
        /// </summary>
        public int Width;

        /// <summary>
        /// TODO: Document
        /// </summary>
        public TableColumnWidthAttribute(int width)
        {
            this.Width = width;
        }
    }
}