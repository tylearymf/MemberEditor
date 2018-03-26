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

    [System.Serializable]
    public class Method : BaseMember<MethodInfo>, IMemberTitle
    {
        public Method(MethodInfo pInfo, string pTypeFullName, UnityEngine.Object pTarget) : base(pInfo, pTypeFullName, pTarget)
        {
            var tParams = string.Empty;
            if (pInfo.GetParameters() != null && pInfo.GetParameters().Length > 0)
            {
                foreach (var item in pInfo.GetParameters())
                {
                    tParams += string.Format("{0} {1},", item.ParameterType, item.Name);
                }
                tParams = tParams.Substring(0, tParams.Length - 1);
            }
            mInfoName = string.Format("{1} {0} ({2})", pInfo.Name, pInfo.ReturnType.ToString(), tParams);
            mMemberName = pInfo.Name;
        }

        public override MethodInfo info
        {
            get
            {
                if (mInfo == null && !mTypeFullName.IsNullOrEmpty() && !mMemberName.IsNullOrEmpty() && !MemberHelper.allTypes.IsNullOrEmpty())
                {
                    Type tType = null;
                    if (MemberHelper.allTypes.TryGetValue(mTypeFullName.ToLower(), out tType))
                    {
                        try
                        {
                            mInfo = tType.GetMethod(mMemberName, BindingFlags.Static | BindingFlags.Instance |
                            BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic);
                        }
                        catch { }
                    }
                }
                return mInfo;
            }
        }

        public override void Call(params object[] pParams)
        {
            var tParamObjs = pParams;
            if (tParamObjs.IsNullOrEmpty())
            {
                try
                {
                    var tAction = (Action)Delegate.CreateDelegate(typeof(Action), target, info.Name);
                    tAction();
                }
                catch
                {
                    try
                    {
                        var tResult = info.Invoke(target, null);
                        Debug.Log(tResult);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogErrorFormat("方法 {0} 调用失败 ：{1}", info.Name, ex.Message);
                    }
                }
            }
            else
            {
                try
                {
                    var tAction = (Action<object[]>)Delegate.CreateDelegate(typeof(Action<object[]>), target, info.Name);
                    tAction(tParamObjs);
                }
                catch
                {
                    try
                    {
                        var tResult = info.Invoke(target, tParamObjs);
                        Debug.Log(tResult);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogErrorFormat("方法 {0} 调用失败 ：{1}", info.Name, ex.Message);
                    }
                }
            }
        }

        public override string NotImplementedDescription(int pIndex = 0)
        {
            var tTypeName = string.Empty;
            if (info.GetParameters().Length == 0)
            {
                tTypeName = info.Name;
            }
            else
            {
                tTypeName = info.GetParameters()[pIndex].ParameterType.FullName;
            }
            return string.Format("未实现绘制类 type:{0}", tTypeName);
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