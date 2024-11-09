using System.Collections.Generic;
using System.Linq;
using Terminal.Models;
using Terminal.Services;

namespace Terminal.Constants
{
    /// <summary>
    /// A <see langword="static"/> collection of constant values for managing a directory.
    /// </summary>
    public static class DirectoryConstants
    {
        /// <summary>
        /// The separator character for a directory.
        /// </summary>
        public const string DirectorySeparator = "/";

        /// <summary>
        /// The prompt character for the terminal, shown after the current directory.
        /// </summary>
        public const string TerminalPromptCharacter = ">";

        /// <summary>
        /// Gets the default directory structure of the file system, filled with all required system files and a user directory.
        /// </summary>
        /// <returns>
        /// A default list of <see cref="DirectoryEntity"/> used in the file system.
        /// </returns>
        public static List<DirectoryEntity> GetDefaultDirectoryStructure()
        {
            List<Permission> adminReadWritePermissions = new() { Permission.AdminRead, Permission.AdminWrite };
            List<Permission> userReadWritePermissions = new() { Permission.UserRead, Permission.UserWrite };
            List<Permission> userExecutablePermissions = new() { Permission.UserRead, Permission.UserExecutable };

            DirectoryFolder rootDirectory = new() { Name = DirectorySeparator, IsRoot = true };
            DirectoryFolder rootSystemDirectory = new() { Name = "system", ParentId = rootDirectory.Id, Permissions = adminReadWritePermissions };
            DirectoryFolder rootUsersDirectory = new() { Name = "users", ParentId = rootDirectory.Id, Permissions = adminReadWritePermissions };
            DirectoryFolder rootTempDirectory = new() { Name = "temp", ParentId = rootDirectory.Id, Permissions = adminReadWritePermissions };
            rootDirectory.Entities = new()
            {
                rootSystemDirectory,
                rootUsersDirectory,
                rootTempDirectory
            };

            // Fill the "/system/programs/" folder with all commands as executable files
            DirectoryFolder systemProgramsDirectory = new() { Name = "programs", ParentId = rootSystemDirectory.Id, Permissions = adminReadWritePermissions };
            systemProgramsDirectory.Entities = UserCommandService.GetAllCommands().Select(command => new DirectoryEntity()
            {
                Name = command,
                ParentId = systemProgramsDirectory.Id,
                Permissions = userExecutablePermissions
            }).ToList();

            rootSystemDirectory.Entities = new()
            {
                new DirectoryFolder() { Name = "config", ParentId = rootSystemDirectory.Id, Permissions = adminReadWritePermissions },
                new DirectoryFolder() { Name = "device", ParentId = rootSystemDirectory.Id, Permissions = adminReadWritePermissions },
                new DirectoryFolder() { Name = "logs", ParentId = rootSystemDirectory.Id, Permissions = adminReadWritePermissions },
                new DirectoryFolder() { Name = "network", ParentId = rootSystemDirectory.Id, Permissions = adminReadWritePermissions },
                systemProgramsDirectory
            };

            rootTempDirectory.Entities = new()
            {
                new DirectoryFolder() { Name = "logs", ParentId = rootTempDirectory.Id, Permissions = adminReadWritePermissions }
            };

            DirectoryFolder userDirectory = new() { Name = "user", ParentId = rootUsersDirectory.Id, Permissions = userReadWritePermissions };
            rootUsersDirectory.Entities = new()
            {
                userDirectory
            };

            DirectoryFolder homeDirectory = new() { Name = "home", ParentId = userDirectory.Id, Permissions = userReadWritePermissions };
            DirectoryFolder mailDirectory = new() { Name = "mail", ParentId = homeDirectory.Id, Permissions = userReadWritePermissions };
            mailDirectory.Entities = new()
            {
                new DirectoryFile()
                {
                    Name = "welcome-to-terminal-os",
                    Extension = "mail",
                    Contents = "This is a mail file in Terminal OS. Welcome!",
                    ParentId = mailDirectory.Id,
                    Permissions = userReadWritePermissions
                }
            };

            homeDirectory.Entities = new() { mailDirectory };

            userDirectory.Entities = new()
            {
                new DirectoryFolder() { Name = "config", ParentId = userDirectory.Id, Permissions = userReadWritePermissions },
                homeDirectory,
                new DirectoryFolder() { Name = "programs", ParentId = userDirectory.Id, Permissions = userReadWritePermissions },
            };

            return new() { rootDirectory };
        }
    }
}
