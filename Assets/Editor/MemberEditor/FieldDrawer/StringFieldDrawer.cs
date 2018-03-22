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

        public override object LayoutDrawer(Field pInfo, int pIndex)
        {
            var tValue = pInfo.GetValue<String>();
            EditorGUI.BeginChangeCheck();
            var tNewValue = EditorGUI.TextField(pInfo.rect, tValue);
            if (EditorGUI.EndChangeCheck())
            {
                tValue = tNewValue;
                pInfo.SetValue<String>(tValue);
            }
            return tValue;
        }

        public override int LayoutHeight()
        {
            return 15;
        }
    }
}