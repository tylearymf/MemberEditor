namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class TempPropertyDrawer : BaseDrawer<Property>
    {
        public override string typeName
        {
            get
            {
                return typeof(TempType).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pIndex)
        {
            var tValue = false;
            if (pInfo.info.CanRead)
            {
                tValue = pInfo.GetValue<bool>();
            }
            EditorGUI.BeginDisabledGroup(!pInfo.info.CanWrite);
            EditorGUI.BeginChangeCheck();
            var tNewValue = EditorGUI.Toggle(pInfo.rect, string.Empty, tValue);
            if (EditorGUI.EndChangeCheck())
            {
                tValue = tNewValue;
                pInfo.SetValue<bool>(tValue);
            }
            EditorGUI.EndDisabledGroup();
            return tValue;
        }

        public override int LayoutHeight(Property pInfo)
        {
            return 15;
        }
    }
}