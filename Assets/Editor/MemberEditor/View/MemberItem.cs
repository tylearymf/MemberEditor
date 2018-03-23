namespace Tylearymf.MemberEditor
{
    using Sirenix.OdinInspector;
    using Sirenix.Utilities.Editor;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using System;

    [System.Serializable]
    public class MemberItem : IMemberTitle
    {
        UnityEngine.Object mComponent;
        System.Type mType;
#pragma warning disable 0414
        UnityEngine.Object mTarget;
        string mComponentName;

        public MemberItem(UnityEngine.Object pComponent, UnityEngine.Object pTarget, Type pType, BindingFlags pFlags, List<string> pMatchTexts = null)
        {
            mComponent = pComponent;
            mTarget = pTarget;
            mComponentName = pType.FullName;
            mType = pType;

            mFields = new List<Field>();
            mPropertys = new List<Property>();
            mMethods = new List<Method>();

            foreach (var item in mType.GetMembers(pFlags))
            {
                if (!pMatchTexts.IsNullOrEmpty())
                {
                    var tContains = true;
                    foreach (var tText in pMatchTexts)
                    {
                        if (!item.Name.Contains(tText))
                        {
                            tContains = false;
                            break;
                        }
                    }
                    if (!tContains) continue;
                }

                switch (item.MemberType)
                {
                    case MemberTypes.Constructor:
                        break;
                    case MemberTypes.Event:
                        break;
                    case MemberTypes.Field:
                        if (item.Name.Contains("BackingField")) continue;
                        mFields.Add(new Field(item as FieldInfo, mType.FullName, mComponent));
                        break;
                    case MemberTypes.Method:
                        mMethods.Add(new Method(item as MethodInfo, mType.FullName, mComponent));
                        break;
                    case MemberTypes.Property:
                        mPropertys.Add(new Property(item as PropertyInfo, mType.FullName, mComponent));
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
        }

        [LabelText("字段")]
        [ShowInInspector]
        [ListDrawerSettings(DraggableItems = false, IsReadOnly = true, HideAddButton = true)]
        [DisableContextMenu(true, true)]
        List<Field> mFields;

        [LabelText("属性")]
        [ShowInInspector]
        [ListDrawerSettings(DraggableItems = false, IsReadOnly = true, HideAddButton = true)]
        [DisableContextMenu(true, true)]
        List<Property> mPropertys;

        [LabelText("方法")]
        [ShowInInspector]
        [ListDrawerSettings(DraggableItems = false, IsReadOnly = true, HideAddButton = true)]
        [DisableContextMenu(true, true)]
        List<Method> mMethods;

        public bool IsNullOrEmpty()
        {
            return mFields.IsNullOrEmpty() && mPropertys.IsNullOrEmpty() && mMethods.IsNullOrEmpty();
        }

        public string TitleName()
        {
            return mComponentName;
        }

        public bool IsClickable()
        {
            return false;
        }
    }
}