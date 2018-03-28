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

        public override object LayoutDrawer(Method pInfo, int pParamIndex = 0)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pParamIndex];
            if (!mValues.ContainsKey(pParamIndex)) mValues.Add(pParamIndex, false);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(tParam.Name, GUILayout.Width(tParam.Name.GetLabelWidth(this)));
            var tNewVal = EditorGUILayout.Toggle(mValues[pParamIndex]);
            EditorGUILayout.EndHorizontal();
            if (GUI.changed)
            {
                mValues[pParamIndex] = tNewVal;
            }
            return mValues[pParamIndex];
        }
    }
}