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

        static public int GetLabelWidth(this string pStr)
        {
            return pStr.IsNullOrEmpty() ? 0 : pStr.Length * 7;
        }
        #endregion

        #region gui
        static public void DrawerListItem<T>(List<T> pSources, List<string> pLabelNames, Rect pRect, bool pDisable, ref bool pShowIndex, int pIntervalWidth, Action<int, List<object>> pOnValueChange, params Func<Rect, T, object>[] pInputFields)
        where T : new()
        {
            if (pSources.IsNullOrEmpty() || pLabelNames.IsNullOrEmpty() || pInputFields.IsNullOrEmpty()
                || (pLabelNames.GetCountIgnoreNull() != pInputFields.GetCountIgnoreNull())) return;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !pDisable;
            EditorGUILayout.LabelField("size", GUILayout.Width(30));
            var tNewCount = EditorGUILayout.IntField(pSources.Count, GUILayout.MaxWidth(70));
            GUI.enabled = true;
            EditorGUILayout.LabelField("show index", GUILayout.Width(70));
            pShowIndex = EditorGUILayout.Toggle(pShowIndex, GUILayout.MaxWidth(100));
            EditorGUILayout.EndHorizontal();
            while (tNewCount < pSources.Count)
            {
                pSources.RemoveAt(pSources.Count - 1);
            }
            while (tNewCount > pSources.Count)
            {
                pSources.Add(new T());
            }

            var pFieldCount = pLabelNames.Count;
            var tSingleItemWidth = (pRect.size.x - pIntervalWidth * (pFieldCount - 1)) / pFieldCount;
            GUI.enabled = !pDisable;
            for (int i = 0, imax = pSources.Count; i < imax; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (pShowIndex)
                {
                    EditorGUILayout.LabelField(new GUIContent(i.ToString()), GUILayout.MaxWidth(20));
                }

                List<object> tResults = new List<object>();
                for (int j = 0; j < pFieldCount; j++)
                {
                    if (!pLabelNames.IsNullOrEmpty() && j < pLabelNames.Count)
                    {
                        EditorGUILayout.LabelField(pLabelNames[j], GUILayout.Width(pLabelNames[j].GetLabelWidth()));
                    }

                    if (pInputFields[j] != null)
                    {
                        tResults.Add(pInputFields[j](Rect.zero, pSources[i]));
                    }
                }

                if (GUI.changed)
                {
                    if (pOnValueChange != null)
                    {
                        pOnValueChange(i, tResults);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            GUI.enabled = true;
            EditorGUILayout.EndVertical();
        }
        #endregion
    }
}
