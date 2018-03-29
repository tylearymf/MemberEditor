namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class IListDrawerAttribute : Attribute
    {
        MemberTypes mMemberType;
        public MemberTypes memberType
        {
            get { return mMemberType; }
        }

        public IListDrawerAttribute(MemberTypes pType)
        {
            mMemberType = pType;
        }
    }
}