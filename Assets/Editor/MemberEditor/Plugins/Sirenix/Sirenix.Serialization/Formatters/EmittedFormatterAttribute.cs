//-----------------------------------------------------------------------
// <copyright file="EmittedFormatterAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if (UNITY_EDITOR || UNITY_STANDALONE) && !ENABLE_IL2CPP
#define CAN_EMIT
#endif

namespace Sirenix.Serialization
{
    using System;

#if CAN_EMIT

#endif

    /// <summary>
    /// Indicates that this formatter type has been emitted. Never put this on a type!
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EmittedFormatterAttribute : Attribute
    {
    }
}