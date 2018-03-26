namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class Matrix4x4_MethodDrawer : BaseDrawer<Method>
    {
        Dictionary<int, Matrix4x4> mValues = new Dictionary<int, Matrix4x4>();

        public override string typeName
        {
            get
            {
                return "UnityEngine.Matrix4x4&";
            }
        }

        public override object LayoutDrawer(Method pInfo, int pIndex)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pIndex];
            if (!mValues.ContainsKey(pIndex)) mValues.Add(pIndex, Matrix4x4.zero);

            var tValue = mValues[pIndex];
            var tNewValue = Matrix4x4.zero;
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
            mValues[pIndex] = tNewValue;
            return mValues[pIndex];
        }

        public override int LayoutHeight(Method pInfo)
        {
            return 15 * 4;
        }
    }
}