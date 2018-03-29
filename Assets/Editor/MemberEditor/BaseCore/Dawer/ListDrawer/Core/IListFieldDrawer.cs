namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    [MemeberDrawer(System.Reflection.MemberTypes.Field)]
    public class IListFieldDrawer : BaseDrawer<Field>
    {
        Dictionary<Field, IListDrawerInfo<Field>> mValues = new Dictionary<Field, IListDrawerInfo<Field>>();
        public override string typeName
        {
            get
            {
                return typeof(IList<>).FullName;
            }
        }

        public override object LayoutDrawer(Field pInfo, int pParamIndex = 0)
        {
            var tIListDrawerDic = MemberHelper.GetIListDrawers(System.Reflection.MemberTypes.Field);
            if (tIListDrawerDic == null) return null;

            var tValues = pInfo.GetValue<IList>();

            var tArgs = pInfo.info.FieldType.GetGenericArguments();
            var tArgType = tArgs.Length == 0 ? pInfo.info.FieldType.GetElementType() : tArgs[0];
            var tArgTypeName = tArgType == null ? string.Empty : tArgType.FullName;

            if (tIListDrawerDic.ContainsKey(tArgTypeName) && tIListDrawerDic[tArgTypeName] != null)
            {
                if (!mValues.ContainsKey(pInfo)) mValues.Add(pInfo, new IListDrawerInfo<Field>());
                var tValue = mValues[pInfo];
                tValue.BaseDrawer = this;
                tValue.Sources = tValues;
                tValue.info = pInfo;

                return tIListDrawerDic[tArgType.FullName].DrawerListItem(tValue);
            }
            else
            {
                EditorGUILayout.LabelField(string.Format("未实现IList绘制类 field:{0}", tArgTypeName));
            }
            return null;
        }
    }
}