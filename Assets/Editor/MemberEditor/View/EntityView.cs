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
    [System.Serializable]
    public class EntityView : IView, IMemberTitle
    {
        [InfoBox("从Hierarchy面板中拖入查看的对象")]
        [LabelText("目标")]
        [ShowInInspector]
        GameObject mTarget;
        GameObject mPreviousTarget;
        string mTargetName;

        [InfoBox("输入关键字搜索成员，多个关键字以空格隔开")]
        [LabelText("搜索成员")]
        [ShowInInspector]
        string mSerachText;
        string mPreviousText;

        [LabelText("实体成员")]
        [ShowInInspector]
        [ListDrawerSettings(DraggableItems = false, IsReadOnly = true, HideAddButton = true)]
        [DisableContextMenu(true, true)]
        List<MemberItem> mMembers = new List<MemberItem>();

        public void Start()
        {
        }

        public void Update()
        {
            if (mTarget == null)
            {
                mMembers.Clear();
                mPreviousTarget = null;
                mSerachText = string.Empty;
                mPreviousText = string.Empty;
                return;
            }

            if (mPreviousTarget && mTarget == mPreviousTarget && mSerachText == mPreviousText) return;
            mPreviousTarget = mTarget;
            mPreviousText = mSerachText;
            mTargetName = mTarget.name;

            var tComponents = mTarget.GetComponents<Component>();
            mMembers.Clear();
            if (tComponents == null) return;
            foreach (var tComponent in tComponents)
            {
                if (tComponent == null || mTarget == null) continue;
                var tMember = new MemberItem(tComponent, mTarget, tComponent.GetType(), MemberHelper.cEntityPropertyFlags, mSerachText.Distinct(' '));
                if (tMember.IsNullOrEmpty()) continue;
                mMembers.Add(tMember);
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