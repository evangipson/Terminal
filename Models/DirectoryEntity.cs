using System;
using System.Collections.Generic;

using Terminal.Constants;

namespace Terminal.Models
{
    public class DirectoryEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string Contents { get; set; } = string.Empty;

        public string Extension { get; set; } = string.Empty;

        public bool IsDirectory { get; set; } = false;

        public List<Permission> Permissions { get; set; } = new() { Permission.None };

        public bool IsRoot { get; set; } = false;

        public Guid ParentId { get; set; }

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
