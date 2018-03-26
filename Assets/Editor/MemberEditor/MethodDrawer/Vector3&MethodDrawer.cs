namespace Tylearymf.MemberEditor
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

        public override object LayoutDrawer(Method pInfo, int pIndex)
        {
            var tInfo = pInfo.info;
            if (!mValues.ContainsKey(pIndex)) mValues.Add(pIndex, Vector3.zero);
            mValues[pIndex] = EditorGUI.Vector3Field(pInfo.rect, string.Empty, mValues[pIndex]);
            return mValues[pIndex];
        }

        public override int LayoutHeight(Method pInfo)
        {
            return 15;
        }
    }
}