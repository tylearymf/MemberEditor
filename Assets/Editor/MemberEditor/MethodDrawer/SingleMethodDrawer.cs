namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class SingleMethodDrawer : BaseDrawer<Method>
    {
        Dictionary<int, Single> mValues = new Dictionary<int, Single>();

        public override string typeName
        {
            get
            {
                return typeof(Single).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pIndex)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pIndex];
            if (!mValues.ContainsKey(pIndex)) mValues.Add(pIndex, 0F);
            mValues[pIndex] = EditorGUI.FloatField(pInfo.rect, tParam.Name, mValues[pIndex]);
            return mValues[pIndex];
        }
    }
}