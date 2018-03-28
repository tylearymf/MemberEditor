namespace Tylearymf.MemberEditor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Sirenix.OdinInspector.Editor;
    using System;
    using UnityEditor;

    [OdinDrawer]
    public sealed class FieldDrawer : PrimitiveCompositeDrawer<Field>
    {
        protected override void DrawPropertyField(IPropertyValueEntry<Field> pEntry, GUIContent pContent)
        {
            if (pEntry.SmartValue == null || pEntry.SmartValue.info == null) return;

            var tInfoDic = MemberHelper.GetDrawInfos(System.Reflection.MemberTypes.Field);
            if (tInfoDic == null) return;
            var tInfo = pEntry.SmartValue.info;
            pEntry.SmartValue.entry = pEntry;
            pEntry.SmartValue.content = pContent;

            bool tIsDrawer = false;
            foreach (var tType in tInfo.FieldType.GetParentTypes())
            {
                var tMemberTypeName = string.Empty;
                if (tType.IsGenericType && tType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    tMemberTypeName = typeof(List<>).FullName;
                }
                else
                {
                    tMemberTypeName = tType.ToString();
                }
                if (tInfoDic.ContainsKey(tMemberTypeName) && tInfoDic[tMemberTypeName] != null)
                {
                    tIsDrawer = true;
                    tInfoDic[tMemberTypeName].LayoutDrawer(pEntry.SmartValue);
                    break;
                }
            }
            if (!tIsDrawer)
            {
                EditorGUILayout.LabelField(pEntry.SmartValue.NotImplementedDescription());
            }
        }
    }
}