#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="MaxValueAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.OdinInspector;
    using UnityEngine;

    /// <summary>
    /// Draws byte properties marked with <see cref="MaxValueAttribute"/>.
    /// </summary>
	/// <seealso cref="MaxValueAttribute"/>
	/// <seealso cref="MinValueAttribute"/>
	/// <seealso cref="RangeAttribute"/>
	/// <seealso cref="MinMaxSliderAttribute"/>
	/// <seealso cref="DelayedAttribute"/>
	/// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3)]
    public sealed class MaxValueAttributeByteDrawer : OdinAttributeDrawer<MaxValueAttribute, byte>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<byte> entry, MaxValueAttribute attribute, GUIContent label)
        {
            this.CallNextDrawer(entry, label);

            byte v = entry.SmartValue;
            if (attribute.MaxValue < v)
            {
                entry.SmartValue = (byte)attribute.MaxValue;
            }
        }
    }

    /// <summary>
    /// Draws double properties marked with <see cref="MaxValueAttribute"/>.
    /// </summary>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3)]
    public sealed class MaxValueAttributeDoubleDrawer : OdinAttributeDrawer<MaxValueAttribute, double>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<double> entry, MaxValueAttribute attribute, GUIContent label)
        {
            this.CallNextDrawer(entry, label);

            double v = entry.SmartValue;
            if (attribute.MaxValue < v)
            {
                entry.SmartValue = attribute.MaxValue;
            }
        }
    }

    /// <summary>
    /// Draws ulong properties marked with <see cref="MaxValueAttribute"/>.
    /// </summary>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3)]
    public sealed class MaxValueAttributeUint64Drawer : OdinAttributeDrawer<MaxValueAttribute, ulong>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<ulong> entry, MaxValueAttribute attribute, GUIContent label)
        {
            this.CallNextDrawer(entry, label);

            ulong v = entry.SmartValue;
            if (attribute.MaxValue < v)
            {
                entry.SmartValue = (ulong)attribute.MaxValue;
            }
        }
    }

    /// <summary>
    /// Draws uint properties marked with <see cref="MaxValueAttribute"/>.
    /// </summary>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3)]
    public sealed class MaxValueAttributeUint32Drawer : OdinAttributeDrawer<MaxValueAttribute, uint>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<uint> entry, MaxValueAttribute attribute, GUIContent label)
        {
            this.CallNextDrawer(entry, label);

            uint v = entry.SmartValue;
            if (attribute.MaxValue < v)
            {
                entry.SmartValue = (uint)attribute.MaxValue;
            }
        }
    }

    /// <summary>
    /// Draws ushort properties marked with <see cref="MaxValueAttribute"/>.
    /// </summary>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3)]
    public sealed class MaxValueAttributeUint16Drawer : OdinAttributeDrawer<MaxValueAttribute, ushort>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<ushort> entry, MaxValueAttribute attribute, GUIContent label)
        {
            this.CallNextDrawer(entry, label);

            ushort v = entry.SmartValue;
            if (attribute.MaxValue < v)
            {
                entry.SmartValue = (ushort)attribute.MaxValue;
            }
        }
    }

    /// <summary>
    /// Draws sbyte properties marked with <see cref="MaxValueAttribute"/>.
    /// </summary>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3)]
    public sealed class MaxValueAttributeSbyteDrawer : OdinAttributeDrawer<MaxValueAttribute, sbyte>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<sbyte> entry, MaxValueAttribute attribute, GUIContent label)
        {
            this.CallNextDrawer(entry, label);

            sbyte v = entry.SmartValue;
            if (attribute.MaxValue < v)
            {
                entry.SmartValue = (sbyte)attribute.MaxValue;
            }
        }
    }

    /// <summary>
    /// Draws long properties marked with <see cref="MaxValueAttribute"/>.
    /// </summary>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3)]
    public sealed class MaxValueAttributeInt64Drawer : OdinAttributeDrawer<MaxValueAttribute, long>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<long> entry, MaxValueAttribute attribute, GUIContent label)
        {
            this.CallNextDrawer(entry, label);

            long v = entry.SmartValue;
            if (attribute.MaxValue < v)
            {
                entry.SmartValue = (long)attribute.MaxValue;
            }
        }
    }

    /// <summary>
    /// Draws int properties marked with <see cref="MaxValueAttribute"/>.
    /// </summary>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3)]
    public sealed class MaxValueAttributeInt32Drawer : OdinAttributeDrawer<MaxValueAttribute, int>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<int> entry, MaxValueAttribute attribute, GUIContent label)
        {
            this.CallNextDrawer(entry, label);

            int v = entry.SmartValue;
            if (attribute.MaxValue < v)
            {
                entry.SmartValue = (int)attribute.MaxValue;
            }
        }
    }

    /// <summary>
    /// Draws short properties marked with <see cref="MaxValueAttribute"/>.
    /// </summary>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3)]
    public sealed class MaxValueAttributeInt16Drawer : OdinAttributeDrawer<MaxValueAttribute, short>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<short> entry, MaxValueAttribute attribute, GUIContent label)
        {
            this.CallNextDrawer(entry, label);

            short v = entry.SmartValue;
            if (attribute.MaxValue < v)
            {
                entry.SmartValue = (short)attribute.MaxValue;
            }
        }
    }

    /// <summary>
    /// Draws float properties marked with <see cref="MaxValueAttribute"/>.
    /// </summary>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3)]
    public sealed class MaxValueAttributeFloatDrawer : OdinAttributeDrawer<MaxValueAttribute, float>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<float> entry, MaxValueAttribute attribute, GUIContent label)
        {
            this.CallNextDrawer(entry, label);

            float v = entry.SmartValue;
            if (attribute.MaxValue < v)
            {
                entry.SmartValue = (float)attribute.MaxValue;
            }
        }
    }

    /// <summary>
    /// Draws Vector2 properties marked with <see cref="MaxValueAttribute"/>.
    /// </summary>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3)]
    public sealed class MaxValueAttributeVector2Drawer : OdinAttributeDrawer<MaxValueAttribute, Vector2>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<Vector2> entry, MaxValueAttribute attribute, GUIContent label)
        {
            this.CallNextDrawer(entry, label);

            Vector2 v = entry.SmartValue;
            if (attribute.MaxValue < v.x ||
                attribute.MaxValue < v.y)
            {
                v.x = attribute.MaxValue < v.x ? (float)attribute.MaxValue : v.x;
                v.y = attribute.MaxValue < v.y ? (float)attribute.MaxValue : v.y;
                entry.SmartValue = v;
            }
        }
    }

    /// <summary>
    /// Draws Vector3 properties marked with <see cref="MaxValueAttribute"/>.
    /// </summary>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3)]
    public sealed class MaxValueAttributeVector3Drawer : OdinAttributeDrawer<MaxValueAttribute, Vector3>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<Vector3> entry, MaxValueAttribute attribute, GUIContent label)
        {
            this.CallNextDrawer(entry, label);

            Vector3 v = entry.SmartValue;
            if (attribute.MaxValue < v.x ||
                attribute.MaxValue < v.y ||
                attribute.MaxValue < v.z)
            {
                v.x = attribute.MaxValue < v.x ? (float)attribute.MaxValue : v.x;
                v.y = attribute.MaxValue < v.y ? (float)attribute.MaxValue : v.y;
                v.z = attribute.MaxValue < v.z ? (float)attribute.MaxValue : v.z;
                entry.SmartValue = v;
            }
        }
    }

    /// <summary>
    /// Draws Vector4 properties marked with <see cref="MaxValueAttribute"/>.
    /// </summary>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3)]
    public sealed class MaxValueAttributeVector4Drawer : OdinAttributeDrawer<MaxValueAttribute, Vector4>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<Vector4> entry, MaxValueAttribute attribute, GUIContent label)
        {
            this.CallNextDrawer(entry, label);

            Vector4 v = entry.SmartValue;
            if (attribute.MaxValue < v.x ||
                attribute.MaxValue < v.y ||
                attribute.MaxValue < v.z ||
                attribute.MaxValue < v.w)
            {
                v.x = attribute.MaxValue < v.x ? (float)attribute.MaxValue : v.x;
                v.y = attribute.MaxValue < v.y ? (float)attribute.MaxValue : v.y;
                v.z = attribute.MaxValue < v.z ? (float)attribute.MaxValue : v.z;
                v.w = attribute.MaxValue < v.w ? (float)attribute.MaxValue : v.w;
                entry.SmartValue = v;
            }
        }
    }

    /// <summary>
    /// Draws decimal properties marked with <see cref="MaxValueAttribute"/>.
    /// </summary>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3)]
    public sealed class MaxValueAttributeDecimalDrawer : OdinAttributeDrawer<MaxValueAttribute, decimal>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<decimal> entry, MaxValueAttribute attribute, GUIContent label)
        {
            this.CallNextDrawer(entry, label);

            if ((decimal)attribute.MaxValue < entry.SmartValue)
            {
                entry.SmartValue = (decimal)attribute.MaxValue;
            }
        }
    }
}
#endif