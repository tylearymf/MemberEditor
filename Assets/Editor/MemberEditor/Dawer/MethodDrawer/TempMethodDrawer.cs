namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class TempMethodDrawer : BaseDrawer<Method>
    {
        Dictionary<int, TempType> mValues = new Dictionary<int, TempType>();

        public override string typeName
        {
            get
            {
                return typeof(TempType).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pParamIndex = 0)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pParamIndex];
            //if (!mValues.ContainsKey(pIndex)) mValues.Add(pIndex, false);
            //EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField(tParam.Name, GUILayout.Width(tParam.Name.GetLabelWidth()));
            //var tNewVal = EditorGUILayout.Toggle(mValues[pIndex]);
            //EditorGUILayout.EndHorizontal();
            //if (GUI.changed)
            //{
            //    mValues[pIndex] = tNewVal;
            //}
            return mValues[pParamIndex];
        }
    }
}