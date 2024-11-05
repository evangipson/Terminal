using System;
using System.Collections.Generic;

namespace Terminal.Models
{
    public class DirectoryEntity
    {
        public Guid Id = Guid.NewGuid();

        public string Name { get; set; }

        public List<DirectoryEntity> Entities { get; set; } = new();

        public bool IsDirectory { get; set; } = false;

        public bool IsRoot { get; set; } = false;

        public Guid ParentId { get; set; }

        public override string ToString()
        {
            if(IsRoot)
            {
                return "/";
            }

            if(IsDirectory)
            {
                return $"{Name}/";
            }

            return Name;
        }
    }
}
