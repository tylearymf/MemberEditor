﻿namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class Vector3_MethodDrawer : BaseDrawer<Method>
    {
        Dictionary<int, Vector3> mValues = new Dictionary<int, Vector3>();

        public override string typeName
        {
            get
            {
                return "UnityEngine.Vector3&";
            }
        }

        public override object LayoutDrawer(Method pInfo, int pParamIndex = 0)
        {
            var tInfo = pInfo.info;
            if (!mValues.ContainsKey(pParamIndex)) mValues.Add(pParamIndex, Vector3.zero);
            var tNewVal = GUIHelper.Vector3Field(mValues[pParamIndex], this, tInfo.GetParameters()[pParamIndex].Name);
            if (GUI.changed)
            {
                mValues[pParamIndex] = tNewVal;
            }
            return mValues[pParamIndex];
        }
    }
}