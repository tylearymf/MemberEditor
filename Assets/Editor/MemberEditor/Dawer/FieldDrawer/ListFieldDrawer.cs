namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Field)]
    public class ListFieldDrawer : BaseDrawer<Field>
    {
        bool mShowIndex = false;
        public override string typeName
        {
            get
            {
                return typeof(List<>).FullName;
            }
        }

        public override object LayoutDrawer(Field pInfo, int pParamIndex = 0)
        {
            var tInfoDic = MemberHelper.GetDrawInfos(System.Reflection.MemberTypes.Field);
            if (tInfoDic == null) return null;

            var tValues = pInfo.GetValue<IList>();
            var tArgType = pInfo.info.FieldType.GetGenericArguments()[0];

            if (tArgType == typeof(System.String))
            {
                var tStringList = tValues as IList<string>;
                var tFuncs = new Func<string, object>[]
                {
                    pVal =>
                    {
                        return EditorGUILayout.TextField(pVal);
                    }
                 };

                GUIHelper.ListField(tStringList, null, false, ref mShowIndex, (pIndex, pVal) =>
                {
                    tValues[pIndex] = pVal[0];
                }, this, pInputFields: tFuncs);
            }
             
            //foreach (var tType in tArgType.GetParentTypes())
            //{
            //    var tMemberTypeName = string.Empty;
            //    if (tType.IsGenericType && tType.GetGenericTypeDefinition() == typeof(List<>))
            //    {
            //        tMemberTypeName = typeof(List<>).FullName;
            //    }
            //    else
            //    {
            //        tMemberTypeName = tType.ToString();
            //    }
            //    if (tInfoDic.ContainsKey(tMemberTypeName) && tInfoDic[tMemberTypeName] != null)
            //    {
            //        tIsDrawer = true;
            //        tInfoDic[tMemberTypeName].LayoutDrawer(pEntry.SmartValue);
            //        break;
            //    }
            //}
            //if (!tIsDrawer)
            //{
            //    EditorGUILayout.LabelField(pEntry.SmartValue.NotImplementedDescription());
            //}

            //GUIHelper.ListField(tValues, null, false, ref mShowIndex, (pIndex, pVal) =>
            //{
            //    tValues[pIndex] = pVal[0];
            //}, this, pInputFields: tFuncs);
            return null;
        }
    }
}