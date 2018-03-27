namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class QuaternionMethodDrawer : BaseDrawer<Method>
    {
        Dictionary<int, Quaternion> mValues = new Dictionary<int, Quaternion>();

        public override string typeName
        {
            get
            {
                return typeof(Quaternion).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pIndex)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pIndex];
            if (!mValues.ContainsKey(pIndex)) mValues.Add(pIndex, Quaternion.identity);
            var tVal = EditorGUI.Vector4Field(pInfo.rect, string.Empty,
                new Vector4(mValues[pIndex].x, mValues[pIndex].y, mValues[pIndex].z, mValues[pIndex].w));
            mValues[pIndex] = new Quaternion(tVal.x, tVal.y, tVal.z, tVal.w);
            return mValues[pIndex];
        }
    }
}