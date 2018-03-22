namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Field)]
    public class Vector3FieldDrawer : BaseDrawer<Field>
    {
        public override string typeName
        {
            get
            {
                return typeof(Vector3).FullName;
            }
        }

        public override object LayoutDrawer(Field pInfo, int pIndex)
        {
            var tVector3 = pInfo.GetValue<Vector3>();
            EditorGUI.BeginChangeCheck();
            var tNewVector3 = EditorGUI.Vector3Field(pInfo.rect, pInfo.info.Name, tVector3);
            if (EditorGUI.EndChangeCheck())
            {
                tVector3 = tNewVector3;
                pInfo.SetValue(tVector3);
            }
            return tVector3;
        }

        public override int LayoutHeight()
        {
            return 30;
        }
    }
}
