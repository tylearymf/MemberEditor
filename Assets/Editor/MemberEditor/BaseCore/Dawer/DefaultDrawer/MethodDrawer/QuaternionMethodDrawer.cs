namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class QuaternionMethodDrawer : BaseDrawer<Method>
    {
        MultiInfo<Method, Quaternion> mValues = new MultiInfo<Method, Quaternion>();

        public override string typeName
        {
            get
            {
                return typeof(Quaternion).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pParamIndex = 0)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pParamIndex];
            var tValue = mValues.GetValue(pParamIndex, pInfo, () => Quaternion.identity);

            var tNewVal = GUIHelper.Vector4Field(tValue.ToVector4(), this, tParam.Name);
            if (GUI.changed)
            {
                mValues.SetValue(pParamIndex, pInfo, tNewVal.ToQuaternion());
            }
            return tValue;
        }
    }
}