namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class Int64PropertyDrawer : BaseDrawer<Property>
    {
        public override string typeName
        {
            get
            {
                return typeof(Int64).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pIndex)
        {
            Int64 tValue = 0;
            if (pInfo.info.CanRead)
            {
                tValue = pInfo.GetValue<Int64>();
            }
            EditorGUI.BeginDisabledGroup(!pInfo.info.CanWrite);
            EditorGUI.BeginChangeCheck();
            var tNewValue = EditorGUI.LongField(pInfo.rect, string.Empty, tValue);
            if (EditorGUI.EndChangeCheck())
            {
                tValue = tNewValue;
                pInfo.SetValue<Int64>(tValue);
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