﻿namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class Vector3MethodDrawer : BaseDrawer<Method>
    {
        Dictionary<int, Vector3> mValues = new Dictionary<int, Vector3>();

        public override string typeName
        {
            get
            {
                return typeof(Vector3).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pIndex)
        {
            var tInfo = pInfo.info;
            if (!mValues.ContainsKey(pIndex)) mValues.Add(pIndex, Vector3.zero);
            var tName = tInfo.GetParameters()[pIndex].Name;

            var tVal = mValues[pIndex];

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(tName, GUILayout.Width(tName.GetLabelWidth() + 5));
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("X", GUILayout.Width(15));
            tVal.x = EditorGUILayout.FloatField(tVal.x);
            EditorGUILayout.LabelField("Y", GUILayout.Width(15));
            tVal.y = EditorGUILayout.FloatField(tVal.y);
            EditorGUILayout.LabelField("Z", GUILayout.Width(15));
            tVal.z = EditorGUILayout.FloatField(tVal.z);
            EditorGUILayout.EndHorizontal();

            if (GUI.changed)
            {
                mValues[pIndex] = tVal;
            }
            return mValues[pIndex];
        }

        public override int LayoutHeight(Method pInfo)
        {
            return 0;
        }
    }
}