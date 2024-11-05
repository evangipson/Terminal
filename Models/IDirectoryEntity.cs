using System;
using System.Collections.Generic;

namespace Terminal.Models
{
    public interface IDirectoryEntity
    {
        Guid Id { get; set; }

        string Name { get; set; }

        string Contents { get; set; }

        string Extension { get; set; }

        bool IsDirectory { get; set; }

        bool IsRoot { get; set; }

        Guid ParentId { get; set; }

        List<IDirectoryEntity> Entities { get; set; }

        string ToString();
    }
}
