﻿namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class StringMethodDrawer : BaseDrawer<Method>
    {
        Dictionary<int, String> mValues = new Dictionary<int, String>();

        public override string typeName
        {
            get
            {
                return typeof(String).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pIndex)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pIndex];
            if (!mValues.ContainsKey(pIndex)) mValues.Add(pIndex, string.Empty);
            mValues[pIndex] = EditorGUI.TextField(pInfo.rect, tParam.Name, mValues[pIndex]);
            return mValues[pIndex];
        }
    }
}