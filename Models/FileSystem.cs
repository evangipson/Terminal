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

        public string GetDirectoryPath(DirectoryEntity directory)
        {
            if(directory?.IsDirectory != true)
            {
                return string.Empty;
            }

            List<DirectoryEntity> directoryPath = new();
            var foundDirectory = directory;
            while (foundDirectory != null)
            {
                directoryPath.Add(foundDirectory);
                foundDirectory = Directories.First(entity => entity.IsRoot).FindDirectory(foundDirectory.ParentId);
            }

            directoryPath.Reverse();
            return string.Concat(directoryPath);
        }
    }
}
