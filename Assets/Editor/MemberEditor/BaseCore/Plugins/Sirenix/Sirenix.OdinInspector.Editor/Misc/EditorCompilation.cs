#if UNITY_EDITOR
////-----------------------------------------------------------------------
//// <copyright file="EditorCompilation.cs" company="Sirenix IVS">
//// Copyright (c) Sirenix IVS. All rights reserved.
//// </copyright>
////-----------------------------------------------------------------------
//namespace Sirenix.OdinInspector.Editor
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Reflection;
//    using System.Reflection.Emit;
//    using System.Runtime.CompilerServices;
//    using Utilities;
//    using Utilities.Editor.CodeGeneration;
//    using UnityEditor;
//    using UnityEngine;
//    using System.IO;

//    /// <summary>
//    /// Provides utilities for compiling custom Unity editors into a .dll in the project.
//    /// </summary>
//    [InitializeOnLoad]
//    public static class EditorCompilation
//    {
//        private const string EMITTED_SUPPRESS_ERROR_TYPE_RESOLVE_ASSEMBLY_NAME = "Sirenix.OdinInspector.EmittedSuppressErrorTypeResolveAssembly";

//        /// <summary>
//        /// <para>An attribute that is put on all automatically generated editors.</para>
//        /// <para>Do not use manually.</para>
//        /// </summary>
//        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
//        public class CompiledEditorAttribute : Attribute
//        {
//            /// <summary>
//            /// The drawn type of the editor.
//            /// </summary>
//            public readonly Type DrawnType;

//            /// <summary>
//            /// The base editor type of the editor.
//            /// </summary>
//            public readonly Type EditorType;

//            /// <summary>
//            /// The name of the drawn type of the editor.
//            /// </summary>
//            public readonly string DrawnTypeName;

//            /// <summary>
//            /// The name of the base editor type of the editor.
//            /// </summary>
//            public readonly string EditorTypeName;

//            /// <summary>
//            /// Initializes a new instance of the <see cref="CompiledEditorAttribute"/> class.
//            /// </summary>
//            public CompiledEditorAttribute(Type drawnType, Type editorType, string drawnTypeName, string editorTypeName)
//            {
//                this.DrawnType = drawnType;
//                this.EditorType = editorType;
//                this.DrawnTypeName = drawnTypeName;
//                this.EditorTypeName = editorTypeName;
//            }
//        }

//        private static HashSet<string> MissingTypeNames = new HashSet<string>();

//        private const string GenerationNamespace = "Sirenix.OdinInspector.GeneratedEditors";
//        private const string EditorContainerName = "CompiledEditorContainer";

//        private const string InspectorAttributesCode = @"
//    [CompilerGenerated]
//    [CustomEditor(typeof({DrawnType}), false, isFallback = false)]";

//        private const string InspectorClassCode = @"
//    public sealed class {EditorNamePrefix}_Editor : {BaseEditorType}
//    {
//    }";

//        private const string DrawnTypeKey = "{DrawnType}";
//        private const string BaseEditorTypeKey = "{BaseEditorType}";
//        private const string EditorNamePrefixKey = "{EditorNamePrefix}";

//        static EditorCompilation()
//        {
//            FixUnityAssemblyVersioningResolution();
//            OverrideUnityTypeResolutionForMissingEditors();

//            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(ass => ass.GetName().Name == SirenixAssetPaths.OdinGeneratedEditorsAssemblyName);

//            if (assembly != null)
//            {
//                try
//                {
//                    Type type = assembly.GetType(GenerationNamespace + "." + EditorContainerName);

//                    if (type != null)
//                    {
//                        bool anyMissing = false;

//                        foreach (CompiledEditorAttribute attr in type.GetAttributes<CompiledEditorAttribute>())
//                        {
//                            if (attr.DrawnType == null)
//                            {
//                                if (InspectorConfig.Instance.AutoRecompileOnChangesDetected)
//                                {
//                                    //Debug.LogWarning("The type " + attr.DrawnTypeName + " has gone missing.");
//                                    MissingTypeNames.Add(attr.DrawnTypeName);
//                                }

//                                anyMissing = true;
//                            }

//                            if (attr.EditorType == null)
//                            {
//                                if (InspectorConfig.Instance.AutoRecompileOnChangesDetected)
//                                {
//                                    //Debug.LogWarning("The type " + attr.EditorTypeName + " has gone missing.");
//                                    MissingTypeNames.Add(attr.DrawnTypeName);
//                                }

