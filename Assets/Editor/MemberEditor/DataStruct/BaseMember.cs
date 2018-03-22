namespace Tylearymf.MemberEditor
{
    using Sirenix.OdinInspector.Editor;
    using System.Collections.Generic;
    using System.Collections;
    using System.Reflection;
    using UnityEngine;
    using System;
    using Sirenix.Utilities.Editor;

    public abstract class BaseMember<T> where T : class
    {
        public BaseMember(T pInfo, UnityEngine.Object pTarget)
        {
            mInfo = pInfo;
            mTarget = pTarget;
        }

        protected T mInfo;
        protected UnityEngine.Object mTarget;
        protected IPropertyValueEntry mEntry;
        protected GUIContent mContent;
        protected Rect mRect;
        protected string mInfoName;

        public T info { get { return mInfo; } }
        public Rect rect
        {
            set { mRect = value; }
            get { return mRect; }
        }

        public IPropertyValueEntry entry
        {
            get { return mEntry; }

            set { mEntry = value; }
        }

        public GUIContent content
        {
            get { return mContent; }

            set { mContent = value; }
        }

        public UnityEngine.Object target
        {
            get
            {
                return mTarget;
            }
        }

        public virtual void SetValue<TValue>(TValue pVal) { }
        public virtual TValue GetValue<TValue>() { return default(TValue); }

        public virtual void Call(params object[] pParams) { }

        /// <summary>
        /// 绘制类未实现的时候会显示以下文本
        /// </summary>
        /// <returns></returns>
        public virtual string NotImplementedDescription(int pParamIndex = 0)
        {
            return "请重写该方法";
        }
    }
}