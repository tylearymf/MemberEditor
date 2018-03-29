namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class EnumMethodDrawer : BaseDrawer<Method>
    {
        MultiInfo<Method, Enum> mValues = new MultiInfo<Method, Enum>();

        public override string typeName
        {
            get
            {
                return typeof(Enum).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pParamIndex = 0)
        {
            var tParam = pInfo.info.GetParameters()[pParamIndex];
            var tValue = mValues.GetValue(pParamIndex, pInfo, () => (Enum)Activator.CreateInstance(tParam.ParameterType));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(tParam.Name, GUILayout.Width(tParam.Name.GetLabelWidth(this)));
            var tNewVal = EditorGUILayout.EnumPopup(tValue);
            EditorGUILayout.EndHorizontal();
            if (GUI.changed)
            {
                mValues.SetValue(pParamIndex, pInfo, tNewVal);
            }
            return tValue;
        }
    }
}