//                                anyMissing = true;
//                            }
//                        }

//                        if (anyMissing)
//                        {
//                            EditorApplication.update += RecompileAsSoonAsPossible;
//                        }
//                        else
//                        {
//                            EditorApplication.update += RecompileAsSoonAsPossibleIfEditorsChanged;
//                        }
//                    }
//                }
//                // Any of the below exceptions indicate that an assembly refresh is in order, likely due to Odin or another .dll having been updated
//                catch (FileNotFoundException)
//                {
//                    EditorApplication.update += RecompileAsSoonAsPossible;
//                }
//                catch (ReflectionTypeLoadException)
//                {
//                    EditorApplication.update += RecompileAsSoonAsPossible;
//                }
//                catch (TypeLoadException)
//                {
//                    EditorApplication.update += RecompileAsSoonAsPossible;
//                }
//            }
//            else
//            {
//                EditorApplication.update += RecompileAsSoonAsPossibleIfAnyEditorsDefined;
//            }
//        }

//        private static void FixUnityAssemblyVersioningResolution()
//        {
//            // If you mark any of Unity's assemblies with the [AssemblyVersion] attribute to get a rolling assembly
//            // version that changes sometimes (or all the time), Odin's hardcoded assembly references to user types
//            // will break.

//            // To fix this case, and all other cases of references to wrongly versioned Unity types not being resolved,
//            // we overload the app domain's type resolution and resolve Unity user assemblies properly regardless of
//            // version.

//            var unityAssemblyPrefixes = new string[]
//            {
//                "Assembly-CSharp-Editor-firstpass",
//                "Assembly-CSharp-firstpass",
//                "Assembly-CSharp-Editor",
//                "Assembly-CSharp",
//                "Assembly-UnityScript-Editor-firstpass",
//                "Assembly-UnityScript-firstpass",
//                "Assembly-UnityScript-Editor",
//                "Assembly-UnityScript",
//                "Assembly-Boo-Editor-firstpass",
//                "Assembly-Boo-firstpass",
//                "Assembly-Boo-Editor",
//                "Assembly-Boo",
//            };

//            AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs args) =>
//            {
//                string name = args.Name;

//                for (int i = 0; i < unityAssemblyPrefixes.Length; i++)
//                {
//                    var prefix = unityAssemblyPrefixes[i];

//                    if (name.StartsWith(prefix))
//                    {
//                        // Remove versioning and other information from name, then resolve basic assembly name
//                        var index = name.IndexOf(',');

//                        if (index >= 0)
//                        {
//                            name = name.Substring(0, index);
//                        }

//                        try
//                        {
//                            return Assembly.Load(name);
//                        }
//                        catch (FileNotFoundException)
//                        {
//                            return null;
//                        }
//                        catch (ReflectionTypeLoadException)
//                        {
//                            return null;
//                        }
//                        catch (TypeLoadException)
//                        {
//                            return null;
//                        }
//                    }
//                }

//                return null;
//            };
//        }

//        private static void OverrideUnityTypeResolutionForMissingEditors()
//        {
//            // Unity will sometimes log a bunch of errors when it fails to resolve any inspected editor types that it
//            // couldn't find, probably because they changed their names or were deleted.
//            //
//            // The below code removes this error message, by changing the AppDomain's type resolution logic and
//            // emitting meaningless types with the correct names on demand so Unity can feel good about itself.
//            //
//            // After writing this monstrosity, *we* certainly don't feel very good about ourselves, but it is the only way.

//            FixUnityAboutWindowBeforeEmit.Fix();

//            ModuleBuilder mb = null;
//            HashSet<string> createdTypes = new HashSet<string>();
//            Type customEditorAttributesType = typeof(CustomEditor).Assembly.GetType("UnityEditor.CustomEditorAttributes");

//            if (customEditorAttributesType == null)
//            {
//                Debug.LogWarning("Odin inspector could not find the internal Unity class 'UnityEditor.CustomEditorAttributes', and so cannot suppress Unity's irrelevant error messages about inspector types going missing. You may see these error messages on recompiles.");
//            }
//            else
//            {
//                ResolveEventHandler typeResolver = (object sender, ResolveEventArgs args) =>
//                {
//                    bool resolve = MissingTypeNames.Contains(args.Name);

//                    if (!resolve)
//                    {
//                        var stackFrames = new System.Diagnostics.StackTrace().GetFrames();

