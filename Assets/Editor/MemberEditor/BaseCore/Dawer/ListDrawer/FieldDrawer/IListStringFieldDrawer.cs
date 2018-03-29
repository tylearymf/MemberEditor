namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    [IListDrawer(System.Reflection.MemberTypes.Field)]
    public class IListStringFieldDrawer : IListDrawerInterface
    {
        public string typeName
        {
            get
            {
                return typeof(String).FullName;
            }
        }

        public object DrawerListItem<T>(IListDrawerInfo<T> pInfo)
        {
            var tValues = pInfo.Sources as IList<string>;

            var tDrawer = new Func<FuncInfo<string, T>, FuncInfo<string, T>>(pFuncInfo =>
            {
                EditorGUILayout.BeginHorizontal();
                pFuncInfo.value = EditorGUILayout.TextField(pFuncInfo.value);
                GUI.color = (pFuncInfo.verIndex & 1) == 0 ? Color.yellow : Color.magenta;
                if (GUILayout.Button("x", ConstHelper.GetButtonStyle(ButtonSizeType.Normal)))
                {
                    pInfo.Sources.Remove(pFuncInfo.value);
                    pFuncInfo.refreshImmediate = true;
                }
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
                return pFuncInfo;
            });

            GUIHelper.ListField(tValues, false, pInfo, (pIndex, pVal) =>
            {
                tValues[pIndex] = pVal;
            }, pDrawer: tDrawer, pDefaultVal: () => string.Empty);

            return tValues;
        }
    }
}