using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

using Terminal.Extensions;

namespace Terminal.Models
{
    /// <summary>
    /// Represents a file system for the terminal.
    /// </summary>
    public class FileSystem
    {
        /// <summary>
        /// A collection of <see cref="DirectoryEntity"/> that represents the structure of the file system.
        /// </summary>
        public List<DirectoryEntity> Directories { get; set; } = [];

        /// <summary>
        /// A unique identifier of the current directory.
        /// </summary>
        public Guid CurrentDirectoryId { get; set; }

        /// <summary>
        /// The root <see cref="DirectoryEntity"/> of the file system. Ignored when saving the <see cref="FileSystem"/> to file.
        /// </summary>
        [JsonIgnore]
        public DirectoryEntity Root => Directories.First(entity => entity.IsRoot);

        /// <summary>
        /// Gets an absolute path for a <see cref="DirectoryEntity"/> in <see cref="Directories"/>.
        /// </summary>
        /// <param name="directory">
        /// The <see cref="DirectoryEntity"/> to get an absolute path for.
        /// </param>
        /// <returns>
        /// An absolute path for a <see cref="DirectoryEntity"/> in <see cref="Directories"/>.
        /// </returns>
        public string GetDirectoryPath(DirectoryEntity directory) => string.Concat(GetAbsoluteDirectory(directory));

        /// <summary>
        /// Gets an absolute path for a <see cref="DirectoryEntity"/> in a <see cref="FileSystem"/>.
        /// </summary>
        /// <param name="entity">
        /// The <see cref="DirectoryEntity"/> to get an absolute path for.
        /// </param>
        /// <returns>
        /// An absolute path for a <see cref="DirectoryEntity"/> in a <see cref="FileSystem"/>.
        /// </returns>
        public string GetEntityPath(DirectoryEntity entity) => string.Concat(GetAbsoluteEntity(entity));

        /// <summary>
        /// Navigates from <see cref="Root"/> and finds the provided <paramref name="directory"/>.
        /// </summary>
        /// <param name="directory">
        /// The <see cref="DirectoryEntity"/> to find.
        /// </param>
        /// <returns>
        /// A collection of <see cref="DirectoryEntity"/>, ending with the provided <paramref name="directory"/>.
        /// </returns>
        private List<DirectoryEntity> GetAbsoluteDirectory(DirectoryEntity directory)
        {
            if (directory?.IsDirectory != true)
            {
                return [Root];
            }

            List<DirectoryEntity> directoryPath = [];
            var foundDirectory = directory;
            while (foundDirectory != null)
            {
                directoryPath.Add(foundDirectory);
                foundDirectory = Root.FindDirectory(foundDirectory.ParentId);
            }

            directoryPath.Reverse();
            return new(directoryPath);
        }

        /// <summary>
        /// Navigates from <see cref="Root"/> and finds the provided <paramref name="entity"/>.
        /// </summary>
        /// <param name="entity">
        /// The <see cref="DirectoryEntity"/> to find.
        /// </param>
        /// <returns>
        /// A collection of <see cref="DirectoryEntity"/>, ending with the provided <paramref name="entity"/>.
        /// </returns>
        private List<DirectoryEntity> GetAbsoluteEntity(DirectoryEntity entity)
        {
            List<DirectoryEntity> directoryPath = [];
            var foundEntity = entity;
            while (foundEntity != null)
            {
                directoryPath.Add(foundEntity);
                foundEntity = Root.FindDirectory(foundEntity.ParentId);
            }

            directoryPath.Reverse();
            return new(directoryPath);
        }
    }
}
