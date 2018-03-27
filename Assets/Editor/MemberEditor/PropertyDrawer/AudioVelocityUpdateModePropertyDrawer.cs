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
            GUI.enabled = pInfo.info.CanWrite;
            var tNewValue = (AudioVelocityUpdateMode)EditorGUILayout.EnumPopup(string.Empty, tValue);
            GUI.enabled = true;
            if (GUI.changed)
            {
                tValue = tNewValue;
                pInfo.SetValue<int>((int)tValue);
            }
            return tValue;
        }
    }
}