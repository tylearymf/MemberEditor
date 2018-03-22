//-----------------------------------------------------------------------
// <copyright file="FolderPathAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
	using System;

	/// <summary>
	/// <para>FolderPath is used on string properties, and provides an interface for directory paths.</para>
	/// </summary>
	/// <example>
	/// <para>The following example demonstrates how FolderPath is used.</para>
	/// <code>
	///	public class FolderPathExamples : MonoBehaviour
	///	{
	///		// By default, FolderPath provides a path relative to the Unity project.
	///		[FolderPath]
	///		public string UnityProjectPath;
	///	
	///		// It is possible to provide custom parent patn. ParentFolder paths can be relative to the Unity project, or absolute.
	///		[FolderPath(ParentFolder = "Assets/Plugins/Sirenix")]
	///		public string RelativeToParentPath;
	///	
	///		// Using ParentFolder, FolderPath can also provide a path relative to a resources folder.
	///		[FolderPath(ParentFolder = "Assets/Resources")]
	///		public string ResourcePath;
	///	
	///		// By setting AbsolutePath to true, the FolderPath will provide an absolute path instead.
	///		[FolderPath(AbsolutePath = true)]
	///		public string AbsolutePath;
	///	
	///		// FolderPath can also be configured to show an error, if the provided path is invalid.
	///		[FolderPath(RequireValidPath = true)]
	///		public string ValidPath;
	///	
	///		// By default, FolderPath will enforce the use of forward slashes. It can also be configured to use backslashes instead.
	///		[FolderPath(UseBackslashes = true)]
	///		public string Backslashes;
	///	
	///		// FolderPath also supports member references with the $ symbol.
	///		[FolderPath(ParentFolder = "$DynamicParent")]
	///		public string DynamicFolderPath;
	///	
	///		public string DynamicParent = "Assets/Plugins/Sirenix";
	///	}
	/// </code>
	/// </example>
	/// <seealso cref="FilePathAttribute"/>
	/// <seealso cref="DisplayAsStringAttribute"/>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class FolderPathAttribute : Attribute
	{
		/// <summary>
		/// If <c>true</c> the FolderPath will provide an absolute path, instead of a relative one.
		/// </summary>
		public bool AbsolutePath { get; set; }

		/// <summary>
		/// ParentFolder provides an override for where the path is relative to. ParentFolder can be relative to the Unity project, or an absolute path.
		/// Supports member referencing with $.
		/// </summary>
		public string ParentFolder { get; set; }

		/// <summary>
		/// If <c>true</c> an error will be displayed for invalid, or missing paths.
		/// </summary>
		public bool RequireValidPath { get; set; }

		/// <summary>
		/// By default FolderPath enforces forward slashes. Set UseBackslashes to <c>true</c> if you want backslashes instead.
		/// </summary>
		public bool UseBackslashes { get; set; }
    }
}