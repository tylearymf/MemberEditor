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
    /// 协议发送界面
    /// </summary>
    public class ProtocolView : IView, IMemberTitle
    {
        [InfoBox("协议界面：暂无介绍")]
        [LabelText("搜索文本")]
        [ShowInInspector]
        string mSearchText = string.Empty;
        string mPreviousText = null;

        [LabelText("协议成员")]
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