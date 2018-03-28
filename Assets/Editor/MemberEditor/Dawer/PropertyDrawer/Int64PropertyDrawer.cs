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

        public override object LayoutDrawer(Property pInfo, int pParamIndex = 0)
        {
            Int64 tValue = 0;
            if (pInfo.info.CanRead)
            {
                tValue = pInfo.GetValue<Int64>();
            }
            GUI.enabled = pInfo.info.CanWrite;
            var tNewValue = EditorGUILayout.LongField(tValue);
            GUI.enabled = true;
            if (GUI.changed)
            {
                tValue = tNewValue;
                pInfo.SetValue<Int64>(tValue);
            }
            return tValue;
        }
    }
}