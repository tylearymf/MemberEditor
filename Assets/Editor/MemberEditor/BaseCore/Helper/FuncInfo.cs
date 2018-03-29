namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public struct FuncInfo<T, Drawer>
    {
        public T value { get; set; }
        public bool refreshImmediate { get; set; }
        public int verIndex { get; set; }
        public IListDrawerInfo<Drawer> drawerInfo { set; get; }
    }
}