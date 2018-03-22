//-----------------------------------------------------------------------
// <copyright file="SerializationData.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Serialization
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Utilities;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    [Serializable]
    public struct SerializationData
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        [SerializeField, HideInInspector]
        public DataFormat SerializedFormat;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        [SerializeField, HideInInspector]
        public byte[] SerializedBytes;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        [SerializeField, HideInInspector]
        public List<UnityEngine.Object> ReferencedUnityObjects;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public const string PrefabModificationsReferencedUnityObjectsFieldName = "PrefabModificationsReferencedUnityObjects";

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public const string PrefabModificationsFieldName = "PrefabModifications";

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public const string PrefabFieldName = "Prefab";

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool HasEditorData
        {
            get
            {
                switch (this.SerializedFormat)
                {
                    case DataFormat.Binary:
                    case DataFormat.JSON:
                        return !(this.SerializedBytesString.IsNullOrWhitespace() && this.SerializedBytes.IsNullOrEmpty());

                    case DataFormat.Nodes:
                        return !this.SerializationNodes.IsNullOrEmpty();

                    default:
                        throw new NotImplementedException(this.SerializedFormat.ToString());
                }
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        [SerializeField, HideInInspector]
        public string SerializedBytesString;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        [SerializeField, HideInInspector]
        public UnityEngine.Object Prefab;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        [SerializeField, HideInInspector]
        public List<UnityEngine.Object> PrefabModificationsReferencedUnityObjects;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        [SerializeField, HideInInspector]
        public List<string> PrefabModifications;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        [SerializeField, HideInInspector]
        public List<SerializationNode> SerializationNodes;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void Reset()
        {
            this.SerializedFormat = DataFormat.Binary;

            if (this.SerializedBytes != null && this.SerializedBytes.Length > 0)
            {
                this.SerializedBytes = new byte[0];
            }

            if (this.ReferencedUnityObjects != null && this.ReferencedUnityObjects.Count > 0)
            {
                this.ReferencedUnityObjects.Clear();
            }

            this.Prefab = null;

            if (this.SerializationNodes != null && this.SerializationNodes.Count > 0)
            {
                this.SerializationNodes.Clear();
            }

            if (this.SerializedBytesString != null && this.SerializedBytesString.Length > 0)
            {
                this.SerializedBytesString = string.Empty;
            }

            if (this.PrefabModificationsReferencedUnityObjects != null && this.PrefabModificationsReferencedUnityObjects.Count > 0)
            {
                this.PrefabModificationsReferencedUnityObjects.Clear();
            }

            if (this.PrefabModifications != null && this.PrefabModifications.Count > 0)
            {
                this.PrefabModifications.Clear();
            }
        }
    }
}