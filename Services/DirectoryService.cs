using System;

using Terminal.Constants;
using Terminal.Models;

namespace Terminal.Services
{
    /// <summary>
    /// A <see langword="static"/> service that manages navigating a <see cref="DirectoryEntity"/> and creating a new <see cref="FileSystem"/>.
    /// </summary>
    public static class DirectoryService
    {
        /// <summary>
        /// Creates a new <see cref="FileSystem"/>. Intended to be used when no file system exists.
        /// </summary>
        /// <returns>
        /// A new <see cref="FileSystem"/> that has it's <see cref="FileSystem.Directories"/> populated.
        /// </returns>
        public static FileSystem CreateNewFileSystem() => new()
        {
            Directories = DirectoryConstants.GetDefaultDirectoryStructure()
        };

        /// <summary>
        /// Finds a file in a <see cref="DirectoryEntity"/>.
        /// </summary>
        /// <param name="node">
        /// The <see cref="DirectoryEntity"/> to begin the search from.
        /// </param>
        /// <param name="fileName">
        /// The name of the file to find.
        /// </param>
        /// <returns>
        /// The file that is found, defaults to <see langword="null"/>.
        /// </returns>
        public static DirectoryEntity FindFile(this DirectoryEntity node, string fileName)
        {
            if (node == null)
            {
                return null;
            }

            if (node.ToString().Equals(fileName, StringComparison.OrdinalIgnoreCase) && !node.IsDirectory)
            {
                return node;
            }

            foreach (var child in node.Entities)
            {
                var found = child.FindFile(fileName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds a folder in a <see cref="DirectoryEntity"/> by name.
        /// </summary>
        /// <param name="node">
        /// The <see cref="DirectoryEntity"/> to begin the search from.
        /// </param>
        /// <param name="name">
        /// The name of the folder to find.
        /// </param>
        /// <returns>
        /// The folder that is found, defaults to <see langword="null"/>.
        /// </returns>
        public static DirectoryEntity FindDirectory(this DirectoryEntity node, string name)
        {
            if (node == null)
            {
                return null;
            }

            if (node.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && node.IsDirectory)
            {
                return node;
            }

            foreach (var child in node.Entities)
            {
                var found = child.FindDirectory(name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds a folder in a <see cref="DirectoryEntity"/> by unique identifier.
        /// </summary>
        /// <param name="node">
        /// The <see cref="DirectoryEntity"/> to begin the search from.
        /// </param>
        /// <param name="id">
        /// The unique identifier of the folder to find.
        /// </param>
        /// <returns>
        /// The folder that is found, defaults to <see langword="null"/>.
        /// </returns>
        public static DirectoryEntity FindDirectory(this DirectoryEntity node, Guid id)
        {
            if (node == null)
            {
                return null;
            }

            if (node.Id.Equals(id) && node.IsDirectory)
            {
                return node;
            }

            foreach (var child in node.Entities)
            {
                var found = child.FindDirectory(id);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
