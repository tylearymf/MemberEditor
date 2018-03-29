namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class SinglePropertyDrawer : BaseDrawer<Property>
    {
        public override string typeName
        {
            get
            {
                return typeof(Single).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pParamIndex = 0)
        {
            var tValue = 0F;
            if (pInfo.info.CanRead)
            {
                tValue = pInfo.GetValue<Single>();
            }
            GUI.enabled = pInfo.info.CanWrite;
            var tNewValue = EditorGUILayout.FloatField(string.Empty, tValue);
            GUI.enabled = true;
            if (GUI.changed)
            {
                tValue = tNewValue;
                pInfo.SetValue<Single>(tValue);
            }
            return tValue;
        }
    }
}