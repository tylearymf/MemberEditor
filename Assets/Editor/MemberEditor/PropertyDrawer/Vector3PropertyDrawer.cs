namespace Tylearymf.MemberEditor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class Vector3PropertyDrawer : BaseDrawer<Property>
    {
        public override string typeName
        {
            get
            {
                return typeof(Vector3).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pIndex)
        {
            var tVector3 = Vector3.zero;
            if (pInfo.info.CanRead)
            {
                tVector3 = pInfo.GetValue<Vector3>();
            }

            EditorGUI.BeginDisabledGroup(!pInfo.info.CanWrite);
            EditorGUI.BeginChangeCheck();
            var tNewVector3 = EditorGUI.Vector3Field(pInfo.rect, string.Empty, tVector3);
            if (EditorGUI.EndChangeCheck())
            {
                tVector3 = tNewVector3;
                pInfo.SetValue(tVector3);
            }
            EditorGUI.EndDisabledGroup();
            return tVector3;
        }

        public override int LayoutHeight(Property pInfo)
        {
            return 15;
        }
    }
}
