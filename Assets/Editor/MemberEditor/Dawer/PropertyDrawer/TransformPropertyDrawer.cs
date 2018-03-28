namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class TransformPropertyDrawer : BaseDrawer<Property>
    {
        public override string typeName
        {
            get
            {
                return typeof(Transform).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pParamIndex = 0)
        {
            Transform tValue = null;
            if (pInfo.info.CanRead)
            {
                tValue = pInfo.GetValue<Transform>();
            }
            GUI.enabled = pInfo.info.CanWrite;
            var tNewValue = EditorGUILayout.ObjectField(tValue, typeof(Transform), false);
            GUI.enabled = true;
            if (GUI.changed)
            {
                tValue = tNewValue as Transform;
                pInfo.SetValue<Transform>(tValue);
            }
            return tValue;
        }
    }
}