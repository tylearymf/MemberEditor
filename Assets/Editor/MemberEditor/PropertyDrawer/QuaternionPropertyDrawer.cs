namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class QuaternionPropertyDrawer : BaseDrawer<Property>
    {
        public override string typeName
        {
            get
            {
                return typeof(Quaternion).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pIndex)
        {
            Quaternion tQuaternion = Quaternion.identity;
            Vector4 tValue = Vector4.zero;
            if (pInfo.info.CanRead)
            {
                tQuaternion = pInfo.GetValue<Quaternion>();
                tValue = new Vector4(tQuaternion.x, tQuaternion.y, tQuaternion.z, tQuaternion.w);
            }
            EditorGUI.BeginDisabledGroup(!pInfo.info.CanWrite);
            EditorGUI.BeginChangeCheck();
            var tNewValue = EditorGUI.Vector4Field(pInfo.rect, string.Empty, tValue);
            if (EditorGUI.EndChangeCheck())
            {
                tQuaternion = new Quaternion(tNewValue.x, tNewValue.y, tNewValue.z, tNewValue.w);
                pInfo.SetValue<Quaternion>(tQuaternion);
            }
            EditorGUI.EndDisabledGroup();
            return tQuaternion;
        }

        public override int LayoutHeight(Property pInfo)
        {
            return 15;
        }
    }
}