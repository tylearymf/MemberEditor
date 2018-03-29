#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="AssemblyGenerator.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities.Editor.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Utilities;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public sealed class AssemblyGenerator : IDisposable
    {
        private Dictionary<string, CodeWriter> codeWriters = new Dictionary<string, CodeWriter>();

        private HashSet<string> referencedAssemblies = new HashSet<string>();
        private Dictionary<string, string> externAliases = new Dictionary<string, string>();

        private HashSet<string> defines = new HashSet<string>();

        private bool isDisposed;
        private bool hasCompiled;
        private bool referenceUnityAssemblies = true;
        private bool referenceUserScriptAssemblies = true;
        private bool includeNonPluginUserScriptAssemblies = true;
        private bool includeEditorAssemblies = false;
        private bool logErrors = true;
        private bool keepFiles = false;
        private bool keepFilesOnError = true;
        private bool includeActiveUnityDefines = true;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public AssemblyGenerator()
        {
            if (!UnityVersion.IsVersionOrGreater(5, 5))
            {
                // Make sure mscorlib is always referenced
                this.referencedAssemblies.Add(typeof(string).Assembly.Location);
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public string DefaultNamespace { get; set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool ReferenceUserScriptAssemblies
        {
            get { return this.referenceUserScriptAssemblies; }
            set { this.referenceUserScriptAssemblies = value; }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool ReferenceUnityAssemblies
        {
            get { return this.referenceUnityAssemblies; }
            set { this.referenceUnityAssemblies = value; }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool IncludeEditorAssemblies
        {
            get { return this.includeEditorAssemblies; }
            set { this.includeEditorAssemblies = value; }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool IncludeNonPluginUserScriptAssemblies
        {
            get { return this.includeNonPluginUserScriptAssemblies; }
            set { this.includeNonPluginUserScriptAssemblies = value; }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool LogErrors
        {
            get { return this.logErrors; }
            set { this.logErrors = value; }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool KeepFilesOnError
        {
            get { return this.keepFilesOnError; }
            set { this.keepFilesOnError = value; }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool KeepFiles
        {
            get { return this.keepFiles; }
            set { this.keepFiles = value; }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool IncludeActiveUnityDefines
        {
            get { return this.includeActiveUnityDefines; }
            set { this.includeActiveUnityDefines = value; }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void AddDefine(string define)
        {
            if (define == null)
            {
                throw new ArgumentNullException("define");
            }

            this.defines.Add(define);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void AddExternAlias(string alias, Assembly assembly)
        {
            if (alias == null)
            {
                throw new ArgumentNullException("alias");
            }

            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            this.CheckNotCompiledOrDisposed();
            this.externAliases.Add(alias, assembly.Location);
            this.AddReferencedAssembly(assembly);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void AddExternAlias(string alias, string assemblyPath)
        {
            if (alias == null)
            {
                throw new ArgumentNullException("alias");
            }

            if (assemblyPath == null)
            {
                throw new ArgumentNullException("assemblyPath");
            }

            this.CheckNotCompiledOrDisposed();

            this.externAliases.Add(alias, assemblyPath);
            this.AddReferencedAssembly(assemblyPath);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void AddReferencedAssembly(Assembly assembly)
        {
            this.CheckNotCompiledOrDisposed();

            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            if (UnityVersion.IsVersionOrGreater(5, 5))
            {
                var assName = assembly.GetName().Name;

                switch (assName)
                {
                    // In 5.5 or greater, Unity automatically adds these, always, without checking first whether they're already there
                    // Hence, we never add them; this list was simply arrived at by trial and error

                    case "mscorlib":
                    case "System.Runtime.Serialization":
                    case "System.Xml.Linq":
                    case "UnityScript":
                    case "UnityScript.Lang":
                    case "Boo.Lang":
                    case "System":
                    case "System.Xml":
                    case "System.Core":
                        return;
                }
            }

            if (!this.referencedAssemblies.Contains(assembly.Location))
            {
                this.referencedAssemblies.Add(assembly.Location);

                foreach (var name in assembly.GetReferencedAssemblies())
                {
                    Assembly referencedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                                                                         .FirstOrDefault(ass => ass.GetName().Name == name.Name);

                    if (referencedAssembly == null)
                    {
                        //Debug.LogWarning("Assembly dependency " + name.Name + " with url '" + name.CodeBase + "' was not loaded in the current AppDomain; trying to load so further dependencies can be resolved.");

                        try
                        {
                            referencedAssembly = Assembly.Load(name);
                        }
                        catch
                        {
                            // Ignore errors here; we give an error message further down if this went wrong
                        }
                    }

                    if (referencedAssembly == null)
                    {
                        Debug.LogError("Failed to find assembly dependency '" + name.FullName + "'! Editor compilation may fail!");
                        continue;
                    }

                    this.AddReferencedAssembly(referencedAssembly);
                }
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void AddReferencedAssembly(string assemblyPath)
        {
            this.CheckNotCompiledOrDisposed();

            if (assemblyPath == null)
            {
                throw new ArgumentNullException("assemblyPath");
            }

            this.referencedAssemblies.Add(assemblyPath);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool Compile(string assemblyPath, bool isUnityAssetsPath = true)
        {
            if (assemblyPath == null)
            {
                throw new ArgumentNullException("assemblyPath");
            }

            this.CheckNotCompiledOrDisposed();
            this.hasCompiled = true;

            if (isUnityAssetsPath)
            {
                assemblyPath = Application.dataPath.TrimEnd('/') + "/" + assemblyPath;
            }

            DirectoryInfo assemblyDir = new DirectoryInfo(Path.GetDirectoryName(assemblyPath));

            if (!assemblyDir.Exists)
            {
                assemblyDir.Create();
            }

            // Project/Assets folder
            DirectoryInfo assetsDir = new DirectoryInfo(Application.dataPath);

            // Project folder
            string projectFolderPath = assetsDir.Parent.FullName + "/";
            string tempPath = projectFolderPath + "Temp/CodeGeneration-" + Guid.NewGuid().ToString() + "/";
            Directory.CreateDirectory(tempPath);

            bool shouldKeepFiles = this.keepFiles;

            try
            {
                if (this.referenceUnityAssemblies)
                {
                    if (this.includeEditorAssemblies)
                    {
                        if (!UnityVersion.IsVersionOrGreater(5, 5))
                        {
                            this.referencedAssemblies.AddRange(AppDomain.CurrentDomain
                                                                        .GetAssemblies()
                                                                        .Where(ass => ass.GetName().Name.StartsWith("Unity"))
                                                                        .Select(ass => ass.Location));
                        }
                    }
                    else
                    {
                        if (!UnityVersion.IsVersionOrGreater(5, 5))
                        {
                            this.referencedAssemblies.AddRange(AppDomain.CurrentDomain
                                                                        .GetAssemblies()
                                                                        .Where(ass => ass.GetName().Name.StartsWith("UnityEngine"))
                                                                        .Select(ass => ass.Location));
                        }
                    }
                }

                DirectoryInfo userScriptAssembliesFolder = new DirectoryInfo(projectFolderPath + "Library/ScriptAssemblies");

                if (this.referenceUserScriptAssemblies && userScriptAssembliesFolder.Exists)
                {
                    FileInfo[] files = userScriptAssembliesFolder.GetFiles("*.dll", SearchOption.AllDirectories);

                    for (int i = 0; i < files.Length; i++)
                    {
                        FileInfo file = files[i];

                        if (file.Extension.ToLowerInvariant() != ".dll")
                        {
                            continue;
                        }

                        bool isEditor = file.Name.Contains("Editor");
                        bool isPlugin = file.Name.Contains("firstpass");

                        bool include = (isPlugin || this.includeNonPluginUserScriptAssemblies)
                                    && (!isEditor || this.includeEditorAssemblies);

                        if (include)
                        {
                            this.referencedAssemblies.Add(file.FullName);
                        }
                    }
                }

                if (this.includeActiveUnityDefines)
                {
                    this.defines.AddRange(EditorUserBuildSettings.activeScriptCompilationDefines);
                }

                string[] filePaths = new string[this.codeWriters.Count];

                int counter = 0;

                foreach (var pair in this.codeWriters)
                {
                    string path = tempPath + pair.Key;
                    filePaths[counter++] = path;

                    using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        string code = pair.Value.GetFinalCode();
                        writer.Write(code);
                    }
                }

                var arr = this.referencedAssemblies.Prepend(this.externAliases.Select(n => n.Key + "=" + n.Value))
                                                   .ToArray();

                //Debug.Log("Referenced assemblies...");
                //arr.ForEach(n => Debug.Log(n));

                string[] messages = EditorUtility.CompileCSharp(filePaths, arr, this.defines.ToArray(), assemblyPath);

                if (messages.Length == 0)
                {
                    return true;
                }

                if (this.keepFilesOnError)
                {
                    shouldKeepFiles = true;
                }

                if (this.logErrors)
                {
                    Debug.LogWarning("The following " + messages.Length + " errors occurred while compiling a generated assembly at '" + assemblyPath + "':");

                    if (this.keepFilesOnError)
                    {
                        Debug.LogWarning("        ( You can view the faulty source code files at the path '" + tempPath + "'. They will remain until Unity is restarted. )");
                    }

                    for (int i = 0; i < messages.Length; i++)
                    {
                        Debug.LogError("        " + messages[i]);
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.Log("The following exception was thrown while compiling generated code:");
                Debug.LogException(ex);
                return false;
            }
            finally
            {
                if (!shouldKeepFiles)
                {
                    Directory.Delete(tempPath, true);
                }

                foreach (var writer in this.codeWriters.Values)
                {
                    writer.Dispose();
                }
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public CodeWriter CreateCodeWriter(string fileName)
        {
            this.CheckNotCompiledOrDisposed();

            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            if (!fileName.EndsWith(".cs", StringComparison.InvariantCultureIgnoreCase))
            {
                fileName = fileName + ".cs";
            }

            if (this.codeWriters.ContainsKey(fileName))
            {
                throw new InvalidOperationException("Cannot create a code writer for the filename '" + fileName + "'; filename already exists in the assembly generator.");
            }

            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            var result = new CodeWriter(writer);
            this.codeWriters[fileName] = result;

            result.Namespace = this.DefaultNamespace;

            return result;
        }

        private void CheckNotCompiledOrDisposed()
        {
            if (this.isDisposed)
            {
                throw new InvalidOperationException("This assembly generator has been disposed and can no longer be used.");
            }

            if (this.hasCompiled)
            {
                throw new InvalidOperationException("This assembly generator has already performed a compilation.");
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;

                foreach (var writer in this.codeWriters.Values)
                {
                    writer.Dispose();
                }
            }
        }
    }
}
#endif