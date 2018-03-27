namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class OpaqueSortModePropertyDrawer : BaseDrawer<Property>
    {
        public override string typeName
        {
            get
            {
                return typeof(OpaqueSortMode).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pIndex)
        {
            var tValue = OpaqueSortMode.Default;
            if (pInfo.info.CanRead)
            {
                tValue = pInfo.GetValue<OpaqueSortMode>();
            }
            EditorGUI.BeginDisabledGroup(!pInfo.info.CanWrite);
            EditorGUI.BeginChangeCheck();
            var tNewValue = (OpaqueSortMode)EditorGUI.EnumPopup(pInfo.rect, string.Empty, tValue);
            if (EditorGUI.EndChangeCheck())
            {
                tValue = tNewValue;
                pInfo.SetValue<int>((int)tValue);
            }
            EditorGUI.EndDisabledGroup();
            return tValue;
        }
    }
}