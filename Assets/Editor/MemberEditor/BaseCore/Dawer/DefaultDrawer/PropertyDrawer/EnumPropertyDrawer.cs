namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    //删掉这个脚本，要判断如果类型是Eunm的，就统一画Enum框，而不是每个枚举都画一次（还有List<ResolutionArray>,这个也要换成List的或者Array的，而不是写死具体的类型）
    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class EnumPropertyDrawer : BaseDrawer<Property>
    {
        Enum mValue;
        public override string typeName
        {
            get
            {
                return typeof(Enum).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pParamIndex = 0)
        {
            if (mValue == null)
            {
                mValue = (Enum)Activator.CreateInstance(pInfo.info.PropertyType);
            }
            if (pInfo.info.CanRead)
            {
                mValue = pInfo.GetValue<Enum>();
            }
            GUI.enabled = pInfo.info.CanWrite;
            var tNewValue = EditorGUILayout.EnumPopup(pInfo.info.Name, mValue);
            GUI.enabled = true;
            if (GUI.changed)
            {
                mValue = tNewValue;
                pInfo.SetValue<Enum>(tNewValue);
            }
            return mValue;
        }
    }
}