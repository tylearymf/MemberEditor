#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="WrapAttributeDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using Sirenix.Utilities;
    using Sirenix.OdinInspector;
    using UnityEngine;

    /// <summary>
    /// Draws short properties marked with <see cref="WrapAttribute"/>.
    /// </summary>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3, 0, 0)]
    public class WrapAttributeInt16Drawer : OdinAttributeDrawer<WrapAttribute, short>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<short> entry, WrapAttribute attribute, UnityEngine.GUIContent label)
        {
            this.CallNextDrawer(entry, label);
            entry.SmartValue = (short)MathUtilities.Wrap(entry.SmartValue, attribute.Min, attribute.Max);
        }
    }

    /// <summary>
    /// Draws int properties marked with <see cref="WrapAttribute"/>.
    /// </summary>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3, 0, 0)]
    public class WrapAttributeInt32Drawer : OdinAttributeDrawer<WrapAttribute, int>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<int> entry, WrapAttribute attribute, UnityEngine.GUIContent label)
        {
            this.CallNextDrawer(entry, label);
            entry.SmartValue = (int)MathUtilities.Wrap(entry.SmartValue, attribute.Min, attribute.Max);
        }
    }

    /// <summary>
    /// Draws long properties marked with <see cref="WrapAttribute"/>.
    /// </summary>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3, 0, 0)]
    public class WrapAttributeInt64Drawer : OdinAttributeDrawer<WrapAttribute, long>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<long> entry, WrapAttribute attribute, UnityEngine.GUIContent label)
        {
            this.CallNextDrawer(entry, label);
            entry.SmartValue = (long)MathUtilities.Wrap(entry.SmartValue, attribute.Min, attribute.Max);
        }
    }

    /// <summary>
    /// Draws float properties marked with <see cref="WrapAttribute"/>.
    /// </summary>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3, 0, 0)]
    public class WrapAttributeFloatDrawer : OdinAttributeDrawer<WrapAttribute, float>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<float> entry, WrapAttribute attribute, UnityEngine.GUIContent label)
        {
            this.CallNextDrawer(entry, label);
            entry.SmartValue = (float)MathUtilities.Wrap(entry.SmartValue, attribute.Min, attribute.Max);
        }
    }

    /// <summary>
    /// Draws double properties marked with <see cref="WrapAttribute"/>.
    /// </summary>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3, 0, 0)]
    public class WrapAttributeDoubleDrawer : OdinAttributeDrawer<WrapAttribute, double>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<double> entry, WrapAttribute attribute, UnityEngine.GUIContent label)
        {
            this.CallNextDrawer(entry, label);
            entry.SmartValue = (double)MathUtilities.Wrap(entry.SmartValue, attribute.Min, attribute.Max);
        }
    }

    /// <summary>
    /// Draws decimal properties marked with <see cref="WrapAttribute"/>.
    /// </summary>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3, 0, 0)]
    public class WrapAttributeDecimalDrawer : OdinAttributeDrawer<WrapAttribute, decimal>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<decimal> entry, WrapAttribute attribute, UnityEngine.GUIContent label)
        {
            this.CallNextDrawer(entry, label);
            entry.SmartValue = (decimal)MathUtilities.Wrap((double)entry.SmartValue, attribute.Min, attribute.Max);
        }
    }

    /// <summary>
    /// Draws Vector2 properties marked with <see cref="WrapAttribute"/>.
    /// </summary>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3, 0, 0)]
    public class WrapAttributeVector2Drawer : OdinAttributeDrawer<WrapAttribute, Vector2>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<Vector2> entry, WrapAttribute attribute, GUIContent label)
        {
            this.CallNextDrawer(entry, label);
            entry.SmartValue = new Vector2(
                MathUtilities.Wrap(entry.SmartValue.x, (float)attribute.Min, (float)attribute.Max),
                MathUtilities.Wrap(entry.SmartValue.y, (float)attribute.Min, (float)attribute.Max));
        }
    }

    /// <summary>
    /// Draws Vector3 properties marked with <see cref="WrapAttribute"/>.
    /// </summary>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3, 0, 0)]
    public class WrapAttributeVector3Drawer : OdinAttributeDrawer<WrapAttribute, Vector3>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<Vector3> entry, WrapAttribute attribute, GUIContent label)
        {
            this.CallNextDrawer(entry, label);
            entry.SmartValue = new Vector3(
                MathUtilities.Wrap(entry.SmartValue.x, (float)attribute.Min, (float)attribute.Max),
                MathUtilities.Wrap(entry.SmartValue.y, (float)attribute.Min, (float)attribute.Max),
                MathUtilities.Wrap(entry.SmartValue.z, (float)attribute.Min, (float)attribute.Max));
        }
    }

    /// <summary>
    /// Draws Vector4 properties marked with <see cref="WrapAttribute"/>.
    /// </summary>
    /// <seealso cref="WrapAttribute"/>
    [OdinDrawer]
    [DrawerPriority(0.3, 0, 0)]
    public class WrapAttributeVector4Drawer : OdinAttributeDrawer<WrapAttribute, Vector4>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        protected override void DrawPropertyLayout(IPropertyValueEntry<Vector4> entry, WrapAttribute attribute, GUIContent label)
        {
            this.CallNextDrawer(entry, label);
            entry.SmartValue = new Vector4(
                MathUtilities.Wrap(entry.SmartValue.x, (float)attribute.Min, (float)attribute.Max),
                MathUtilities.Wrap(entry.SmartValue.y, (float)attribute.Min, (float)attribute.Max),
                MathUtilities.Wrap(entry.SmartValue.z, (float)attribute.Min, (float)attribute.Max),
                MathUtilities.Wrap(entry.SmartValue.w, (float)attribute.Min, (float)attribute.Max));
        }
    }

    // byte
    //[OdinDrawer]
    //[DrawerPriority(0.3, 0, 0)]
    //public class WrapAttributeByteDrawer : InspectorAttributeDrawer<WrapAttribute, byte>
    //{
    //    protected override void DrawPropertyLayout(PropertyValueEntry<byte> entry, WrapAttribute attribute, UnityEngine.GUIContent label, params UnityEngine.GUILayoutOption[] options)
    //    {
    //      this.CallNextDrawer(entry, label);
    //		Rect position = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
    //		double v = MathUtilities.Wrap(label != null ? EditorGUI.FloatField(position, label, entry.SmartValue) : EditorGUI.FloatField(position, entry.SmartValue), attribute.Min, attribute.Max);
    //		entry.SmartValue = (byte)
    //			(v <= byte.MinValue ? (byte.MaxValue < attribute.Max ? byte.MaxValue : attribute.Max) :
    //			(v >= byte.MaxValue ? (byte.MinValue > attribute.Min ? byte.MinValue : attribute.Min) : v));
    //	}
    //}

    // sbyte
    //[OdinDrawer]
    //[DrawerPriority(0.3, 0, 0)]
    //public class WrapAttributeSbyteDrawer : InspectorAttributeDrawer<WrapAttribute, sbyte>
    //{
    //    protected override void DrawPropertyLayout(PropertyValueEntry<sbyte> entry, WrapAttribute attribute, UnityEngine.GUIContent label, params UnityEngine.GUILayoutOption[] options)
    //    {
    //      this.CallNextDrawer(entry, label);
    //		entry.SmartValue = (sbyte)MathUtilities.Wrap(entry.SmartValue, attribute.Min, attribute.Max);
    //    }
    //}

    // uint16
    //[OdinDrawer]
    //[DrawerPriority(0.3, 0, 0)]
    //public class WrapAttributeUInt16Drawer : InspectorAttributeDrawer<WrapAttribute, ushort>
    //{
    //    protected override void DrawPropertyLayout(PropertyValueEntry<ushort> entry, WrapAttribute attribute, UnityEngine.GUIContent label, params UnityEngine.GUILayoutOption[] options)
    //    {
    //      this.CallNextDrawer(entry, label);
    //		Rect position = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
    //		double v = MathUtilities.Wrap(label != null ? EditorGUI.FloatField(position, label, entry.SmartValue) : EditorGUI.FloatField(position, entry.SmartValue), attribute.Min, attribute.Max);
    //		entry.SmartValue = (ushort)
    //			(v <= ushort.MinValue ? (ushort.MaxValue < attribute.Max ? ushort.MaxValue : attribute.Max) :
    //			(v >= ushort.MaxValue ? (ushort.MinValue > attribute.Min ? ushort.MinValue : attribute.Min) : v));
    //    }
    //}

    // uint32
    //[OdinDrawer]
    //[DrawerPriority(0.3, 0, 0)]
    //public class WrapAttributeUInt32Drawer : InspectorAttributeDrawer<WrapAttribute, uint>
    //{
    //    protected override void DrawPropertyLayout(PropertyValueEntry<uint> entry, WrapAttribute attribute, UnityEngine.GUIContent label, params UnityEngine.GUILayoutOption[] options)
    //    {
    //      this.CallNextDrawer(entry, label);
    //		Rect position = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
    //		double v = MathUtilities.Wrap(label != null ? EditorGUI.FloatField(position, label, entry.SmartValue) : EditorGUI.FloatField(position, entry.SmartValue), attribute.Min, attribute.Max);
    //		entry.SmartValue = (uint)
    //			(v <= uint.MinValue ? (uint.MaxValue < attribute.Max ? uint.MaxValue : attribute.Max) :
    //			(v >= uint.MaxValue ? (uint.MinValue > attribute.Min ? uint.MinValue : attribute.Min) : v));
    //    }
    //}

    // uint64
    //[OdinDrawer]
    //[DrawerPriority(0.3, 0, 0)]
    //public class WrapAttributeUInt64Drawer : InspectorAttributeDrawer<WrapAttribute, ulong>
    //{
    //    protected override void DrawPropertyLayout(PropertyValueEntry<ulong> entry, WrapAttribute attribute, UnityEngine.GUIContent label, params UnityEngine.GUILayoutOption[] options)
    //    {
    //      this.CallNextDrawer(entry, label);
    //		Rect position = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
    //		double v = MathUtilities.Wrap(label != null ? EditorGUI.FloatField(position, label, entry.SmartValue) : EditorGUI.FloatField(position, entry.SmartValue), attribute.Min, attribute.Max);
    //		entry.SmartValue = (ulong)
    //			(v <= ulong.MinValue ? (ulong.MaxValue < attribute.Max ? ulong.MaxValue : attribute.Max) :
    //			(v >= ulong.MaxValue ? (ulong.MinValue > attribute.Min ? ulong.MinValue : attribute.Min)  : v));
    //    }
    //}
}
#endif