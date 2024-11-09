using System;
using System.Collections.Generic;

namespace Terminal.Models
{
    public class DirectoryEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string Contents { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public bool IsDirectory { get; set; } = false;

        public bool IsRoot { get; set; } = false;

        public Guid ParentId { get; set; }

        public List<DirectoryEntity> Entities { get; set; } = new();

        public override string ToString()
        {
            if(IsDirectory)
            {
                if (IsRoot)
                {
                    return "/";
                }

                return $"{Name}/";
            }

            return $"{Name}.{Extension}";
        }
    }
}
