namespace Tylearymf.MemberEditor
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using System.Reflection;
    using System.Linq;
    using Sirenix.Utilities.Editor;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector.Editor.Drawers;
    using Sirenix.Serialization;
    using Sirenix.Utilities.Editor.CodeGeneration;
    using Sirenix.Utilities;
    using UnityEditor;

    static public class MemberHelper
    {
        #region field & const
        public const int cFieldDefaultHeight = 20;
        public const int cPropertyDefaultHeight = 20;
        public const int cMethodDefaultHeight = 20;
        public const int cStaticViewMaxShowCount = 5;

        public const BindingFlags cEntityPropertyFlags = BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic;
        public const BindingFlags cStaticPropertyFlags = BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic;

        static Dictionary<MemberTypes, Dictionary<string, DrawerInfo>> sDrawerInfoDic;
        static Dictionary<string, Type> sAllTypes;
        static HashSet<string> sIngoreNamespaces = new HashSet<string>()
        {
            typeof(MemberHelper).Namespace,
            typeof(OdinEditorWindow).Namespace,
            typeof(HideIfAttributeDrawer).Namespace,
            typeof(IDataWriter).Namespace,
            typeof(DragHandle).Namespace,
            typeof(CodeWriter).Namespace,
            typeof(WeakValueGetter).Namespace,
        };
        static HashSet<Type> sIngoreBaseTypes = new HashSet<Type>()
        {
            typeof(System.Attribute),
        };
        #endregion

        #region property
        static public Dictionary<MemberTypes, Dictionary<string, DrawerInfo>> drawerInfoDic
        {
            get { return sDrawerInfoDic; }
        }
        static public Dictionary<string, Type> allTypes
        {
            get { return sAllTypes; }
        }
        #endregion

        #region init
        static public void Init()
        {
            LoadAssembly();
            InitDrawerInfoDic();
        }

        static void InitDrawerInfoDic()
        {
            if (!sDrawerInfoDic.IsNullOrEmpty()) return;
            sDrawerInfoDic = new Dictionary<MemberTypes, Dictionary<string, DrawerInfo>>();
            var tAssembly = Assembly.GetAssembly(typeof(BaseDrawer<>));
            if (tAssembly == null) return;
            var tCustomList = tAssembly.GetTypes().Where(x => x.GetCustomAttributes(typeof(MemeberDrawerAttribute), false).Length > 0).ToList();
            if (tCustomList == null) return;
            foreach (var item in tCustomList)
            {
                var tAtt = item.GetCustomAttributes(false).Single(x => x is MemeberDrawerAttribute);
                if (tAtt == null) continue;
                var tDrawerAtt = (MemeberDrawerAttribute)tAtt;
                var tMemberType = tDrawerAtt.memberType;
                if (!sDrawerInfoDic.ContainsKey(tMemberType))
                {
                    sDrawerInfoDic.Add(tMemberType, new Dictionary<string, DrawerInfo>());
                }
                var tDrawer = new DrawerInfo(item, tMemberType);
                sDrawerInfoDic[tMemberType][tDrawer.typeName] = tDrawer;
            }
        }

        static void LoadAssembly()
        {
            if (!sAllTypes.IsNullOrEmpty()) return;
            string[] tAssemblys = { "Assembly-CSharp", "Assembly-CSharp-firstpass", "Assembly-CSharp-Editor", "Assembly-CSharp-firstpass", "UnityEngine" };
            sAllTypes = new Dictionary<string, Type>();
            foreach (var tAssemblyName in tAssemblys)
            {
                Assembly tAssembly = null;
                try
                {
                    tAssembly = Assembly.Load(tAssemblyName);
                }
                catch { }
                if (tAssembly == null) continue;
                var tTypes = tAssembly.GetTypes();
                if (tTypes.IsNullOrEmpty()) continue;
                foreach (var tType in tTypes)
                {
                    var tNamespace = tType.Namespace;
                    var tBaseType = tType.BaseType;

                    if (tType.IsAbstract || tType.IsGenericType || !tType.IsClass) continue;

                    var tVal = !tNamespace.IsNullOrEmpty() && sIngoreNamespaces.Contains(tNamespace);
                    if (tVal) continue;

                    tVal = false;
                    foreach (var item in sIngoreBaseTypes)
                    {
                        if (CheckTypeIsEqual(tType, item))
                        {
                            tVal = true;
                            break;
                        }
                    }
                    if (tVal) continue;
                    if (sAllTypes.ContainsKey(tType.FullName)) continue;
                    sAllTypes.Add(tType.FullName.ToLower(), tType);
                }
            }
        }

        static bool CheckTypeIsEqual(Type pLeft, Type pRight)
        {
            if (pLeft == null) return false;
            if (pLeft == pRight) return true;
            return CheckTypeIsEqual(pLeft.BaseType, pRight);
        }
        #endregion

        #region logic
        static public Dictionary<string, DrawerInfo> GetDrawInfos(MemberTypes pMemberTypes)
        {
            if (drawerInfoDic == null || !drawerInfoDic.ContainsKey(pMemberTypes)) return null;
            return drawerInfoDic[pMemberTypes];
        }
        #endregion

        #region extension
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
        #endregion

        #region gui
        static public void DrawerListItem<T>(IList<T> pSources, List<string> pLabelNames, Rect pRect, int pIntervalWidth, int pItemHeigth, Action<int, List<T>> pOnValueChange, params Func<T>[] pInputFields)
        {
            var tCount = pSources.Count;
            var tWidth = (pRect.size.x - pIntervalWidth * (tCount - 1)) / tCount;
            for (int i = 0, imax = pSources.Count; i < imax; i++)
            {
                var tRect = pRect;

                EditorGUI.BeginChangeCheck();
                tRect.position = pRect.position + new Vector2(0, (i + 1) * pItemHeigth) + new Vector2(pIntervalWidth * 0, 0);
                var tLabelWidth = pLabelNames[0].Length * 7;
                tRect.size = new Vector2(tLabelWidth, pItemHeigth);
                EditorGUI.LabelField(tRect, new GUIContent(pLabelNames[0]));
                tRect.size = new Vector2(tWidth - tLabelWidth, pItemHeigth);
                tRect.position = pRect.position + new Vector2(tLabelWidth + tWidth * 0, (i + 1) * pItemHeigth) + new Vector2(pIntervalWidth * 0, 0);
                if (tRect.xMin >= tRect.xMax) tRect.size = Vector2.one;
                T t1 = default(T);
                if (pInputFields[0] != null)
                {
                    t1 = pInputFields[0]();
                }
                //var t1 = EditorGUI.IntField(tRect, pSources[i].width);

                tRect.position = pRect.position + new Vector2(tWidth * 1, (i + 1) * pItemHeigth) + new Vector2(pIntervalWidth * 1, 0);
                tLabelWidth = pLabelNames[1].Length * 7;
                tRect.size = new Vector2(tLabelWidth, pItemHeigth);
                EditorGUI.LabelField(tRect, new GUIContent(pLabelNames[1]));
                tRect.size = new Vector2(tWidth - tLabelWidth, pItemHeigth);
                tRect.position = pRect.position + new Vector2(tLabelWidth + tWidth * 1, (i + 1) * pItemHeigth) + new Vector2(pIntervalWidth * 1, 0);
                if (tRect.xMin >= tRect.xMax) tRect.size = Vector2.one;
                T t2 = default(T);
                if (pInputFields[1] != null)
                {
                    t2 = pInputFields[1]();
                }

                tRect.position = pRect.position + new Vector2(tWidth * 2, (i + 1) * pItemHeigth) + new Vector2(pIntervalWidth * 2, 0);
                tLabelWidth = pLabelNames[2].Length * 7;
                tRect.size = new Vector2(tLabelWidth, pItemHeigth);
                EditorGUI.LabelField(tRect, new GUIContent(pLabelNames[2]));
                tRect.size = new Vector2(tWidth - tLabelWidth, pItemHeigth);
                tRect.position = pRect.position + new Vector2(tLabelWidth + tWidth * 2, (i + 1) * pItemHeigth) + new Vector2(pIntervalWidth * 2, 0);
                if (tRect.xMin >= tRect.xMax) tRect.size = Vector2.one;
                //var t3 = EditorGUI.IntField(tRect, pSources[i].refreshRate);
                T t3 = default(T);
                if (pInputFields[1] != null)
                {
                    t3 = pInputFields[2]();
                }

                if (EditorGUI.EndChangeCheck())
                {
                    if (pOnValueChange != null)
                    {
                        pOnValueChange(i, new List<T>()
                        {
                            t1,t2,t3
                        });
                    }
                }
            }
        }
        #endregion
    }
}
