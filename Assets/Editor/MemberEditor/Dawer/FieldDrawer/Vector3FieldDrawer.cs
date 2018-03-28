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

        public override object LayoutDrawer(Field pInfo, int pParamIndex = 0)
        {
            var tVector3 = pInfo.GetValue<Vector3>();
            var tNewVector3 = GUIHelper.Vector3Field(tVector3, this, pInfo.info.Name);
            if (GUI.changed)
            {
                tVector3 = tNewVector3;
                pInfo.SetValue(tVector3);
            }
            return tVector3;
        }
    }
}
