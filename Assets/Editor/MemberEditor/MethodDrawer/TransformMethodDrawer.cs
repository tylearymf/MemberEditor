namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class TransformMethodDrawer : BaseDrawer<Method>
    {
        Dictionary<int, Transform> mValues = new Dictionary<int, Transform>();

        public override string typeName
        {
            get
            {
                return typeof(Transform).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pIndex)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pIndex];
            if (!mValues.ContainsKey(pIndex)) mValues.Add(pIndex, null);
            mValues[pIndex] = EditorGUI.ObjectField(pInfo.rect, tParam.Name, mValues[pIndex], typeof(Transform), true) as Transform;
            return mValues[pIndex];
        }

        public override int LayoutHeight()
        {
            return 15;
        }
    }
}