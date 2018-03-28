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
        bool mShowIndex = false;
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
            var tArgType = pInfo.info.FieldType.GetGenericArguments()[0];
            var tArgTypeName = tArgType.FullName;

            if (tIListDrawerDic.ContainsKey(tArgTypeName) && tIListDrawerDic[tArgTypeName] != null)
            {
                return tIListDrawerDic[tArgType.FullName].DrawerListItem(tValues, this);
            }
            else
            {
                EditorGUILayout.LabelField(string.Format("未实现IList绘制类 field:{0}", tArgTypeName));
            }
            return null;
        }
    }
}