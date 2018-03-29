#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="RangeAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
	using Sirenix.Utilities.Editor;
	using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws byte properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
	/// <seealso cref="RangeAttribute"/>
	/// <seealso cref="MinValueAttribute"/>
	/// <seealso cref="MaxValueAttribute"/>
	/// <seealso cref="MinMaxSliderAttribute"/>
	/// <seealso cref="DelayedAttribute"/>
	/// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class RangeAttributeByteDrawer : OdinAttributeDrawer<RangeAttribute, byte>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<byte> entry, RangeAttribute attribute, GUIContent label)
        {
            int value = label == null ?
                EditorGUILayout.IntSlider(entry.SmartValue, Math.Max(byte.MinValue, (int)attribute.min), Math.Min(byte.MaxValue, (int)attribute.max)) :
                EditorGUILayout.IntSlider(label, entry.SmartValue, Math.Max(byte.MinValue, (int)attribute.min), Math.Min(byte.MaxValue, (int)attribute.max));

            if (value < byte.MinValue)
            {
                value = byte.MinValue;
            }
            else if (value > byte.MaxValue)
            {
                value = byte.MaxValue;
            }

            entry.SmartValue = (byte)value;
        }
    }

    /// <summary>
    /// Draws double properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class RangeAttributeDoubleDrawer : OdinAttributeDrawer<RangeAttribute, double>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<double> entry, RangeAttribute attribute, GUIContent label)
        {
            double value = entry.SmartValue;

            if (value < float.MinValue)
            {
                value = float.MinValue;
            }
            else if (value > float.MaxValue)
            {
                value = float.MaxValue;
            }

            entry.SmartValue = label == null ?
                EditorGUILayout.Slider((float)value, attribute.min, attribute.max) :
                EditorGUILayout.Slider(label, (float)value, attribute.min, attribute.max);
        }
    }

    /// <summary>
    /// Draws float properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class RangeAttributeFloatDrawer : OdinAttributeDrawer<RangeAttribute, float>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<float> entry, RangeAttribute attribute, GUIContent label)
        {
            entry.SmartValue = label == null ?
                EditorGUILayout.Slider(entry.SmartValue, attribute.min, attribute.max) :
                EditorGUILayout.Slider(label, entry.SmartValue, attribute.min, attribute.max);
        }
    }

	/// <summary>
	/// Draws decimal properties marked with <see cref="RangeAttribute"/>.
	/// </summary>
	[OdinDrawer]
	public sealed class RangeAttributeDecimalDrawer : OdinAttributeDrawer<RangeAttribute, decimal>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(IPropertyValueEntry<decimal> entry, RangeAttribute attribute, GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			float value = SirenixEditorFields.RangeFloatField(label, (float)entry.SmartValue, attribute.min, attribute.max);

			if (EditorGUI.EndChangeCheck())
			{
				entry.SmartValue = (decimal)value;
			}
		}
	}

    /// <summary>
    /// Draws short properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class RangeAttributeInt16Drawer : OdinAttributeDrawer<RangeAttribute, short>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<short> entry, RangeAttribute attribute, GUIContent label)
        {
            int value = label == null ?
                EditorGUILayout.IntSlider(entry.SmartValue, Math.Max(short.MinValue, (int)attribute.min), Math.Min(short.MaxValue, (int)attribute.max)) :
                EditorGUILayout.IntSlider(label, entry.SmartValue, Math.Max(short.MinValue, (int)attribute.min), Math.Min(short.MaxValue, (int)attribute.max));

            if (value < short.MinValue)
            {
                value = short.MinValue;
            }
            else if (value > short.MaxValue)
            {
                value = short.MaxValue;
            }

            entry.SmartValue = (short)value;
        }
    }

    /// <summary>
    /// Draws int properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class RangeAttributeInt32Drawer : OdinAttributeDrawer<RangeAttribute, int>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<int> entry, RangeAttribute attribute, GUIContent label)
        {
            entry.SmartValue = label == null ?
                EditorGUILayout.IntSlider(entry.SmartValue, (int)attribute.min, (int)attribute.max) :
                EditorGUILayout.IntSlider(label, entry.SmartValue, (int)attribute.min, (int)attribute.max);
        }
    }

    /// <summary>
    /// Draws long properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class RangeAttributeInt64Drawer : OdinAttributeDrawer<RangeAttribute, long>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<long> entry, RangeAttribute attribute, GUIContent label)
        {
            long uValue = entry.SmartValue;

            if (uValue < int.MinValue)
            {
                uValue = int.MinValue;
            }
            else if (uValue > int.MaxValue)
            {
                uValue = int.MaxValue;
            }

            int value = label == null ?
                EditorGUILayout.IntSlider((int)uValue, (int)attribute.min, (int)attribute.max) :
                EditorGUILayout.IntSlider(label, (int)uValue, (int)attribute.min, (int)attribute.max);

            entry.SmartValue = value;
        }
    }

    /// <summary>
    /// Draws sbyte properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class RangeAttributeSByteDrawer : OdinAttributeDrawer<RangeAttribute, sbyte>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<sbyte> entry, RangeAttribute attribute, GUIContent label)
        {
            int value = label == null ?
                EditorGUILayout.IntSlider(entry.SmartValue, Math.Max(sbyte.MinValue, (int)attribute.min), Math.Min(sbyte.MaxValue, (int)attribute.max)) :
                EditorGUILayout.IntSlider(label, entry.SmartValue, Math.Max(sbyte.MinValue, (int)attribute.min), Math.Min(sbyte.MaxValue, (int)attribute.max));

            if (value < sbyte.MinValue)
            {
                value = sbyte.MinValue;
            }
            else if (value > sbyte.MaxValue)
            {
                value = sbyte.MaxValue;
            }

            entry.SmartValue = (sbyte)value;
        }
    }

    /// <summary>
    /// Draws ushort properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class RangeAttributeUInt16Drawer : OdinAttributeDrawer<RangeAttribute, ushort>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<ushort> entry, RangeAttribute attribute, GUIContent label)
        {
            int value = label == null ?
                EditorGUILayout.IntSlider(entry.SmartValue, Math.Max(ushort.MinValue, (int)attribute.min), Math.Min(ushort.MaxValue, (int)attribute.max)) :
                EditorGUILayout.IntSlider(label, entry.SmartValue, Math.Max(ushort.MinValue, (int)attribute.min), Math.Min(ushort.MaxValue, (int)attribute.max));

            if (value < ushort.MinValue)
            {
                value = ushort.MinValue;
            }
            else if (value > ushort.MaxValue)
            {
                value = ushort.MaxValue;
            }

            entry.SmartValue = (ushort)value;
        }
    }

    /// <summary>
    /// Draws uint properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class RangeAttributeUInt32Drawer : OdinAttributeDrawer<RangeAttribute, uint>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<uint> entry, RangeAttribute attribute, GUIContent label)
        {
            uint uValue = entry.SmartValue;

            if (uValue > int.MaxValue)
            {
                uValue = int.MaxValue;
            }

            int value = label == null ?
                EditorGUILayout.IntSlider((int)uValue, Math.Max(0, (int)attribute.min), (int)attribute.max) :
                EditorGUILayout.IntSlider(label, (int)uValue, Math.Max(0, (int)attribute.min), (int)attribute.max);

            if (value < 0)
            {
                value = 0;
            }

            entry.SmartValue = (uint)value;
        }
    }

    /// <summary>
    /// Draws ulong properties marked with <see cref="RangeAttribute"/>.
    /// </summary>
    /// <seealso cref="RangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class RangeAttributeUInt64Drawer : OdinAttributeDrawer<RangeAttribute, ulong>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<ulong> entry, RangeAttribute attribute, GUIContent label)
        {
            ulong uValue = entry.SmartValue;

            if (uValue > int.MaxValue)
            {
                uValue = int.MaxValue;
            }

            int value = label == null ?
                EditorGUILayout.IntSlider((int)uValue, Math.Max(0, (int)attribute.min), (int)attribute.max) :
                EditorGUILayout.IntSlider(label, (int)uValue, Math.Max(0, (int)attribute.min), (int)attribute.max);

            if (value < 0)
            {
                value = 0;
            }

            entry.SmartValue = (ulong)value;
        }
    }
}
#endif