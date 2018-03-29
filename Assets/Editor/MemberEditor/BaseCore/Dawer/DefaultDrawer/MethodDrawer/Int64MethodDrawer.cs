namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class Int64MethodDrawer : BaseDrawer<Method>
    {
        MultiInfo<Method, Int64> mValues = new MultiInfo<Method, Int64>();

        public override string typeName
        {
            get
            {
                return typeof(Int64).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pParamIndex = 0)
        {
            var tParam = pInfo.info.GetParameters()[pParamIndex];
            var tValue = mValues.GetValue(pParamIndex, pInfo, () => 0);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(tParam.Name, GUILayout.Width(tParam.Name.GetLabelWidth(this)));
            var tNewVal = EditorGUILayout.LongField(tValue);
            EditorGUILayout.EndHorizontal();
            if (GUI.changed)
            {
                mValues.SetValue(pParamIndex, pInfo, tNewVal);
            }
            return tValue;
        }
    }
}