using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Services;

namespace Terminal.Models
{
    public class FileSystem
    {
        public List<DirectoryEntity> Directories { get; set; } = new();

        public Guid CurrentDirectoryId { get; set; }

        private DirectoryEntity Root => Directories.First(entity => entity.IsRoot);

        public string GetDirectoryPath(DirectoryEntity directory) => string.Concat(GetAbsoluteDirectory(directory));

        public List<DirectoryEntity> GetAbsoluteDirectory(DirectoryEntity directory)
        {
            if (directory?.IsDirectory != true)
            {
                return new() { Root };
            }

            List<DirectoryEntity> directoryPath = new();
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
