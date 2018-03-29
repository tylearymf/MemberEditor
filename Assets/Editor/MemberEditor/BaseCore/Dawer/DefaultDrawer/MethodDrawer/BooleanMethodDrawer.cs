﻿namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class BooleanMethodDrawer : BaseDrawer<Method>
    {
        MultiInfo<Method, bool> mValues = new MultiInfo<Method, bool>();

        public override string typeName
        {
            get
            {
                return typeof(Boolean).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pParamIndex = 0)
        {
            var tParam = pInfo.info.GetParameters()[pParamIndex];
            var tValue = mValues.GetValue(pParamIndex, pInfo, () => false);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(tParam.Name, GUILayout.Width(tParam.Name.GetLabelWidth(this)));
            var tNewVal = EditorGUILayout.Toggle(tValue);
            EditorGUILayout.EndHorizontal();
            if (GUI.changed)
            {
                mValues.SetValue(pParamIndex, pInfo, tNewVal);
            }
            return tValue;
        }
    }
}