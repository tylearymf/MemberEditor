﻿namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Field)]
    public class TempFieldDrawer : BaseDrawer<Field>
    {
        public override string typeName
        {
            get
            {
                return typeof(TempType).FullName;
            }
        }

        public override object LayoutDrawer(Field pInfo, int pParamIndex = 0)
        {
            var tValue = false;
            tValue = pInfo.GetValue<bool>();
            var tNewValue = EditorGUILayout.Toggle(tValue);
            if (GUI.changed)
            {
                tValue = tNewValue;
                pInfo.SetValue<bool>(tValue);
            }
            return tValue;
        }
    }
}