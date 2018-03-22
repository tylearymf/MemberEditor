#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyRangeAttributeDrawer.cs" company="Sirenix IVS">
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
    /// Draws byte properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
	/// <seealso cref="PropertyRangeAttribute"/>
	/// <seealso cref="MinValueAttribute"/>
	/// <seealso cref="MaxValueAttribute"/>
	/// <seealso cref="MinMaxSliderAttribute"/>
	/// <seealso cref="DelayedAttribute"/>
	/// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class PropertyRangeAttributeByteDrawer : OdinAttributeDrawer<PropertyRangeAttribute, byte>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<byte> entry, PropertyRangeAttribute attribute, GUIContent label)
        {
            int value = label == null ?
                EditorGUILayout.IntSlider(entry.SmartValue, Math.Max(byte.MinValue, (int)attribute.Min), Math.Min(byte.MaxValue, (int)attribute.Max)) :
                EditorGUILayout.IntSlider(label, entry.SmartValue, Math.Max(byte.MinValue, (int)attribute.Min), Math.Min(byte.MaxValue, (int)attribute.Max));

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
    /// Draws double properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class PropertyRangeAttributeDoubleDrawer : OdinAttributeDrawer<PropertyRangeAttribute, double>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<double> entry, PropertyRangeAttribute attribute, GUIContent label)
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
                EditorGUILayout.Slider((float)value, (float)attribute.Min, (float)attribute.Max) :
                EditorGUILayout.Slider(label, (float)value, (float)attribute.Min, (float)attribute.Max);
        }
    }

    /// <summary>
    /// Draws float properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class PropertyRangeAttributeFloatDrawer : OdinAttributeDrawer<PropertyRangeAttribute, float>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<float> entry, PropertyRangeAttribute attribute, GUIContent label)
        {
            entry.SmartValue = SirenixEditorFields.RangeFloatField(label, entry.SmartValue, (float)attribute.Min, (float)attribute.Max);
        }
    }
	
	/// <summary>
	/// Draws decimal properties marked with <see cref="PropertyRangeAttribute"/>.
	/// </summary>
	[OdinDrawer]
	public sealed class PropertyRangeAttributeDecimalDrawer : OdinAttributeDrawer<PropertyRangeAttribute, decimal>
	{
		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(IPropertyValueEntry<decimal> entry, PropertyRangeAttribute attribute, GUIContent label)
		{
			EditorGUI.BeginChangeCheck();
			float value = SirenixEditorFields.RangeFloatField(label, (float)entry.SmartValue, (float)attribute.Min, (float)attribute.Max);

			if (EditorGUI.EndChangeCheck())
			{
				entry.SmartValue = (decimal)value;
			}
		}
	}

    /// <summary>
    /// Draws short properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class PropertyRangeAttributeInt16Drawer : OdinAttributeDrawer<PropertyRangeAttribute, short>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<short> entry, PropertyRangeAttribute attribute, GUIContent label)
        {
            int value = label == null ?
                EditorGUILayout.IntSlider(entry.SmartValue, Math.Max(short.MinValue, (int)attribute.Min), Math.Min(short.MaxValue, (int)attribute.Max)) :
                EditorGUILayout.IntSlider(label, entry.SmartValue, Math.Max(short.MinValue, (int)attribute.Min), Math.Min(short.MaxValue, (int)attribute.Max));

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
    /// Draws int properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class PropertyRangeAttributeInt32Drawer : OdinAttributeDrawer<PropertyRangeAttribute, int>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<int> entry, PropertyRangeAttribute attribute, GUIContent label)
        {
            entry.SmartValue = label == null ?
                EditorGUILayout.IntSlider(entry.SmartValue, (int)attribute.Min, (int)attribute.Max) :
                EditorGUILayout.IntSlider(label, entry.SmartValue, (int)attribute.Min, (int)attribute.Max);
        }
    }

    /// <summary>
    /// Draws long properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class PropertyRangeAttributeInt64Drawer : OdinAttributeDrawer<PropertyRangeAttribute, long>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<long> entry, PropertyRangeAttribute attribute, GUIContent label)
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
                EditorGUILayout.IntSlider((int)uValue, (int)attribute.Min, (int)attribute.Max) :
                EditorGUILayout.IntSlider(label, (int)uValue, (int)attribute.Min, (int)attribute.Max);

            entry.SmartValue = value;
        }
    }

    /// <summary>
    /// Draws sbyte properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class PropertyRangeAttributeSByteDrawer : OdinAttributeDrawer<PropertyRangeAttribute, sbyte>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<sbyte> entry, PropertyRangeAttribute attribute, GUIContent label)
        {
            int value = label == null ?
                EditorGUILayout.IntSlider(entry.SmartValue, Math.Max(sbyte.MinValue, (int)attribute.Min), Math.Min(sbyte.MaxValue, (int)attribute.Max)) :
                EditorGUILayout.IntSlider(label, entry.SmartValue, Math.Max(sbyte.MinValue, (int)attribute.Min), Math.Min(sbyte.MaxValue, (int)attribute.Max));

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
    /// Draws ushort properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class PropertyRangeAttributeUInt16Drawer : OdinAttributeDrawer<PropertyRangeAttribute, ushort>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<ushort> entry, PropertyRangeAttribute attribute, GUIContent label)
        {
            int value = label == null ?
                EditorGUILayout.IntSlider(entry.SmartValue, Math.Max(ushort.MinValue, (int)attribute.Min), Math.Min(ushort.MaxValue, (int)attribute.Max)) :
                EditorGUILayout.IntSlider(label, entry.SmartValue, Math.Max(ushort.MinValue, (int)attribute.Min), Math.Min(ushort.MaxValue, (int)attribute.Max));

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
    /// Draws uint properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class PropertyRangeAttributeUInt32Drawer : OdinAttributeDrawer<PropertyRangeAttribute, uint>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<uint> entry, PropertyRangeAttribute attribute, GUIContent label)
        {
            uint uValue = entry.SmartValue;

            if (uValue > int.MaxValue)
            {
                uValue = int.MaxValue;
            }

            int value = label == null ?
                EditorGUILayout.IntSlider((int)uValue, Math.Max(0, (int)attribute.Min), (int)attribute.Max) :
                EditorGUILayout.IntSlider(label, (int)uValue, Math.Max(0, (int)attribute.Min), (int)attribute.Max);

            if (value < 0)
            {
                value = 0;
            }

            entry.SmartValue = (uint)value;
        }
    }

    /// <summary>
    /// Draws ulong properties marked with <see cref="PropertyRangeAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyRangeAttribute"/>
    /// <seealso cref="MinValueAttribute"/>
    /// <seealso cref="MaxValueAttribute"/>
    /// <seealso cref="MinMaxSliderAttribute"/>
    /// <seealso cref="DelayedAttribute"/>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    public sealed class PropertyRangeAttributeUInt64Drawer : OdinAttributeDrawer<PropertyRangeAttribute, ulong>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<ulong> entry, PropertyRangeAttribute attribute, GUIContent label)
        {
            ulong uValue = entry.SmartValue;

            if (uValue > int.MaxValue)
            {
                uValue = int.MaxValue;
            }

            int value = label == null ?
                EditorGUILayout.IntSlider((int)uValue, Math.Max(0, (int)attribute.Min), (int)attribute.Max) :
                EditorGUILayout.IntSlider(label, (int)uValue, Math.Max(0, (int)attribute.Min), (int)attribute.Max);

            if (value < 0)
            {
                value = 0;
            }

            entry.SmartValue = (ulong)value;
        }
    }
}
#endif