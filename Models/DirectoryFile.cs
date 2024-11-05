using System;
using System.Collections.Generic;

namespace Terminal.Models
{
    public class DirectoryFile : IDirectoryEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string Contents { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public bool IsDirectory { get; set; } = false;

        public bool IsRoot { get; set; } = false;

        public Guid ParentId { get; set; }

        public List<IDirectoryEntity> Entities { get; set; } = new();

        public override string ToString() => string.Join('.', Name, Extension);
    }
}
