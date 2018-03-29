//-----------------------------------------------------------------------
// <copyright file="UnityReferenceResolver.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Serialization
{
    using System;
    using System.Collections.Generic;
    using Utilities;

    /// <summary>
    /// Resolves external index references to Unity objects.
    /// </summary>
    /// <seealso cref="Sirenix.Serialization.IExternalIndexReferenceResolver" />
    /// <seealso cref="Sirenix.Serialization.ICacheNotificationReceiver" />
    public sealed class UnityReferenceResolver : IExternalIndexReferenceResolver, ICacheNotificationReceiver
    {
        private Dictionary<UnityEngine.Object, int> referenceIndexMapping = new Dictionary<UnityEngine.Object, int>(32);
        private List<UnityEngine.Object> referencedUnityObjects;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityReferenceResolver"/> class.
        /// </summary>
        public UnityReferenceResolver()
        {
            this.referencedUnityObjects = new List<UnityEngine.Object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityReferenceResolver"/> class with a list of Unity objects.
        /// </summary>
        /// <param name="referencedUnityObjects">The referenced Unity objects.</param>
        public UnityReferenceResolver(List<UnityEngine.Object> referencedUnityObjects)
        {
            this.SetReferencedUnityObjects(referencedUnityObjects);
        }

        /// <summary>
        /// Gets the currently referenced Unity objects.
        /// </summary>
        /// <returns>A list of the currently referenced Unity objects.</returns>
        public List<UnityEngine.Object> GetReferencedUnityObjects()
        {
            return this.referencedUnityObjects;
        }

        /// <summary>
        /// Sets the referenced Unity objects of the resolver to a given list, or a new list if the value is null.
        /// </summary>
        /// <param name="referencedUnityObjects">The referenced Unity objects to set, or null if a new list is required.</param>
        public void SetReferencedUnityObjects(List<UnityEngine.Object> referencedUnityObjects)
        {
            if (referencedUnityObjects == null)
            {
                referencedUnityObjects = new List<UnityEngine.Object>();
            }

            this.referencedUnityObjects = referencedUnityObjects;
            this.referenceIndexMapping.Clear();

            for (int i = 0; i < this.referencedUnityObjects.Count; i++)
            {
                if (object.ReferenceEquals(this.referencedUnityObjects[i], null) == false)
                {
                    if (!this.referenceIndexMapping.ContainsKey(this.referencedUnityObjects[i]))
                    {
                        this.referenceIndexMapping.Add(this.referencedUnityObjects[i], i);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the specified value can be referenced externally via this resolver.
        /// </summary>
        /// <param name="value">The value to reference.</param>
        /// <param name="index">The index of the resolved value, if it can be referenced.</param>
        /// <returns>
        ///   <c>true</c> if the reference can be resolved, otherwise <c>false</c>.
        /// </returns>
        public bool CanReference(object value, out int index)
        {
            if (this.referencedUnityObjects == null)
            {
                this.referencedUnityObjects = new List<UnityEngine.Object>(32);
            }

            var obj = value as UnityEngine.Object;

            if (object.ReferenceEquals(null, obj) == false)
            {
                if (this.referenceIndexMapping.TryGetValue(obj, out index) == false)
                {
                    index = this.referencedUnityObjects.Count;
                    this.referenceIndexMapping.Add(obj, index);
                    this.referencedUnityObjects.Add(obj);
                }

                return true;
            }

            index = -1;
            return false;
        }

        /// <summary>
        /// Tries to resolve the given reference index to a reference value.
        /// </summary>
        /// <param name="index">The index to resolve.</param>
        /// <param name="value">The resolved value.</param>
        /// <returns>
        ///   <c>true</c> if the index could be resolved to a value, otherwise <c>false</c>.
        /// </returns>
        public bool TryResolveReference(int index, out object value)
        {
            if (this.referencedUnityObjects == null || index < 0 || index >= this.referencedUnityObjects.Count)
            {
                value = null;
                return false;
            }

            value = this.referencedUnityObjects[index];
            return true;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            this.referencedUnityObjects = null;
            this.referenceIndexMapping.Clear();
        }

        void ICacheNotificationReceiver.OnFreed()
        {
            this.Reset();
        }

        void ICacheNotificationReceiver.OnClaimed()
        {
        }
    }
}