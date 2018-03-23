namespace Tylearymf.MemberEditor
{
    using Sirenix.Utilities.Editor;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector;
    using System.Collections.Generic;
    using System.Collections;
    using UnityEngine;
    using UnityEditor;
    using System;
    using System.Linq;

    /// <summary>
    /// 静态调用界面
    /// </summary>
    [System.Serializable]
    public class StaticView : IView, IMemberTitle
    {
        [InfoBox("输入关键字搜索脚本")]
        [LabelText("搜索脚本")]
        [ShowInInspector]
        string mSearchClassText;
        string mPreviousClassText;

        [InfoBox("输入关键字搜索成员，多个关键字以空格隔开")]
        [LabelText("搜索成员")]
        [ShowInInspector]
        string mSearchMemberText;
        string mPreviousMemberText;

        [LabelText("静态成员")]
        [ShowInInspector]
        [ListDrawerSettings(DraggableItems = false, IsReadOnly = true, HideAddButton = true)]
        [DisableContextMenu(true, true)]
        List<MemberItem> mMembers = new List<MemberItem>();

        Dictionary<string, Type> mAllTypes;

        public void Start()
        {
            mAllTypes = MemberHelper.allTypes;
        }

        public void Update()
        {
            if (mSearchClassText == mPreviousClassText && mSearchMemberText == mPreviousMemberText) return;
            mPreviousClassText = mSearchClassText;
            mPreviousMemberText = mSearchMemberText;

            if (mAllTypes.IsNullOrEmpty() || mSearchClassText.IsNullOrEmpty())
            {
                mMembers.Clear();
                return;
            }

            var tSearchChars = mSearchClassText.Distinct(' ');
            var tMatchTypes = new HashSet<Type>();
            foreach (var tTypeName in mAllTypes.Keys)
            {
                var tContains = true;
                foreach (var tSearchChar in tSearchChars)
                {
                    if (!tTypeName.Contains(tSearchChar))
                    {
                        tContains = false;
                        break;
                    }
                }
                if (!tContains || mAllTypes[tTypeName] == null) continue;
                tMatchTypes.Add(mAllTypes[tTypeName]);
                if (tMatchTypes.Count >= MemberHelper.cStaticViewMaxShowCount) break;
            }

            if (tMatchTypes.IsNullOrEmpty())
            {
                mMembers.Clear();
                return;
            }
            mMembers.Clear();
            foreach (var tType in tMatchTypes)
            {
                var tMember = new MemberItem(null, null, tType, MemberHelper.cStaticPropertyFlags, mSearchMemberText.Distinct(' '));
                if (tMember.IsNullOrEmpty()) continue;
                mMembers.Add(tMember);
            }
        }

        public string TitleName()
        {
            return mSearchClassText;
        }

        public bool IsClickable()
        {
            return false;
        }
    }
}