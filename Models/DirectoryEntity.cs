using System;
using System.Collections.Generic;

using Terminal.Constants;
using Terminal.Enums;

namespace Terminal.Models
{
    /// <summary>
    /// A base entity that lives in a directory in the file system, either a file or a folder.
    /// </summary>
    public class DirectoryEntity
    {
        /// <summary>
        /// A unique identifier for a <see cref="DirectoryEntity"/>. Defaults to <see cref="Guid.NewGuid"/>.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The name of a <see cref="DirectoryEntity"/>. Defaults to <see cref="string.Empty"/>.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The file content of a <see cref="DirectoryEntity"/>. Defaults to <see cref="string.Empty"/>.
        /// </summary>
        public string Contents { get; set; } = string.Empty;

        /// <summary>
        /// The extension of a <see cref="DirectoryEntity"/>. Defaults to <see cref="string.Empty"/>.
        /// </summary>
        public string Extension { get; set; } = string.Empty;

        /// <summary>
        /// A flag that determines if a <see cref="DirectoryEntity"/> is a folder. Defaults to <see langword="false"/>.
        /// </summary>
        public bool IsDirectory { get; set; } = false;

        /// <summary>
        /// A collection of <see cref="Permission"/> for a <see cref="DirectoryEntity"/>. Defaults to <see cref="Permission.None"/>.
        /// </summary>
        public List<Permission> Permissions { get; set; } = new() { Permission.None };

        /// <summary>
        /// A flag that determines if a <see cref="DirectoryEntity"/> is the root folder of a file system. Defaults to <see langword="false"/>.
        /// </summary>
        public bool IsRoot { get; set; } = false;

        /// <summary>
        /// A reference to a parent <see cref="Id"/> for a <see cref="DirectoryEntity"/>.
        /// </summary>
        public Guid ParentId { get; set; }

        /// <summary>
        /// A collection of <see cref="DirectoryEntity"/> objects that are in a <see cref="DirectoryEntity"/>.
        /// </summary>
        public List<DirectoryEntity> Entities { get; set; } = new();

        public override string ToString()
        {
            if(IsDirectory)
            {
                if (IsRoot)
                {
                    return DirectoryConstants.DirectorySeparator;
                }

                return string.Concat(Name, DirectoryConstants.DirectorySeparator);
            }

            return string.IsNullOrEmpty(Extension) ? Name : string.Join('.', Name, Extension);
        }
    }
}
