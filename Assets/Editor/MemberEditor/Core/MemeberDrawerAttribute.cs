namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    [AttributeUsage(AttributeTargets.Class)]
    public class MemeberDrawerAttribute : Attribute
    {
        MemberTypes mMemberType;
        public MemberTypes memberType
        {
            get { return mMemberType; }
        }

        public MemeberDrawerAttribute(MemberTypes pType)
        {
            mMemberType = pType;
        }
    }
}