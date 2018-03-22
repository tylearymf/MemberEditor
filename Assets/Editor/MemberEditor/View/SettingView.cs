﻿namespace Tylearymf.MemberEditor
{
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using Sirenix.Utilities.Editor;

    public class SettingView : IView, IMemberTitle
    {
        [ColorPalette("Fall")]
        public Color a;

        public void Start()
        {
        }

        public void Update()
        {
        }

        public string TitleName()
        {
            return typeof(SettingView).Name;
        }

        public bool IsClickable()
        {
            return false;
        }
    }
}