namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class TempMethodDrawer : BaseDrawer<Method>
    {
        Dictionary<int, TempType> mValues = new Dictionary<int, TempType>();

        public override string typeName
        {
            get
            {
                return typeof(TempType).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pIndex)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pIndex];
            //if (!mValues.ContainsKey(pIndex)) mValues.Add(pIndex, false);
            //mValues[pIndex] = EditorGUI.Toggle(pInfo.rect, tParam.Name, mValues[pIndex]);
            return mValues[pIndex];
        }

        public override int LayoutHeight(Method pInfo)
        {
            return 15;
        }
    }
}