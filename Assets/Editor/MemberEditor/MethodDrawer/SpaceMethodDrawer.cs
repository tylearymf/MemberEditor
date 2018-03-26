namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class SpaceMethodDrawer : BaseDrawer<Method>
    {
        Dictionary<int, Space> mValues = new Dictionary<int, Space>();

        public override string typeName
        {
            get
            {
                return typeof(Space).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pIndex)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pIndex];
            if (!mValues.ContainsKey(pIndex)) mValues.Add(pIndex, Space.Self);
            mValues[pIndex] = (Space)EditorGUI.EnumPopup(pInfo.rect, tParam.Name, mValues[pIndex]);
            return (int)mValues[pIndex];
        }

        public override int LayoutHeight()
        {
            return 15;
        }
    }
}