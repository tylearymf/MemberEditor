//-----------------------------------------------------------------------
// <copyright file="EditorOnlyModeConfigUtility.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR

namespace Sirenix.OdinInspector
{
    using Sirenix.Utilities;
    using System.Reflection;

    internal static class EditorOnlyModeConfigUtility
    {
        private static bool initialized;

        private static object instance;
        private static WeakValueGetter<bool> isInEditorOnlyModeGetter;
        private static bool isSerializationEnabled;

        public const string SERIALIZATION_DISABLED_ERROR_TEXT =
            "ERROR: EDITOR ONLY MODE ENABLED\n\n" +
            "Odin is currently in editor only mode, meaning the serialization system is disabled in builds. " +
            "This class is specially serialized by Odin - if you try to compile with this class in your project, you *will* get compiler errors. " +
            "Either disable editor only mode in Tools -> Odin Inspector -> Preferences -> Editor Only Mode, or make sure that this type does not " +
            "inherit from a type that is serialized by Odin.";

        public static bool IsSerializationEnabled
        {
            get
            {
                if (!initialized)
                {
                    initialized = true;

                    var editorOnlyModeConfigType = AssemblyUtilities.GetTypeByCachedFullName("Sirenix.OdinInspector.Editor.EditorOnlyModeConfig");
                    var instanceProperty = editorOnlyModeConfigType.GetProperty("Instance", Flags.StaticAnyVisibility | BindingFlags.FlattenHierarchy);
                    var isInEditorOnlyModeField = editorOnlyModeConfigType.GetField("isInEditorOnlyMode", Flags.InstanceAnyVisibility);
                    instance = instanceProperty.GetValue(null, null);
                    isInEditorOnlyModeGetter = EmitUtilities.CreateWeakInstanceFieldGetter<bool>(editorOnlyModeConfigType, isInEditorOnlyModeField);
                }

                return !isInEditorOnlyModeGetter(ref instance);
            }
        }
    }
}

#endif