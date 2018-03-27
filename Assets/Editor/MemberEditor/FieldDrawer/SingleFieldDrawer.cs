namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Field)]
    public class SingleFieldDrawer : BaseDrawer<Field>
    {
        public override string typeName
        {
            get
            {
                return typeof(Single).FullName;
            }
        }

        public override object LayoutDrawer(Field pInfo, int pIndex)
        {
            var tValue = pInfo.GetValue<Single>();
            var tNewValue = EditorGUILayout.FloatField(tValue);
            if (GUI.changed)
            {
                tValue = tNewValue;
                pInfo.SetValue<Single>(tValue);
            }
            return tValue;
        }
    }
}