namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class RotationOrderMethodDrawer : BaseDrawer<Method>
    {
        Dictionary<int, RotationOrder> mValues = new Dictionary<int, RotationOrder>();

        public override string typeName
        {
            get
            {
                return "UnityEngine.RotationOrder";
            }
        }

        public override object LayoutDrawer(Method pInfo, int pIndex)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pIndex];
            if (!mValues.ContainsKey(pIndex)) mValues.Add(pIndex, RotationOrder.OrderXYZ);
            mValues[pIndex] = (RotationOrder)EditorGUI.EnumPopup(pInfo.rect, tParam.Name, mValues[pIndex]);
            return (int)mValues[pIndex];
        }
    }
}

enum RotationOrder
{
    OrderXYZ = 0,
    OrderXZY = 1,
    OrderYZX = 2,
    OrderYXZ = 3,
    OrderZXY = 4,
    OrderZYX = 5
}