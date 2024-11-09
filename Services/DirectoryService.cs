﻿using System;

using Terminal.Constants;
using Terminal.Models;

namespace Terminal.Services
{
    public static class DirectoryService
    {
        public static FileSystem CreateNewFileSystem() => new()
        {
            Directories = DirectoryConstants.GetDefaultDirectoryStructure()
        };

        public static DirectoryEntity FindFile(this DirectoryEntity node, string fileName)
        {
            if (node == null)
            {
                return null;
            }

            if (node.ToString().Equals(fileName, StringComparison.OrdinalIgnoreCase) && !node.IsDirectory)
            {
                return node;
            }

            foreach (var child in node.Entities)
            {
                var found = child.FindFile(fileName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        public static DirectoryEntity FindDirectory(this DirectoryEntity node, string name)
        {
            if (node == null)
            {
                return null;
            }

            if (node.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && node.IsDirectory)
            {
                return node;
            }

            foreach (var child in node.Entities)
            {
                var found = child.FindDirectory(name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        public static DirectoryEntity FindDirectory(this DirectoryEntity node, Guid id)
        {
            if (node == null)
            {
                return null;
            }

            if (node.Id.Equals(id) && node.IsDirectory)
            {
                return node;
            }

            foreach (var child in node.Entities)
            {
                var found = child.FindDirectory(id);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
