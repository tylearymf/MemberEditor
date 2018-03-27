namespace Tylearymf.MemberEditor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Sirenix.OdinInspector.Editor;
    using System;
    using UnityEditor;

    [OdinDrawer]
    public sealed class PropertyDrawer : PrimitiveCompositeDrawer<Property>
    {
        protected override void DrawPropertyField(IPropertyValueEntry<Property> pEntry, GUIContent pContent)
        {
            if (pEntry.SmartValue == null || pEntry.SmartValue.info == null) return;
            var tInfo = pEntry.SmartValue.info;
            var tMemberTypeName = tInfo.PropertyType.ToString();

            var tInfoDic = MemberHelper.GetDrawInfos(System.Reflection.MemberTypes.Property);
            if (tInfoDic == null) return;

            pEntry.SmartValue.entry = pEntry;
            pEntry.SmartValue.content = pContent;

            if (tInfoDic.ContainsKey(tMemberTypeName) && tInfoDic[tMemberTypeName] != null)
            {
                tInfoDic[tMemberTypeName].LayoutDrawer(pEntry.SmartValue);
            }
            else
            {
                EditorGUILayout.LabelField(pEntry.SmartValue.NotImplementedDescription());
            }
        }
    }
}