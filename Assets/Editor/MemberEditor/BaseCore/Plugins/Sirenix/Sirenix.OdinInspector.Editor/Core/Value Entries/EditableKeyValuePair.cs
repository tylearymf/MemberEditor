#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PropertyStrongListElementValueEntry.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using Sirenix.Serialization;
    using System;
    using UnityEngine;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public struct EditableKeyValuePair<TKey, TValue> : IEquatable<EditableKeyValuePair<TKey, TValue>>
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        [Space(2)]
        [OdinSerialize, Delayed, DisableContextMenu, ShowInInspector, SuppressInvalidAttributeError]
        public TKey Key;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        [OdinSerialize, ShowInInspector]
        public TValue Value;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public EditableKeyValuePair(TKey key, TValue value)
        {
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool Equals(EditableKeyValuePair<TKey, TValue> other)
        {
            // We consider these to be equal if only the key is equal
            return PropertyValueEntry<TKey>.EqualityComparer(this.Key, other.Key);
        }
    }
}
#endif