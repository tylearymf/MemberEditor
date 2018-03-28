﻿namespace Tylearymf.MemberEditor
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
        static Dictionary<MemberTypes, Dictionary<string, IListDrawer>> sIListDrawerDic;

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
        static public Dictionary<MemberTypes, Dictionary<string, IListDrawer>> iListDrawerDic
        {
            get { return sIListDrawerDic; }
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
            if (sDrawerInfoDic.IsNullOrEmpty())
            {
                sDrawerInfoDic = new Dictionary<MemberTypes, Dictionary<string, DrawerInfo>>();
                var tAssembly = Assembly.GetAssembly(typeof(BaseDrawer<>));
                if (tAssembly == null) return;
                var tCustomList = tAssembly.GetTypes().Where(x => x.GetCustomAttributes(typeof(MemeberDrawerAttribute), false).Length == 1).ToList();
                if (tCustomList == null) return;
                foreach (var item in tCustomList)
                {
                    var tAtt = item.GetCustomAttributes(typeof(MemeberDrawerAttribute), false);
                    if (tAtt == null || tAtt.Length != 1) continue;
                    var tDrawerAtt = (MemeberDrawerAttribute)tAtt[0];
                    var tMemberType = tDrawerAtt.memberType;
                    if (!sDrawerInfoDic.ContainsKey(tMemberType))
                    {
                        sDrawerInfoDic.Add(tMemberType, new Dictionary<string, DrawerInfo>());
                    }
                    var tDrawer = new DrawerInfo(item, tMemberType);
                    sDrawerInfoDic[tMemberType][tDrawer.typeName] = tDrawer;
                }
            }
            if (sIListDrawerDic.isNullOrEmpty())
            {
                sIListDrawerDic = new Dictionary<MemberTypes, Dictionary<string, IListDrawer>>();
                var tAssembly = Assembly.GetAssembly(typeof(IListDrawer));
                if (tAssembly == null) return;
                var tCustomList = tAssembly.GetTypes().Where(x => x.GetCustomAttributes(typeof(IListDrawerAttribute), false).Length == 1).ToList();
                if (tCustomList == null) return;
                foreach (var item in tCustomList)
                {
                    var tAtt = item.GetCustomAttributes(typeof(IListDrawerAttribute), false);
                    if (tAtt == null || tAtt.Length != 1) continue;
                    var tDrawerAtt = (IListDrawerAttribute)tAtt[0];
                    var tMemberType = tDrawerAtt.memberType;
                    if (!sIListDrawerDic.ContainsKey(tMemberType))
                    {
                        sIListDrawerDic.Add(tMemberType, new Dictionary<string, IListDrawer>());
                    }
                    var tIListDrawer = Activator.CreateInstance(item) as IListDrawer;
                    sIListDrawerDic[tMemberType][tIListDrawer.typeName] = tIListDrawer;
                }
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

        static public Dictionary<string, IListDrawer> GetIListDrawers(MemberTypes pMemberTypes)
        {
            if (iListDrawerDic == null || !iListDrawerDic.ContainsKey(pMemberTypes)) return null;
            return iListDrawerDic[pMemberTypes];
        }
        #endregion
    }
}
