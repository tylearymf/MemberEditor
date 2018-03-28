namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class RectPropertyDrawer : BaseDrawer<Property>
    {
        public override string typeName
        {
            get
            {
                return typeof(Rect).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pParamIndex = 0)
        {
            var tValue = Rect.zero;
            if (pInfo.info.CanRead)
            {
                tValue = pInfo.GetValue<Rect>();
            }
            GUI.enabled = pInfo.info.CanWrite;
            var tNewValue = EditorGUILayout.RectField(tValue);
            GUI.enabled = true;
            if (GUI.changed)
            {
                tValue = tNewValue;
                pInfo.SetValue<Rect>(tValue);
            }
            return tValue;
        }
    }
}