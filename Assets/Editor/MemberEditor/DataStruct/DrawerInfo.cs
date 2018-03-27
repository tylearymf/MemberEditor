namespace Tylearymf.MemberEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    public class DrawerInfo
    {
        public DrawerInfo(Type pType, MemberTypes pMemberTpes)
        {
            mType = pType;
            var t = Activator.CreateInstance(mType);
            if (t == null) return;
            switch (pMemberTpes)
            {
                case MemberTypes.Constructor:
                    break;
                case MemberTypes.Event:
                    break;
                case MemberTypes.Field:
                    mBaseFieldDrawer = t as BaseDrawer<Field>;
                    mTypeName = mBaseFieldDrawer.typeName;
                    break;
                case MemberTypes.Method:
                    mBaseMethodDrawer = t as BaseDrawer<Method>;
                    mTypeName = mBaseMethodDrawer.typeName;
                    break;
                case MemberTypes.Property:
                    mBasePropertyDrawer = t as BaseDrawer<Property>;
                    mTypeName = mBasePropertyDrawer.typeName;
                    break;
                case MemberTypes.TypeInfo:
                    break;
                case MemberTypes.Custom:
                    break;
                case MemberTypes.NestedType:
                    break;
                case MemberTypes.All:
                    break;
                default:
                    break;
            }
        }

        Type mType;
        string mTypeName;
        BaseDrawer<Field> mBaseFieldDrawer;
        BaseDrawer<Property> mBasePropertyDrawer;
        BaseDrawer<Method> mBaseMethodDrawer;

        public string typeName
        {
            get
            {
                if (string.IsNullOrEmpty(mTypeName))
                {
                    LogErrorInfo();
                }
                return mTypeName;
            }
        }

        public object LayoutDrawer<T>(T pInfo, int pIndex = 0)
        {
            if (mBaseFieldDrawer != null) return mBaseFieldDrawer.LayoutDrawer(pInfo as Field, pIndex);
            else if (mBasePropertyDrawer != null) return mBasePropertyDrawer.LayoutDrawer(pInfo as Property, pIndex);
            else if (mBaseMethodDrawer != null) return mBaseMethodDrawer.LayoutDrawer(pInfo as Method, pIndex);

            LogErrorInfo();
            return null;
        }

        void LogErrorInfo()
        {
            Debug.LogErrorFormat("未实现逻辑！ Type：{0}", mType == null ? string.Empty : mType.FullName);
        }
    }
}
