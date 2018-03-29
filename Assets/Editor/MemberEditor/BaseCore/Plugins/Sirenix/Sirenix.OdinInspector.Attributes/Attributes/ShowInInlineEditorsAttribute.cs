//-----------------------------------------------------------------------
// <copyright file="ShowInInlineEditorsAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// Hides a property if it is drawn within an <see cref="InlineEditorAttribute"/>.
    /// </summary>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class ShowInInlineEditorsAttribute : Attribute
    {
    }
}