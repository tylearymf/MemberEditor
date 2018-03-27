namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class RenderingPathMethodDrawer : BaseDrawer<Method>
    {
        Dictionary<int, RenderingPath> mValues = new Dictionary<int, RenderingPath>();

        public override string typeName
        {
            get
            {
                return typeof(RenderingPath).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pIndex)
        {
            var tInfo = pInfo.info;
            var tParam = tInfo.GetParameters()[pIndex];
            if (!mValues.ContainsKey(pIndex)) mValues.Add(pIndex, RenderingPath.UsePlayerSettings);
            mValues[pIndex] = (RenderingPath)EditorGUI.EnumPopup(pInfo.rect, tParam.Name, mValues[pIndex]);
            return (int)mValues[pIndex];
        }
    }
}