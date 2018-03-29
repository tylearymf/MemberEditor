namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class BooleanPropertyDrawer : BaseDrawer<Property>
    {
        public override string typeName
        {
            get
            {
                return typeof(Boolean).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pParamIndex = 0)
        {
            var tValue = false;
            if (pInfo.info.CanRead)
            {
                tValue = pInfo.GetValue<bool>();
            }
            GUI.enabled = pInfo.info.CanWrite;
            var tNewValue = EditorGUILayout.Toggle(tValue);
            GUI.enabled = true;
            if (GUI.changed)
            {
                tValue = tNewValue;
                pInfo.SetValue<bool>(tValue);
            }
            return tValue;
        }
    }
}