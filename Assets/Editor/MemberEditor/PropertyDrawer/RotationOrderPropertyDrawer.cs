namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class RotationOrderPropertyDrawer : BaseDrawer<Property>
    {
        public override string typeName
        {
            get
            {
                return "UnityEngine.RotationOrder";
            }
        }

        public override object LayoutDrawer(Property pInfo, int pIndex)
        {
            var tValue = RotationOrder.OrderXYZ;
            if (pInfo.info.CanRead)
            {
                tValue = pInfo.GetValue<RotationOrder>();
            }
            EditorGUI.BeginDisabledGroup(!pInfo.info.CanWrite);
            EditorGUI.BeginChangeCheck();
            var tNewValue = (RotationOrder)EditorGUI.EnumPopup(pInfo.rect, pInfo.info.Name, tValue);
            if (EditorGUI.EndChangeCheck())
            {
                tValue = tNewValue;
                pInfo.SetValue<int>((int)tNewValue);
            }
            EditorGUI.EndDisabledGroup();
            return tValue;
        }

        public override int LayoutHeight()
        {
            return 15;
        }
    }
}