﻿//-----------------------------------------------------------------------
// <copyright file="HideInNonPrefabsAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// Hides a property if it is drawn from a non prefab instance or asset.
    /// </summary>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class HideInNonPrefabsAttribute : Attribute
    {

    }
}
