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

        public override object LayoutDrawer(Property pInfo, int pParamIndex = 0)
        {
            var tQuaternion = Quaternion.identity;
            var tValue = Vector4.zero;
            if (pInfo.info.CanRead)
            {
                tQuaternion = pInfo.GetValue<Quaternion>();
                tValue = new Vector4(tQuaternion.x, tQuaternion.y, tQuaternion.z, tQuaternion.w);
            }
            GUI.enabled = pInfo.info.CanWrite;
            var tNewValue = GUIHelper.Vector4Field(tValue, this);
            GUI.enabled = true;
            if (GUI.changed)
            {
                tQuaternion = new Quaternion(tNewValue.x, tNewValue.y, tNewValue.z, tNewValue.w);
                pInfo.SetValue<Quaternion>(tQuaternion);
            }
            return tQuaternion;
        }
    }
}