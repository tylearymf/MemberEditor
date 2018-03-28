namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class Matrix4x4PropertyDrawer : BaseDrawer<Property>
    {
        public override string typeName
        {
            get
            {
                return typeof(Matrix4x4).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pParamIndex = 0)
        {
            var tValue = Matrix4x4.identity;
            if (pInfo.info.CanRead)
            {
                tValue = pInfo.GetValue<Matrix4x4>();
            }
            var tNewValue = GUIHelper.Matrix4x4Field(tValue);
            if (GUI.changed)
            {
                tValue = tNewValue;
                pInfo.SetValue<Matrix4x4>(tValue);
            }
            return tValue;
        }
    }
}