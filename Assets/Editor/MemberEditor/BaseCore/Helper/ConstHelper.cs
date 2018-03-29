namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    static public class ConstHelper
    {
        public const int cFieldDefaultHeight = 20;
        public const int cPropertyDefaultHeight = 20;
        public const int cMethodDefaultHeight = 20;
        public const int cStaticViewMaxShowCount = 5;

        public const BindingFlags cEntityPropertyFlags = BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic;
        public const BindingFlags cStaticPropertyFlags = BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic;

        static public GUILayoutOption[] GetButtonStyle(ButtonSizeType pType)
        {
            switch (pType)
            {
                case ButtonSizeType.Small:
                    return new GUILayoutOption[] { GUILayout.Width(20) };
                case ButtonSizeType.Normal:
                    return new GUILayoutOption[] { GUILayout.Width(50) };
                case ButtonSizeType.Large:
                    return new GUILayoutOption[] { GUILayout.Width(100) };
                default:
                    return new GUILayoutOption[] { GUILayout.Width(50) };
            }
        }
    }

    public enum ButtonSizeType
    {
        Small,
        Normal,
        Large
    }
}