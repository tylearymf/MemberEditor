namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class StringPropertyDrawer : BaseDrawer<Property>
    {
        public override string typeName
        {
            get
            {
                return typeof(String).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pIndex)
        {
            var tValue = string.Empty;
            if (pInfo.info.CanRead)
            {
                tValue = pInfo.GetValue<String>();
            }
            EditorGUI.BeginDisabledGroup(!pInfo.info.CanWrite);
            EditorGUI.BeginChangeCheck();
            var tNewValue = EditorGUI.TextField(pInfo.rect, tValue);
            if (EditorGUI.EndChangeCheck())
            {
                tValue = tNewValue;
                pInfo.SetValue<String>(tValue);
            }
            EditorGUI.EndDisabledGroup();
            return tValue;
        }
    }
}