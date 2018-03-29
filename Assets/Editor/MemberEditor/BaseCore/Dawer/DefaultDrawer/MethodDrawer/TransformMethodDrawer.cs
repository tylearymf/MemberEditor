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
        MultiInfo<Method, Transform> mValues = new MultiInfo<Method, Transform>();

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
            var tValue = mValues.GetValue(pParamIndex, pInfo, () => null);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(tParam.Name, GUILayout.Width(tParam.Name.GetLabelWidth(this)));
            var tNewVal = EditorGUILayout.ObjectField(tValue, typeof(Transform), true) as Transform;
            EditorGUILayout.EndHorizontal();
            if (GUI.changed)
            {
                mValues.SetValue(pParamIndex, pInfo, tNewVal);
            }
            return tValue;
        }
    }
}