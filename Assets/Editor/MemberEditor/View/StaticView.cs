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
    public class StaticView : IView, IMemberTitle
    {
        [InfoBox("例如需要调用A类下的b的静态方法，就在输入框中输入“A”，然后选中“A”，继续输入“b”，最后点击按钮调用“b”")]
        [LabelText("搜索文本")]
        [ShowInInspector]
        string mSearchText;
        string mPreviousText;

        [LabelText("静态成员")]
        [ShowInInspector]
        [ListDrawerSettings(DraggableItems = false, IsReadOnly = true, HideAddButton = true)]
        [DisableContextMenu(true, true)]
        List<MemberItem> mMembers;

        Dictionary<string, Type> mAllTypes;

        public void Start()
        {
            mAllTypes = MemberHelper.allTypes;
        }

        public void Update()
        {
            if (mSearchText == mPreviousText) return;
            mPreviousText = mSearchText;

            if (mAllTypes.IsNullOrEmpty() || mSearchText.IsNullOrEmpty())
            {
                mMembers = null;
                return;
            }

            var tSearchChars = mSearchText.Distinct(' ');
            var tMatchTypes = new HashSet<Type>();
            foreach (var tTypeName in mAllTypes.Keys)
            {
                var tContains = true;
                foreach (var tSearchChar in tSearchChars)
                {
                    if (tTypeName.IndexOf(tSearchChar) == -1)
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
                mMembers = null;
                return;
            }
            mMembers = new List<MemberItem>(tMatchTypes.GetCountIgnoreNull());
            foreach (var tType in tMatchTypes)
            {
                mMembers.Add(new MemberItem(null, null, tType, MemberHelper.cStaticPropertyFlags));
            }
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