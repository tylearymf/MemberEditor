//-----------------------------------------------------------------------
// <copyright file="GlobalConfigAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities
{
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// <para>This attribute is used by classes deriving from GlobalConfig and specifies the asset path for the generated config file.</para>
    /// </summary>
    /// <seealso cref="EditorGlobalConfigAttribute"/>
    /// <seealso cref="GlobalConfig{T}"/>
    [AttributeUsage(AttributeTargets.Class)]
    public class GlobalConfigAttribute : Attribute
    {
        private string assetPath;

        /// <summary>
        /// Gets the full asset path including Application.dataPath. Only relevant if IsInResourcesFolder is false.
        /// </summary>
        public string FullPath
        {
            get { return Application.dataPath + "/" + this.AssetPath; }
        }

        /// <summary>
        /// Gets the relative asset path. Only relevant if IsInResourcesFolder is false.
        /// </summary>
        public string AssetPath
        {
            get
            {
                string path = this.assetPath
                    .Trim()
                    .TrimEnd('/', '\\')
                    .TrimStart('/', '\\')
                    .Replace('\\', '/') + "/";

                return path;
            }
        }

        /// <summary>
        /// Gets the resources path. Only relevant if IsInResourcesFolder is true.
        /// </summary>
        public string ResourcesPath
        {
            get
            {
                string resourcesPath = "";
                if (this.IsInResourcesFolder)
                {
                    Stack<string> folders = new Stack<string>();

                    // Find nearest resource folder.
                    var currDir = new DirectoryInfo(this.FullPath);
                    while (currDir.Name.Equals("resources", StringComparison.OrdinalIgnoreCase) == false)
                    {
                        folders.Push(currDir.Name);
                        currDir = currDir.Parent;
                    }

                    while (folders.Any())
                    {
                        resourcesPath += folders.Pop() + "/";
                    }
                }

                return resourcesPath;
            }
        }

        /// <summary>
        /// Whether the config should be associated with an asset in the project. If false, no config asset will be generated or loaded, and a new "temporary" config instance will be created for every reload. This is true by default.
        /// </summary>
        public bool UseAsset { get; set; }

        /// <summary>
        /// Gets a value indicating whether this asset is located within a resource folder.
        /// </summary>
        public bool IsInResourcesFolder
        {
            get { return this.AssetPath.Contains("/resources/", StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalConfigAttribute"/> class.
        /// </summary>
        public GlobalConfigAttribute()
            : this("Assets/Resources/Global Settings")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalConfigAttribute"/> class.
        /// </summary>
        /// <param name="assetPath">The relative asset. Remember to locate the path within a resources folder if you want the config file to be loaded at runtime without the Unity Editor.</param>
        public GlobalConfigAttribute(string assetPath)
        {
            this.assetPath = assetPath;
            this.UseAsset = true;
        }
    }
}