//                        for (int i = 0; i < stackFrames.Length; i++)
//                        {
//                            var method = stackFrames[i].GetMethod();

//                            if (method.Name == "Rebuild" && method.DeclaringType == customEditorAttributesType)
//                            {
//                                resolve = true;
//                                break;
//                            }
//                        }
//                    }

//                    if (!resolve)
//                    {
//                        return null;
//                    }

//                    if (mb == null)
//                    {
//                        var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName() { Name = EMITTED_SUPPRESS_ERROR_TYPE_RESOLVE_ASSEMBLY_NAME }, AssemblyBuilderAccess.Run);
//                        mb = ab.DefineDynamicModule(EMITTED_SUPPRESS_ERROR_TYPE_RESOLVE_ASSEMBLY_NAME);
//                    }

//                    if (!createdTypes.Contains(args.Name))
//                    {
//                        var typeBuilder = mb.DefineType(args.Name, TypeAttributes.Public, typeof(UnityEngine.Object));
//                        typeBuilder.CreateType();

//                        createdTypes.Add(args.Name);
//                    }

//                    return mb.Assembly;
//                };

//                ResolveEventHandler assemblyResolver = (object sender, ResolveEventArgs args) =>
//                {
//                    bool resolve = false;

//                    var stackFrames = new System.Diagnostics.StackTrace().GetFrames();

//                    for (int i = 0; i < stackFrames.Length; i++)
//                    {
//                        var method = stackFrames[i].GetMethod();

//                        if (method.Name == "Rebuild" && method.DeclaringType == customEditorAttributesType)
//                        {
//                            resolve = true;
//                            break;
//                        }
//                    }

//                    if (!resolve)
//                    {
//                        return null;
//                    }

//                    if (mb == null)
//                    {
//                        var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName() { Name = EMITTED_SUPPRESS_ERROR_TYPE_RESOLVE_ASSEMBLY_NAME }, AssemblyBuilderAccess.Run);
//                        mb = ab.DefineDynamicModule(EMITTED_SUPPRESS_ERROR_TYPE_RESOLVE_ASSEMBLY_NAME);
//                    }

//                    return mb.Assembly;
//                };

//                AppDomain.CurrentDomain.TypeResolve += typeResolver;
//                AppDomain.CurrentDomain.AssemblyResolve += assemblyResolver;
//            }
//        }

//        private static void RecompileAsSoonAsPossibleIfEditorsChanged()
//        {
//            if (InspectorConfig.Instance)
//            {
//                if (!Application.isPlaying)
//                {
//                    Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(ass => ass.GetName().Name == SirenixAssetPaths.OdinGeneratedEditorsAssemblyName);
//                    var type = assembly.GetType(GenerationNamespace + "." + EditorContainerName);
//                    var editors = InspectorTypeDrawingConfigDrawer.GetEditorsForCompilation();

//                    var editorAttributes = type.GetAttributes<CompiledEditorAttribute>().ToList();

//                    if (editors.Length != editorAttributes.Count)
//                    {
//                        //Debug.Log("Types inspected by Odin have changed. You may get some Unity error messages about inspected types being null; this is expected. Triggering a generated editors recompile to automatically fix this. (You can stop this behaviour in Window->Odin Preferences->Editor Types)");

//                        TriggerAutomaticRecompile(editors);
//                    }
//                    else
//                    {
//                        for (int i = 0; i < editorAttributes.Count; i++)
//                        {
//                            var attr = editorAttributes[i];

//                            if (attr.EditorType != InspectorConfig.Instance.DrawingConfig.GetEditorType(attr.DrawnType))
//                            {
//                                //Debug.Log("Types inspected by Odin have changed. You may get some Unity error messages about inspected types being null; this is expected. Triggering a generated editors recompile to automatically fix this. (You can stop this behaviour in Window->Odin Preferences->Editor Types)");
//                                TriggerAutomaticRecompile(editors);
//                                break;
//                            }
//                        }
//                    }
//                }

//                EditorApplication.update -= RecompileAsSoonAsPossibleIfEditorsChanged;
//            }
//        }

//        private static void LogEditorRecompilationMessage(TypeDrawerPair[] editors)
//        {
//            if (editors == null)
//            {
//                return;
//            }

