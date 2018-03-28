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

            var tIsDrawerAll = true;
            var tParmTypes = tMethodInfo.GetParameters();
            object[] tParamObjs = tParmTypes.IsNullOrEmpty() ? null : new object[tParmTypes.Length];
            for (int i = 0, imax = tParmTypes.Length; i < imax; i++)
            {
                var tIsDrawer = false;
                var tParam = tParmTypes[i];
                pEntry.SmartValue.entry = pEntry;
                pEntry.SmartValue.content = pContent;

                foreach (var tType in tParam.ParameterType.GetParentTypes())
                {
                    var tMemberTypeName = tType.FullName;
                    var tDrawerInfo = tInfoDic.TryGetValue(tMemberTypeName, null);
                    if (tDrawerInfo != null)
                    {
                        tIsDrawer = true;
                        var tObj = tDrawerInfo.LayoutDrawer(pEntry.SmartValue, i);
                        tParamObjs[i] = tObj;
                        break;
                    }
                }

                if (!tIsDrawer)
                {
                    tIsDrawerAll = false;
                    EditorGUILayout.LabelField(pEntry.SmartValue.NotImplementedDescription(i));
                }
            }

            GUI.skin.button.richText = true;
            GUI.color = new Color(0.3f, 0.8f, 0.8f, 1);

            if (tIsDrawerAll && GUILayout.Button(string.Format("<color=#CEA736FF>点击调用该方法</color>“{0}”", tMethodInfo.Name)))
            {
                tMethod.Call(tParamObjs);
            }
            GUI.color = Color.white;
        }
    }
}