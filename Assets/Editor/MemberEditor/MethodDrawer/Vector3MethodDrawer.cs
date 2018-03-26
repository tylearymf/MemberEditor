namespace Tylearymf.MemberEditor
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
            var tRect = pInfo.rect;
            var tLabelWidth = tName.Length * 15;
            tRect.size = new Vector2(tLabelWidth, 15);
            EditorGUI.LabelField(tRect, tName);
            tRect = pInfo.rect;
            tRect.position = new Vector2(tRect.position.x + tLabelWidth, tRect.position.y);
            tRect.size = new Vector2(tRect.size.x - tLabelWidth, tRect.size.y);
            mValues[pIndex] = EditorGUI.Vector3Field(tRect, string.Empty, mValues[pIndex]);
            return mValues[pIndex];
        }

        public override int LayoutHeight(Method pInfo)
        {
            return 15;
        }
    }
}