//            string msg;
//            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(ass => ass.GetName().Name == SirenixAssetPaths.OdinGeneratedEditorsAssemblyName);
//            if (assembly == null)
//            {
//                msg = "Compiling new " + SirenixAssetPaths.OdinGeneratedEditorsAssemblyName + ".dll, see all generated editor types below:";

//                for (int i = 0; i < editors.Length; i++)
//                {
//                    msg += "\n   Editor Type: " + editors[i].DrawnTypeName + " Editor Name: " + editors[i].EditorTypeName + ".";
//                }
//            }
//            else
//            {
//                msg = "An automatic recompilation of the " + SirenixAssetPaths.OdinGeneratedEditorsAssemblyName + ".dll has occured. More info below";

//                try
//                {
//                    var type = assembly.GetType(GenerationNamespace + "." + EditorContainerName);
//                    var editorAttributes = type.GetAttributes<CompiledEditorAttribute>().ToList();

//                    HashSet<string> newEditors = editors.Select(x =>
//                    {
//                        var t = InspectorTypeDrawingConfig.TypeBinder.BindToType(x.DrawnTypeName);
//                        if (t == null)
//                        {
//                            Debug.LogError("Could not resolve the compiled editor type '" + x.DrawnTypeName + "'.");
//                            return null;
//                        }
//                        return t.GetNiceFullName();
//                    })
//                    .Where(n => n != null)
//                    .ToHashSet();

//                    HashSet<string> oldAttributeTypeNames = editorAttributes.Select(x => x.DrawnTypeName).ToHashSet();

//                    for (int i = 0; i < editorAttributes.Count; i++)
//                    {
//                        if (!newEditors.Contains(editorAttributes[i].DrawnTypeName))
//                        {
//                            msg += "\n   Removed editor for the type " + editorAttributes[i].DrawnTypeName + ".";
//                        }
//                    }

//                    for (int i = 0; i < editors.Length; i++)
//                    {
//                        var drawnType = InspectorTypeDrawingConfig.TypeBinder.BindToType(editors[i].DrawnTypeName);
//                        if (!oldAttributeTypeNames.Contains(drawnType.GetNiceFullName()))
//                        {
//                            msg += "\n   Added editor for the type " + editors[i].DrawnTypeName + ".";
//                        }
//                    }
//                }
//                // Any of the below exceptions indicate that an assembly refresh is in order, likely due to Odin or another .dll having been updated
//                catch (FileNotFoundException ex)
//                {
//                    msg += "\n   Assembly references were invalid or could not be found: " + ex.FileName;
//                }
//                catch (ReflectionTypeLoadException)
//                {
//                    msg += "\n   Some referenced types could not be loaded; ReflectionTypeLoadException.";
//                }
//                catch (TypeLoadException ex)
//                {
//                    msg += "\n   A referenced type could not be loaded: " + ex.TypeName;
//                }
//            }

//            Debug.Log(msg);
//        }

//        private static void RecompileAsSoonAsPossibleIfAnyEditorsDefined()
//        {
//            if (InspectorConfig.Instance)
//            {
//                if (!Application.isPlaying)
//                {
//                    RuntimeHelpers.RunClassConstructor(typeof(InspectorTypeDrawingConfigDrawer).TypeHandle);
//                    var editors = InspectorTypeDrawingConfigDrawer.GetEditorsForCompilation();

//                    if (editors.Length > 0)
//                    {
//                        //Debug.Log("Generating Odin editor assembly because a pre-existing editor assembly wasn't detected. (You can stop this behaviour in Window->Odin Preferences->Editor Types)");
//                        TriggerAutomaticRecompile(editors);
//                    }
//                }

//                EditorApplication.update -= RecompileAsSoonAsPossibleIfAnyEditorsDefined;
//            }
//        }

//        private static void RecompileAsSoonAsPossible()
//        {
//            if (InspectorConfig.Instance)
//            {
//                if (!Application.isPlaying)
//                {
//                    //Debug.Log("Types inspected by Odin have changed. You may get some Unity error messages about inspected types being null; this is expected. Triggering a generated editors recompile to automatically fix this. (You can stop this behaviour in Window->Odin Preferences->Editor Types)");
//                    TriggerAutomaticRecompile();
//                    EditorApplication.update -= RecompileAsSoonAsPossible;
//                }
//            }
//        }

