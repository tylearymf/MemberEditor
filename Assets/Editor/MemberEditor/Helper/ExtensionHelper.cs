namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    static public class ExtensionHelper
    {
        static public V TryGetValue<T, V>(this Dictionary<T, V> pDic, T pKey, V pDefaultValue)
        {
            if (pDic.IsNullOrEmpty() || !pDic.ContainsKey(pKey)) return pDefaultValue;
            return pDic[pKey];
        }

        static public int GetCountIgnoreNull<T>(this ICollection<T> pCollection)
        {
            return pCollection == null ? 0 : pCollection.Count;
        }

        static public bool IsNullOrEmpty<T>(this ICollection<T> pCollection)
        {
            return pCollection == null || pCollection.Count == 0;
        }

        static public bool IsNullOrEmpty(this string pStr)
        {
            return string.IsNullOrEmpty(pStr);
        }

        static public string GetParamName(this ParameterInfo pParam)
        {
            return pParam == null ? string.Empty : string.Format(" < {0} >", pParam.Name);
        }

        /// <summary>
        /// 以指定字符切割字符串，并去重 且转小写
        /// </summary>
        static public List<string> Distinct(this string pStr, char pChar)
        {
            if (pStr.IsNullOrEmpty()) return null;
            var tHashSet = new HashSet<string>();
            var tSplits = pStr.Split(new char[] { pChar }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in tSplits)
            {
                tHashSet.Add(item.ToLower());
            }
            return tHashSet.ToList();
        }

        static public int GetLabelWidth(this string pStr)
        {
            return pStr.IsNullOrEmpty() ? 0 : pStr.Length * 7;
        }
    }
}