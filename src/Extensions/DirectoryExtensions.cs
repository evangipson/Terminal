using System;
using System.Linq;

using Terminal.Models;

namespace Terminal.Extensions
{
    /// <summary>
    /// A <see langword="static"/> collection of extension methods for managing a directory.
    /// </summary>
    public static class DirectoryExtensions
    {
        /// <summary>
        /// Finds a file or folder in a <see cref="DirectoryEntity"/>.
        /// </summary>
        /// <param name="node">
        /// The <see cref="DirectoryEntity"/> to begin the search from.
        /// </param>
        /// <param name="entityName">
        /// The name of the entity to find.
        /// </param>
        /// <returns>
        /// The file or folder that is found, defaults to <see langword="null"/>.
        /// </returns>
        public static DirectoryEntity FindEntity(this DirectoryEntity node, string entityName)
        {
            if (node == null)
            {
                return null;
            }

            if (node.ToString().Equals(entityName, StringComparison.OrdinalIgnoreCase))
            {
                return node;
            }

            foreach (var child in node.Entities)
            {
                var found = child.FindEntity(entityName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

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

            if (node.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase) && !node.IsDirectory)
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
            if (node == null || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            if (node.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && node.IsDirectory)
            {
                return node;
            }

            if (name.Contains('/'))
            {
                var nextDirectory = name.StartsWith('/')
                    ? name.TrimStart('/').Split('/').First()
                    : name.Split('/').First();

                var subDirectory = node.FindDirectory(nextDirectory);
                if (subDirectory != null)
                {
                    return subDirectory.FindDirectory(string.Join('/', name.Split('/').Skip(1)));
                }

                return null;
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
