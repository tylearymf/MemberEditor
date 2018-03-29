namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class MultiInfo<T1, T2>
    {
        Dictionary<T1, Dictionary<int, T2>> mValueDic = new Dictionary<T1, Dictionary<int, T2>>();

        public T2 GetValue(int pParamIndex, T1 pMethod, Func<T2> pDefaultVal)
        {
            if (mValueDic == null)
            {
                mValueDic = new Dictionary<T1, Dictionary<int, T2>>();
            }
            if (!mValueDic.ContainsKey(pMethod))
            {
                mValueDic.Add(pMethod, new Dictionary<int, T2>());
            }
            if (!mValueDic[pMethod].ContainsKey(pParamIndex))
            {
                mValueDic[pMethod].Add(pParamIndex, pDefaultVal());
            }
            return mValueDic[pMethod][pParamIndex];
        }

        public void SetValue(int pParamIndex, T1 pMethod, T2 pInfo)
        {
            if (mValueDic.isNullOrEmpty() || !mValueDic.ContainsKey(pMethod) || !mValueDic[pMethod].ContainsKey(pParamIndex)) return;
            mValueDic[pMethod][pParamIndex] = pInfo;
        }
    }
}