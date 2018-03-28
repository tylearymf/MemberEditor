namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    [IListDrawer(System.Reflection.MemberTypes.Field)]
    public class IListStringFieldDrawer : IListDrawer
    {
        IList<string> mValues;
        bool mShowIndex = false;

        public string typeName
        {
            get
            {
                return typeof(String).FullName;
            }
        }

        public object DrawerListItem<T>(IList pSources, BaseDrawer<T> pDrawer)
        {
            mValues = pSources as IList<string>;
            var tInputFields = new Func<FuncInfo1<string>, FuncInfo<string>>[]
            {
                pVal =>
                {
                    EditorGUILayout.BeginHorizontal();
                    var tVal = EditorGUILayout.TextField(pVal.value);
                    var tRefresh = false;
                    GUI.color = (pVal.index1 & 1) == 0 ? Color.yellow  : Color.magenta;
                    if (GUILayout.Button("x",GUILayout.Width(50)))
                    {
                        pSources.Remove(pVal);
                        tRefresh = true;
                    }
                      GUI.color = Color.white;
                    EditorGUILayout.EndHorizontal();
                    return new FuncInfo<string>(){ value = tVal,refresh =tRefresh  };
                }
             };

            GUIHelper.ListField(mValues, null, false, ref mShowIndex, (pIndex, pVal) =>
            {
                mValues[pIndex] = pVal[0] == null ? string.Empty : pVal[0].ToString();
            }, pDrawer, pInputFields: tInputFields);

            return mValues;
        }
    }
}