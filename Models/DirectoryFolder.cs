using System.Collections.Generic;
using System.Linq;
using Terminal.Constants;

namespace Terminal.Models
{
    public class DirectoryFolder : DirectoryEntity
    {
        public DirectoryFolder()
        {
            IsDirectory = true;
        }

        public List<DirectoryEntity> Directories => Entities.Where(entity => entity.IsDirectory).ToList();

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
