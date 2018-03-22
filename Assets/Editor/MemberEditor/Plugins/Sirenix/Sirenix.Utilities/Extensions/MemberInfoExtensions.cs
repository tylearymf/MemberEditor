//-----------------------------------------------------------------------
// <copyright file="MemberInfoExtensions.cs" company="Sirenix IVS">
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
    /// MemberInfo method extensions.
    /// </summary>
    public static class MemberInfoExtensions
    {
        /// <summary>
        /// Returns true if the attribute whose type is specified by the generic argument is defined on this member
        /// </summary>
        public static bool IsDefined<T>(this MemberInfo member, bool inherit) where T : Attribute
        {
            return member.IsDefined(typeof(T), inherit);
        }

        /// <summary>
        /// Returns true if the attribute whose type is specified by the generic argument is defined on this member
        /// </summary>
        public static bool IsDefined<T>(this MemberInfo member) where T : Attribute
        {
            return IsDefined<T>(member, false);
        }

        /// <summary>
        /// Returns the first found custom attribute of type T on this member
        /// Returns null if none was found
        /// </summary>
        public static T GetAttribute<T>(this MemberInfo member, bool inherit) where T : Attribute
        {
            var all = GetAttributes<T>(member, inherit).ToArray();
            return all.IsNullOrEmpty() ? null : all[0];
        }

        /// <summary>
        /// Returns the first found non-inherited custom attribute of type T on this member
        /// Returns null if none was found
        /// </summary>
        public static T GetAttribute<T>(this MemberInfo member) where T : Attribute
        {
            return GetAttribute<T>(member, false);
        }

        /// <summary>
        /// Gets all attributes of the specified generic type.
        /// </summary>
        /// <param name="member">The member.</param>
        public static IEnumerable<T> GetAttributes<T>(this MemberInfo member) where T : Attribute
        {
            return GetAttributes<T>(member, false);
        }

        /// <summary>
        /// Gets all attributes of the specified generic type.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="inherit">If true, specifies to also search the ancestors of element for custom attributes.</param>
        public static IEnumerable<T> GetAttributes<T>(this MemberInfo member, bool inherit) where T : Attribute
        {
            return member.GetCustomAttributes(typeof(T), inherit).Cast<T>();
        }

        /// <summary>
        /// Gets all attribute instances defined on a MemeberInfo.
        /// </summary>
        /// <param name="member">The member.</param>
        public static Attribute[] GetAttributes(this MemberInfo member)
        {
            return member.GetAttributes<Attribute>().ToArray();
        }

        /// <summary>
        /// Gets all attribute instances on a MemberInfo.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="inherit">If true, specifies to also search the ancestors of element for custom attributes.</param>
        public static Attribute[] GetAttributes(this MemberInfo member, bool inherit)
        {
            return member.GetAttributes<Attribute>(inherit).ToArray();
        }

        /// <summary>
        /// If this member is a method, returns the full method name (name + params) otherwise the member name paskal splitted
        /// </summary>
        public static string GetNiceName(this MemberInfo member)
        {
            var method = member as MethodBase;
            string result;
            if (method != null)
            {
                result = method.GetFullName();
            }
            else
            {
                result = member.Name;
            }

            return result.ToTitleCase();
        }

        /// <summary>
        /// Determines whether a FieldInfo, PropertyInfo or MethodInfo is static.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>
        ///   <c>true</c> if the specified member is static; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public static bool IsStatic(this MemberInfo member)
        {
            var field = member as FieldInfo;
            if (field != null)
            {
                return field.IsStatic;
            }

            var property = member as PropertyInfo;
            if (property != null)
            {
                return property.CanRead ? property.GetGetMethod(true).IsStatic : property.GetSetMethod(true).IsStatic;
            }

            var method = member as MethodBase;
            if (method != null)
            {
                return method.IsStatic;
            }

            var @event = member as EventInfo;
            if (@event != null)
            {
                return @event.GetRaiseMethod(true).IsStatic;
            }

            var type = member as Type;
            if (type != null)
            {
                return type.IsSealed && type.IsAbstract;
            }

            string message = string.Format(
                CultureInfo.InvariantCulture,
                "Unable to determine IsStatic for member {0}.{1}" +
                "MemberType was {2} but only fields, properties and methods are supported.",
                member.Name,
                member.MemberType,
                Environment.NewLine);

            throw new NotSupportedException(message);
        }

        /// <summary>
        /// Determines whether the specified member is an alias.
        /// </summary>
        /// <param name="memberInfo">The member to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified member is an alias; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAlias(this MemberInfo memberInfo)
        {
            return memberInfo is MemberAliasFieldInfo
                || memberInfo is MemberAliasPropertyInfo
                || memberInfo is MemberAliasMethodInfo;
        }

        /// <summary>
        /// Returns the original, backing member of an alias member if the member is an alias.
        /// </summary>
        /// <param name="memberInfo">The member to check.</param>
        /// /// <param name="throwOnNotAliased">if set to <c>true</c> an exception will be thrown if the member is not aliased.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">The member was not aliased; this only occurs if throwOnNotAliased is true.</exception>
        public static MemberInfo DeAlias(this MemberInfo memberInfo, bool throwOnNotAliased = false)
        {
            MemberAliasFieldInfo aliasFieldInfo = memberInfo as MemberAliasFieldInfo;

            if (aliasFieldInfo != null)
            {
                return aliasFieldInfo.AliasedField;
            }

            MemberAliasPropertyInfo aliasPropertyInfo = memberInfo as MemberAliasPropertyInfo;

            if (aliasPropertyInfo != null)
            {
                return aliasPropertyInfo.AliasedProperty;
            }

            MemberAliasMethodInfo aliasMethodInfo = memberInfo as MemberAliasMethodInfo;

            if (aliasMethodInfo != null)
            {
                return aliasMethodInfo.AliasedMethod;
            }

            if (throwOnNotAliased)
            {
                throw new ArgumentException("The member " + memberInfo.GetNiceName() + " was not aliased.");
            }

            return memberInfo;
        }
    }
}