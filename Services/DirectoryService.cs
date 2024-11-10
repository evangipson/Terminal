using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

using Terminal.Constants;
using Terminal.Enums;
using Terminal.Extensions;
using Terminal.Models;

namespace Terminal.Services
{
    /// <summary>
    /// A collection of constant values for managing a directory.
    /// </summary>
    public partial class DirectoryService : Node
    {
        public FileSystem FileSystem;

        public override void _Ready()
        {
            FileSystem = new()
            {
                Directories = DirectoryConstants.GetDefaultDirectoryStructure()
            };

            SetCurrentDirectory(GetHomeDirectory());
        }

        public void SetCurrentDirectory(DirectoryEntity newCurrentDirectory)
        {
            if (newCurrentDirectory?.IsDirectory != true || FileSystem == null)
            {
                return;
            }

            FileSystem.CurrentDirectoryId = newCurrentDirectory.Id;
        }

        public void SetCurrentDirectory(string newDirectoryPath)
        {
            if (string.IsNullOrEmpty(newDirectoryPath))
            {
                return;
            }

            if (newDirectoryPath == "/")
            {
                SetCurrentDirectory(GetRootDirectory());
            }

            List<string> directoryTokensInPath = newDirectoryPath.Split('/').ToList();
            DirectoryEntity newCurrentDirectory = GetCurrentDirectory().FindDirectory(directoryTokensInPath.LastOrDefault().TrimEnd('/'));
            newCurrentDirectory ??= GetRootDirectory().FindDirectory(newDirectoryPath.TrimEnd('/'));

            SetCurrentDirectory(newCurrentDirectory);
        }

        public DirectoryEntity GetParentDirectory(DirectoryEntity currentDirectory) => GetRootDirectory().FindDirectory(currentDirectory.ParentId) ?? GetRootDirectory();

        public DirectoryEntity GetRootDirectory() => FileSystem?.Root;

        public DirectoryEntity GetCurrentDirectory() => GetRootDirectory().FindDirectory(FileSystem?.CurrentDirectoryId ?? Guid.Empty) ?? GetRootDirectory();

        public string GetCurrentDirectoryPath() => FileSystem?.GetDirectoryPath(GetCurrentDirectory());

        public string GetAbsoluteDirectoryPath(DirectoryEntity directory) => FileSystem?.GetDirectoryPath(directory);

        public string GetRelativeDirectoryPath(DirectoryEntity directory) => FileSystem?.GetDirectoryPath(directory).Replace(GetCurrentDirectoryPath(), string.Empty);

        public string GetAbsoluteEntityPath(DirectoryEntity entity) => FileSystem?.GetEntityPath(entity);

        public string GetRelativeEntityPath(DirectoryEntity entity) => FileSystem?.GetEntityPath(entity).Replace(GetCurrentDirectoryPath(), string.Empty);

        public DirectoryEntity GetRelativeEntity(string entityName) => GetCurrentDirectory().FindEntity(entityName);

        public DirectoryEntity GetRelativeFile(string fileName) => GetCurrentDirectory().FindFile(fileName);

        public DirectoryEntity GetAbsoluteFile(string fileName) => GetRootDirectory().FindFile(fileName);

        public DirectoryEntity GetRelativeDirectory(string directoryName) => GetCurrentDirectory().FindDirectory(directoryName.TrimEnd('/'));

        public DirectoryEntity GetAbsoluteDirectory(string directoryPath) => GetRootDirectory().FindDirectory(directoryPath);

        public DirectoryEntity GetHomeDirectory() => GetRootDirectory().FindDirectory("users/user/home");

        public void CreateFile(string fileName)
        {
            var newFile = new DirectoryFile()
            {
                ParentId = GetCurrentDirectory().Id,
                Permissions = new() { Permission.UserRead, Permission.UserWrite }
            };

            var name = fileName;
            var fileTokens = fileName.Split('.');
            if (fileTokens.Length > 1)
            {
                name = string.Join('.', fileTokens.Take(fileTokens.Length - 1));
                newFile.Extension = fileTokens.Last();
            }

            newFile.Name = name;
            GetCurrentDirectory().Entities.Add(newFile);
        }

        public void CreateDirectory(string directoryName)
        {
            var newDirectory = new DirectoryFolder()
            {
                Name = directoryName,
                ParentId = GetCurrentDirectory().Id
            };

            GetCurrentDirectory().Entities.Add(newDirectory);
        }
    }
}
