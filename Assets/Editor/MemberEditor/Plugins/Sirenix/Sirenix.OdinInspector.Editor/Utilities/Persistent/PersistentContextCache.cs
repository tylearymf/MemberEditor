#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="PersistentContextCache.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector.Editor
{
    using UnityEngine;
    using UnityEditor;
    using Sirenix.Serialization;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Sirenix.OdinInspector;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System.Collections;
    using System.Linq;

    /// <summary>
    /// Persistent Context cache object.
    /// </summary>
    [InitializeOnLoad]
    [GlobalConfig(UseAsset = false)]
    public class PersistentContextCache : GlobalConfig<PersistentContextCache>
    {
        private static readonly string tempCacheFilename = "PersistentContextCache.cache";

        private const int defaultApproximateSizePerEntry = 150;

        private static bool configsLoaded = false;

        private static bool internalEnableCaching;

        private static int internalMaxCacheByteSize;

        private static bool internalWriteToFile;

        static PersistentContextCache()
        {
            EditorApplication.update -= UpdateCallback;
            EditorApplication.update += UpdateCallback;

            UnityEditorEventUtility.DelayAction(() => Instance.LoadCache());
        }

        private static string FormatSize(int size)
        {
            return
                size > 1000000 ? ((size / 1000000).ToString() + " MB") :
                size > 1000 ? ((size / 1000).ToString() + " kB") :
                (size.ToString() + " bytes");
        }

        private static void LoadConfigs()
        {
            if (!configsLoaded)
            {
                internalEnableCaching = EditorPrefs.GetBool("PersistentContextCache.EnableCaching", true);
                internalMaxCacheByteSize = EditorPrefs.GetInt("PersistentContextCache.MaxCacheByteSize", 1000000);
                internalWriteToFile = EditorPrefs.GetBool("PersistentContextCache.WriteToFile", true);
                configsLoaded = true;
            }
        }

        private static void UpdateCallback()
        {
            CachePurger.Run();
        }

        private int approximateSizePerEntry;

        [NonSerialized]
        private IndexedDictionary<IContextKey, GlobalPersistentContext> cache = new IndexedDictionary<IContextKey, GlobalPersistentContext>();

        /// <summary>
        /// Estimated cache size in bytes.
        /// </summary>
        public int CacheSize { get { return (this.approximateSizePerEntry > 0 ? this.approximateSizePerEntry : defaultApproximateSizePerEntry) * this.EntryCount; } }

        /// <summary>
        /// The current number of context entries in the cache.
        /// </summary>
        public int EntryCount { get { return this.cache.Count; } }

        /// <summary>
        /// If <c>true</c> then persistent context is disabled entirely.
        /// </summary>
        [ShowInInspector]
        public bool EnableCaching
        {
            get
            {
                LoadConfigs();
                return internalEnableCaching;
            }
            set
            {
                internalEnableCaching = value;
                EditorPrefs.SetBool("PersistentContextCache.EnableCaching", value);
            }
        }

        /// <summary>
        /// If <c>true</c> the context will be saved to a file in the temp directory.
        /// </summary>
        [ShowInInspector]
        [EnableIf("EnableCaching")]
        public bool WriteToFile
        {
            get
            {
                LoadConfigs();
                return internalWriteToFile;
            }
            set
            {
                internalWriteToFile = value;
                EditorPrefs.SetBool("PersistentContextCache.WriteToFile", value);
            }
        }

        /// <summary>
        /// The max size of the cache in bytes.
        /// </summary>
        [ShowInInspector]
        [EnableIf("EnableCaching")]
        [CustomValueDrawer("DrawCacheSize")]
        [SuffixLabel("KB", Overlay = true)]
        public int MaxCacheByteSize
        {
            get
            {
                LoadConfigs();
                return internalMaxCacheByteSize;
            }
            private set
            {
                internalMaxCacheByteSize = value;
                EditorPrefs.SetInt("PersistentContextCache.MaxCacheByteSize", value);
            }
        }

        [ShowInInspector]
        [FilePath, ReadOnly]
        private string CacheFileLocation
        {
            get { return Path.Combine(SirenixAssetPaths.OdinTempPath, tempCacheFilename).Replace("\\", "/");  }
            set { }
        }

        [ShowInInspector]
        [ProgressBar(0, 100), SuffixLabel("$CurrentCacheSizeSuffix", Overlay = true), ReadOnly]
        private int CurrentCacheSize
        {
            get
            {
                LoadConfigs();
                return (int)((float)this.CacheSize / (float)this.MaxCacheByteSize * 100f);
            }
        }

        private string CurrentCacheSizeSuffix
        {
            get { return StringUtilities.NicifyByteSize(this.CacheSize, 1) + " / " + StringUtilities.NicifyByteSize(this.MaxCacheByteSize, 1); }
        }

        internal GlobalPersistentContext<TValue> GetContext<TKey1, TValue>(TKey1 alpha, out bool isNew)
        {
            IContextKey key = new ContextKey<TKey1>(alpha);
            return this.TryGetContext<TValue>(key, out isNew);
        }

        internal GlobalPersistentContext<TValue> GetContext<TKey1, TKey2, TValue>(TKey1 alpha, TKey2 beta, out bool isNew)
        {
            IContextKey key = new ContextKey<TKey1, TKey2>(alpha, beta);
            return this.TryGetContext<TValue>(key, out isNew);
        }

        internal GlobalPersistentContext<TValue> GetContext<TKey1, TKey2, TKey3, TValue>(TKey1 alpha, TKey2 beta, TKey3 gamma, out bool isNew)
        {
            IContextKey key = new ContextKey<TKey1, TKey2, TKey3>(alpha, beta, gamma);
            return this.TryGetContext<TValue>(key, out isNew);
        }

        internal GlobalPersistentContext<TValue> GetContext<TKey1, TKey2, TKey3, TKey4, TValue>(TKey1 alpha, TKey2 beta, TKey3 gamma, TKey4 delta, out bool isNew)
        {
            IContextKey key = new ContextKey<TKey1, TKey2, TKey3, TKey4>(alpha, beta, gamma, delta);
            return this.TryGetContext<TValue>(key, out isNew);
        }

        internal GlobalPersistentContext<TValue> GetContext<TKey1, TKey2, TKey3, TKey4, TKey5, TValue>(TKey1 alpha, TKey2 beta, TKey3 gamma, TKey4 delta, TKey5 epsilon, out bool isNew)
        {
            IContextKey key = new ContextKey<TKey1, TKey2, TKey3, TKey4, TKey5>(alpha, beta, gamma, delta, epsilon);
            return this.TryGetContext<TValue>(key, out isNew);
        }

        private int DrawCacheSize(int value, GUIContent label)
        {
            value /= 1000;

            value = SirenixEditorFields.DelayedIntField("Max Cache Size", value);
            value = value < 1 ? 1 : value > 10000 ? 10000 : value;

            return value * 1000;
        }

        private void OnDomainUnload(object sender, EventArgs e)
        {
            this.SaveCache();
        }

        private void OnEnable()
        {
            // This is always called after a reload.
            AppDomain.CurrentDomain.DomainUnload -= this.OnDomainUnload;
            AppDomain.CurrentDomain.DomainUnload += this.OnDomainUnload;
        }

        [Button(ButtonSizes.Medium), ButtonGroup]
        [EnableIf("EnableCaching")]
        private void LoadCache()
        {
            try
            {
                this.approximateSizePerEntry = defaultApproximateSizePerEntry;

                var file = Path.Combine(SirenixAssetPaths.OdinTempPath, tempCacheFilename).Replace("\\", "/");
                FileInfo info = new FileInfo(file);
                
                if (info.Exists)
                {
                    using (FileStream stream = info.OpenRead())
                    {
                        DeserializationContext context = new DeserializationContext();
                        context.Config.DebugContext.LoggingPolicy = LoggingPolicy.Silent; // Shut up...
                        context.Config.DebugContext.ErrorHandlingPolicy = ErrorHandlingPolicy.Resilient; // ...  and do your job!

                        this.cache = SerializationUtility.DeserializeValue<IndexedDictionary<IContextKey, GlobalPersistentContext>>(stream, DataFormat.Binary, new List<UnityEngine.Object>(), context);
                    }

                    if (this.EntryCount > 0)
                    {
                        this.approximateSizePerEntry = (int)(info.Length / this.EntryCount);
                    }
                }
                else
                {
                    this.cache.Clear();
                }
            }
            catch (Exception ex)
            {
                this.cache = new IndexedDictionary<IContextKey, GlobalPersistentContext>();
                Debug.LogError("Exception happened when loading Persistent Context from file.");
                Debug.LogException(ex);
            }
        }

        [Button(ButtonSizes.Medium), ButtonGroup]
        [EnableIf("EnableCaching")]
        private void SaveCache()
        {
            if (this.WriteToFile && this.EnableCaching)
            {
                try
                {
                    this.approximateSizePerEntry = defaultApproximateSizePerEntry;
                    string file = Path.Combine(SirenixAssetPaths.OdinTempPath, tempCacheFilename).Replace("\\", "/");
                    FileInfo info = new FileInfo(file);

                    // If there's no entries, delete the old file and don't make a new one.
                    if (this.cache.Count == 0)
                    {
                        if (info.Exists)
                        {
                            this.DeleteCache();
                        }
                    }
                    else
                    {
                        // Create dictionary
                        if (!Directory.Exists(SirenixAssetPaths.OdinTempPath))
                        {
                            Directory.CreateDirectory(SirenixAssetPaths.OdinTempPath);
                        }

                        using (FileStream stream = info.OpenWrite())
                        {
                            List<UnityEngine.Object> unityReferences;
                            SerializationUtility.SerializeValue(this.cache, stream, DataFormat.Binary, out unityReferences);

                            // Log error if any Unity references were serialized.
                            if (unityReferences != null && unityReferences.Count > 0)
                            {
                                Debug.LogError("Don't use Unity Objects with Persistent Context!!!"); // @Todo change this message.
                            }
                        }

                        // Update size estimate.
                        this.approximateSizePerEntry = (int)(info.Length / this.EntryCount);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Exception happened when saving Persistent Context to file.");
                    Debug.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Delete the persistent cache file.
        /// </summary>
        [Button(ButtonSizes.Medium), ButtonGroup]
        [EnableIf("EnableCaching")]
        public void DeleteCache()
        {
            this.approximateSizePerEntry = defaultApproximateSizePerEntry;
            this.cache.Clear();

            string path = Path.Combine(SirenixAssetPaths.OdinTempPath, tempCacheFilename).Replace("\\", "/");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private GlobalPersistentContext<TValue> TryGetContext<TValue>(IContextKey key, out bool isNew)
        {
            GlobalPersistentContext context;
            if (this.EnableCaching && this.cache.TryGetValue(key, out context) && context is GlobalPersistentContext<TValue>)
            {
                isNew = false;
                return (GlobalPersistentContext<TValue>)context;
            }
            else
            {
                isNew = true;
                GlobalPersistentContext<TValue> c = GlobalPersistentContext<TValue>.Create();
                this.cache[key] = c;

                return c;
            }
        }

        private interface IContextKey
        {
        }

        [Serializable]
        private struct ContextKey<TKey1> : IContextKey, IEquatable<ContextKey<TKey1>>
        {
            public TKey1 Alpha;

            public ContextKey(TKey1 alpha)
            {
                this.Alpha = alpha;
            }

            public bool Equals(ContextKey<TKey1> other)
            {
                return EqualityComparer<TKey1>.Default.Equals(this.Alpha, other.Alpha);
            }

            public override bool Equals(object obj)
            {
                return obj is ContextKey<TKey1> && this.Equals((ContextKey<TKey1>)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return this.Alpha.GetHashCode();
                }
            }

            public override string ToString()
            {
                return "Single: <" + this.Alpha.ToString() + ">";
            }
        }

        [Serializable]
        private struct ContextKey<TKey1, TKey2> : IContextKey, IEquatable<ContextKey<TKey1, TKey2>>
        {
            public TKey1 Alpha;
            public TKey2 Beta;

            public ContextKey(TKey1 alpha, TKey2 beta)
            {
                this.Alpha = alpha;
                this.Beta = beta;
            }

            public bool Equals(ContextKey<TKey1, TKey2> other)
            {
                return
                    EqualityComparer<TKey1>.Default.Equals(this.Alpha, other.Alpha) &&
                    EqualityComparer<TKey2>.Default.Equals(this.Beta, other.Beta);
            }

            public override bool Equals(object obj)
            {
                return obj is ContextKey<TKey1, TKey2> && this.Equals((ContextKey<TKey1, TKey2>)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + this.Alpha.GetHashCode();
                    hash = hash * 31 + this.Beta.GetHashCode();
                    return hash;
                }
            }

            public override string ToString()
            {
                return
                    "Double: <" + this.Alpha.ToString() + ">, " +
                    "<" + this.Beta.ToString() + ">";
            }
        }

        [Serializable]
        private struct ContextKey<TKey1, TKey2, TKey3> : IContextKey, IEquatable<ContextKey<TKey1, TKey2, TKey3>>
        {
            public TKey1 Alpha;
            public TKey2 Beta;
            public TKey3 Gamma;

            public ContextKey(TKey1 alpha, TKey2 beta, TKey3 gamma)
            {
                this.Alpha = alpha;
                this.Beta = beta;
                this.Gamma = gamma;
            }

            public bool Equals(ContextKey<TKey1, TKey2, TKey3> other)
            {
                return
                    EqualityComparer<TKey1>.Default.Equals(this.Alpha, other.Alpha) &&
                    EqualityComparer<TKey2>.Default.Equals(this.Beta, other.Beta) &&
                    EqualityComparer<TKey3>.Default.Equals(this.Gamma, other.Gamma);
            }

            public override bool Equals(object obj)
            {
                return obj is ContextKey<TKey1, TKey2, TKey3> && this.Equals((ContextKey<TKey1, TKey2, TKey3>)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + this.Alpha.GetHashCode();
                    hash = hash * 31 + this.Beta.GetHashCode();
                    hash = hash * 31 + this.Gamma.GetHashCode();
                    return hash;
                }
            }

            public override string ToString()
            {
                return
                    "ContextKey(3): <" + this.Alpha.ToString() + ">, " +
                    "<" + this.Beta.ToString() + ">, " +
                    "<" + this.Gamma.ToString() + ">";
            }
        }

        [Serializable]
        private struct ContextKey<TKey1, TKey2, TKey3, TKey4> : IContextKey, IEquatable<ContextKey<TKey1, TKey2, TKey3, TKey4>>
        {
            public TKey1 Alpha;
            public TKey2 Beta;
            public TKey4 Delta;
            public TKey3 Gamma;
            public ContextKey(TKey1 alpha, TKey2 beta, TKey3 gamma, TKey4 delta)
            {
                this.Alpha = alpha;
                this.Beta = beta;
                this.Gamma = gamma;
                this.Delta = delta;
            }

            public bool Equals(ContextKey<TKey1, TKey2, TKey3, TKey4> other)
            {
                return
                    EqualityComparer<TKey1>.Default.Equals(this.Alpha, other.Alpha) &&
                    EqualityComparer<TKey2>.Default.Equals(this.Beta, other.Beta) &&
                    EqualityComparer<TKey3>.Default.Equals(this.Gamma, other.Gamma) &&
                    EqualityComparer<TKey4>.Default.Equals(this.Delta, other.Delta);
            }

            public override bool Equals(object obj)
            {
                return obj is ContextKey<TKey1, TKey2, TKey3, TKey4> && this.Equals((ContextKey<TKey1, TKey2, TKey3, TKey4>)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 311 + this.Alpha.GetHashCode();
                    hash = hash * 311 + this.Beta.GetHashCode();
                    hash = hash * 311 + this.Gamma.GetHashCode();
                    hash = hash * 311 + this.Delta.GetHashCode();
                    return hash;
                }
            }

            public override string ToString()
            {
                return
                    "Quad: <" + this.Alpha.ToString() + ">, " +
                    "<" + this.Beta.ToString() + ">, " +
                    "<" + this.Gamma.ToString() + ">, " +
                    "<" + this.Delta.ToString() + ">";
            }
        }

        [Serializable]
        private struct ContextKey<TKey1, TKey2, TKey3, TKey4, TKey5> : IContextKey, IEquatable<ContextKey<TKey1, TKey2, TKey3, TKey4, TKey5>>
        {
            public TKey1 Alpha;
            public TKey2 Beta;
            public TKey4 Delta;
            public TKey5 Epsilon;
            public TKey3 Gamma;
            public ContextKey(TKey1 alpha, TKey2 beta, TKey3 gamma, TKey4 delta, TKey5 epsilon)
            {
                this.Alpha = alpha;
                this.Beta = beta;
                this.Gamma = gamma;
                this.Delta = delta;
                this.Epsilon = epsilon;
            }

            public bool Equals(ContextKey<TKey1, TKey2, TKey3, TKey4, TKey5> other)
            {
                return
                    EqualityComparer<TKey1>.Default.Equals(this.Alpha, other.Alpha) &&
                    EqualityComparer<TKey2>.Default.Equals(this.Beta, other.Beta) &&
                    EqualityComparer<TKey3>.Default.Equals(this.Gamma, other.Gamma) &&
                    EqualityComparer<TKey4>.Default.Equals(this.Delta, other.Delta) &&
                    EqualityComparer<TKey5>.Default.Equals(this.Epsilon, other.Epsilon);
            }

            public override bool Equals(object obj)
            {
                return obj is ContextKey<TKey1, TKey2, TKey3, TKey4, TKey5> && this.Equals((ContextKey<TKey1, TKey2, TKey3, TKey4, TKey5>)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 311 + this.Alpha.GetHashCode();
                    hash = hash * 311 + this.Beta.GetHashCode();
                    hash = hash * 311 + this.Gamma.GetHashCode();
                    hash = hash * 311 + this.Delta.GetHashCode();
                    hash = hash * 311 + this.Epsilon.GetHashCode();
                    return hash;
                }
            }

            public override string ToString()
            {
                return
                    "Quintuple: <" + this.Alpha.ToString() + ">, " +
                    "<" + this.Beta.ToString() + ">, " +
                    "<" + this.Gamma.ToString() + ">, " +
                    "<" + this.Delta.ToString() + ">, " +
                    "<" + this.Epsilon.ToString() + ">";
            }
        }

        private static class CachePurger
        {
            private static List<KeyValuePair<int, GlobalPersistentContext>> buffer = new List<KeyValuePair<int, GlobalPersistentContext>>();
            private static double lastUpdate;
            private static IEnumerator purger;
            public static void Run()
            {
                if (!HasInstanceLoaded)
                {
                    return;
                }

                if (purger != null)
                {
                    double start = EditorApplication.timeSinceStartup;

                    // Dirty, dirty do while.
                    do
                    {
                        if (!purger.MoveNext())
                        {
                            EndPurge();
                            return;
                        }
                    }
                    while (EditorApplication.timeSinceStartup - start < 0.005f);
                }
                else if (EditorApplication.timeSinceStartup - lastUpdate > 1.0)
                {
                    //Instance.Serialize();
                    //Instance.UpdateCacheSize();
                    lastUpdate = EditorApplication.timeSinceStartup;

                    if (Instance.CacheSize > Instance.MaxCacheByteSize)
                    {
                        int count = (Instance.CacheSize - Instance.MaxCacheByteSize) / (Instance.CacheSize / Instance.EntryCount) + 1;
                        //count = count <= 1 ? 1 : count >= 1000 ? 1000 : count;
                        //Debug.Log("Purging " + count + " persistent entries");
                        purger = Purge(count);
                    }
                }
            }

            private static void EndPurge()
            {
                if (purger != null)
                {
                    purger = null;
                    buffer.Clear();
                    lastUpdate = EditorApplication.timeSinceStartup;
                }
            }

            private static IEnumerator Purge(int count)
            {
                double searchStartTime = EditorApplication.timeSinceStartup;
                long newest = DateTime.Now.Ticks;

                // Search
                for (int i = 0; i < Instance.EntryCount; i++)
                {
                    var entry = Instance.cache.Get(i);
                    bool added = false;

                    if (entry.Value.TimeStamp < newest)
                    {
                        // Try and insert the current entry into the buffer.
                        for (int j = 0; j < buffer.Count; j++)
                        {
                            if (buffer[j].Value.TimeStamp >= entry.Value.TimeStamp)
                            {
                                if (buffer.Count >= count)
                                {
                                    buffer[buffer.Count - 1] = new KeyValuePair<int, GlobalPersistentContext>(i, entry.Value);
                                    break;
                                }
                                else
                                {
                                    buffer.Insert(j, new KeyValuePair<int, GlobalPersistentContext>(i, entry.Value));
                                    break;
                                }
                            }
                        }
                    }

                    // If no place was found, add the entry to the end, if there's still place.
                    if (!added && buffer.Count < count)
                    {
                        buffer.Add(new KeyValuePair<int, GlobalPersistentContext>(i, entry.Value));
                        added = true;
                    }

                    if (added)
                    {
                        newest = buffer[buffer.Count - 1].Value.TimeStamp;
                    }

                    yield return null;
                }

                //Debug.Log("Purge search completed in " + (int)((EditorApplication.timeSinceStartup - searchStartTime) * 1000) + " ms.");

                // Complete purge.
                //for (int i = 0; i < buffer.Count; i++)
                //{
                //	Instance.cache.Remove(buffer[i].Key);
                //}
                foreach (var i in buffer.OrderByDescending(e => e.Key))
                {
                    //if (Instance.EntryCount > i.Key)
                    //{
                    Instance.cache.RemoveAt(i.Key);
                    //}

                    yield return null;
                }
            }
        }
    }
}
#endif