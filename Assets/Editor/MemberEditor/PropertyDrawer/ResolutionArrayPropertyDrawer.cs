namespace Tylearymf.MemberEditor
{
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class ResolutionArrayPropertyDrawer : BaseDrawer<Property>
    {
        List<Resolution> mValues;
        bool mShowIndex = false;

        public override string typeName
        {
            get
            {
                return typeof(Resolution[]).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pIndex)
        {
            if (pInfo.info.CanRead)
            {
                var tArray = pInfo.GetValue<Resolution[]>();
                mValues = tArray == null ? new List<Resolution>() : tArray.ToList();
            }
            if (mValues == null) return null;

            MemberHelper.DrawerListItem(mValues, new List<string>() { "width", "height", "refreshRate" }, pRect: pInfo.rect, pDisable: !pInfo.info.CanWrite,
               pShowIndex: ref mShowIndex, pIntervalWidth: 10, pOnValueChange: (pIdx, pVal) =>
            {
                mValues[pIdx] = new Resolution()
                {
                    width = (int)pVal[0],
                    height = (int)pVal[1],
                    refreshRate = (int)pVal[2],
                };
            }, pInputFields: new Func<Rect, Resolution, object>[]
            {
                (pRect,pVal) =>
                {
                    return EditorGUILayout.IntField(pVal.width);
                },
                (pRect,pVal) =>
                {
                    return EditorGUILayout.IntField(pVal.height);
                 },
                 (pRect,pVal) =>
                 {
                     return EditorGUILayout.IntField(pVal.refreshRate);
                 },
            });
            return mValues.ToArray();
        }

        public override int LayoutHeight(Property pInfo)
        {
            return 0;
        }
    }
}