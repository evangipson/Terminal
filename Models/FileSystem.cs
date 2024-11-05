using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Services;

namespace Terminal.Models
{
    public class FileSystem
    {
        public List<IDirectoryEntity> Directories { get; set; } = new();

        public Guid CurrentDirectoryId { get; set; }

        public IDirectoryEntity Root => Directories.First(entity => entity.IsRoot);

        public string GetDirectoryPath(IDirectoryEntity directory) => string.Concat(GetAbsoluteDirectory(directory));

        public List<IDirectoryEntity> GetAbsoluteDirectory(IDirectoryEntity directory)
        {
            if (directory?.IsDirectory != true)
            {
                return new() { Root };
            }

            List<IDirectoryEntity> directoryPath = new();
            var foundDirectory = directory;
            while (foundDirectory != null)
            {
                directoryPath.Add(foundDirectory);
                foundDirectory = Root.FindDirectory(foundDirectory.ParentId);
            }

            directoryPath.Reverse();
            return new(directoryPath);
        }
    }
}