//        private static void TriggerAutomaticRecompile(TypeDrawerPair[] editors = null)
//        {
//            if (InspectorConfig.Instance.AutoRecompileOnChangesDetected)
//            {
//                if (EditorPrefs.HasKey("PREVENT_SIRENIX_FILE_GENERATION"))
//                {
//                    Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(ass => ass.GetName().Name == SirenixAssetPaths.OdinGeneratedEditorsAssemblyName);

//                    Type type = assembly == null ? null : assembly.GetType(GenerationNamespace + "." + EditorContainerName);
//                    List<CompiledEditorAttribute> editorAttributes = type == null ? null : type.GetAttributes<CompiledEditorAttribute>().ToList();

//                    if (editorAttributes == null || editorAttributes.Count != 0)
//                    {
//                        Debug.LogWarning(SirenixAssetPaths.OdinGeneratedEditorsAssemblyName + " was generated with 0 editors declared because the PREVENT_SIRENIX_FILE_GENERATION key was defined in Unity's EditorPrefs.");
//                        CompileEditors(new TypeDrawerPair[0]);
//                    }
//                    else
//                    {
//                        Debug.LogWarning(SirenixAssetPaths.OdinGeneratedEditorsAssemblyName + " was prevented from being regenerated because the PREVENT_SIRENIX_FILE_GENERATION key was defined in Unity's EditorPrefs.");
//                    }

//                    return;
//                }

//                CompileEditors(editors ?? InspectorTypeDrawingConfigDrawer.GetEditorsForCompilation());
//            }
//            else
//            {
//                //Debug.LogWarning("Types inspected by Odin have changed. You may get some Unity error messages about inspected types being null. To fix this, go to Window->Odin Preferences->Editor Types and recompile editors. (You can enable automatic recompilation in Window->Odin Preferences->Editor Types)");
//            }
//        }

//        /// <summary>
//        /// <para>Compiles a given set of editors into a .dll in the assembly. The path to the .dll is determined using <see cref="SirenixAssetPaths.OdinGeneratedEditorsPath"/>.</para>
//        /// <para>If editor compilation fails, the generated compilation files will be kept in a randomly named folder in the Temp folder of the current Unity project. The precise path, and compilation errors, will be logged to the console.</para>
//        /// </summary>
//        public static bool CompileEditors(TypeDrawerPair[] editors)
//        {
//            if (editors == null)
//            {
//                throw new ArgumentNullException("editors");
//            }

//            if (InspectorConfig.Instance.EnableEditorGenerationLogging)
//            {
//                LogEditorRecompilationMessage(editors);
//            }

//            using (AssemblyGenerator generator = new AssemblyGenerator())
//            {
//                generator.DefaultNamespace = GenerationNamespace;
//                generator.ReferenceUnityAssemblies = true;
//                generator.ReferenceUserScriptAssemblies = true;
//                generator.IncludeEditorAssemblies = true;
//                generator.IncludeNonPluginUserScriptAssemblies = true;
//                generator.IncludeActiveUnityDefines = true;
//                generator.KeepFiles = true;
//                generator.KeepFilesOnError = true;
//                generator.LogErrors = true;

//                if (editors.Length > 0)
//                {
//                    generator.AddReferencedAssembly(typeof(OdinEditor).Assembly);
//                }

//                HashSet<Type> excludeTypes = new HashSet<Type>();

//                Dictionary<Assembly, string> externAliases = new Dictionary<Assembly, string>();
//                List<Type> invalidTypeNames = new List<Type>();
//                Dictionary<Assembly, List<Type>> invalidAssemblies = new Dictionary<Assembly, List<Type>>();

//                // Create extern aliases for all assemblies to prevent type name conflicts during compilation
//                {
//                    foreach (var typeDrawerPair in editors)
//                    {
//                        Type drawnType = InspectorTypeDrawingConfig.TypeBinder.BindToType(typeDrawerPair.DrawnTypeName);
//                        Type editorType = InspectorTypeDrawingConfig.TypeBinder.BindToType(typeDrawerPair.EditorTypeName);

//                        if (drawnType == null || editorType == null)
//                        {
//                            continue;
//                        }

//                        if (drawnType.Assembly.GetName().Name.Contains(" ")) // Unity's version of Mono has errors for assemblies with spaces in their names
//                        {
//                            List<Type> types;

//                            if (!invalidAssemblies.TryGetValue(drawnType.Assembly, out types))
//                            {
//                                types = new List<Type>();
//                                invalidAssemblies.Add(drawnType.Assembly, types);
//                            }

