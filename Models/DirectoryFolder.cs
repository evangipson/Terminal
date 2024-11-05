using System;
using System.Collections.Generic;
using System.Linq;

namespace Terminal.Models
{
    public class DirectoryFolder : IDirectoryEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string Contents { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public bool IsDirectory { get; set; } = true;

        public bool IsRoot { get; set; } = false;

        public Guid ParentId { get; set; }

        public List<IDirectoryEntity> Directories => Entities.Where(entity => entity.IsDirectory).ToList();

        public List<IDirectoryEntity> Files => Entities.Except(Directories).ToList();

        public List<IDirectoryEntity> Entities { get; set; } = new();

        public override string ToString()
        {
            if (IsRoot)
            {
                return "/";
            }

            return $"{Name}/";
        }
    }
}
