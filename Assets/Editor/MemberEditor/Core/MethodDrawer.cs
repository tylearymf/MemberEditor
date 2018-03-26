namespace Tylearymf.MemberEditor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Sirenix.OdinInspector.Editor;
    using System;
    using UnityEditor;
    using Sirenix.OdinInspector;

    [OdinDrawer]
    public sealed class MethodDrawer : PrimitiveCompositeDrawer<Method>
    {
        protected override void DrawPropertyField(IPropertyValueEntry<Method> pEntry, GUIContent pContent)
        {
            if (pEntry.SmartValue == null || pEntry.SmartValue.info == null) return;
            var tMethod = pEntry.SmartValue;
            var tMethodInfo = tMethod.info;

            var tInfoDic = MemberHelper.GetDrawInfos(System.Reflection.MemberTypes.Method);
            if (tInfoDic == null) return;

            var tDrawButton = true;
            var tParmTypes = tMethodInfo.GetParameters();
            object[] tParamObjs = tParmTypes.IsNullOrEmpty() ? null : new object[tParmTypes.Length];
            for (int i = 0, imax = tParmTypes.Length; i < imax; i++)
            {
                var tParam = tParmTypes[i];
                var tMemberTypeName = tParam.ParameterType.FullName;
                var tDrawerInfo = tInfoDic.TryGetValue(tMemberTypeName, null);

                var tHeight = tDrawerInfo == null ? MemberHelper.cPropertyDefaultHeight : tDrawerInfo.LayoutHeight(pEntry.SmartValue);
                var tRect = EditorGUILayout.GetControlRect(false, tHeight);

                pEntry.SmartValue.rect = new Rect(tRect.position, tRect.size);
                pEntry.SmartValue.entry = pEntry;
                pEntry.SmartValue.content = pContent;

                if (tDrawerInfo == null)
                {
                    EditorGUI.LabelField(pEntry.SmartValue.rect, pEntry.SmartValue.NotImplementedDescription(i));
                    tDrawButton = false;
                }
                else
                {
                    var tObj = tDrawerInfo.LayoutDrawer(pEntry.SmartValue, i);
                    tParamObjs[i] = tObj;
                }
            }

            GUI.skin.button.richText = true;
            GUI.color = new Color(0.3f, 0.8f, 0.8f, 1);

            if (tDrawButton && GUILayout.Button(string.Format("<color=#CEA736FF>点击调用该方法</color>“{0}”", tMethodInfo.Name)))
            {
                tMethod.Call(tParamObjs);
            }
            GUI.color = Color.white;
        }
    }
}