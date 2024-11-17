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
    /// A global singleton that is responsible for getting and setting directory values.
    /// </summary>
    public partial class DirectoryService : Node
    {
        /// <summary>
        /// The <see cref="Models.FileSystem"/> of the console.
        /// </summary>
        public FileSystem FileSystem;

        public override void _Ready()
        {
            FileSystem = new()
            {
                Directories = DirectoryConstants.GetDefaultDirectoryStructure()
            };

            SetCurrentDirectory(GetHomeDirectory());
        }

        /// <summary>
        /// Sets the current directory of the <see cref="FileSystem"/> to the provided <paramref name="newCurrentDirectory"/>.
        /// <para>
        /// Does nothing if the provided <paramref name="newCurrentDirectory"/> can't be found.
        /// </para>
        /// </summary>
        /// <param name="newCurrentDirectory">
        /// The new directory to use as the current directory.
        /// </param>
        public void SetCurrentDirectory(DirectoryEntity newCurrentDirectory)
        {
            if (newCurrentDirectory?.IsDirectory != true || FileSystem == null)
            {
                return;
            }

            FileSystem.CurrentDirectoryId = newCurrentDirectory.Id;
        }

        /// <summary>
        /// Sets the current directory of the <see cref="FileSystem"/> to the provided <paramref name="newDirectoryPath"/>.
        /// <para>
        /// Does nothing if the provided <paramref name="newDirectoryPath"/> can't be resolved.
        /// </para>
        /// </summary>
        /// <param name="newDirectoryPath">
        /// The new directory path to parse and use as the current directory.
        /// </param>
        public void SetCurrentDirectory(string newDirectoryPath)
        {
            if (string.IsNullOrEmpty(newDirectoryPath))
            {
                return;
            }

            if (newDirectoryPath == TerminalCharactersConstants.Separator.ToString())
            {
                SetCurrentDirectory(GetRootDirectory());
            }

            List<string> directoryTokensInPath = newDirectoryPath.Split('/').ToList();
            DirectoryEntity newCurrentDirectory = GetCurrentDirectory().FindDirectory(directoryTokensInPath.LastOrDefault().TrimEnd('/'));
            newCurrentDirectory ??= GetRootDirectory().FindDirectory(newDirectoryPath.TrimEnd('/'));

            SetCurrentDirectory(newCurrentDirectory);
        }

        /// <summary>
        /// Gets the parent directory of the provided <paramref name="currentDirectory"/>.
        /// <para>
        /// Gets the root directory if a parent directory is not found.
        /// </para>
        /// </summary>
        /// <param name="currentDirectory">
        /// The file or folder to get the parent of.
        /// </param>
        /// <returns>
        /// The parent directory of the <paramref name="currentDirectory"/>.
        /// </returns>
        public DirectoryEntity GetParentDirectory(DirectoryEntity currentDirectory) => GetRootDirectory().FindDirectory(currentDirectory.ParentId) ?? GetRootDirectory();

        /// <summary>
        /// Gets the root directory of the <see cref="FileSystem"/>.
        /// <para>
        /// Defaults to <see langword="null"/> if the <see cref="FileSystem"/> is <see langword="null"/>.
        /// </para>
        /// </summary>
        /// <returns>
        /// The root directory of the <see cref="FileSystem"/>.
        /// </returns>
        public DirectoryEntity GetRootDirectory() => FileSystem?.Root;

        /// <summary>
        /// Gets the current directory of the <see cref="FileSystem"/>.
        /// <para>
        /// If the current directory isn't found, gets the root directory.
        /// </para>
        /// </summary>
        /// <returns>
        /// The current directory of the <see cref="FileSystem"/>.
        /// </returns>
        public DirectoryEntity GetCurrentDirectory() => GetRootDirectory().FindDirectory(FileSystem?.CurrentDirectoryId ?? Guid.Empty) ?? GetRootDirectory();

        /// <summary>
        /// Gets the absolute path of the current directory.
        /// <para>
        /// Defaults to <see langword="null"/> if the <see cref="FileSystem"/> is <see langword="null"/>.
        /// </para>
        /// </summary>
        /// <returns>
        /// The absolute path of the current directory of the <see cref="FileSystem"/>.
        /// </returns>
        public string GetCurrentDirectoryPath()
        {
            if (GetHomeDirectory().FindDirectory(GetCurrentDirectory().Id) != null)
            {
                var homeDirectoryPath = FileSystem?.GetDirectoryPath(GetHomeDirectory());
                return string.Concat(TerminalCharactersConstants.HomeDirectory,
                    TerminalCharactersConstants.Separator,
                    FileSystem?.GetDirectoryPath(GetCurrentDirectory()).Replace(homeDirectoryPath, string.Empty));
            }

            return FileSystem?.GetDirectoryPath(GetCurrentDirectory());
        }

        /// <summary>
        /// Gets an absolute path for the provided <paramref name="directory"/> folder.
        /// <para>
        /// Defaults to <see langword="null"/> if the <see cref="FileSystem"/> is <see langword="null"/>.
        /// </para>
        /// </summary>
        /// <param name="directory">
        /// The folder to get the absolute directory path for.
        /// </param>
        /// <returns>
        /// An absolute path of the provided <paramref name="directory"/> folder.
        /// </returns>
        public string GetAbsoluteDirectoryPath(DirectoryEntity directory)
        {
            if(GetHomeDirectory().FindDirectory(directory.Id) != null)
            {
                var homeDirectoryPath = FileSystem?.GetDirectoryPath(GetHomeDirectory());
                return string.Concat(TerminalCharactersConstants.HomeDirectory,
                    TerminalCharactersConstants.Separator,
                    FileSystem?.GetDirectoryPath(directory).Replace(homeDirectoryPath, string.Empty));
            }

            return FileSystem?.GetDirectoryPath(directory);
        }

        /// <summary>
        /// Gets an absolute path for the provided <paramref name="entity"/> file or folder.
        /// <para>
        /// Defaults to <see langword="null"/> if the <see cref="FileSystem"/> is <see langword="null"/>.
        /// </para>
        /// </summary>
        /// <param name="entity">
        /// The file or folder to get the absolute path for.
        /// </param>
        /// <returns>
        /// An absolute path of the provided <paramref name="entity"/> file or folder.
        /// </returns>
        public string GetAbsoluteEntityPath(DirectoryEntity entity)
        {
            if (GetHomeDirectory().FindDirectory(entity.ParentId) != null)
            {
                var homeDirectoryPath = FileSystem?.GetDirectoryPath(GetHomeDirectory());
                return string.Concat(TerminalCharactersConstants.HomeDirectory,
                    TerminalCharactersConstants.Separator,
                    FileSystem?.GetEntityPath(entity).Replace(homeDirectoryPath, string.Empty));
            }

            return FileSystem?.GetEntityPath(entity);
        }

        /// <summary>
        /// Gets a relative path for the provided <paramref name="entity"/> file or folder.
        /// <para>
        /// Defaults to <see langword="null"/> if the <see cref="FileSystem"/> is <see langword="null"/>.
        /// </para>
        /// </summary>
        /// <param name="entity">
        /// The file or folder to get the relative path for.
        /// </param>
        /// <returns>
        /// A relative path of the provided <paramref name="entity"/> file or folder.
        /// </returns>
        public string GetRelativeEntityPath(DirectoryEntity entity)
        {
            if (GetHomeDirectory().FindDirectory(entity.ParentId) != null)
            {
                var homeDirectoryPath = FileSystem?.GetDirectoryPath(GetHomeDirectory());
                var currentDirectoryPath = GetCurrentDirectoryPath().Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                if (string.IsNullOrEmpty(currentDirectoryPath))
                {
                    return FileSystem?.GetEntityPath(entity)
                        .Replace(homeDirectoryPath, string.Empty)
                        .TrimStart('/');
                }

                return FileSystem?.GetEntityPath(entity)
                    .Replace(homeDirectoryPath, string.Empty)
                    .Replace($"{currentDirectoryPath}/", string.Empty)
                    .TrimStart('/');
            }

            return FileSystem?.GetEntityPath(entity).Replace(GetCurrentDirectoryPath(), string.Empty);
        }

        /// <summary>
        /// Gets a file with the provided <paramref name="fileName"/> from the current directory.
        /// </summary>
        /// <param name="fileName">
        /// The name of the file to get.
        /// </param>
        /// <returns>
        /// A file with the provided <paramref name="fileName"/> from the current directory.
        /// </returns>
        public DirectoryEntity GetRelativeFile(string fileName)
        {
            //if(fileName.TrimStart('/').Contains('/'))
            //{
            //    var trimmedRelativeFilePath = string.Join(TerminalCharactersConstants.Separator, fileName.Split(TerminalCharactersConstants.Separator, StringSplitOptions.RemoveEmptyEntries).SkipLast(1));
            //    return GetCurrentDirectory().FindDirectory(trimmedRelativeFilePath).FindFile(fileName);
            //}

            return GetCurrentDirectory().FindFile(fileName);
        }

        /// <summary>
        /// Gets a file with the provided <paramref name="fileName"/> from the root directory.
        /// </summary>
        /// <param name="fileName">
        /// The name of the file to get.
        /// </param>
        /// <returns>
        /// A file with the provided <paramref name="fileName"/> from the root directory.
        /// </returns>
        public DirectoryEntity GetAbsoluteFile(string fileName) => GetRootDirectory().FindFile(fileName);

        /// <summary>
        /// Gets a folder with the provided <paramref name="directoryName"/> from the current directory.
        /// </summary>
        /// <param name="directoryName">
        /// The name of the folder to get.
        /// </param>
        /// <returns>
        /// A folder with the provided <paramref name="directoryName"/> from the current directory.
        /// </returns>
        public DirectoryEntity GetRelativeDirectory(string directoryName) => GetCurrentDirectory().FindDirectory(directoryName.TrimEnd('/'));

        /// <summary>
        /// Gets a folder with the provided <paramref name="directoryPath"/> from the root directory.
        /// </summary>
        /// <param name="directoryPath">
        /// The name of the folder to get.
        /// </param>
        /// <returns>
        /// A folder with the provided <paramref name="directoryPath"/> from the root directory.
        /// </returns>
        public DirectoryEntity GetAbsoluteDirectory(string directoryPath) => GetRootDirectory().FindDirectory(directoryPath);

        /// <summary>
        /// Gets the home folder for the current user.
        /// <para>
        /// Defaults to the <c>/users/user/home/</c> directory.
        /// </para>
        /// </summary>
        /// <returns>
        /// The home folder folder.
        /// </returns>
        public DirectoryEntity GetHomeDirectory() => GetRootDirectory().FindDirectory("users/user/home");

        /// <summary>
        /// Views a file by returning it's contents.
        /// </summary>
        /// <param name="fileName">
        /// The name of the file to view the contents of.
        /// </param>
        /// <returns>
        /// The <paramref name="fileName"/> contents.
        /// </returns>
        public string ViewFile(string fileName)
        {
            var file = GetRelativeFile(fileName);
            if (file == null)
            {
                return $"\"{fileName}\" does not exist.";
            }

            if (file.Permissions.Contains(Permission.AdminExecutable) || file.Permissions.Contains(Permission.UserExecutable))
            {
                return $"\"{fileName}\" is an executable.";
            }

            if (!file.Permissions.Contains(Permission.UserRead))
            {
                return $"Insufficient permissions to view the \"{fileName}\" file.";
            }

            return file.Contents;
        }

        /// <summary>
        /// Creates a file with the provided <paramref name="fileName"/> in the current directory.
        /// <para>
        /// Default permissions are <see cref="Permission.UserRead"/> and <see cref="Permission.UserWrite"/>.
        /// </para>
        /// </summary>
        /// <param name="fileName">
        /// The name of the file to create in the current directory.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> containing the status of the file creation.
        /// </returns>
        public string CreateFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return "File can't be made without a name.";
            }

            var existingFile = GetRelativeFile(fileName);
            if (existingFile != null)
            {
                return $"File with the name of '{fileName}' already exists.";
            }

            var currentDirectory = GetCurrentDirectory();
            if (!currentDirectory.Permissions.Contains(Permission.UserWrite))
            {
                return "Insufficient permissions to create a file in the current directory.";
            }

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
            return $"New file '{fileName}' created.";
        }

        /// <summary>
        /// Deletes a file from the file system.
        /// </summary>
        /// <param name="fileName">
        /// The name of the file to delete.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> containing the status of the file deletion.
        /// </returns>
        public string DeleteFile(string fileName)
        {
            var currentDirectory = GetCurrentDirectory();
            if (!currentDirectory.Permissions.Contains(Permission.UserWrite))
            {
                return $"Insufficient permissions to delete \"{fileName}\" in the current directory.";
            }

            if (string.IsNullOrEmpty(fileName))
            {
                return "File can't be deleted without a name.";
            }

            var existingFile = GetRelativeFile(fileName);
            if (existingFile == null)
            {
                return $"No file with the name of \"{fileName}\" exists.";
            }

            DeleteEntity(existingFile);
            return $"\"{fileName}\" deleted.";
        }

        /// <summary>
        /// Sets the current directory to the <paramref name="newDirectory"/>.
        /// </summary>
        /// <param name="newDirectory">
        /// The name of the directory to set as the current directory.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> containing the status of the directory change.
        /// </returns>
        public string ChangeDirectory(string newDirectory)
        {
            var directory = newDirectory.ToLowerInvariant() switch
            {
                ".." => GetParentDirectory(GetCurrentDirectory()),
                "." or "./" => GetCurrentDirectory(),
                "~" => GetHomeDirectory(),
                "root" or "/" => GetRootDirectory(),
                _ => newDirectory.StartsWith(TerminalCharactersConstants.Separator)
                    ? GetAbsoluteDirectory(newDirectory.TrimEnd(TerminalCharactersConstants.Separator))
                    : newDirectory.Contains(TerminalCharactersConstants.HomeDirectory)
                        ? GetHomeDirectory().FindDirectory(newDirectory.Split(TerminalCharactersConstants.HomeDirectory).LastOrDefault()?.TrimEnd(TerminalCharactersConstants.Separator))
                        : GetRelativeDirectory(newDirectory.TrimEnd(TerminalCharactersConstants.Separator))
            };

            if (directory == null)
            {
                return $"\"{newDirectory}\" is not a directory.";
            }

            if (!directory.Permissions.Contains(Permission.UserRead))
            {
                return $"Insufficient permissions to enter the \"{GetAbsoluteDirectoryPath(directory)}\" directory.";
            }

            SetCurrentDirectory(directory);
            return string.Empty;
        }

        /// <summary>
        /// Creates a folder with the provided <paramref name="directoryName"/> in the current directory.
        /// <para>
        /// Default permissions are <see cref="Permission.UserRead"/> and <see cref="Permission.UserWrite"/>.
        /// </para>
        /// </summary>
        /// <param name="directoryName">
        /// The name of the folder to create in the current directory.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> containing the status of the directory creation.
        /// </returns>
        public string CreateDirectory(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
            {
                return "Directory can't be made without a name.";
            }

            var existingDirectory = GetRelativeDirectory(directoryName);
            if (existingDirectory != null)
            {
                return $"Directory with the name of '{directoryName}' already exists.";
            }

            var currentDirectory = GetCurrentDirectory();
            if (!currentDirectory.Permissions.Contains(Permission.UserWrite))
            {
                return $"Insufficient permissions to create a directory in the current directory.";
            }

            var newDirectory = new DirectoryFolder()
            {
                Name = directoryName,
                Permissions = new() { Permission.UserRead, Permission.UserWrite },
                ParentId = GetCurrentDirectory().Id
            };

            GetCurrentDirectory().Entities.Add(newDirectory);
            return $"New directory '{directoryName}' created.";
        }

        /// <summary>
        /// Deletes the <paramref name="directoryName"/>.
        /// </summary>
        /// <param name="directoryName">
        /// The name of the directory to delete.
        /// </param>
        /// <param name="arguments">
        /// A collection of arguments provided by the user.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> containing the status of the directory deletion.
        /// </returns>
        public string DeleteDirectory(string directoryName, IEnumerable<string> arguments)
        {
            var currentDirectory = GetCurrentDirectory();
            if (!currentDirectory.Permissions.Contains(Permission.UserWrite))
            {
                return $"Insufficient permissions to delete \"{directoryName}\" in the current directory.";
            }

            if (string.IsNullOrEmpty(directoryName))
            {
                return "Directory can't be deleted without a name.";
            }

            var existingDirectory = GetRelativeDirectory(directoryName);
            if (existingDirectory == null)
            {
                return $"No folder with the name of \"{directoryName}\" exists.";
            }

            var recurse = arguments.Contains("-r") || arguments.Contains("--recurse");
            if (recurse == false && existingDirectory.Entities.Any())
            {
                return $"Cannot delete the \"{directoryName}\" directory, it has files or folders in it.";
            }

            DeleteEntity(existingDirectory);
            return $"\"{directoryName}\" deleted.";
        }

        /// <summary>
        /// Moves a file from one location to another.
        /// </summary>
        /// <param name="arguments">
        /// A collection of arguments that should contain the file name and destination.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> containing the status of the file move.
        /// </returns>
        public string MoveFile(IEnumerable<string> arguments)
        {
            var fileName = arguments.FirstOrDefault();
            if (string.IsNullOrEmpty(fileName))
            {
                return "Cannot move file without a file name.";
            }

            var destinationPath = arguments.Skip(1).FirstOrDefault();
            if (string.IsNullOrEmpty(destinationPath))
            {
                return $"Cannot move file without a destination.";
            }

            var fileToMove = GetRelativeFile(fileName);
            if (fileName.Trim('/').Contains('/'))
            {
                var fileNameWithoutPath = fileName.Split('/').LastOrDefault();
                var filePath = fileName.Split('/').SkipLast(1);
                fileToMove = GetRelativeDirectory(string.Join('/', filePath)).FindFile(fileNameWithoutPath);
            }
            if (fileToMove == null)
            {
                return $"Cannot move, file \"{fileName}\" does not exist in the current directory.";
            }

            var destinationDirectory = GetAbsoluteDirectory(destinationPath.TrimEnd('/'));
            if (destinationDirectory == null)
            {
                return $"Cannot move, destination folder \"{destinationDirectory}\" does not exist.";
            }

            MoveEntity(fileToMove, destinationDirectory);
            return $"\"{fileToMove}\" moved to \"{destinationDirectory}\".";
        }

        /// <summary>
        /// Moves a folder and all child files and folders from one location to another.
        /// </summary>
        /// <param name="arguments">
        /// A collection of arguments that should contain the folder name and destination.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> containing the status of the folder move.
        /// </returns>
        public string MoveDirectory(IEnumerable<string> arguments)
        {
            var directoryToMovePath = arguments.FirstOrDefault();
            if (string.IsNullOrEmpty(directoryToMovePath))
            {
                return "Cannot move directory without a directory name.";
            }

            var destinationPath = arguments.Skip(1).FirstOrDefault();
            if (string.IsNullOrEmpty(destinationPath))
            {
                return $"Cannot move directory without a destination.";
            }

            var directoryToMove = GetRelativeDirectory(directoryToMovePath);
            if (directoryToMove == null)
            {
                return $"Cannot move, directory \"{directoryToMove}\" does not exist in the current directory.";
            }

            var destinationDirectory = GetAbsoluteDirectory(destinationPath);
            if (destinationDirectory == null)
            {
                return $"Cannot move, destination folder \"{destinationDirectory}\" does not exist.";
            }

            MoveEntity(directoryToMove, destinationDirectory);
            return $"\"{directoryToMove}\" moved to \"{destinationDirectory}\".";
        }

        private void DeleteEntity(DirectoryEntity entity)
        {
            if(GetCurrentDirectory().Entities.Contains(entity))
            {
                GetCurrentDirectory().Entities.Remove(entity);
            }
        }

        private void MoveEntity(DirectoryEntity entity, DirectoryEntity destination)
        {
            if (GetRootDirectory().FindDirectory(destination.Id) == null)
            {
                GD.Print($"Attempted to move {entity} into {destination} but could not find destination folder \"{destination}\" in the file system.");
                return;
            }

            if (GetCurrentDirectory().FindEntity($"{entity}") == null)
            {
                GD.Print($"Attempted to move {entity} into {destination} but could not find target entity \"{entity}\" in the file system.");
                return;
            }

            if (destination == entity)
            {
                GD.Print($"Attempted to move {entity} into {destination} but they are the same entity.");
                return;
            }

            if(destination.Entities.Contains(entity))
            {
                GD.Print($"Attempted to move {entity} into {destination} but {destination} already contains {entity}.");
                return;
            }

            if(!GetCurrentDirectory().FindDirectory(entity.ParentId).Entities.Remove(entity))
            {
                GD.Print($"Attempted to remove {entity} from it's parent folder, but was unable to.");
                return;
            }

            entity.ParentId = destination.Id;
            destination.Entities.Add(entity);
        }
    }
}
