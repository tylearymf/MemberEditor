//-----------------------------------------------------------------------
// <copyright file="DirectoryInfoExtensions.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.Utilities
{
    using System.IO;

    /// <summary>
    /// DirectoryInfo method extensions.
    /// </summary>
    public static class DirectoryInfoExtensions
    {
        /// <summary>
        /// Determines whether the directory has a given directory in its hierarchy of children.
        /// </summary>
        /// <param name="parentDir">The parent directory.</param>
        /// <param name="subDir">The sub directory.</param>
        public static bool HasSubDirectory(this DirectoryInfo parentDir, DirectoryInfo subDir)
        {
            var parentDirName = parentDir.FullName.TrimEnd('\\', '/');

            while (subDir != null)
            {
                if (subDir.FullName.TrimEnd('\\', '/') == parentDirName)
                {
                    return true;
                }
                else
                {
                    subDir = subDir.Parent;
                }
            }

            return false;
        }

        /// <summary>
        /// Finds a parent directory with a given name, or null if no such parent directory exists.
        /// </summary>
        public static DirectoryInfo FindParentDirectoryWithName(this DirectoryInfo dir, string folderName)
        {
            if (dir.Parent == null)
            {
                return null;
            }

            if (string.Equals(dir.Name, folderName, System.StringComparison.InvariantCultureIgnoreCase))
            {
                return dir;
            }

            return dir.Parent.FindParentDirectoryWithName(folderName);
        }
    }
}