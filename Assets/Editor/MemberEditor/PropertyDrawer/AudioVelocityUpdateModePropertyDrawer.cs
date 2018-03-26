namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class AudioVelocityUpdateModePropertyDrawer : BaseDrawer<Property>
    {
        public override string typeName
        {
            get
            {
                return typeof(AudioVelocityUpdateMode).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pIndex)
        {
            var tValue = AudioVelocityUpdateMode.Auto;
            if (pInfo.info.CanRead)
            {
                tValue = pInfo.GetValue<AudioVelocityUpdateMode>();
            }
            EditorGUI.BeginDisabledGroup(!pInfo.info.CanWrite);
            EditorGUI.BeginChangeCheck();
            var tNewValue = (AudioVelocityUpdateMode)EditorGUI.EnumPopup(pInfo.rect, string.Empty, tValue);
            if (EditorGUI.EndChangeCheck())
            {
                tValue = tNewValue;
                pInfo.SetValue<int>((int)tValue);
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