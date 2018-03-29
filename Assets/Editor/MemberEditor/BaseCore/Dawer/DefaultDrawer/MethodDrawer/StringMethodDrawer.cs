namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class StringMethodDrawer : BaseDrawer<Method>
    {
        MultiInfo<Method, String> mValues = new MultiInfo<Method, String>();

        public override string typeName
        {
            get
            {
                return typeof(String).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pParamIndex = 0)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pParamIndex];
            var tValue = mValues.GetValue(pParamIndex, pInfo, () => string.Empty);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(tParam.Name, GUILayout.Width(tParam.Name.GetLabelWidth(this)));
            var tNewVal = EditorGUILayout.TextField(tValue);
            EditorGUILayout.EndHorizontal();
            if (GUI.changed)
            {
                mValues.SetValue(pParamIndex, pInfo, tNewVal);
            }
            return tValue;
        }
    }
}