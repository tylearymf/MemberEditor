namespace Tylearymf.MemberEditor
{
    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector;
    using System.Collections.Generic;
    using System.Collections;
    using UnityEngine;
    using UnityEditor;
    using Sirenix.Utilities.Editor;

    /// <summary>
    /// 通知下发界面
    /// </summary>
    public class NotifyView : IView, IMemberTitle
    {
        [InfoBox("通知界面：暂无介绍")]
        [LabelText("搜索文本")]
        [ShowInInspector]
        string mSearchText = string.Empty;
        string mPreviousText = null;

        [LabelText("通知成员")]
        [ShowInInspector]
        [ListDrawerSettings(DraggableItems = false, IsReadOnly = true, HideAddButton = true)]
        [DisableContextMenu(true, true)]
        List<MemberItem> mMembers = new List<MemberItem>();

        public void Start()
        {
        }

        public void Update()
        {
            if (mSearchText == mPreviousText) return;
            mPreviousText = mSearchText;

        }

        public string TitleName()
        {
            return mSearchText;
        }

        public bool IsClickable()
        {
            return false;
        }
    }
}