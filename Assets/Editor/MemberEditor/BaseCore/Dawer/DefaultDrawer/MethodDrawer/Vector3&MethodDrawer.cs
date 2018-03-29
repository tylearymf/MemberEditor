namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class Vector3_MethodDrawer : BaseDrawer<Method>
    {
        MultiInfo<Method, Vector3> mValues = new MultiInfo<Method, Vector3>();

        public override string typeName
        {
            get
            {
                return "UnityEngine.Vector3&";
            }
        }

        public override object LayoutDrawer(Method pInfo, int pParamIndex = 0)
        {
            var tInfo = pInfo.info;
            var tValue = mValues.GetValue(pParamIndex, pInfo, () => Vector3.zero);

            var tNewVal = GUIHelper.Vector3Field(tValue, this, tInfo.GetParameters()[pParamIndex].Name);
            if (GUI.changed)
            {
                mValues.SetValue(pParamIndex, pInfo, tNewVal);
            }
            return tValue;
        }
    }
}