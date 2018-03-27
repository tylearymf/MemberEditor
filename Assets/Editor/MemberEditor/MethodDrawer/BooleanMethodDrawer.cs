namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class BooleanMethodDrawer : BaseDrawer<Method>
    {
        Dictionary<int, bool> mValues = new Dictionary<int, bool>();

        public override string typeName
        {
            get
            {
                return typeof(Boolean).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pIndex)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pIndex];
            if (!mValues.ContainsKey(pIndex)) mValues.Add(pIndex, false);
            var tNewVal = EditorGUILayout.Toggle(tParam.Name, mValues[pIndex]);
            if (GUI.changed)
            {
                mValues[pIndex] = tNewVal;
            }
            return mValues[pIndex];
        }
    }
}