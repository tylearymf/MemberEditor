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
        MultiInfo<Method, Matrix4x4> mValues = new MultiInfo<Method, Matrix4x4>();

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
            var tValue = mValues.GetValue(pParamIndex, pInfo, () => Matrix4x4.identity);
            var tNewValue = GUIHelper.Matrix4x4Field(tValue);
            if (GUI.changed)
            {
                mValues.SetValue(pParamIndex, pInfo, tNewValue);
            }
            return tValue;
        }
    }
}