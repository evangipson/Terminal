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

        public override string ToString()
        {
            if (IsRoot)
            {
                return DirectoryConstants.DirectorySeparator;
            }

            return string.Concat(Name, DirectoryConstants.DirectorySeparator);
        }
    }
}
