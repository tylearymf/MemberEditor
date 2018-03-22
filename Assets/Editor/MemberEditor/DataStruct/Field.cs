namespace Tylearymf.MemberEditor
{
    using UnityEngine;
    using System.Reflection;
    using System;
    using Sirenix.Utilities.Editor;
    using System.Text.RegularExpressions;

    public class Field : BaseMember<FieldInfo>, IMemberTitle
    {
        public Field(FieldInfo pInfo, UnityEngine.Object pTarget) : base(pInfo, pTarget)
        {
            mInfoName = pInfo == null ? string.Empty : string.Format("{0} ({1}) ", pInfo.Name, pInfo.FieldType.ToString());
        }

        public override void SetValue<T>(T pVal)
        {
            if (info == null) return;
            info.SetValue(mTarget, pVal);
        }

        public override T GetValue<T>()
        {
            if (info == null) return default(T);
            object tVal = null;
            try
            {
                tVal = info.GetValue(mTarget);
            }
            catch { }
            if (tVal == null) return default(T);
            return (T)tVal;
        }

        public override string NotImplementedDescription(int pIndex = 0)
        {
            return string.Format("未实现绘制类 type:{0} field:{1}", info.FieldType.ToString(), info.Name);
        }

        public string TitleName()
        {
            return mInfoName;
        }

        public bool IsClickable()
        {
            return false;
        }
    }
}