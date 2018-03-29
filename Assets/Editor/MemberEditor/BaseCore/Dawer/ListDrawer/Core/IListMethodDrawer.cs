namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    [MemeberDrawer(System.Reflection.MemberTypes.Method)]
    public class IListMethodDrawer : BaseDrawer<Method>
    {
        MultiInfo<Method, IListDrawerInfo<Method>> mValues = new MultiInfo<Method, IListDrawerInfo<Method>>();

        public override string typeName
        {
            get
            {
                return typeof(IList<>).FullName;
            }
        }

        public override object LayoutDrawer(Method pInfo, int pParamIndex = 0)
        {
            var tIListDrawerDic = MemberHelper.GetIListDrawers(System.Reflection.MemberTypes.Method);
            if (tIListDrawerDic == null) return null;
            var tParam = pInfo.info.GetParameters()[pParamIndex];

            var tValue = mValues.GetValue(pParamIndex, pInfo, () => new IListDrawerInfo<Method>()
            {
                Sources = Activator.CreateInstance(tParam.ParameterType) as IList,
                BaseDrawer = this,
                info = pInfo,
            });

            var tArgs = tParam.ParameterType.GetGenericArguments();
            var tArgType = tArgs.Length == 0 ? tParam.ParameterType.GetElementType() : tArgs[0];
            var tArgTypeName = tArgType == null ? string.Empty : tArgType.FullName;

            object tReuslt = null;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(tParam.Name.GetLabelWidth(this)));
            EditorGUILayout.LabelField(tParam.Name, GUILayout.Width(tParam.Name.GetLabelWidth(this)));
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical("Box");
            if (tIListDrawerDic.ContainsKey(tArgTypeName) && tIListDrawerDic[tArgTypeName] != null)
            {
                tReuslt = tIListDrawerDic[tArgType.FullName].DrawerListItem(tValue);
            }
            else
            {
                EditorGUILayout.LabelField(string.Format("未实现IList绘制类 field:{0}", tArgTypeName));
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            return tReuslt;
        }
    }
}