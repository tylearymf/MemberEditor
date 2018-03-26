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

            EditorGUI.BeginDisabledGroup(!pInfo.info.CanWrite);
            var tRect = pInfo.rect;
            tRect.size = new Vector2(tRect.size.x, 15);
            int tNewCount = Mathf.Max(0, EditorGUI.IntField(tRect, "Size", mValues.Count));
            while (tNewCount < mValues.Count)
            {
                mValues.RemoveAt(mValues.Count - 1);
            }
            while (tNewCount > mValues.Count)
            {
                mValues.Add(new Resolution());
            }
            var tInteval = 10;
            var tWidth = (pInfo.rect.size.x - tInteval * (3 - 1)) / 3;
            string[] tLabelNames = { "width", "height", "refreshRate123124" };
            //var tLabelWidth = tLabelNames.OrderByDescending(x => x.Length).FirstOrDefault().Length * 7;
            for (int i = 0, imax = mValues.Count; i < imax; i++)
            {
                tRect = pInfo.rect;

                EditorGUI.BeginChangeCheck();
                tRect.position = pInfo.rect.position + new Vector2(0, (i + 1) * 15) + new Vector2(tInteval * 0, 0);
                var tLabelWidth = tLabelNames[0].Length * 7;
                tRect.size = new Vector2(tLabelWidth, 15);
                EditorGUI.LabelField(tRect, new GUIContent(tLabelNames[0]));
                tRect.size = new Vector2(tWidth - tLabelWidth, 15);
                tRect.position = pInfo.rect.position + new Vector2(tLabelWidth + tWidth * 0, (i + 1) * 15) + new Vector2(tInteval * 0, 0);
                if (tRect.xMin >= tRect.xMax) tRect.size = Vector2.one;
                var t1 = EditorGUI.IntField(tRect, mValues[i].width);

                tRect.position = pInfo.rect.position + new Vector2(tWidth * 1, (i + 1) * 15) + new Vector2(tInteval * 1, 0);
                tLabelWidth = tLabelNames[1].Length * 7;
                tRect.size = new Vector2(tLabelWidth, 15);
                EditorGUI.LabelField(tRect, new GUIContent(tLabelNames[1]));
                tRect.size = new Vector2(tWidth - tLabelWidth, 15);
                tRect.position = pInfo.rect.position + new Vector2(tLabelWidth + tWidth * 1, (i + 1) * 15) + new Vector2(tInteval * 1, 0);
                if (tRect.xMin >= tRect.xMax) tRect.size = Vector2.one;
                var t2 = EditorGUI.IntField(tRect, mValues[i].height);

                tRect.position = pInfo.rect.position + new Vector2(tWidth * 2, (i + 1) * 15) + new Vector2(tInteval * 2, 0);
                tLabelWidth = tLabelNames[2].Length * 7;
                tRect.size = new Vector2(tLabelWidth, 15);
                EditorGUI.LabelField(tRect, new GUIContent(tLabelNames[2]));
                tRect.size = new Vector2(tWidth - tLabelWidth, 15);
                tRect.position = pInfo.rect.position + new Vector2(tLabelWidth + tWidth * 2, (i + 1) * 15) + new Vector2(tInteval * 2, 0);
                if (tRect.xMin >= tRect.xMax) tRect.size = Vector2.one;
                var t3 = EditorGUI.IntField(tRect, mValues[i].refreshRate);
                if (EditorGUI.EndChangeCheck())
                {
                    mValues[i] = new Resolution()
                    {
                        width = t1,
                        height = t2,
                        refreshRate = t3,
                    };
                }
            }
            EditorGUI.EndDisabledGroup();

            return mValues.ToArray();
        }

        public override int LayoutHeight(Property pInfo)
        {
            var tHeight = 15;
            if (pInfo.info.CanRead)
            {
                var tArray = pInfo.GetValue<Resolution[]>();
                tHeight = Mathf.Max((tArray.GetCountIgnoreNull() + 1) * 15, 15);
            }
            return tHeight;
        }
    }
}