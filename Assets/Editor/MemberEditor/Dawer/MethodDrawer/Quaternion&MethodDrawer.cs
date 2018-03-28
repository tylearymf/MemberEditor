namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class Quaternion_MethodDrawer : BaseDrawer<Method>
    {
        Dictionary<int, Quaternion> mValues = new Dictionary<int, Quaternion>();

        public override string typeName
        {
            get
            {
                return "UnityEngine.Quaternion&";
            }
        }

        public override object LayoutDrawer(Method pInfo, int pParamIndex = 0)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pParamIndex];
            if (!mValues.ContainsKey(pParamIndex)) mValues.Add(pParamIndex, Quaternion.identity);
            var tNewVal = GUIHelper.Vector4Field(new Vector4(mValues[pParamIndex].x, mValues[pParamIndex].y, mValues[pParamIndex].z, mValues[pParamIndex].w), this, tParam.Name);
            if (GUI.changed)
            {
                mValues[pParamIndex] = new Quaternion(tNewVal.x, tNewVal.y, tNewVal.z, tNewVal.w);
            }
            return mValues[pParamIndex];
        }
    }
}