//-----------------------------------------------------------------------
// <copyright file="MemberFinder.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Globalization;

namespace Sirenix.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public static class MemberFinderExtensions
    {
        /// <summary>
        /// <para>Find members of the given type, while providing good error messages based on the following search filters provided.</para>
        /// <para>See <see cref="MemberFinder"/> for more information.</para>
        /// </summary>
        public static MemberFinder FindMember(this Type type)
        {
            return MemberFinder.Start(type);
        }
    }

    /// <summary>
    /// <para>
    /// The MemberFinder is a utility class often used by Odin drawers to find fields, methods, and
    /// properties while providing good user-friendly error messages based on the search criteria.
    /// </para>
    /// <para>
    /// The MemberFinder is initialized by ether using the extensions method type.FindMember() or MemberFinder.Start(type),
    /// and must be ended again using ether TryGetMember() or GetMember().
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// // Finds any member (FieldInfo, PropertyInfo or MethodInfo) "memberName" with a return type of string.
    /// string errorMessage;
    /// MemberInfo myMember = typeof(MyType)
    ///                         .FindMember()
    ///                         .IsNamed("memberName")
    ///                         .HasReturnType(typeof(string))
    ///                         .HasNoParameters()
    ///                         .GetMember(out errorMessage);
    /// If no matching member was found, myMember will be null and errorMessage will contain a proper formatted error message based on the search filters provided.
    /// </code>
    /// </example>
    public class MemberFinder
    {
        private Type type;
        private Cache<MemberFinder> cache;
        private ConditionFlags conditionFlags;
        private string name;
        private Type returnType;
        private List<Type> paramTypes = new List<Type>();
        private bool returnTypeCanInherit;

        [Flags]
        private enum ConditionFlags
        {
            None = 0,
            IsStatic = 1 << 1,
            IsProperty = 1 << 2,
            IsInstance = 1 << 3,
            IsDeclaredOnly = 1 << 4,
            HasNoParamaters = 1 << 5,
            IsMethod = 1 << 6,
            IsField = 1 << 7,
            IsPublic = 1 << 8,
            IsNonPublic = 1 << 9,
            //HasName = 1 << 10,
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberFinder"/> class.
        /// </summary>
        public MemberFinder()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberFinder"/> class.
        /// </summary>
        public MemberFinder(Type type)
        {
            this.Start(type, null);
        }

        /// <summary>
        /// <para>Find members of the given type, while providing good error messages based on the following search filters provided.</para>
        /// </summary>
        public static MemberFinder Start<T>()
        {
            var cache = Cache<MemberFinder>.Claim();
            return cache.Value.Start(typeof(T), cache);
        }

        /// <summary>
        /// <para>Find members of the given type, while providing good error messages based on the following search filters provided.</para>
        /// </summary>
        public static MemberFinder Start(Type type)
        {
            var cache = Cache<MemberFinder>.Claim();
            return cache.Value.Start(type, cache);
        }

        /// <summary>
        /// Can be true for both fields, properties and methods.
        /// </summary>
        /// <returns></returns>
        public MemberFinder HasNoParameters()
        {
            this.conditionFlags |= ConditionFlags.HasNoParamaters;
            return this;
        }

        /// <summary>
        /// Exclude members found in base-types.
        /// </summary>
        public MemberFinder IsDeclaredOnly()
        {
            this.conditionFlags |= ConditionFlags.IsDeclaredOnly;
            return this;
        }

        /// <summary>
        /// <para>Only include methods with the following parameter.</para>
        /// <para>Calling this will also exclude fields and properties.</para>
        /// <para>Parameter type inheritance is supported.</para>
        /// </summary>
        public MemberFinder HasParameters(Type param1)
        {
            this.conditionFlags |= ConditionFlags.IsMethod;
            this.paramTypes.Add(param1);
            return this;
        }

        /// <summary>
        /// <para>Only include methods with the following parameters.</para>
        /// <para>Calling this will also exclude fields and properties.</para>
        /// <para>Parameter type inheritance is supported.</para>
        /// </summary>
        public MemberFinder HasParameters(Type param1, Type param2)
        {
            this.conditionFlags |= ConditionFlags.IsMethod;
            this.paramTypes.Add(param1);
            this.paramTypes.Add(param2);
            return this;
        }

        /// <summary>
        /// <para>Only include methods with the following parameters.</para>
        /// <para>Calling this will also exclude fields and properties.</para>
        /// <para>Parameter type inheritance is supported.</para>
        /// </summary>
        public MemberFinder HasParameters(Type param1, Type param2, Type param3)
        {
            this.conditionFlags |= ConditionFlags.IsMethod;
            this.paramTypes.Add(param1);
            this.paramTypes.Add(param2);
            this.paramTypes.Add(param3);
            return this;
        }

        /// <summary>
        /// <para>Only include methods with the following parameters.</para>
        /// <para>Calling this will also exclude fields and properties.</para>
        /// <para>Parameter type inheritance is supported.</para>
        /// </summary>
        public MemberFinder HasParameters(Type param1, Type param2, Type param3, Type param4)
        {
            this.conditionFlags |= ConditionFlags.IsMethod;
            this.paramTypes.Add(param1);
            this.paramTypes.Add(param2);
            this.paramTypes.Add(param3);
            this.paramTypes.Add(param4);
            return this;
        }

        /// <summary>
        /// <para>Only include methods with the following parameters.</para>
        /// <para>Calling this will also exclude fields and properties.</para>
        /// <para>Parameter type inheritance is supported.</para>
        /// </summary>
        public MemberFinder HasParameters<T>()
        {
            return this.HasParameters(typeof(T));
        }

        /// <summary>
        /// <para>Only include methods with the following parameters.</para>
        /// <para>Calling this will also exclude fields and properties.</para>
        /// <para>Parameter type inheritance is supported.</para>
        /// </summary>
        public MemberFinder HasParameters<T1, T2>()
        {
            return this.HasParameters(typeof(T1), typeof(T2));
        }

        /// <summary>
        /// <para>Only include methods with the following parameters.</para>
        /// <para>Calling this will also exclude fields and properties.</para>
        /// <para>Parameter type inheritance is supported.</para>
        /// </summary>
        public MemberFinder HasParameters<T1, T2, T3>()
        {
            return this.HasParameters(typeof(T1), typeof(T2), typeof(T3));
        }

        /// <summary>
        /// <para>Only include methods with the following parameters.</para>
        /// <para>Calling this will also exclude fields and properties.</para>
        /// <para>Parameter type inheritance is supported.</para>
        /// </summary>
        public MemberFinder HasParameters<T1, T2, T3, T4>()
        {
            return this.HasParameters(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }

        /// <summary>
        /// Determines whether [has return type] [the specified return type].
        /// </summary>
        public MemberFinder HasReturnType(Type returnType, bool inherit = false)
        {
            this.returnTypeCanInherit = inherit;
            this.returnType = returnType;
            return this;
        }

        /// <summary>
        /// Can be true for both fields, properties and methods.
        /// </summary>
        public MemberFinder HasReturnType<T>(bool inherit = false)
        {
            return this.HasReturnType(typeof(T), inherit);
        }

        /// <summary>
        /// Calls IsField() and IsProperty().
        /// </summary>
        public MemberFinder IsFieldOrProperty()
        {
            this.IsField();
            this.IsProperty();
            return this;
        }

        /// <summary>
        /// Only include static members. By default, both static and non-static members are included.
        /// </summary>
        public MemberFinder IsStatic()
        {
            this.conditionFlags |= ConditionFlags.IsStatic;
            return this;
        }

        /// <summary>
        /// Only include non-static members. By default, both static and non-static members are included.
        /// </summary>
        public MemberFinder IsInstance()
        {
            this.conditionFlags |= ConditionFlags.IsInstance;
            return this;
        }

        /// <summary>
        /// Specify the name of the member.
        /// </summary>
        public MemberFinder IsNamed(string name)
        {
            this.name = name;
            return this;
        }

        /// <summary>
        /// <para>Excludes fields and methods if nether IsField() or IsMethod() is called. Otherwise includes properties.</para>
        /// <para>By default, all member types are included.</para>
        /// </summary>
        public MemberFinder IsProperty()
        {
            this.conditionFlags |= ConditionFlags.IsProperty;
            return this;
        }

        /// <summary>
        /// <para>Excludes fields and properties if nether IsField() or IsProperty() is called. Otherwise includes methods.</para>
        /// <para>By default, all member types are included.</para>
        /// </summary>
        public MemberFinder IsMethod()
        {
            this.conditionFlags |= ConditionFlags.IsMethod;
            return this;
        }

        /// <summary>
        /// <para>Excludes properties and methods if nether IsProperty() or IsMethod() is called. Otherwise includes fields.</para>
        /// <para>By default, all member types are included.</para>
        /// </summary>
        public MemberFinder IsField()
        {
            this.conditionFlags |= ConditionFlags.IsField;
            return this;
        }

        /// <summary>
        /// <para>Excludes non-public members if IsNonPublic() has not yet been called. Otherwise includes public members.</para>
        /// <para>By default, both public and non-public members are included.</para>
        /// </summary>
        public MemberFinder IsPublic()
        {
            this.conditionFlags |= ConditionFlags.IsPublic;
            return this;
        }

        /// <summary>
        /// <para>Excludes public members if IsPublic() has not yet been called. Otherwise includes non-public members.</para>
        /// <para>By default, both public and non-public members are included.</para>
        /// </summary>
        public MemberFinder IsNonPublic()
        {
            this.conditionFlags |= ConditionFlags.IsNonPublic;
            return this;
        }

        /// <summary>
        /// Excludes fields and properties, and only includes methods with a return type of void.
        /// </summary>
        public MemberFinder ReturnsVoid()
        {
            this.conditionFlags |= ConditionFlags.IsMethod;
            return this.HasReturnType(typeof(void));
        }

        /// <summary>
        /// <para>Gets the member based on the search filters provided</para>
        /// <para>Returns null if no member was found.</para>
        /// </summary>
        public T GetMember<T>() where T : MemberInfo
        {
            string errorMessage = null;
            return GetMember<T>(out errorMessage);
        }

        /// <summary>
        /// <para>Gets the member based on the search filters provided, and provides a proper error message if no members was found.</para>
        /// </summary>
        public T GetMember<T>(out string errorMessage) where T : MemberInfo
        {
            T memberInfo;
            TryGetMember(out memberInfo, out errorMessage);
            return memberInfo;
        }

        /// <summary>
        /// <para>Gets the member based on the search filters provided, and provides a proper error message if no members was found.</para>
        /// </summary>
        public MemberInfo GetMember(out string errorMessage)
        {
            MemberInfo memberInfo;
            TryGetMember(out memberInfo, out errorMessage);
            return memberInfo;
        }

        /// <summary>
        /// <para>Try gets the member based on the search filters provided, and provides a proper error message if no members was found.</para>
        /// </summary>
        public bool TryGetMember<T>(out T memberInfo, out string errorMessage) where T : MemberInfo
        {
            MemberInfo m;
            bool result = TryGetMember(out m, out errorMessage);
            memberInfo = m as T;
            return result;
        }

        /// <summary>
        /// <para>Try gets the member based on the search filters provided, and provides a proper error message if no members was found.</para>
        /// </summary>
        public bool TryGetMember(out MemberInfo memberInfo, out string errorMessage)
        {
            MemberInfo[] memberInfos;
            if (TryGetMembers(out memberInfos, out errorMessage))
            {
                memberInfo = memberInfos[0];
                return true;
            }
            else
            {
                memberInfo = null;
                return false;
            }
        }

        /// <summary>
        /// <para>Try gets all members based on the search filters provided, and provides a proper error message if no members was found.</para>
        /// </summary>
        public bool TryGetMembers(out MemberInfo[] memberInfos, out string errorMessage)
        {
            try
            {
                //if (this.name == null && this.HasCondition(ConditionFlags.HasName))
                //{
                //    memberInfos = null;
                //    errorMessage = ""
                //}

                IEnumerable<MemberInfo> tmpMemberInfos = Enumerable.Empty<MemberInfo>();
                BindingFlags bindingFlags = this.HasCondition(ConditionFlags.IsDeclaredOnly) ? BindingFlags.DeclaredOnly : BindingFlags.FlattenHierarchy;

                bool hasNoParamaters = this.HasCondition(ConditionFlags.HasNoParamaters);
                bool isInstance = this.HasCondition(ConditionFlags.IsInstance);
                bool isStatic = this.HasCondition(ConditionFlags.IsStatic);
                bool isPublic = this.HasCondition(ConditionFlags.IsPublic);
                bool isNonPublic = this.HasCondition(ConditionFlags.IsNonPublic);
                bool isMethod = this.HasCondition(ConditionFlags.IsMethod);
                bool isField = this.HasCondition(ConditionFlags.IsField);
                bool isProperty = this.HasCondition(ConditionFlags.IsProperty);

                if (!isPublic && !isNonPublic)
                {
                    isPublic = true;
                    isNonPublic = true;
                }

                if (!isStatic && !isInstance)
                {
                    isStatic = true;
                    isInstance = true;
                }

                if (!(isField || isProperty || isMethod))
                {
                    isMethod = true;
                    isField = true;
                    isProperty = true;
                }

                if (isInstance) bindingFlags |= BindingFlags.Instance;
                if (isStatic) bindingFlags |= BindingFlags.Static;
                if (isPublic) bindingFlags |= BindingFlags.Public;
                if (isNonPublic) bindingFlags |= BindingFlags.NonPublic;

                if (isMethod && isField && isProperty)
                {
                    if (this.name == null)
                    {
                        tmpMemberInfos = this.type.GetAllMembers(bindingFlags);
                    }
                    else
                    {
                        tmpMemberInfos = this.type.GetAllMembers(bindingFlags).Where(n => n.Name == this.name);
                    }

                    if (hasNoParamaters)
                    {
                        tmpMemberInfos = tmpMemberInfos.Where(x => x is MethodInfo == false || (x as MethodInfo).GetParameters().Length == 0);
                    }
                }
                else
                {
                    if (isMethod)
                    {
                        IEnumerable<MethodInfo> methodInfos = this.name == null ? this.type.GetAllMembers<MethodInfo>(bindingFlags) : this.type.GetAllMembers<MethodInfo>(bindingFlags).Where(x => x.Name == name);

                        if (hasNoParamaters)
                        {
                            methodInfos = methodInfos.Where(x => x.GetParameters().Length == 0);
                        }
                        else if (this.paramTypes.Count > 0)
                        {
                            methodInfos = methodInfos.Where(x => x.HasParamaters(this.paramTypes));
                        }

                        tmpMemberInfos = methodInfos.OfType<MemberInfo>();
                    }

                    if (isField)
                    {
                        if (this.name == null)
                        {
                            tmpMemberInfos = tmpMemberInfos.Append(this.type.GetAllMembers<FieldInfo>(bindingFlags).Cast<MemberInfo>());
                        }
                        else
                        {
                            tmpMemberInfos = tmpMemberInfos.Append(this.type.GetAllMembers<FieldInfo>(bindingFlags).Where(n => n.Name == this.name).Cast<MemberInfo>());
                        }
                    }

                    if (isProperty)
                    {
                        if (this.name == null)
                        {
                            tmpMemberInfos = tmpMemberInfos.Append(this.type.GetAllMembers<PropertyInfo>(bindingFlags).Cast<MemberInfo>());
                        }
                        else
                        {
                            tmpMemberInfos = tmpMemberInfos.Append(this.type.GetAllMembers<PropertyInfo>(bindingFlags).Where(n => n.Name == this.name).Cast<MemberInfo>());
                        }
                    }
                }

                if (this.returnType != null)
                {
                    if (this.returnTypeCanInherit)
                    {
                        tmpMemberInfos = tmpMemberInfos.Where(x => x.GetReturnType().InheritsFrom(this.returnType));
                    }
                    else
                    {
                        tmpMemberInfos = tmpMemberInfos.Where(x => x.GetReturnType() == this.returnType);
                    }
                }

                memberInfos = tmpMemberInfos.ToArray();

                if (memberInfos != null && memberInfos.Length != 0)
                {
                    errorMessage = null;
                    return true;
                }
                else
                {
                    MemberInfo namedMember = this.name == null ? null : this.type.GetMember(this.name, Flags.AllMembers).FirstOrDefault(t =>
                    t is MethodInfo && isMethod ||
                    t is FieldInfo && isField ||
                    t is PropertyInfo && isProperty);

                    if (namedMember != null)
                    {
                        string accessModifies = namedMember.IsStatic() ? "Static " : "Non-static ";
                        bool noParamaterExpected = hasNoParamaters && namedMember is MethodInfo && (namedMember as MethodInfo).GetParameters().Length > 0;
                        if (noParamaterExpected)
                        {
                            errorMessage = accessModifies + "method " + this.name + " can not take parameters.";
                            return false;
                        }

                        bool wrongParameters = isMethod && this.paramTypes.Count > 0 && namedMember is MethodInfo && (namedMember as MethodInfo).HasParamaters(this.paramTypes) == false;
                        if (wrongParameters)
                        {
                            errorMessage = accessModifies + "method " + this.name + " must have the following parameters: " + string.Join(", ", this.paramTypes.Select(x => x.GetNiceName()).ToArray()) + ".";
                            return false;
                        }

                        bool wrongReturnType = this.returnType != null && this.returnType != namedMember.GetReturnType();
                        if (wrongReturnType)
                        {
                            if (this.returnTypeCanInherit)
                            {
                                errorMessage = accessModifies + namedMember.MemberType.ToString().ToLower(CultureInfo.InvariantCulture) + " " + this.name + " must have a return type that is assignable from " + this.returnType.GetNiceName() + ".";
                            }
                            else
                            {
                                errorMessage = accessModifies + namedMember.MemberType.ToString().ToLower(CultureInfo.InvariantCulture) + " " + this.name + " must have a return type of " + this.returnType.GetNiceName() + ".";
                            }
                            return false;
                        }
                    }

                    int modCount = (isField ? 1 : 0) + (isProperty ? 1 : 0) + (isMethod ? 1 : 0);

                    string strMemberTypes = (isField ? ("fields" + (modCount-- > 1 ? (modCount == 1 ? " or " : ", ") : " ")) : string.Empty) +
                                        (isProperty ? ("properties" + (modCount-- > 1 ? (modCount == 1 ? " or " : ", ") : " ")) : string.Empty) +
                                        (isMethod ? ("methods" + (modCount-- > 1 ? (modCount == 1 ? " or " : ", ") : " ")) : string.Empty);

                    string strAccessModifiers = (isPublic != isNonPublic ? (isPublic ? "public " : "non-public ") : string.Empty) +
                                           (isStatic != isInstance ? (isStatic ? "static " : "non-static ") : string.Empty);

                    string strReturnType = this.returnType == null ? " " : ("with a return type of " + this.returnType.GetNiceName() + " ");

                    string strParameters = this.paramTypes.Count == 0 ? " " : (strReturnType == " " ? "" : "and ") + "with the parameter signature (" + string.Join(", ", this.paramTypes.Select(n => n.GetNiceName()).ToArray()) + ") ";

                    if (this.name == null)
                    {
                        errorMessage = "No " + strAccessModifiers + strMemberTypes + strReturnType + strParameters + "was found in " + this.type.GetNiceName() + ".";
                        return false;
                    }
                    else
                    {
                        errorMessage = "No " + strAccessModifiers + strMemberTypes + "named " + this.name + " " + strReturnType + strParameters + "was found in " + this.type.GetNiceName() + ".";
                        return false;
                    }
                }
            }
            finally
            {
                Cache<MemberFinder>.Release(this.cache);
            }
        }

        private MemberFinder Start(Type type, Cache<MemberFinder> cache)
        {
            this.type = type;
            this.cache = cache;
            this.Reset();
            return this;
        }

        private void Reset()
        {
            this.returnType = null;
            this.name = null;
            this.conditionFlags = ConditionFlags.None;
            this.paramTypes.Clear();
        }

        private bool HasCondition(ConditionFlags flag)
        {
            return (this.conditionFlags & flag) == flag;
        }
    }
}