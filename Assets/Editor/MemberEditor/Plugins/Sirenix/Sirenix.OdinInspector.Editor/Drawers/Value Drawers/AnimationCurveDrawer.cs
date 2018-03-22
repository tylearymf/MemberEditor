#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AnimationCurveDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using UnityEngine;

    /// <summary>
    /// Animation curve property drawer.
    /// </summary>
    [OdinDrawer]
    public sealed class AnimationCurveDrawer : DrawWithUnityBaseDrawer<AnimationCurve>
    {
    }
}
#endif