#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AOTGenerationConfig.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Serialization
{
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.Scripting;

    /// <summary>
    /// Contains configuration for generating an assembly that provides increased AOT support in Odin.
    /// </summary>
    [SirenixEditorConfig]
    public class AOTGenerationConfig : GlobalConfig<AOTGenerationConfig>
    {
        private const string LINK_XML_CONTENTS =
@"<linker>
       <assembly fullname=""" + FormatterEmitter.PRE_EMITTED_ASSEMBLY_NAME + @""" preserve=""all""/>
</linker>";

        private static readonly TwoWaySerializationBinder TypeBinder = new DefaultSerializationBinder();

        [Serializable]
        private class TypeEntry
        {
            [NonSerialized]
            public bool IsInitialized;

            [NonSerialized]
            public bool IsNew;

            [NonSerialized]
            public string NiceTypeName;

            public string TypeName;

            public bool IsCustom;

            public bool Emit;

            public Type Type;
        }

        private string AutomateBeforeBuildsSuffix { get { return this.EnableAutomateBeforeBuilds ? "Extra experimental" : "The automation feature is only available in Unity 5.6 and up"; } }
        private bool EnableAutomateBeforeBuilds { get { return UnityVersion.IsVersionOrGreater(5, 6); } }
        private bool ShowAutomateConfig { get { return this.EnableAutomateBeforeBuilds && this.automateBeforeBuilds; } }

        [SerializeField, ToggleLeft, EnableIf("EnableAutomateBeforeBuilds"), SuffixLabel("$AutomateBeforeBuildsSuffix", false)]
        private bool automateBeforeBuilds = false;

        [SerializeField, ToggleLeft, ShowIf("ShowAutomateConfig")]
        private bool deleteDllAfterBuilds = true;

        [SerializeField, ShowIf("ShowAutomateConfig")]
        private List<BuildTarget> automateForPlatforms = new List<BuildTarget>()
        {
            BuildTarget.iOS,
            BuildTarget.WebGL,
        };

        [SerializeField, HideInInspector]
        private long lastScan;

        [SerializeField, PropertyOrder(4)]
        [ListDrawerSettings(DraggableItems = false, OnTitleBarGUI = "GenericVariantsTitleGUI", HideAddButton = true)]
        private List<TypeEntry> supportSerializedTypes;

        /// <summary>
        /// <para>
        /// Whether to automatically scan the project and generate an AOT dll, right before builds. This will only affect platforms that are in the <see cref="AutomateForPlatforms"/> list.
        /// </para>
        /// <para>
        /// **This will only work on Unity 5.6 and higher!**
        /// </para>
        /// </summary>
        public bool AutomateBeforeBuilds
        {
            get { return this.automateBeforeBuilds; }
            set { this.automateBeforeBuilds = value; }
        }

        /// <summary>
        /// Whether to automatically delete the generated AOT dll after a build has completed.
        /// </summary>
        public bool DeleteDllAfterBuilds
        {
            get { return this.deleteDllAfterBuilds; }
            set { this.deleteDllAfterBuilds = value; }
        }

        /// <summary>
        /// A list of platforms to automatically scan the project and generate an AOT dll for, right before builds. This will do nothing unless <see cref="AutomateBeforeBuilds"/> is true.
        /// </summary>
        public List<BuildTarget> AutomateForPlatforms
        {
            get { return this.automateForPlatforms; }
        }

        /// <summary>
        /// The path to the AOT folder that the AOT .dll and linker file is created in, relative to the current project folder.
        /// </summary>
        public string AOTFolderPath { get { return "Assets/" + SirenixAssetPaths.SirenixAssembliesPath + "AOT/"; } }

        //[ToggleLeft]
        //[TitleGroup("Generate AOT DLL"), PropertyOrder(9)]
        //[InfoBox("If 'Emit AOT Formatters' is enabled, Odin will also generate serialization formatters for types which need it. This removes the need for reflection on AOT platforms, and can significantly speed up serialization.")]
        //[SerializeField]
        //private bool emitAOTFormatters = true;

        [OnInspectorGUI, PropertyOrder(-10000)]
        private void DrawExperimentalText()
        {
            if (Event.current.type == EventType.Repaint)
            {
                var rect = GUIHelper.GetCurrentLayoutRect();
                rect.height = 20;
                rect.y -= 35;
                GUI.Label(rect, "Experimental", new GUIStyle(SirenixGUIStyles.RightAlignedGreyMiniLabel) { fontSize = 16 });
            }
        }

        private void GenericVariantsTitleGUI()
        {
            SirenixEditorGUI.VerticalLineSeparator();
            GUILayout.Label("Last scan: " + DateTime.FromBinary(this.lastScan).ToString(), EditorStyles.centeredGreyMiniLabel);

            if (SirenixEditorGUI.ToolbarButton(new GUIContent("  Sort  ")))
            {
                this.SortTypes();
            }

            if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
            {
                this.supportSerializedTypes.Insert(0, new TypeEntry() { IsCustom = true, Emit = true });
            }
        }

        private void SortTypes()
        {
            Comparison<TypeEntry> sorter = (TypeEntry a, TypeEntry b) =>
            {
                bool aType = a.Type != null;
                bool bType = b.Type != null;

                if (aType != bType)
                {
                    return aType ? 1 : -1;
                }
                else if (!aType)
                {
                    return (a.TypeName ?? "").CompareTo(b.TypeName ?? "");
                }

                if (a.IsCustom != b.IsCustom)
                {
                    return a.IsCustom ? -1 : 1;
                }

                if (a.IsNew != b.IsNew)
                {
                    return a.IsNew ? -1 : 1;
                }

                return (a.NiceTypeName ?? "").CompareTo(b.NiceTypeName ?? "");
            };

            this.supportSerializedTypes.Sort(sorter);
            //this.emitFormattersForTypes.Sort(sorter);
        }

        [Button("Scan Project", 36), HorizontalGroup("ButtonMargin", 0.2f, PaddingRight = -4), PropertyOrder(2)]
        private void ScanProjectButton()
        {
            EditorApplication.delayCall += ScanProject;
        }

        /// <summary>
        /// Scans the entire project for types to support AOT serialization for.
        /// </summary>
        public void ScanProject()
        {
            List<Type> serializedTypes;

            if (this.ProcessProject(out serializedTypes))
            {
                this.RegisterTypes(this.supportSerializedTypes, serializedTypes);
                this.SortTypes();

                this.lastScan = DateTime.Now.Ticks;
                EditorUtility.SetDirty(this);
            }
        }

        private void RegisterTypes(List<TypeEntry> typeEntries, List<Type> types)
        {
            var preExistingNonCustomTypes = typeEntries
                .Where(n => !n.IsCustom && n.Type != null)
                .Select(n => n.Type)
                .ToHashSet();

            typeEntries.RemoveAll(n => !n.IsCustom);

            var preExistingCustomTypes = typeEntries
                .Where(n => n.Type != null)
                .Select(n => n.Type)
                .ToHashSet();

            typeEntries.AddRange(types
                .Where(type => !preExistingCustomTypes.Contains(type))
                .Select(type => new TypeEntry()
                {
                    Type = type,
                    TypeName = TypeBinder.BindToName(type),
                    NiceTypeName = type.GetNiceName(),
                    IsCustom = false,
                    Emit = true,
                    IsNew = !preExistingNonCustomTypes.Contains(type),
                    IsInitialized = false
                }));

            this.InitializeTypeEntries();
        }

        [OnInspectorGUI, PropertyOrder(-1)]
        private void DrawTopInfoBox()
        {
            SirenixEditorGUI.InfoMessageBox(
                "On AOT-compiled platforms, Unity's code stripping can remove classes that the serialization system needs, " +
                "or fail to generate code for needed variants of generic types. Therefore, Odin can create an assembly that " +
                "directly references all functionality that is needed at runtime, to ensure it is available.");
        }

        [OnInspectorGUI, HorizontalGroup("ButtonMargin"), PropertyOrder(1)]
        private void DrawWarning()
        {
            EditorGUILayout.HelpBox(
                "Scanning the entire project might take a while. It will scan the entire project for relevant types including " +
                "ScriptableObjects, prefabs and scenes. Modified type entries will not be touched.",
                MessageType.Warning);
        }

        private bool ProcessProject(out List<Type> serializedTypes)
        {
            serializedTypes = null;

            HashSet<Type> seenSerializedTypes = new HashSet<Type>();

            Action<Type> registerType = null;

            registerType = (type) =>
            {
                if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return;
                if (type.IsAbstract || type.IsInterface) return;
                if (type.IsGenericType && (type.IsGenericTypeDefinition || !type.IsFullyConstructedGenericType())) return;

                seenSerializedTypes.Add(type);

                if (type.IsGenericType)
                {
                    foreach (var arg in type.GetGenericArguments())
                    {
                        registerType(arg);
                    }
                }
            };

            Action<Type> onLocatedEmitType = (type) =>
            {
                var typeFlags = AssemblyUtilities.GetAssemblyTypeFlag(type.Assembly);

                if ((typeFlags & AssemblyTypeFlags.PluginEditorTypes) == AssemblyTypeFlags.PluginEditorTypes ||
                    (typeFlags & AssemblyTypeFlags.UnityEditorTypes) == AssemblyTypeFlags.UnityEditorTypes ||
                    (typeFlags & AssemblyTypeFlags.UserEditorTypes) == AssemblyTypeFlags.UserEditorTypes ||
                    (typeFlags & AssemblyTypeFlags.EditorTypes) == AssemblyTypeFlags.EditorTypes)
                {
                    return;
                }

                registerType(type);
            };

            Action<Type> onSerializedType = (type) =>
            {
                // We need variants of serializers for enums specifically
                if (!type.IsEnum) return;

                var typeFlags = AssemblyUtilities.GetAssemblyTypeFlag(type.Assembly);

                if ((typeFlags & AssemblyTypeFlags.PluginEditorTypes) == AssemblyTypeFlags.PluginEditorTypes ||
                    (typeFlags & AssemblyTypeFlags.UnityEditorTypes) == AssemblyTypeFlags.UnityEditorTypes ||
                    (typeFlags & AssemblyTypeFlags.UserEditorTypes) == AssemblyTypeFlags.UserEditorTypes ||
                    (typeFlags & AssemblyTypeFlags.EditorTypes) == AssemblyTypeFlags.EditorTypes)
                {
                    return;
                }

                registerType(type);
            };

            Action<IFormatter> onLocatedFormatter = (formatter) =>
            {
                var typeFlags = AssemblyUtilities.GetAssemblyTypeFlag(formatter.SerializedType.Assembly);

                if ((typeFlags & AssemblyTypeFlags.PluginEditorTypes) == AssemblyTypeFlags.PluginEditorTypes ||
                    (typeFlags & AssemblyTypeFlags.UnityEditorTypes) == AssemblyTypeFlags.UnityEditorTypes ||
                    (typeFlags & AssemblyTypeFlags.UserEditorTypes) == AssemblyTypeFlags.UserEditorTypes ||
                    (typeFlags & AssemblyTypeFlags.EditorTypes) == AssemblyTypeFlags.EditorTypes)
                {
                    return;
                }

                var type = formatter.SerializedType;

                if (type != null)
                {
                    registerType(type);
                }
            };

            FormatterLocator.OnLocatedEmittableFormatterForType += onLocatedEmitType;
            FormatterLocator.OnLocatedFormatter += onLocatedFormatter;
            Serializer.OnSerializedType += onSerializedType;

            try
            {
                UnitySerializationUtility.ForceEditorModeSerialization = true;

                if (!this.ProcessScenes())
                {
                    Debug.Log("Project scan canceled while scanning scenes.");
                    return false;
                }

                if (!this.ProcessAssets())

                {
                    Debug.Log("Project scan canceled while scanning assets.");
                    return false;
                }

                serializedTypes = seenSerializedTypes.ToList();

                return true;
            }
            finally
            {
                UnitySerializationUtility.ForceEditorModeSerialization = false;

                FormatterLocator.OnLocatedEmittableFormatterForType -= onLocatedEmitType;
                FormatterLocator.OnLocatedFormatter -= onLocatedFormatter;
                Serializer.OnSerializedType -= onSerializedType;

                EditorUtility.ClearProgressBar(); // Just to be sure we don't forget that
            }
        }

        private bool ProcessAssets()
        {
            var assetPaths =
                AssetDatabase.FindAssets("t:ScriptableObject")
                .Append(AssetDatabase.FindAssets("t:GameObject"))
                .Select(n => AssetDatabase.GUIDToAssetPath(n))
                .ToList();

            if (assetPaths.Count == 0)
            {
                return true;
            }

            try
            {
                for (int i = 0; i < assetPaths.Count; i++)
                {
                    string assetPath = assetPaths[i];

                    if (EditorUtility.DisplayCancelableProgressBar("Scanning asset " + i, assetPath, i / assetPaths.Count))
                    {
                        return false;
                    }

                    var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

                    if (assets == null || assets.Length == 0)
                    {
                        continue;
                    }

                    foreach (var asset in assets)
                    {
                        if (asset is ISerializationCallbackReceiver)
                        {
                            (asset as ISerializationCallbackReceiver).OnBeforeSerialize();
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return true;
        }

        private bool ProcessScenes()
        {
            var scenePaths =
                AssetDatabase.FindAssets("t:Scene")
                .Select(n => AssetDatabase.GUIDToAssetPath(n))
                .ToList();

            bool hasDirtyScenes = false;

            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                if (EditorSceneManager.GetSceneAt(i).isDirty)
                {
                    hasDirtyScenes = true;
                    break;
                }
            }

            if (hasDirtyScenes && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return false;
            }

            var oldSceneSetup = EditorSceneManager.GetSceneManagerSetup();

            try
            {
                for (int i = 0; i < scenePaths.Count; i++)
                {
                    var scenePath = scenePaths[i];

                    if (EditorUtility.DisplayCancelableProgressBar("Scanning Scenes", "Scene " + (i + 1) + "/" + scenePaths.Count + " - " + scenePath, (float)i / scenePaths.Count))
                    {
                        return false;
                    }

                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                    var sceneGOs = UnityEngine.Object.FindObjectsOfType<GameObject>();

                    foreach (var go in sceneGOs)
                    {
                        if ((go.hideFlags & HideFlags.DontSaveInBuild) == 0)
                        {
                            foreach (var component in go.GetComponents<ISerializationCallbackReceiver>())
                            {
                                component.OnBeforeSerialize();

                                var prefabSupporter = component as ISupportsPrefabSerialization;

                                if (prefabSupporter != null)
                                {
                                    // Also force a serialization of the object's prefab modifications, in case there are unknown types in there

                                    List<UnityEngine.Object> objs = null;
                                    var mods = UnitySerializationUtility.DeserializePrefabModifications(prefabSupporter.SerializationData.PrefabModifications, prefabSupporter.SerializationData.PrefabModificationsReferencedUnityObjects);
                                    UnitySerializationUtility.SerializePrefabModifications(mods, ref objs);
                                }
                            }
                        }
                    }
                }

                // Load a new empty scene that will be unloaded immediately, just to be sure we completely clear all changes made by the scan
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
            finally
            {
                try
                {
                    EditorUtility.DisplayProgressBar("Restoring scene setup", "", 0.5f);
                    EditorSceneManager.RestoreSceneManagerSetup(oldSceneSetup);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }

            return true;
        }

        /// <summary>
        /// Generates an AOT DLL, using the current configuration of the AOTGenerationConfig instance.
        /// </summary>
        [TitleGroup("Generate AOT DLL", "Sirenix/Assemblies/AOT/" + FormatterEmitter.PRE_EMITTED_ASSEMBLY_NAME + ".dll", indent: false), PropertyOrder(9)]
        [Button(ButtonSizes.Large)]
        public void GenerateDLL()
        {
            FixUnityAboutWindowBeforeEmit.Fix();

            const string NAME = FormatterEmitter.PRE_EMITTED_ASSEMBLY_NAME;

            var dirPath = this.AOTFolderPath;
            var newDllPath = dirPath + NAME;
            var fullDllPath = newDllPath + ".dll";
            var fullLinkXmlPath = dirPath + "link.xml";

            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName() { Name = NAME }, AssemblyBuilderAccess.Save, dirPath);
            var module = assembly.DefineDynamicModule(NAME);

            // The following is a fix for Unity's crappy Mono runtime that doesn't know how to do this sort
            //  of stuff properly
            //
            // We must manually remove the "Default Dynamic Assembly" module that is automatically defined,
            //  otherwise a reference to that non-existent module will be saved into the assembly's IL, and
            //  that will cause a multitude of issues.
            //
            // We do this by forcing there to be only one module - the one we just defined, and we set the
            //   manifest module to be that module as well.
            {
                var modulesField = assembly.GetType().GetField("modules", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var manifestModuleField = assembly.GetType().GetField("manifest_module", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (modulesField != null)
                {
                    modulesField.SetValue(assembly, new ModuleBuilder[] { module });
                }

                if (manifestModuleField != null)
                {
                    manifestModuleField.SetValue(assembly, module);
                }
            }

            var type = module.DefineType(NAME + ".PreventCodeStrippingViaReferences", TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.NotPublic);

            CustomAttributeBuilder attributeBuilder = new CustomAttributeBuilder(typeof(PreserveAttribute).GetConstructor(Type.EmptyTypes), new object[0]);
            type.SetCustomAttribute(attributeBuilder);

            var staticConstructor = type.DefineTypeInitializer();
            var il = staticConstructor.GetILGenerator();

            HashSet<Type> seenTypes = new HashSet<Type>();

            //var endPoint = il.DefineLabel();
            //il.Emit(OpCodes.Br, endPoint);

            foreach (var typeEntry in this.supportSerializedTypes)
            {
                if (typeEntry.Type == null || !typeEntry.Emit) continue;

                if (typeEntry.Type.IsAbstract || typeEntry.Type.IsInterface)
                {
                    Debug.LogError("Skipping type '" + typeEntry.Type.GetNiceFullName() + "'! Type is abstract or an interface.");
                    continue;
                }

                if (typeEntry.Type.IsGenericType && (typeEntry.Type.IsGenericTypeDefinition || !typeEntry.Type.IsFullyConstructedGenericType()))
                {
                    Debug.LogError("Skipping type '" + typeEntry.Type.GetNiceFullName() + "'! Type is a generic type definition, or its arguments contain generic parameters; type must be a fully constructed generic type.");
                    continue;
                }

                var serializedType = typeEntry.Type;

                if (seenTypes.Contains(serializedType)) continue;

                seenTypes.Add(serializedType);

                // Reference serialized type
                {
                    if (serializedType.IsValueType)
                    {
                        var local = il.DeclareLocal(serializedType);

                        il.Emit(OpCodes.Ldloca, local);
                        il.Emit(OpCodes.Initobj, serializedType);
                    }
                    else
                    {
                        var constructor = serializedType.GetConstructor(Type.EmptyTypes);

                        if (constructor != null)
                        {
                            il.Emit(OpCodes.Newobj, constructor);
                            il.Emit(OpCodes.Pop);
                        }
                    }
                }

                // Reference and/or create formatter type
                if (!FormatterUtilities.IsPrimitiveType(serializedType))
                {
                    var formatter = FormatterLocator.GetFormatter(serializedType, SerializationPolicies.Unity);

                    if (formatter.GetType().IsDefined<EmittedFormatterAttribute>())
                    {
                        //// Emit an actual AOT formatter into the generated assembly

                        //if (this.emitAOTFormatters)
                        //{
                        //    var emittedFormatter = FormatterEmitter.EmitAOTFormatter(typeEntry.Type, module, SerializationPolicies.Unity);
                        //    var emittedFormatterConstructor = emittedFormatter.GetConstructor(Type.EmptyTypes);

                        //    il.Emit(OpCodes.Newobj, emittedFormatterConstructor);
                        //    il.Emit(OpCodes.Pop);
                        //}
                    }
                    else
                    {
                        // Just reference the pre-existing formatter

                        var formatterConstructor = formatter.GetType().GetConstructor(Type.EmptyTypes);

                        if (formatterConstructor != null)
                        {
                            il.Emit(OpCodes.Newobj, formatterConstructor);
                            il.Emit(OpCodes.Pop);
                        }
                    }

                    // Make sure we have a proper reflection formatter variant if all else goes wrong
                    il.Emit(OpCodes.Newobj, typeof(ReflectionFormatter<>).MakeGenericType(typeEntry.Type).GetConstructor(Type.EmptyTypes));
                    il.Emit(OpCodes.Pop);
                }

                ConstructorInfo serializerConstructor;

                // Reference serializer variant
                if (serializedType.IsEnum)
                {
                    serializerConstructor = typeof(EnumSerializer<>).MakeGenericType(serializedType).GetConstructor(Type.EmptyTypes);
                }
                else
                {
                    serializerConstructor = typeof(ComplexTypeSerializer<>).MakeGenericType(serializedType).GetConstructor(Type.EmptyTypes);
                }

                il.Emit(OpCodes.Newobj, serializerConstructor);
                il.Emit(OpCodes.Pop);
            }

            //il.MarkLabel(endPoint);
            il.Emit(OpCodes.Ret);

            type.CreateType();

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            if (File.Exists(fullDllPath))
            {
                File.Delete(fullDllPath);
            }

            if (File.Exists(fullDllPath + ".meta"))
            {
                File.Delete(fullDllPath + ".meta");
            }

            try
            {
                AssetDatabase.Refresh();
            }
            catch (Exception)
            {
                // Sigh, Unity 5.3.0
            }

            assembly.Save(NAME);

            File.Move(newDllPath, fullDllPath);
            File.WriteAllText(fullLinkXmlPath, LINK_XML_CONTENTS);

            try
            {
                AssetDatabase.Refresh();
            }
            catch (Exception)
            {
                // Sigh, Unity 5.3.0
            }

            var pluginImporter = PluginImporter.GetAtPath(fullDllPath) as PluginImporter;

            if (pluginImporter != null)
            {
                //pluginImporter.ClearSettings();

                pluginImporter.SetCompatibleWithEditor(false);
                pluginImporter.SetCompatibleWithAnyPlatform(true);

                // Disable for all standalones
                pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux, false);
                pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, false);
                pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneLinuxUniversal, false);

                // StandaloneOSXUniversal (<= 2017.2) / StandaloneOSX (>= 2017.3)
                pluginImporter.SetCompatibleWithPlatform((BuildTarget)2, false);

                if (!UnityVersion.IsVersionOrGreater(2017, 3))
                {
                    pluginImporter.SetCompatibleWithPlatform((BuildTarget)4, false);        // StandaloneOSXIntel
                    pluginImporter.SetCompatibleWithPlatform((BuildTarget)27, false);       // StandaloneOSXIntel64
                }

                pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, false);
                pluginImporter.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, false);

                //pluginImporter.SetCompatibleWithPlatform(BuildTarget.Android, false);

                pluginImporter.SaveAndReimport();
            }

            AssetDatabase.SaveAssets();
        }

        [OnInspectorGUI, PropertyOrder(-1000)]
        private void OnGUIInitializeTypeEntries()
        {
            if (Event.current.type != EventType.Layout)
            {
                return;
            }

            this.InitializeTypeEntries();
        }

        private void InitializeTypeEntries()
        {
            this.supportSerializedTypes = this.supportSerializedTypes ?? new List<TypeEntry>();

            //this.emitFormattersForTypes = this.emitFormattersForTypes ?? new List<TypeEntry>();

            // Type is not serialized by Unity. Deserialize on the editor is not always called when reloading, neither is OnEnabled...
            // So right now we do it like this. if you have better ideas I'm all ears.
            foreach (var item in this.supportSerializedTypes/*.Concat(this.emitFormattersForTypes)*/)
            {
                if (!item.IsInitialized)
                {
                    if (item.Type == null)
                    {
                        if (item.TypeName != null)
                        {
                            item.Type = TypeBinder.BindToType(item.TypeName);
                        }

                        if (item.Type != null)
                        {
                            item.NiceTypeName = item.Type.GetNiceName();
                        }
                    }
                    item.IsInitialized = true;
                }
            }
        }

        [OdinDrawer]
        private class TypeEntryDrawer : OdinValueDrawer<TypeEntry>
        {
            // private static readonly GUIStyle deletedLabelStyle = new GUIStyle("sv_label_3") { margin = new RectOffset(3, 3, 2, 0), alignment = TextAnchor.MiddleCenter };
            private static readonly GUIStyle MissingLabelStyle = new GUIStyle("sv_label_6") { margin = new RectOffset(3, 3, 2, 0), alignment = TextAnchor.MiddleCenter };

            private static readonly GUIStyle NewLabelStyle = new GUIStyle("sv_label_3") { margin = new RectOffset(3, 3, 2, 0), alignment = TextAnchor.MiddleCenter };
            private static readonly GUIStyle ChangedLabelStyle = new GUIStyle("sv_label_4") { margin = new RectOffset(3, 3, 2, 0), alignment = TextAnchor.MiddleCenter };

            protected override void DrawPropertyLayout(IPropertyValueEntry<TypeEntry> entry, GUIContent label)
            {
                var value = entry.SmartValue;
                var valueChanged = false;
                var rect = EditorGUILayout.GetControlRect();
                var toggleRect = rect.SetWidth(20);
                rect.xMin += 20;

                // Init
                if (string.IsNullOrEmpty(value.NiceTypeName) && value.Type != null)
                {
                    value.NiceTypeName = value.Type.GetNiceName();
                }

                if (value.IsNew || value.IsCustom || value.Type == null)
                {
                    rect.xMax -= 78;
                }

                // Toggle
                GUIHelper.PushGUIEnabled(value.Type != null);
                valueChanged = value.Emit != (value.Emit = EditorGUI.Toggle(toggleRect, value.Emit));
                GUIHelper.PopGUIEnabled();

                // TextField
                rect.y += 2;
                GUI.SetNextControlName(entry.Property.Path);
                bool isEditing = GUI.GetNameOfFocusedControl() == entry.Property.Path || value.Type == null;
                GUIHelper.PushColor(isEditing ? Color.white : new Color(0, 0, 0, 0));
                var newName = EditorGUI.TextField(rect, value.TypeName, EditorStyles.textField);
                GUIHelper.PopColor();

                // TextField overlay
                if (!isEditing)
                {
                    GUI.Label(rect, value.NiceTypeName);
                }

                if (value.IsNew || value.IsCustom || value.Type == null)
                {
                    rect.xMin = rect.xMax + 3;
                    rect.width = 75;
                }

                // Labels
                if (value.Type == null)
                {
                    EditorGUI.LabelField(rect, GUIHelper.TempContent("MISSING"), MissingLabelStyle);
                }
                else if (value.IsNew)
                {
                    EditorGUI.LabelField(rect, GUIHelper.TempContent("NEW"), NewLabelStyle);
                }
                else if (value.IsCustom)
                {
                    EditorGUI.LabelField(rect, GUIHelper.TempContent("MODIFIED"), ChangedLabelStyle);
                }

                // Set values
                if ((newName ?? "") != (value.TypeName ?? ""))
                {
                    value.TypeName = newName;
                    value.IsCustom = true;
                    value.Type = TypeBinder.BindToType(value.TypeName);
                    value.NiceTypeName = value.Type == null ? value.TypeName : value.Type.GetNiceName();
                    valueChanged = true;
                }

                if (valueChanged)
                {
                    value.IsCustom = true;
                    entry.Values.ForceMarkDirty();
                }
            }
        }
    }
}
#endif