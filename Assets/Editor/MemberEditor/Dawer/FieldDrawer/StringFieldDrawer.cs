namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Field)]
    public class StringFieldDrawer : BaseDrawer<Field>
    {
        public override string typeName
        {
            get
            {
                return typeof(String).FullName;
            }
        }

        public override object LayoutDrawer(Field pInfo, int pParamIndex = 0)
        {
            var tValue = pInfo.GetValue<String>();
            var tNewValue = EditorGUILayout.TextField(tValue);
            if (GUI.changed)
            {
                tValue = tNewValue;
                pInfo.SetValue<String>(tValue);
            }
            return tValue;
        }
    }
}