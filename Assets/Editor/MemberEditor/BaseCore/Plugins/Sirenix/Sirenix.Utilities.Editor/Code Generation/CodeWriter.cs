#if UNITY_EDITOR
//-----------------------------------------------------------------------
// <copyright file="CodeWriter.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Sirenix.Utilities.Editor.CodeGeneration
{
    using System.Globalization;
    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Not yet documented.
    /// </summary>
    public sealed class CodeWriter : IDisposable
    {
        // Using a random GUID ensures that no matter what people write, they won't accidentally write a StartPlaceholder.
        private static readonly string StartPlaceholder = Guid.NewGuid().ToString();

        private string code;
        private bool disposed = false;
        private bool finalized;
        private bool hasBegun;
        private HashSet<string> seenNamespaces = new HashSet<string>();
        private HashSet<Type> seenTypes = new HashSet<Type>();
        private HashSet<string> externAliases = new HashSet<string>();

        private int segments;
        private StringBuilder stringBuilder = new StringBuilder();
        private TextWriter writer;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public CodeWriter()
            : this(new StreamWriter(new MemoryStream()))
        {
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public CodeWriter(Stream stream)
            : this(new StreamWriter(stream))
        {
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public CodeWriter(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            this.IndentWithTabs = false;
            this.SpacesPerIndent = 4;
            this.writer = writer;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public int Indent { get; set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public bool IndentWithTabs { get; set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public int SpacesPerIndent { get; set; }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void AddExternAlias(string alias)
        {
            if (alias == null)
            {
                throw new ArgumentNullException("alias");
            }

            this.externAliases.Add(alias);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void BeginGetter(AccessModifier? access = null)
        {
            if (access != null)
            {
                this.Write(access.Value.Stringify() + " ");
            }

            this.WriteLine("get");
            this.BeginSegment();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void BeginConstructor(AccessModifier access, string typeName)
        {
            this.BeginConstructor(access, typeName, null, new Type[0], new string[0]);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void BeginConstructor(AccessModifier access, string typeName, string baseString)
        {
            this.BeginConstructor(access, typeName, baseString, new Type[0], new string[0]);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void BeginConstructor(AccessModifier access, string typeName, string baseString, Type[] args, string[] argNames)
        {
            if (args == null || argNames == null || typeName == null)
            {
                throw new ArgumentNullException();
            }

            if (args.Length != argNames.Length)
            {
                throw new ArgumentException("Length of args and argNames must be the same.");
            }

            this.seenTypes.AddRange(args);

            var accessStr = access.Stringify();

            this.WriteLine(string.Join("", new string[]
            {
                accessStr + " ",
                typeName,
                CodeGenerationUtilities.PrintAsParameters(args, argNames, true, false)
            }));

            if (baseString != null)
            {
                this.NewLine();
                this.Indent++;
                this.Write(baseString);
                this.Indent--;
            }

            this.BeginSegment();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void EndConstructor()
        {
            this.EndSegment();
            this.EmptyLine();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void BeginMethod(AccessModifier access, Type returnType, string name)
        {
            this.BeginMethod(access, returnType, name, new Type[0], new string[0]);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void BeginMethod(AccessModifier access, Type returnType, string name, Type[] args, string[] argNames)
        {
            this.BeginMethod(access, returnType, name, args, argNames, false, false, false);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void BeginMethod(AccessModifier access, Type returnType, string name, bool isStatic, bool isOverride)
        {
            this.BeginMethod(access, returnType, name, new Type[0], new string[0], isStatic, isOverride, false);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void BeginMethod(AccessModifier access, Type returnType, string name, Type[] args, string[] argNames, bool isStatic, bool isOverride, bool isExtension)
        {
            if (string.IsNullOrEmpty(name) || args == null || argNames == null)
            {
                throw new ArgumentNullException();
            }

            if (args.Length != argNames.Length)
            {
                throw new ArgumentException("Length of args and argNames must be the same.");
            }

            if (isStatic && isOverride)
            {
                throw new ArgumentException("Method cannot be both static and override.");
            }

            if (isExtension && args.Length == 0)
            {
                throw new ArgumentException("Extension methods must have at least one argument.");
            }

            if (returnType == typeof(void))
            {
                returnType = null;
            }

            if (returnType != null)
            {
                this.seenTypes.Add(returnType);
            }

            this.seenTypes.AddRange(args);

            var accessStr = access.Stringify();

            if (isStatic)
            {
                accessStr += " static";
            }

            if (isOverride)
            {
                accessStr += " override";
            }

            this.WriteLine(string.Join("", new string[]
            {
                accessStr + " ",
                returnType == null ? "void " : returnType.GetNiceName() + " ",
                name,
                CodeGenerationUtilities.PrintAsParameters(args, argNames, true, isExtension)
            }));

            this.BeginSegment();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void BeginProperty(AccessModifier access, Type propertyType, string name, bool isStatic = false)
        {
            if (propertyType == null || string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException();
            }

            this.seenTypes.Add(propertyType);

            this.WriteLine(string.Join(" ", new string[]
            {
                access.Stringify() + (isStatic ? " static" : ""),
                propertyType.GetNiceName(),
                name
            }));

            this.BeginSegment();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void BeginSegment()
        {
            this.WriteLine("{");
            this.Indent++;
            this.segments++;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void BeginSetter(AccessModifier? access = null)
        {
            this.EnsureHasBegun();

            if (access != null)
            {
                this.Write(access.Value.Stringify() + " ");
            }

            this.WriteLine("set");
            this.BeginSegment();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.GetFinalCode();
                this.writer.Dispose();

                this.seenTypes = null;
                this.seenNamespaces = null;
                this.disposed = true;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void EmptyLine()
        {
            this.WriteLine("");
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void EndGetter()
        {
            this.EndSegment();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void EndMethod()
        {
            this.EndSegment();
            this.EmptyLine();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void EndProperty()
        {
            this.EndSegment();
            this.EmptyLine();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void EndSegment(string add = null)
        {
            if (this.segments <= 0)
            {
                throw new InvalidOperationException("Cannot end a segment when there are no more segments to end!");
            }

            this.Indent--;
            this.segments--;
            this.WriteLine("}" + add ?? "");
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void EndSetter()
        {
            this.EndSegment();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public string GetFinalCode()
        {
            if (this.finalized)
            {
                return this.code;
            }

            while (this.segments > 0)
            {
                this.EndSegment();
            }

            foreach (var type in this.seenTypes)
            {
                if (type.Namespace != null)
                {
                    this.seenNamespaces.Add(type.Namespace);
                }

                if (type.IsGenericType)
                {
                    this.ProcessGenericType(type);
                }
            }

            StringBuilder prev = this.stringBuilder;
            this.stringBuilder = new StringBuilder();

            this.Indent = this.Namespace == null ? 0 : 1;

            foreach (var alias in this.externAliases)
            {
                this.WriteLine("extern alias " + alias + ";");
            }

            foreach (var name in this.seenNamespaces)
            {
                if (name != this.Namespace)
                {
                    this.WriteLine("using " + name + ";");
                }
            }

            prev.Replace(StartPlaceholder, this.stringBuilder.ToString());

            this.stringBuilder = prev;

            this.code = this.stringBuilder.ToString();
            this.writer.Write(this.code);
            this.finalized = true;

            return this.code;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void BeginType(AccessModifier accessModifier, TypeDeclaration declaration, string typeName)
        {
            this.BeginType(accessModifier, declaration, typeName, null, null);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void BeginType(AccessModifier accessModifier, TypeDeclaration declaration, string typeName, Type inheritsFrom, params Type[] implementInterfaces)
        {
            if (typeName.IsNullOrWhitespace())
            {
                throw new ArgumentNullException("className");
            }

            if (implementInterfaces != null)
            {
                for (int i = 0; i < implementInterfaces.Length; i++)
                {
                    Type @interface = implementInterfaces[i];

                    if (@interface == null)
                    {
                        throw new ArgumentNullException("interfaces at index " + i);
                    }

                    if (!@interface.IsInterface)
                    {
                        throw new ArgumentException("The interface type " + @interface.FullName + " at index " + i + " is not an interface.");
                    }
                }
            }

            this.WriteLineIndentation();
            this.Write(accessModifier.Stringify());

            switch (declaration)
            {
                case TypeDeclaration.Class:
                    this.Write(" class ");
                    break;

                case TypeDeclaration.StaticClass:
                    this.Write(" static class ");
                    break;

                case TypeDeclaration.SealedClass:
                    this.Write(" sealed class ");
                    break;

                case TypeDeclaration.Struct:
                    this.Write(" struct ");
                    break;

                default:
                    throw new NotImplementedException(declaration.ToString());
            }

            this.Write(typeName);

            if (inheritsFrom != null || (implementInterfaces != null && implementInterfaces.Length > 0))
            {
                this.Write(" : ");
            }

            if (inheritsFrom != null)
            {
                this.RegisterTypeSeen(inheritsFrom);
                this.Write(inheritsFrom.GetNiceName());
            }

            if (implementInterfaces != null && implementInterfaces.Length > 0)
            {
                this.RegisterTypesSeen(implementInterfaces);

                for (int i = 0; i < implementInterfaces.Length; i++)
                {
                    if (i > 0 || inheritsFrom != null)
                    {
                        this.Write(", ");
                    }

                    this.Write(implementInterfaces[i].GetNiceName());
                }
            }

            this.NewLine();
            this.BeginSegment();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void EndType()
        {
            this.EndSegment();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void PasteChunk(string chunk)
        {
            var lines = chunk.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                var trimmed = line.TrimStart('\t', ' ');

                if (trimmed.StartsWith("{", StringComparison.InvariantCulture) || trimmed.EndsWith("{", StringComparison.InvariantCulture))
                {
                    this.BeginSegment();
                }
                else if (trimmed.StartsWith("}", StringComparison.InvariantCulture))
                {
                    this.EndSegment();
                }
                else if (trimmed.StartsWith(": base", StringComparison.InvariantCulture))
                {
                    this.Indent++;
                    this.WriteLine(trimmed);
                    this.Indent--;
                }
                else
                {
                    this.WriteLine(trimmed);
                }
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void UseNamespace(string nameSpace)
        {
            this.seenNamespaces.Add(nameSpace);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void RegisterTypeSeen(Type type)
        {
            this.seenTypes.Add(type);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void RegisterTypesSeen(IEnumerable<Type> types)
        {
            this.seenTypes.AddRange(types);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void RegisterTypesSeen(params Type[] types)
        {
            this.seenTypes.AddRange(types);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void Write(string content)
        {
            this.EnsureHasBegun();

            this.stringBuilder.Append(content);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void WriteAttribute(Type attributeType, string paramsContent = null, params Type[] usingExtraTypes)
        {
            if (attributeType == null || usingExtraTypes == null)
            {
                throw new ArgumentNullException();
            }

            if (attributeType.ImplementsOrInherits(typeof(Attribute)) == false)
            {
                throw new ArgumentException("Type " + attributeType.Name + " is not an attribute.");
            }

            this.seenTypes.Add(attributeType);
            this.seenTypes.AddRange(usingExtraTypes);

            this.WriteLine(
                "["
                + attributeType.GetNiceName()
                + (paramsContent != null ? "(" + paramsContent + ")" : "")
                + "]");
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void WriteAutoProperty(AccessModifier access, Type propertyType, string name, AccessModifier? writeAccess)
        {
            if (propertyType == null || string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException();
            }

            this.seenTypes.Add(propertyType);

            this.WriteLine(string.Concat(new string[]
            {
                access.Stringify() + " ",
                propertyType.GetNiceName() + " ",
                name + " ",
                "{ get; ",
                writeAccess != null ? writeAccess.Value.Stringify() + " " : "",
                "set; }",
                Environment.NewLine
            }));

            this.EmptyLine();
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void WriteComment(string content)
        {
            this.WriteLine("// " + content);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void WriteField(AccessModifier access, Type fieldType, string name)
        {
            if (fieldType == null || string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException();
            }

            this.seenTypes.Add(fieldType);

            this.WriteLine(string.Join(" ", new string[]
            {
                access.Stringify(),
                fieldType.GetNiceName(),
                name
            }) + ";");
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void WriteLine(string content, params object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                object arg = args[i];

                Type t = arg as Type;

                if (t != null)
                {
                    this.RegisterTypeSeen(t);
                    args[i] = t.GetNiceName();
                    continue;
                }

                MemberInfo memberInfo = arg as MemberInfo;

                if (memberInfo != null)
                {
                    args[i] = memberInfo.Name;
                    continue;
                }
            }

            string formatted = string.Format(CultureInfo.InvariantCulture, content, args);
            this.WriteLine(formatted);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void WriteLineIndentation()
        {
            this.EnsureHasBegun();

            if (this.IndentWithTabs)
            {
                for (int i = 0; i < this.Indent; i++)
                {
                    this.stringBuilder.Append('\t');
                }
            }
            else
            {
                for (int i = 0; i < this.Indent * this.SpacesPerIndent; i++)
                {
                    this.stringBuilder.Append(' ');
                }
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void NewLine()
        {
            this.EnsureHasBegun();
            this.stringBuilder.Append(Environment.NewLine);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void WriteLine(string content)
        {
            this.WriteLineIndentation();

            this.stringBuilder.Append(content);
            this.stringBuilder.Append(Environment.NewLine);
        }

        private void CheckNotFinalized()
        {
            if (this.finalized)
            {
                throw new InvalidOperationException("The CodeWriter has been finalized and no code can be written any more.");
            }
        }

        private void EnsureHasBegun()
        {
            this.CheckNotFinalized();

            if (this.hasBegun)
            {
                return;
            }

            this.hasBegun = true;

            this.WriteBeginningComments();

            if (!this.Namespace.IsNullOrWhitespace())
            {
                this.WriteNamespace();
                this.BeginSegment();
            }

            this.Write(StartPlaceholder);
            this.EmptyLine();
        }

        private void ProcessGenericType(Type type)
        {
            foreach (var tArg in type.GetGenericArguments())
            {
                if (tArg.Namespace != null)
                {
                    this.seenNamespaces.Add(tArg.Namespace);
                }

                if (tArg.IsGenericType)
                {
                    this.ProcessGenericType(tArg);
                }
            }
        }

        private void WriteBeginningComments()
        {
            string str = "This file has been automatically generated by a tool and should not be changed manually.";
            string dashes = new string('-', str.Length);

            this.WriteComment(dashes);
            this.WriteComment(str);
            this.WriteComment(dashes);
        }

        private void WriteNamespace()
        {
            if (!this.Namespace.IsNullOrWhitespace())
            {
                this.WriteLine("namespace " + this.Namespace);
            }
        }
    }
}
#endif