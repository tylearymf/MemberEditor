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

        public override object LayoutDrawer(Property pInfo, int pParamIndex = 0)
        {
            var tVector3 = Vector3.zero;
            if (pInfo.info.CanRead)
            {
                tVector3 = pInfo.GetValue<Vector3>();
            }

            GUI.enabled = pInfo.info.CanWrite;
            var tNewVector3 = GUIHelper.Vector3Field(tVector3, this, pInfo.info.Name);
            GUI.enabled = true;
            if (GUI.changed)
            {
                tVector3 = tNewVector3;
                pInfo.SetValue(tVector3);
            }
            EditorGUI.EndDisabledGroup();
            return tVector3;
        }
    }
}
