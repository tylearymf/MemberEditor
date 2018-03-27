﻿namespace Tylearymf.MemberEditor
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
            var tInfo = pEntry.SmartValue.info;
            var tMemberTypeName = tInfo.FieldType.ToString();

            var tInfoDic = MemberHelper.GetDrawInfos(System.Reflection.MemberTypes.Field);
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