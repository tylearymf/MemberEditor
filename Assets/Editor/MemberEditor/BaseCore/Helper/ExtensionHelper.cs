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

        static public bool isNullOrEmpty(this ICollection pCollection)
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

        static public int GetLabelWidth<T>(this string pStr, BaseDrawer<T> pDrawer)
        {
            if (pDrawer != null)
            {
                var tType = pDrawer.GetType();
                var tCustomArrList = tType.GetCustomAttributes(typeof(MemeberDrawerAttribute), false);
                if (tCustomArrList != null && tCustomArrList.Length == 1)
                {
                    var tDrawerAtt = (MemeberDrawerAttribute)tCustomArrList[0];
                    switch (tDrawerAtt.memberType)
                    {
                        case MemberTypes.Method:
                            return pStr.IsNullOrEmpty() ? 0 : (pStr.Length * 7 + 5);
                    }
                }
            }
            return pStr.IsNullOrEmpty() ? 0 : pStr.Length * 7;
        }

        public static IEnumerable<Type> GetParentTypes(this Type pType)
        {
            if ((pType == null) || (pType.BaseType == null)) yield break;

            var tBaseType = pType;
            foreach (var tType in tBaseType.GetInterfaces())
            {
                yield return tType;
            }

            while (tBaseType != null)
            {
                yield return tBaseType;
                tBaseType = tBaseType.BaseType;
            }
        }

        static public Quaternion ToQuaternion(this Vector4 pVal)
        {
            return new Quaternion(pVal.x, pVal.y, pVal.z, pVal.w);
        }

        static public Vector4 ToVector4(this Quaternion pVal)
        {
            return new Vector4(pVal.x, pVal.y, pVal.z, pVal.w);
        }
    }
}