namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Field)]
    public class BooleanFieldDrawer : BaseDrawer<Field>
    {
        public override string typeName
        {
            get
            {
                return typeof(Boolean).FullName;
            }
        }

        public override object LayoutDrawer(Field pInfo, int pIndex)
        {
            var tValue = false;
            tValue = pInfo.GetValue<bool>();
            EditorGUI.BeginChangeCheck();
            var tNewValue = EditorGUI.Toggle(pInfo.rect, string.Empty, tValue);
            if (EditorGUI.EndChangeCheck())
            {
                tValue = tNewValue;
                pInfo.SetValue<bool>(tValue);
            }
            return tValue;
        }

        public override int LayoutHeight(Field pInfo)
        {
            return 15;
        }
    }
}