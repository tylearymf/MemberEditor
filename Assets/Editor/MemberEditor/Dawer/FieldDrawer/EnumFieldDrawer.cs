namespace Tylearymf.MemberEditor
{
    using UnityEngine;
    using UnityEditor;
    using System;

    [MemeberDrawer(System.Reflection.MemberTypes.Field)]
    public class EnumFieldDrawer : BaseDrawer<Field>
    {
        public override string typeName
        {
            get
            {
                return typeof(Enum).FullName;
            }
        }

        public override object LayoutDrawer(Field pInfo, int pParamIndex = 0)
        {
            var tVal = pInfo.GetValue<Enum>();
            var tNewVal = EditorGUILayout.EnumPopup(tVal);
            if (GUI.changed)
            {
                tVal = tNewVal;
                pInfo.SetValue<Enum>(tVal);
            }
            return tVal;
        }
    }
}