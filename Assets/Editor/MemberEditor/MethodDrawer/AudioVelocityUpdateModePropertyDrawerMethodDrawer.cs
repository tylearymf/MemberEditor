namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class AudioVelocityUpdateModePropertyDrawerMethodDrawer : BaseDrawer<Method>
    {
        Dictionary<int, AudioVelocityUpdateMode> mValues = new Dictionary<int, AudioVelocityUpdateMode>();

        public override string typeName
        {
            get
            {
                return typeof(AudioVelocityUpdateMode).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pIndex)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pIndex];
            if (!mValues.ContainsKey(pIndex)) mValues.Add(pIndex, AudioVelocityUpdateMode.Auto);
            var tNewVal = (AudioVelocityUpdateMode)EditorGUILayout.EnumPopup(tParam.Name, mValues[pIndex]);
            if (GUI.changed)
            {
                mValues[pIndex] = tNewVal;
            }
            return (int)mValues[pIndex];
        }
    }
}