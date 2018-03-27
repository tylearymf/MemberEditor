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

        public override object LayoutDrawer(Method pInfo, int pIndex)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pIndex];
            if (!mValues.ContainsKey(pIndex)) mValues.Add(pIndex, Matrix4x4.zero);
            var tNewValue = GUIHelper.DrawerMatrix4x4(mValues[pIndex]);
            if (GUI.changed)
            {
                mValues[pIndex] = tNewValue;
            }
            return mValues[pIndex];
        }
    }
}