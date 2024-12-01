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
        /// A flag that determines if a <see cref="DirectoryEntity"/> is the home folder of a user. Defaults to <see langword="false"/>.
        /// </summary>
        public bool IsHome { get; set; } = false;

        /// <summary>
        /// A reference to a parent <see cref="Id"/> for a <see cref="DirectoryEntity"/>.
        /// </summary>
        public Guid ParentId { get; set; }

        /// <summary>
        /// A collection of <see cref="DirectoryEntity"/> objects that are in a <see cref="DirectoryEntity"/>.
        /// </summary>
        public List<DirectoryEntity> Entities { get; set; } = new();

        /// <summary>
        /// Renders a file or folder in a console-appropriate manner.
        /// <para>
        /// Files will be rendered as just their <see cref="Name"/>, or their <see cref="Name"/> and <see cref="Extension"/>
        /// joined with a ".", if <see cref="Extension"/> is not <see langword="null"/> or empty (i.e.: <c>filename.txt</c>).
        /// </para>
        /// <para>
        /// Folders will be rendered as their name with the <see cref="TerminalCharactersConstants.Separator"/> suffixed (i.e.: <c>directoryname/</c>).
        /// The folder with the <see cref="IsRoot"/> flag set to true will be rendered as only <see cref="TerminalCharactersConstants.Separator"/>.
        /// </para>
        /// </summary>
        /// <returns>
        /// A <see langword="string"/> representation of the file or folder.
        /// </returns>
        public override string ToString()
        {
            if(IsDirectory)
            {
                if (IsRoot)
                {
                    return TerminalCharactersConstants.Separator.ToString();
                }

                return string.Concat(Name, TerminalCharactersConstants.Separator);
            }

            return string.IsNullOrEmpty(Extension) ? Name : string.Join('.', Name, Extension);
        }
    }
}
