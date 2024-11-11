using System.Collections.Generic;
using System.Linq;

using Terminal.Constants;

namespace Terminal.Models
{
    /// <summary>
    /// An implementation of <see cref="DirectoryEntity"/> for a folder.
    /// </summary>
    public class DirectoryFolder : DirectoryEntity
    {
        /// <summary>
        /// Creates a new <see cref="DirectoryFolder"/>, and sets <see cref="DirectoryEntity.IsDirectory"/> to <see langword="true"/>.
        /// </summary>
        public DirectoryFolder()
        {
            IsDirectory = true;
        }

        /// <summary>
        /// A list of <see cref="DirectoryEntity.Entities"/> that have the <see cref="DirectoryEntity.IsDirectory"/> flag set to <see langword="true"/>.
        /// </summary>
        public List<DirectoryEntity> Directories => Entities.Where(entity => entity.IsDirectory).ToList();

        /// <summary>
        /// A list of <see cref="DirectoryEntity.Entities"/> that have the <see cref="DirectoryEntity.IsDirectory"/> flag set to <see langword="false"/>.
        /// </summary>
        public List<DirectoryEntity> Files => Entities.Except(Directories).ToList();

        /// <summary>
        /// Renders a folder in a console-appropriate manner.
        /// <para>
        /// Folders will be rendered as their name with the <see cref="TerminalCharactersConstants.Separator"/> suffixed (i.e.: <c>directoryname/</c>).
        /// The folder with the <see cref="DirectoryEntity.IsRoot"/> flag set to true will be rendered as only <see cref="TerminalCharactersConstants.Separator"/>.
        /// </para>
        /// </summary>
        /// <returns>
        /// A <see langword="string"/> representation of the folder.
        /// </returns>
        public override string ToString()
        {
            if (IsRoot)
            {
                return TerminalCharactersConstants.Separator.ToString();
            }

            return string.Concat(Name, TerminalCharactersConstants.Separator);
        }
    }
}
