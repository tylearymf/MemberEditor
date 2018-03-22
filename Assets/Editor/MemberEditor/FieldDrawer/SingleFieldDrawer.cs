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
            EditorGUI.BeginChangeCheck();
            var tNewValue = EditorGUI.FloatField(pInfo.rect, string.Empty, tValue);
            if (EditorGUI.EndChangeCheck())
            {
                tValue = tNewValue;
                pInfo.SetValue<Single>(tValue);
            }
            return tValue;
        }

        public override int LayoutHeight()
        {
            return 15;
        }
    }
}