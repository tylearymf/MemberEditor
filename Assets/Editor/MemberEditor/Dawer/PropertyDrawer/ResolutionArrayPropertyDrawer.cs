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
        Resolution[] mValues;
        bool mShowIndex = false;
        readonly List<string> mLabelNames = new List<string>() { "width", "height", "refreshRate" };

        public override string typeName
        {
            get
            {
                return typeof(Resolution[]).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pParamIndex = 0)
        {
            if (pInfo.info.CanRead)
            {
                mValues = pInfo.GetValue<Resolution[]>();
            }
            if (mValues == null) return null;

            GUIHelper.ListField(mValues, mLabelNames, pDisable: !pInfo.info.CanWrite,
               pShowIndex: ref mShowIndex, pOnValueChange: (pIdx, pVal) =>
            {
                mValues[pIdx] = new Resolution()
                {
                    width = (int)pVal[0],
                    height = (int)pVal[1],
                    refreshRate = (int)pVal[2],
                };
            }, pInputFields: new Func<Resolution, object>[]
            {
                (pVal) =>
                {
                    return EditorGUILayout.IntField(pVal.width);
                },
                (pVal) =>
                {
                    return EditorGUILayout.IntField(pVal.height);
                 },
                 (pVal) =>
                 {
                     return EditorGUILayout.IntField(pVal.refreshRate);
                 },
            }, pDrawer: this);
            return mValues;
        }
    }
}