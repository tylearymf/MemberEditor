namespace Tylearymf.MemberEditor
{
    using Sirenix.OdinInspector;
    using Sirenix.Utilities.Editor;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;

    /// <summary>
    /// 实体调用界面
    /// </summary>
    public class EntityView : IView, IMemberTitle
    {
        [InfoBox("从场景中拖入查看的对象，然后点击对应的按钮设置其字段、属性或者调用其方法（可以输入部分搜索成员）")]
        [LabelText("目标")]
        [ShowInInspector]
        GameObject mTarget;
        GameObject mPreviousTarget;
        string mTargetName;

        [LabelText("搜索文本")]
        [ShowInInspector]
        string mSerachText;

        [LabelText("实体成员")]
        [ShowInInspector]
        [ListDrawerSettings(DraggableItems = false, IsReadOnly = true, HideAddButton = true)]
        [DisableContextMenu(true, true)]
        List<MemberItem> mMembers;

        public void Start()
        {
        }

        public void Update()
        {
            if (mTarget == null)
            {
                mMembers = null;
                mPreviousTarget = null;
                return;
            }
            if (mPreviousTarget && mTarget == mPreviousTarget) return;
            mPreviousTarget = mTarget;
            mTargetName = mTarget.name;

            var tComponents = mTarget.GetComponents<Component>();
            mMembers = new List<MemberItem>();
            if (tComponents == null) return;
            foreach (var tComponent in tComponents)
            {
                if (tComponent == null || mTarget == null) continue;
                mMembers.Add(new MemberItem(tComponent, mTarget, tComponent.GetType(), MemberHelper.cEntityPropertyFlags));
            }
        }

        public string TitleName()
        {
            return mTargetName;
        }

        public bool IsClickable()
        {
            return false;
        }
    }
}