namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    [IListDrawer(System.Reflection.MemberTypes.Field)]
    public class IListTestInfoFieldDrawer : IListDrawerInterface
    {
        public string typeName
        {
            get
            {
                return typeof(TestInfo).FullName;
            }
        }

        public object DrawerListItem<T>(IListDrawerInfo<T> pInfo)
        {
            var tValues = pInfo.Sources as IList<TestInfo>;
            var tDrawer = new Func<FuncInfo<TestInfo, T>, FuncInfo<TestInfo, T>>(pFuncInfo =>
            {
                if (pFuncInfo.value == null) return pFuncInfo;
                EditorGUILayout.BeginHorizontal();
                pFuncInfo.value.a = EditorGUILayout.TextField(pFuncInfo.value.a);
                pFuncInfo.value.b = EditorGUILayout.FloatField(pFuncInfo.value.b);
                pFuncInfo.value.c = EditorGUILayout.Toggle(pFuncInfo.value.c);
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
             }, pDrawer: tDrawer, pDefaultVal: () => new TestInfo(), pNewList: () =>
             {
                 var tInfo = pInfo as IListDrawerInfo<Field>;
                 var tNewList = Activator.CreateInstance(tInfo.info.info.FieldType) as IList;
                 if (tNewList != null)
                 {
                     tInfo.info.SetValue(tNewList);
                 }
             });

            return tValues;
        }
    }
}