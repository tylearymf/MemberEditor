#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DrawerInfo.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.OdinInspector.Editor
{
    using System;
    using Utilities;
    using Drawers;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public class DrawerInfo : IEquatable<DrawerInfo>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        public readonly Type DrawerType;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public readonly Type DrawnAttributeType;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public readonly Type DrawnValueType;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public readonly OdinDrawerAttribute OdinDrawerAttribute;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public readonly DrawerPriority Priority = DrawerPriority.AutoPriority;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public DrawerInfo(Type drawerType, Type drawnValueType, Type drawnAttributeType)
            : this(drawerType, drawnValueType, drawnAttributeType, drawerType.GetAttribute<OdinDrawerAttribute>(false))
        {
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public DrawerInfo(Type drawerType, Type drawnValueType, Type drawnAttributeType, OdinDrawerAttribute odinDrawerAttribute)
        {
            this.DrawerType = drawerType;
            this.DrawnValueType = drawnValueType;
            this.DrawnAttributeType = drawnAttributeType;
            this.OdinDrawerAttribute = odinDrawerAttribute;

            //
            // Figure out the drawer's priority
            //

            DrawerPriorityAttribute priorityAttribute = null;

            if (drawerType.IsGenericType)
            {
                //
                // Special case for Unity property drawers;
                // Allow them to specify their priority if they
                // want, and if they haven't, assign them
                // downgraded priorities so Odin drawers always
                // override them by default.
                //

                Type unityDrawer = null;

                if (drawerType.GetGenericTypeDefinition() == typeof(UnityPropertyDrawer<,>) || drawerType.GetGenericTypeDefinition() == typeof(UnityDecoratorAttributeDrawer<,,>) || drawerType.GetGenericTypeDefinition() == typeof(UnityPropertyAttributeDrawer<,,>) || drawerType.GetGenericTypeDefinition() == typeof(AbstractTypeUnityPropertyDrawer<,,>))
                {
                    unityDrawer = drawerType.GetGenericArguments()[0];
                }

                if (unityDrawer != null)
                {
                    priorityAttribute = unityDrawer.GetCustomAttribute<DrawerPriorityAttribute>();
                }
            }

            if (priorityAttribute == null)
            {
                priorityAttribute = drawerType.GetCustomAttribute<DrawerPriorityAttribute>();
            }

            if (priorityAttribute != null)
            {
                this.Priority = priorityAttribute.Priority;
            }

            if (this.Priority == DrawerPriority.AutoPriority)
            {
                if (this.DrawnAttributeType != null)
                {
                    this.Priority = DrawerPriority.AttributePriority;
                }
                else
                {
                    this.Priority = DrawerPriority.ValuePriority;
                }
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var value = this.DrawerType.GetHashCode();

                if (this.DrawnAttributeType != null) value ^= this.DrawnAttributeType.GetHashCode();
                if (this.DrawnValueType != null) value ^= this.DrawnValueType.GetHashCode();

                return value;
            }
        }

        public bool Equals(DrawerInfo other)
        {
            return other != null
                && other.DrawerType == this.DrawerType
                && other.DrawnAttributeType == this.DrawnAttributeType
                && other.DrawnValueType == this.DrawnValueType
                && other.Priority == this.Priority;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (this.DrawnAttributeType != null)
            {
                if (this.DrawnValueType != null)
                {
                    return "AttributeValueDrawer { " + this.DrawerType.GetNiceName() + " -> Attr:" + this.DrawnAttributeType.GetNiceName() + " & Value:" + this.DrawnValueType.GetNiceName() + (this.DrawnValueType.IsGenericParameter ? " " + this.DrawnValueType.GetGenericParameterConstraintsString() : "") + " }";
                }
                else
                {
                    return "OmniAttributeDrawer { " + this.DrawerType.GetNiceName() + " -> " + this.DrawnAttributeType.GetNiceName() + (this.DrawnAttributeType.IsGenericParameter ? " " + this.DrawnAttributeType.GetGenericParameterConstraintsString() : "") + " }";
                }
            }
            else
            {
                return "PropertyDrawer { " + this.DrawerType.GetNiceName() + " -> " + this.DrawnValueType.GetNiceName() + (this.DrawnValueType.IsGenericParameter ? " " + this.DrawnValueType.GetGenericParameterConstraintsString() : "") + " }";
            }
        }
    }
}
#endif