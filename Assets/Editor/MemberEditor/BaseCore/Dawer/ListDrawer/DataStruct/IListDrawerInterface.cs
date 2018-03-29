namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    public interface IListDrawerInterface
    {
        string typeName { get; }
        object DrawerListItem<T>(IListDrawerInfo<T> pInfo);
    }
}