//                            types.Add(drawnType);
//                            excludeTypes.Add(drawnType);
//                            continue;
//                        }
//                        else if (!TypeExtensions.IsValidIdentifier(drawnType.FullName))
//                        {
//                            invalidTypeNames.Add(drawnType);
//                            excludeTypes.Add(drawnType);
//                            continue;
//                        }

//                        if (!externAliases.ContainsKey(drawnType.Assembly))
//                        {
//                            string alias = "Assembly" + Guid.NewGuid().ToString("N");
//                            externAliases[drawnType.Assembly] = alias;
//                            generator.AddExternAlias(alias, drawnType.Assembly);
//                        }
//                    }
//                }

//                // Write CompiledEditorContainer class, which ensures that we can check up on when
//                // drawn types or base editors are no longer there, and trigger a recompile.
//                {
//                    CodeWriter writer = generator.CreateCodeWriter(EditorContainerName);

//                    if (editors.Length > 0)
//                    {
//                        writer.RegisterTypeSeen(typeof(CompiledEditorAttribute));
//                    }

//                    foreach (TypeDrawerPair typeDrawerPair in editors)
//                    {
//                        Type drawnType = InspectorTypeDrawingConfig.TypeBinder.BindToType(typeDrawerPair.DrawnTypeName);
//                        Type editorType = InspectorTypeDrawingConfig.TypeBinder.BindToType(typeDrawerPair.EditorTypeName);

//                        if (drawnType == null || editorType == null || excludeTypes.Contains(drawnType))
//                        {
//                            continue;
//                        }

//                        generator.AddReferencedAssembly(drawnType.Assembly);
//                        generator.AddReferencedAssembly(editorType.Assembly);

//                        string drawnTypeName = externAliases[drawnType.Assembly] + "::" + drawnType.GetNiceFullName();
//                        writer.AddExternAlias(externAliases[drawnType.Assembly]);

//                        writer.WriteAttribute(typeof(CompiledEditorAttribute), "typeof(" + drawnTypeName + "), typeof(" + editorType.GetNiceFullName() + "), \"" + drawnType.GetNiceFullName() + "\", \"" + editorType.GetNiceFullName() + "\"");
//                    }

//                    writer.BeginType(AccessModifier.Internal, TypeDeclaration.StaticClass, EditorContainerName);
//                    writer.EndType();
//                }

//                // Write drawers
//                if (editors.Length > 0)
//                {
//                    CodeWriter writer = generator.CreateCodeWriter("Drawers");

//                    writer.RegisterTypeSeen(typeof(CompilerGeneratedAttribute));
//                    writer.RegisterTypeSeen(typeof(CustomEditor));

//                    foreach (TypeDrawerPair typeDrawerPair in editors)
//                    {
//                        Type drawnType = InspectorTypeDrawingConfig.TypeBinder.BindToType(typeDrawerPair.DrawnTypeName);
//                        Type editorType = InspectorTypeDrawingConfig.TypeBinder.BindToType(typeDrawerPair.EditorTypeName);

//                        if (drawnType != null && editorType != null && !drawnType.IsGenericTypeDefinition && InspectorTypeDrawingConfig.UnityInspectorEditorIsValidBase(editorType, drawnType) && !excludeTypes.Contains(drawnType))
//                        {
//                            generator.AddReferencedAssembly(drawnType.Assembly);
//                            generator.AddReferencedAssembly(editorType.Assembly);

//                            string drawnTypeName = externAliases[drawnType.Assembly] + "::" + drawnType.GetNiceFullName();

//                            writer.AddExternAlias(externAliases[drawnType.Assembly]);

//                            writer.PasteChunk(InspectorAttributesCode.Replace(DrawnTypeKey, drawnTypeName));

//                            if (editorType.IsDefined<CanEditMultipleObjects>())
//                            {
//                                writer.WriteAttribute(typeof(CanEditMultipleObjects));
//                            }

//                            string editorNamePrefix;

//                            if (drawnType.IsGenericType)
//                            {
//                                editorNamePrefix = drawnTypeName.Replace(" ", "")
//                                                                .Replace(",", "_")
//                                                                .Replace("<", "_")
//                                                                .Replace(">", "")
//                                                                .Replace("::", "_");
//                            }
//                            else
//                            {
//                                editorNamePrefix = drawnTypeName.Replace('.', '_')
//                                                                .Replace("::", "_");
//                            }

