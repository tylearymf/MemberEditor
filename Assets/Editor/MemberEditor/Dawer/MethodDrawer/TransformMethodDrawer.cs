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

        public override object LayoutDrawer(Method pInfo, int pParamIndex = 0)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pParamIndex];
            if (!mValues.ContainsKey(pParamIndex)) mValues.Add(pParamIndex, null);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(tParam.Name, GUILayout.Width(tParam.Name.GetLabelWidth(this)));
            var tNewVal = EditorGUILayout.ObjectField(mValues[pParamIndex], typeof(Transform), true) as Transform;
            EditorGUILayout.EndHorizontal();
            if (GUI.changed)
            {
                mValues[pParamIndex] = tNewVal;
            }
            return mValues[pParamIndex];
        }
    }
}