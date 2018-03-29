namespace Tylearymf.MemberEditor
{
    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector;
    using System.Collections.Generic;
    using System.Collections;
    using UnityEngine;
    using UnityEditor;

    public class IListDrawerInfo<T>
    {
        public IList Sources;
        public BaseDrawer<T> BaseDrawer;
        public bool ShowIndex = false;
        public T info;
    }
}