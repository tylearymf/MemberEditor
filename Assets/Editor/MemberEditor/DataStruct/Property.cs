namespace Tylearymf.MemberEditor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Reflection;
    using Sirenix.OdinInspector.Editor;
    using System;
    using Sirenix.OdinInspector;
    using Sirenix.Utilities.Editor;
    using System.Text.RegularExpressions;

    public class Property : BaseMember<PropertyInfo>, IMemberTitle
    {
        public Property(PropertyInfo pInfo, UnityEngine.Object pTarget) : base(pInfo, pTarget)
        {
            mInfoName = pInfo == null ? string.Empty : string.Format("{0} ({1}) ", pInfo.Name, pInfo.PropertyType.ToString());
        }

        public override void SetValue<T>(T pVal)
        {
            if (info == null) return;
            info.SetValue(mTarget, pVal, null);
        }

        public override T GetValue<T>()
        {
            if (info == null) return default(T);
            object tVal = null;
            try
            {
                tVal = info.GetValue(mTarget, null);
            }
            catch { }
            if (tVal == null) return default(T);
            return (T)tVal;
        }

        public override string NotImplementedDescription(int pIndex = 0)
        {
            return string.Format("未实现绘制类 type:{0} property:{1}", info.PropertyType.ToString(), info.Name);
        }

        public string TitleName()
        {
            if (string.IsNullOrEmpty(mInfoName)) return string.Empty;
            var tMatch = Regex.Match(mInfoName, @"<(?<Name>\w+)>k__BackingField");
            return tMatch.Success ? tMatch.Groups["Name"].Value : mInfoName;
        }

        public bool IsClickable()
        {
            return false;
        }
    }
}