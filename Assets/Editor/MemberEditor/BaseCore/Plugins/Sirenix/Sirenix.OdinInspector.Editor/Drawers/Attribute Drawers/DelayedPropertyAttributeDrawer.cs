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
    /// Draws char properties marked with <see cref="DelayedPropertyAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedPropertyAttributeCharDrawer : OdinAttributeDrawer<DelayedPropertyAttribute, char>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<char> entry, DelayedPropertyAttribute attribute, GUIContent label)
        {
            entry.SmartValue = SirenixEditorGUI.DynamicPrimitiveField(label, entry.SmartValue);
        }
    }

    /// <summary>
    /// Draws string properties marked with <see cref="DelayedPropertyAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedPropertyAttributeStringDrawer : OdinAttributeDrawer<DelayedPropertyAttribute, string>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<string> entry, DelayedPropertyAttribute attribute, GUIContent label)
        {
            entry.SmartValue = label == null ?
                EditorGUILayout.DelayedTextField(entry.SmartValue) :
                EditorGUILayout.DelayedTextField(label, entry.SmartValue);
        }
    }

    /// <summary>
    /// Draws sbyte properties marked with <see cref="DelayedPropertyAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedPropertyAttributeSByteDrawer : OdinAttributeDrawer<DelayedPropertyAttribute, sbyte>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<sbyte> entry, DelayedPropertyAttribute attribute, GUIContent label)
        {
            int value = label == null ?
                EditorGUILayout.DelayedIntField(entry.SmartValue) :
                EditorGUILayout.DelayedIntField(label, entry.SmartValue);

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
    /// Draws byte properties marked with <see cref="DelayedPropertyAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedPropertyAttributeByteDrawer : OdinAttributeDrawer<DelayedPropertyAttribute, byte>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<byte> entry, DelayedPropertyAttribute attribute, GUIContent label)
        {
            int value = label == null ?
                EditorGUILayout.DelayedIntField(entry.SmartValue) :
                EditorGUILayout.DelayedIntField(label, entry.SmartValue);

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
    /// Draws short properties marked with <see cref="DelayedPropertyAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedPropertyAttributeInt16Drawer : OdinAttributeDrawer<DelayedPropertyAttribute, short>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<short> entry, DelayedPropertyAttribute attribute, GUIContent label)
        {
            int value = label == null ?
                EditorGUILayout.DelayedIntField(entry.SmartValue) :
                EditorGUILayout.DelayedIntField(label, entry.SmartValue);

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
    /// Draws ushort properties marked with <see cref="DelayedPropertyAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedPropertyAttributeUInt16Drawer : OdinAttributeDrawer<DelayedPropertyAttribute, ushort>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<ushort> entry, DelayedPropertyAttribute attribute, GUIContent label)
        {
            int value = label == null ?
                EditorGUILayout.DelayedIntField(entry.SmartValue) :
                EditorGUILayout.DelayedIntField(label, entry.SmartValue);

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
    /// Draws int properties marked with <see cref="DelayedPropertyAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedPropertyAttributeInt32Drawer : OdinAttributeDrawer<DelayedPropertyAttribute, int>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<int> entry, DelayedPropertyAttribute attribute, GUIContent label)
        {
            entry.SmartValue = label == null ?
                EditorGUILayout.DelayedIntField(entry.SmartValue) :
                EditorGUILayout.DelayedIntField(label, entry.SmartValue);
        }
    }

    /// <summary>
    /// Draws uint properties marked with <see cref="DelayedPropertyAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedPropertyAttributeUInt32Drawer : OdinAttributeDrawer<DelayedPropertyAttribute, uint>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<uint> entry, DelayedPropertyAttribute attribute, GUIContent label)
        {
            uint value = entry.SmartValue;
            string str = value.ToString();

            str = label == null ?
                EditorGUILayout.DelayedTextField(str) :
                EditorGUILayout.DelayedTextField(label, str);

            if (GUI.changed && uint.TryParse(str, out value))
            {
                entry.SmartValue = value;
            }
        }
    }

    /// <summary>
    /// Draws long properties marked with <see cref="DelayedPropertyAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedPropertyAttributeInt64Drawer : OdinAttributeDrawer<DelayedPropertyAttribute, long>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<long> entry, DelayedPropertyAttribute attribute, GUIContent label)
        {
            long value = entry.SmartValue;
            string str = value.ToString();

            str = label == null ?
                EditorGUILayout.DelayedTextField(str) :
                EditorGUILayout.DelayedTextField(label, str);

            if (GUI.changed && long.TryParse(str, out value))
            {
                entry.SmartValue = value;
            }
        }
    }

    /// <summary>
    /// Draws ulong properties marked with <see cref="DelayedPropertyAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedPropertyAttributeUInt64Drawer : OdinAttributeDrawer<DelayedPropertyAttribute, ulong>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<ulong> entry, DelayedPropertyAttribute attribute, GUIContent label)
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
    /// Draws float properties marked with <see cref="DelayedPropertyAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedPropertyAttributeFloatDrawer : OdinAttributeDrawer<DelayedPropertyAttribute, float>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<float> entry, DelayedPropertyAttribute attribute, GUIContent label)
        {
            entry.SmartValue = label == null ?
                EditorGUILayout.DelayedFloatField(entry.SmartValue) :
                EditorGUILayout.DelayedFloatField(label, entry.SmartValue);
        }
    }

    /// <summary>
    /// Draws double properties marked with <see cref="DelayedPropertyAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedPropertyAttributeDoubleDrawer : OdinAttributeDrawer<DelayedPropertyAttribute, double>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<double> entry, DelayedPropertyAttribute attribute, GUIContent label)
        {
            double value = entry.SmartValue;
            string str = value.ToString();

            str = label == null ?
                EditorGUILayout.DelayedTextField(str) :
                EditorGUILayout.DelayedTextField(label, str);

            if (GUI.changed && double.TryParse(str, out value))
            {
                entry.SmartValue = value;
            }
        }
    }

    /// <summary>
    /// Draws decimal properties marked with <see cref="DelayedPropertyAttribute"/>.
    /// </summary>
    [OdinDrawer]
    public sealed class DelayedPropertyAttributeDecimalDrawer : OdinAttributeDrawer<DelayedPropertyAttribute, decimal>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<decimal> entry, DelayedPropertyAttribute attribute, GUIContent label)
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