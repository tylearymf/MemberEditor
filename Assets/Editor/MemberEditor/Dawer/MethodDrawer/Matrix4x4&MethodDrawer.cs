namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class Matrix4x4_MethodDrawer : BaseDrawer<Method>
    {
        Dictionary<int, Matrix4x4> mValues = new Dictionary<int, Matrix4x4>();

        public override string typeName
        {
            get
            {
                return "UnityEngine.Matrix4x4&";
            }
        }

        public override object LayoutDrawer(Method pInfo, int pParamIndex = 0)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pParamIndex];
            if (!mValues.ContainsKey(pParamIndex)) mValues.Add(pParamIndex, Matrix4x4.zero);
            var tNewValue = GUIHelper.Matrix4x4Field(mValues[pParamIndex]);
            if (GUI.changed)
            {
                mValues[pParamIndex] = tNewValue;
            }
            return mValues[pParamIndex];
        }
    }
}