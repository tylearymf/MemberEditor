#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AtomContainerAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

[assembly: Sirenix.OdinInspector.Editor.AtomContainer]

namespace Sirenix.OdinInspector.Editor
{
    using System;

    [AttributeUsage(AttributeTargets.Assembly)]
    public class AtomContainerAttribute : Attribute
    {
    }
}
#endif