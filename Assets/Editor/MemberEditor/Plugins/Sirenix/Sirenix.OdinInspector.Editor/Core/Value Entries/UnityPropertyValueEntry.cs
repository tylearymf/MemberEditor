#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="UnityPropertyValueEntry.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Represents the values of an <see cref="InspectorProperty"/> for a Unity-only property value, and contains utilities for querying the values' type and getting and setting them.
    /// </summary>
    public sealed class UnityPropertyValueEntry<TParent, TValue> : PropertyValueEntry<TParent, TValue>
    {
        private static readonly Func<SerializedProperty, TValue> ValueGetter = SerializedPropertyUtilities.GetValueGetter<TValue>();
        private static readonly Action<SerializedProperty, TValue> ValueSetter = SerializedPropertyUtilities.GetValueSetter<TValue>();

        private UnityPropertyValueEntry()
        {
        }

        /// <summary>
        /// The value category of this value entry.
        /// </summary>
        public override PropertyValueCategory ValueCategory { get { return PropertyValueCategory.Member; } }

        /// <summary>
        /// Gets the actual boxed value of the tree target.
        /// </summary>
        protected override object GetActualBoxedValue(TParent parent)
        {
            return this.GetActualValue(parent);
        }

        /// <summary>
        /// Gets the actual value of the tree target.
        /// </summary>
        protected override TValue GetActualValue(TParent parent)
        {
            if (ValueGetter == null || ValueSetter == null)
            {
                Debug.LogError("Can't get a value of type " + typeof(TValue).GetNiceName() + " directly from a Unity property.");
                return default(TValue);
            }

            var unityProp = this.Property.Tree.GetUnityPropertyForPath(this.Property.UnityPropertyPath);

            if (unityProp == null || unityProp.serializedObject.targetObject is EmittedScriptableObject)
            {
                Debug.LogError("Could not get Unity property at path " + this.Property.UnityPropertyPath);
                return default(TValue);
            }

            return ValueGetter(unityProp);
        }

        /// <summary>
        /// Sets the actual target tree value.
        /// </summary>
        protected override void SetActualBoxedValueImplementation(int index, object value)
        {
            this.SetActualValueImplementation(index, (TValue)value);
        }

        /// <summary>
        /// Sets the actual target tree value.
        /// </summary>
        protected override void SetActualValueImplementation(int index, TValue value)
        {
            if (ValueGetter == null || ValueSetter == null)
            {
                Debug.LogError("Can't set a value of type " + typeof(TValue).GetNiceName() + " directly to a Unity property.");
                return;
            }

            var unityProp = this.Property.Tree.GetUnityPropertyForPath(this.Property.UnityPropertyPath);

            if (unityProp == null || unityProp.serializedObject.targetObject is EmittedScriptableObject)
            {
                Debug.LogError("Could not get Unity property at path " + this.Property.UnityPropertyPath);
            }

            ValueSetter(unityProp, value);
        }
    }
}
#endif