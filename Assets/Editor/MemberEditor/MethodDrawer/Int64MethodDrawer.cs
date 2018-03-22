namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class Int64MethodDrawer : BaseDrawer<Method>
    {
        Dictionary<int, Int64> mValues = new Dictionary<int, Int64>();

        public override string typeName
        {
            get
            {
                return typeof(Int64).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pIndex)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pIndex];
            if (!mValues.ContainsKey(pIndex)) mValues.Add(pIndex, 0);
            mValues[pIndex] = EditorGUI.LongField(pInfo.rect, tParam.Name, mValues[pIndex]);
            return mValues[pIndex];
        }

        public override int LayoutHeight()
        {
            return 15;
        }
    }
}