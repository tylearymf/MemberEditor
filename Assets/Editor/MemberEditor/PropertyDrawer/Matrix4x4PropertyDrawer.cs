namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class Matrix4x4PropertyDrawer : BaseDrawer<Property>
    {
        public override string typeName
        {
            get
            {
                return typeof(Matrix4x4).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pIndex)
        {
            var tValue = Matrix4x4.identity;
            var tNewValue = Matrix4x4.identity;
            if (pInfo.info.CanRead)
            {
                tValue = pInfo.GetValue<Matrix4x4>();
            }
            EditorGUI.BeginDisabledGroup(!pInfo.info.CanWrite);
            EditorGUI.BeginChangeCheck();
            var tRect = pInfo.rect;
            for (int i = 0; i < 4; i++)
            {
                tRect.position = pInfo.rect.position + new Vector2(0, i * 15);
                var tVal = EditorGUI.Vector4Field(tRect, string.Empty, new Vector4(tValue[i, 0], tValue[i, 1], tValue[i, 2], tValue[i, 3]));
                tNewValue[i, 0] = tVal.x;
                tNewValue[i, 1] = tVal.y;
                tNewValue[i, 2] = tVal.z;
                tNewValue[i, 3] = tVal.w;
            }
            if (EditorGUI.EndChangeCheck())
            {
                tValue = tNewValue;
                pInfo.SetValue<Matrix4x4>(tValue);
            }
            EditorGUI.EndDisabledGroup();
            return tValue;
        }

        public override int LayoutHeight()
        {
            return 15 * 4;
        }
    }
}