namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    [IListDrawer(System.Reflection.MemberTypes.Property)]
    public class IListResolutionPropertyDrawer : IListDrawerInterface
    {
        public string typeName
        {
            get
            {
                return typeof(Resolution).FullName;
            }
        }

        public object DrawerListItem<T>(IListDrawerInfo<T> pInfo)
        {
            var tValues = pInfo.Sources as IList<Resolution>;
            var tDrawer = new Func<FuncInfo<Resolution, T>, FuncInfo<Resolution, T>>(pFuncInfo =>
             {
                 EditorGUILayout.BeginHorizontal();
                 var tInfo = pFuncInfo.drawerInfo as IListDrawerInfo<Property>;
                 GUI.enabled = tInfo.info.info.CanWrite;
                 var tResolution = pFuncInfo.value;
                 EditorGUILayout.LabelField("width", GUILayout.Width("width".GetLabelWidth(pInfo.BaseDrawer)));
                 tResolution.width = EditorGUILayout.IntField(tResolution.width);
                 EditorGUILayout.LabelField("height", GUILayout.Width("height".GetLabelWidth(pInfo.BaseDrawer)));
                 tResolution.height = EditorGUILayout.IntField(tResolution.height);
                 EditorGUILayout.LabelField("refreshRate", GUILayout.Width("refreshRate".GetLabelWidth(pInfo.BaseDrawer)));
                 tResolution.refreshRate = EditorGUILayout.IntField(tResolution.refreshRate);
                 GUI.enabled = true;
                 EditorGUILayout.EndHorizontal();
                 return pFuncInfo;
             });

            var tDrawerInfo = pInfo as IListDrawerInfo<Property>;
            GUIHelper.ListField(tValues, !tDrawerInfo.info.info.CanWrite, pInfo, (pIndex, pVal) =>
             {
                 tValues[pIndex] = pVal;
             }, pDrawer: tDrawer, pDefaultVal: () => new Resolution());

            return tValues;
        }
    }
}