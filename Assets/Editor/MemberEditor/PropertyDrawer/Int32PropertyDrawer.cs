namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class Int32PropertyDrawer : BaseDrawer<Property>
    {
        public override string typeName
        {
            get
            {
                return typeof(Int32).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pIndex)
        {
            Int32 tValue = 0;
            if (pInfo.info.CanRead)
            {
                tValue = pInfo.GetValue<Int32>();
            }
            EditorGUI.BeginDisabledGroup(!pInfo.info.CanWrite);
            EditorGUI.BeginChangeCheck();
            var tNewValue = EditorGUI.IntField(pInfo.rect, string.Empty, tValue);
            if (EditorGUI.EndChangeCheck())
            {
                tValue = tNewValue;
                pInfo.SetValue<Int32>(tValue);
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