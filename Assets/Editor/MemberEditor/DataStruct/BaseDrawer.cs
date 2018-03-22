namespace Tylearymf.MemberEditor
{
    using System;
    using UnityEngine;

    public abstract class BaseDrawer<T>
    {
        /// <summary>
        /// 绘制的实例的类型名 如： return typeof(String).FullName;
        /// </summary>
        public abstract string typeName { get; }

        /// <summary>
        /// 布局
        /// </summary>
        /// <param name="pInfo"></param>
        public abstract object LayoutDrawer(T pInfo, int pParamIndex = 0);

        /// <summary>
        /// 返回该成员的布局高度
        /// </summary>
        /// <returns></returns>
        public abstract int LayoutHeight();
    }
}