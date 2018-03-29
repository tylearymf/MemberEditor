namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    [MemeberDrawer(System.Reflection.MemberTypes.Property)]
    public class IListPropertyDrawer : BaseDrawer<Property>
    {
        Dictionary<Property, IListDrawerInfo<Property>> mValues = new Dictionary<Property, IListDrawerInfo<Property>>();

        public override string typeName
        {
            get
            {
                return typeof(IList<>).FullName;
            }
        }

        public override object LayoutDrawer(Property pInfo, int pParamIndex = 0)
        {
            var tIListDrawerDic = MemberHelper.GetIListDrawers(System.Reflection.MemberTypes.Property);
            if (tIListDrawerDic == null) return null;
            IList tValues = null;

            if (pInfo.info.CanRead)
            {
                tValues = pInfo.GetValue<IList>();
            }
            if (tValues == null) return null;

            var tArgs = pInfo.info.PropertyType.GetGenericArguments();
            var tArgType = tArgs.Length == 0 ? pInfo.info.PropertyType.GetElementType() : tArgs[0];
            var tArgTypeName = tArgType == null ? string.Empty : tArgType.FullName;

            if (tIListDrawerDic.ContainsKey(tArgTypeName) && tIListDrawerDic[tArgTypeName] != null)
            {
                if (!mValues.ContainsKey(pInfo)) mValues.Add(pInfo, new IListDrawerInfo<Property>());
                var tValue = mValues[pInfo];
                tValue.BaseDrawer = this;
                tValue.Sources = tValues;
                tValue.info = pInfo;

                return tIListDrawerDic[tArgType.FullName].DrawerListItem(tValue);
            }
            else
            {
                EditorGUILayout.LabelField(string.Format("未实现IList绘制类 field:{0}", tArgTypeName));
                return null;
            }
        }
    }
}