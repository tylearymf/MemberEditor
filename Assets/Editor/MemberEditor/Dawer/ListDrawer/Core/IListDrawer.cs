namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    public interface IListDrawer
    {
        string typeName { get; }
        object DrawerListItem<T>(IList pSources, BaseDrawer<T> pDrawer);
    }
}