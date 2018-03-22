////-----------------------------------------------------------------------
//// <copyright file="EditorGlobalConfigAttribute.cs" company="Sirenix IVS">
//// Copyright (c) Sirenix IVS. All rights reserved.
//// </copyright>
////-----------------------------------------------------------------------
//namespace Sirenix.Utilities
//{
//    using System;
//    using System.Linq;
//    using UnityEngine;

//    /// <summary>
//    /// <para>This attribute is used by classes deriving from GlobalConfig and specifies the menu item path for the preference window and the asset path for the generated config file.</para>
//    /// <para>This attribute has been marked obsolete. Checkout the OdinMenuItemAttribtue instead. </para>
//    /// </summary>
//    /// <seealso cref="GlobalConfig{T}"/>
//    [Obsolete("Use the OdinMenuItemAttribute instead.")]
//    public abstract class EditorGlobalConfigAttribute : GlobalConfigAttribute
//    {
//        /// <summary>
//        /// The menu item path
//        /// </summary>
//        public readonly string MenuItemPath;

//        /// <summary>
//        /// Gets the editor window title.
//        /// </summary>
//        protected abstract string EditorWindowTitle { get; }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="EditorGlobalConfigAttribute"/> class.
//        /// </summary>
//        /// <param name="path">The path.</param>
//        /// <param name="menuItem">The menu item.</param>
//        protected EditorGlobalConfigAttribute(string path, string menuItem)
//            : base(path.Replace("\\", "/").TrimEnd('/') + "/")
//        {
//            this.MenuItemPath = menuItem;
//        }

//        /// <summary>
//        /// Opens the Odin preferences window for any given GlobalConfig.
//        /// </summary>
//        /// <param name="config">The configuration.</param>
//        /// <exception cref="System.ArgumentException">Config parameter must inherit from <see cref="GlobalConfig{T}"/></exception>
//        public void OpenWindow(UnityEngine.Object config)
//        {
//            if (config.GetType().InheritsFrom(typeof(GlobalConfig<>)) == false)
//            {
//                throw new ArgumentException("Config parameter must inherit from GlobalConfig<>");
//            }

//            var windowType = AssemblyUtilities.GetType("Sirenix.OdinInspector.Editor.SirenixPreferencesWindow");
//            if (windowType != null)
//            {
//                windowType.GetMethods().Where(x => x.Name == "OpenGlobalConfigWindow" && x.GetParameters().Length == 2).First()
//                .MakeGenericMethod(this.GetType())
//                .Invoke(null, new object[] { this.EditorWindowTitle, config });
//            }
//            else
//            {
//                Debug.LogError("Failed to open window, could not find Sirenix.OdinInspector.Editor.GlobalConfigWindow");
//            }
//        }

//        /// <summary>
//        /// Opens the Odin preferences window.
//        /// </summary>
//        /// <param name="title">The title.</param>
//        /// <param name="config">The configuration.</param>
//        public static void OpenWindow<T>(string title, UnityEngine.Object config = null) where T : EditorGlobalConfigAttribute
//        {
//            var windowType = AssemblyUtilities.GetType("Sirenix.OdinInspector.Editor.SirenixPreferencesWindow");
//            if (windowType != null)
//            {
//                windowType.GetMethods().Where(x => x.Name == "OpenGlobalConfigWindow" && x.GetParameters().Length == 2).First()
//                .MakeGenericMethod(typeof(T))
//                .Invoke(null, new object[] { title, null });
//            }
//            else
//            {
//                Debug.LogError("Failed to open window, could not find Sirenix.OdinInspector.Editor.GlobalConfigWindow");
//            }
//        }
//    }
//}