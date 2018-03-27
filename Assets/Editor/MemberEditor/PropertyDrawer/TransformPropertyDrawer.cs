namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class TransformPropertyDrawer : BaseDrawer<Property>
    {
        public override string typeName
        {
            get
            {
                return typeof(Transform).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pIndex)
        {
            Transform tValue = null;
            if (pInfo.info.CanRead)
            {
                tValue = pInfo.GetValue<Transform>();
            }
            EditorGUI.BeginDisabledGroup(!pInfo.info.CanWrite);
            EditorGUI.BeginChangeCheck();
            var tNewValue = EditorGUI.ObjectField(pInfo.rect, tValue, typeof(Transform), false);
            if (EditorGUI.EndChangeCheck())
            {
                tValue = tNewValue as Transform;
                pInfo.SetValue<Transform>(tValue);
            }
            EditorGUI.EndDisabledGroup();
            return tValue;
        }
    }
}