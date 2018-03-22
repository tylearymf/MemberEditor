//-----------------------------------------------------------------------
// <copyright file="HideInPrefabAssetsAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
    /// Hides a property if it is drawn from a prefab asset.
    /// </summary>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class HideInPrefabAssetsAttribute : Attribute
    {

    }
}
