namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class Int32MethodDrawer : BaseDrawer<Method>
    {
        Dictionary<int, Int32> mValues = new Dictionary<int, Int32>();

        public override string typeName
        {
            get
            {
                return typeof(Int32).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pParamIndex = 0)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pParamIndex];
            if (!mValues.ContainsKey(pParamIndex)) mValues.Add(pParamIndex, 0);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(tParam.Name, GUILayout.Width(tParam.Name.GetLabelWidth(this)));
            var tNewVal = EditorGUILayout.IntField(mValues[pParamIndex]);
            EditorGUILayout.EndHorizontal();
            if (GUI.changed)
            {
                mValues[pParamIndex] = tNewVal;
            }
            return mValues[pParamIndex];
        }
    }
}