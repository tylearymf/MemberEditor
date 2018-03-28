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

        public override object LayoutDrawer(Method pInfo, int pParamIndex = 0)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pParamIndex];
            if (!mValues.ContainsKey(pParamIndex)) mValues.Add(pParamIndex, Space.Self);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(tParam.Name, GUILayout.Width(tParam.Name.GetLabelWidth(this)));
            var tNewVal = (Space)EditorGUILayout.EnumPopup(mValues[pParamIndex]);
            EditorGUILayout.EndHorizontal();
            if (GUI.changed)
            {
                mValues[pParamIndex] = tNewVal;
            }
            return (int)mValues[pParamIndex];
        }
    }
}