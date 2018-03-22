#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="DelayedAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor.Drawers
{
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws char properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedAttributeCharDrawer : OdinAttributeDrawer<DelayedAttribute, char>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<char> entry, DelayedAttribute attribute, GUIContent label)
        {
			EditorGUI.BeginChangeCheck();
			string s = new string(entry.SmartValue, 1);
			s = SirenixEditorFields.DelayedTextField(label, s);

			if (EditorGUI.EndChangeCheck() && s.Length > 0)
			{
				entry.SmartValue = s[0];
			}
        }
    }

    /// <summary>
    /// Draws string properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedAttributeStringDrawer : OdinAttributeDrawer<DelayedAttribute, string>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<string> entry, DelayedAttribute attribute, GUIContent label)
        {
			entry.SmartValue = SirenixEditorFields.DelayedTextField(label, entry.SmartValue);
        }
    }

    /// <summary>
    /// Draws sbyte properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedAttributeSByteDrawer : OdinAttributeDrawer<DelayedAttribute, sbyte>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<sbyte> entry, DelayedAttribute attribute, GUIContent label)
        {
            int value = SirenixEditorFields.DelayedIntField(label, entry.SmartValue);

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
    /// Draws byte properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedAttributeByteDrawer : OdinAttributeDrawer<DelayedAttribute, byte>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<byte> entry, DelayedAttribute attribute, GUIContent label)
        {
            int value = SirenixEditorFields.DelayedIntField(label, entry.SmartValue);

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
    /// Draws short properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedAttributeInt16Drawer : OdinAttributeDrawer<DelayedAttribute, short>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<short> entry, DelayedAttribute attribute, GUIContent label)
        {
            int value = SirenixEditorFields.DelayedIntField(label, entry.SmartValue);

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
    /// Draws ushort properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedAttributeUInt16Drawer : OdinAttributeDrawer<DelayedAttribute, ushort>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<ushort> entry, DelayedAttribute attribute, GUIContent label)
        {
            int value = SirenixEditorFields.DelayedIntField(label, entry.SmartValue);

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
    /// Draws int properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedAttributeInt32Drawer : OdinAttributeDrawer<DelayedAttribute, int>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<int> entry, DelayedAttribute attribute, GUIContent label)
        {
            entry.SmartValue = SirenixEditorFields.DelayedIntField(label, entry.SmartValue);
        }
    }

    /// <summary>
    /// Draws uint properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedAttributeUInt32Drawer : OdinAttributeDrawer<DelayedAttribute, uint>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<uint> entry, DelayedAttribute attribute, GUIContent label)
        {
			long value = SirenixEditorFields.DelayedLongField(label, entry.SmartValue);

			if (value < uint.MinValue)
			{
				value = uint.MinValue;
			}
			else if (value > uint.MaxValue)
			{
				value = uint.MaxValue;
			}

			entry.SmartValue = (uint)value;
        }
    }

    /// <summary>
    /// Draws long properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedAttributeInt64Drawer : OdinAttributeDrawer<DelayedAttribute, long>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<long> entry, DelayedAttribute attribute, GUIContent label)
        {
            entry.SmartValue = SirenixEditorFields.DelayedLongField(label, entry.SmartValue);
        }
    }

    /// <summary>
    /// Draws ulong properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedAttributeUInt64Drawer : OdinAttributeDrawer<DelayedAttribute, ulong>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<ulong> entry, DelayedAttribute attribute, GUIContent label)
        {
            ulong value = entry.SmartValue;
            string str = value.ToString();

            str = label == null ?
                EditorGUILayout.DelayedTextField(str) :
                EditorGUILayout.DelayedTextField(label, str);

            if (GUI.changed && ulong.TryParse(str, out value))
            {
                entry.SmartValue = value;
            }
        }
    }

    /// <summary>
    /// Draws float properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedAttributeFloatDrawer : OdinAttributeDrawer<DelayedAttribute, float>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<float> entry, DelayedAttribute attribute, GUIContent label)
        {
            entry.SmartValue = SirenixEditorFields.DelayedFloatField(label, entry.SmartValue);
        }
    }

    /// <summary>
    /// Draws double properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedAttributeDoubleDrawer : OdinAttributeDrawer<DelayedAttribute, double>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<double> entry, DelayedAttribute attribute, GUIContent label)
        {
            entry.SmartValue = SirenixEditorFields.DelayedDoubleField(label, entry.SmartValue);
        }
    }

    /// <summary>
    /// Draws decimal properties marked with <see cref="DelayedAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedAttributeDecimalDrawer : OdinAttributeDrawer<DelayedAttribute, decimal>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<decimal> entry, DelayedAttribute attribute, GUIContent label)
        {
            decimal value = entry.SmartValue;
            string str = value.ToString();

            str = label == null ?
                EditorGUILayout.DelayedTextField(str) :
                EditorGUILayout.DelayedTextField(label, str);

            if (GUI.changed && decimal.TryParse(str, out value))
            {
                entry.SmartValue = value;
            }
        }
    }
}
#endif