//                            writer.PasteChunk(InspectorClassCode.Replace(EditorNamePrefixKey, editorNamePrefix).Replace(BaseEditorTypeKey, editorType.GetNiceFullName()));
//                        }
//                    }
//                }

//                // Give errors for conflicts
//                if (!InspectorConfig.Instance.DisableTypeNameConflictWarning)
//                {
//                    HashSet<string> duplicateTypeNames = new HashSet<string>();

//                    // Find duplicates
//                    {
//                        HashSet<string> seenNames = new HashSet<string>();

//                        foreach (TypeDrawerPair typeDrawerPair in editors)
//                        {
//                            Type drawnType = InspectorTypeDrawingConfig.TypeBinder.BindToType(typeDrawerPair.DrawnTypeName);

//                            if (drawnType == null) continue;

//                            if (seenNames.Contains(drawnType.FullName))
//                            {
//                                duplicateTypeNames.Add(drawnType.FullName);
//                            }
//                            else
//                            {
//                                seenNames.Add(drawnType.FullName);
//                            }
//                        }
//                    }

//                    if (duplicateTypeNames.Count > 0)
//                    {
//                        Debug.LogWarning("Warning: There were type name collisions during generated editors compilation. Odin is capable of compiling editors with type name collisions, but bugs in Unity's handling of editors means that Unity is unlikely to draw Odin for more than one of the types with the same name. The following " + duplicateTypeNames.Count + " warnings detail the type names in question:  (You can disable this warning in the Editor Types preferences)");

//                        foreach (var typeName in duplicateTypeNames)
//                        {
//                            var duplicateTypes = editors.Where(n => n.DrawnTypeName != null)
//                                                        .Select(n => InspectorTypeDrawingConfig.TypeBinder.BindToType(n.DrawnTypeName))
//                                                        .Where(n => n != null && n.FullName == typeName)
//                                                        .ToList();

//                            string warningMessage =
//                                "        " + duplicateTypes.Count + " types with the full name '" + typeName + "' exist in the following " +
//                                "assemblies: " + string.Join(", ", duplicateTypes.Select(n => "\"" + n.Assembly.FullName + "\"").ToArray());

//                            Debug.LogWarning(warningMessage);
//                        }
//                    }
//                }

//                // Give errors for reserved names
//                if (invalidTypeNames.Count > 0)
//                {
//                    Debug.LogWarning("Warning: Attempted to generate editors for types with invalid names. The types have been disabled in future editor compiles. Odin is unable to generate editors for any types with invalid names. The following " + invalidTypeNames.Count + " warnings detail the names in question:");

//                    foreach (var type in invalidTypeNames)
//                    {
//                        InspectorConfig.Instance.DrawingConfig.SetEditorType(type, null);
//                        Debug.LogWarning("        A type with the invalid name identifier '" + type.FullName + "' was declared in the assembly '" + type.Assembly.FullName + "'.");
//                    }

//                    EditorUtility.SetDirty(InspectorConfig.Instance);
//                    AssetDatabase.SaveAssets();
//                    InspectorTypeDrawingConfigDrawer.UpdateRootGroupConflicts();
//                }

//                // Give errors for types in invalid assemblies
//                if (invalidAssemblies.Count > 0)
//                {
//                    var types = invalidAssemblies.SelectMany(n => n.Value).ToList();

//                    Debug.LogWarning("Warning: Attempted to generate editors for types in invalid assemblies (assemblies with names that have spaces in them). The types have been disabled in future editor compiles. Errors in Unity's version of Mono means that you cannot have custom editors for types that are in assemblies with spaces in their names. The following " + types.Count + " warnings detail the types in question:");

//                    foreach (var type in types)
//                    {
//                        InspectorConfig.Instance.DrawingConfig.SetEditorType(type, null);
//                        Debug.LogWarning("        The type '" + type.GetNiceFullName() + "' in the invalid assembly '" + type.Assembly.GetName().Name + "' has been disabled.");
//                    }

//                    EditorUtility.SetDirty(InspectorConfig.Instance);
//                    AssetDatabase.SaveAssets();
//                    InspectorTypeDrawingConfigDrawer.UpdateRootGroupConflicts();
//                }

//                var result = generator.Compile(SirenixAssetPaths.OdinGeneratedEditorsPath);

//                if (result)
//                {
//                    AssetDatabase.Refresh();
//                }

//                return result;
//            }
//        }
//    }
//}
#endif