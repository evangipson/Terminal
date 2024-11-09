using System.Collections.Generic;
using Terminal.Models;

namespace Terminal.Constants
{
    public static class DirectoryConstants
    {
        public const string DirectorySeparator = "/";

        public const string TerminalPromptCharacter = ">";

        public static List<DirectoryEntity> GetDefaultDirectoryStructure()
        {
            DirectoryFolder rootDirectory = new() { Name = "/", IsRoot = true };
            DirectoryFolder rootSystemDirectory = new() { Name = "system", ParentId = rootDirectory.Id };
            DirectoryFolder rootUsersDirectory = new() { Name = "users", ParentId = rootDirectory.Id };
            DirectoryFolder rootTempDirectory = new() { Name = "temp", ParentId = rootDirectory.Id };
            rootDirectory.Entities = new()
            {
                rootSystemDirectory,
                rootUsersDirectory,
                rootTempDirectory
            };

            rootSystemDirectory.Entities = new()
            {
                new DirectoryFolder() { Name = "config", ParentId = rootSystemDirectory.Id },
                new DirectoryFolder() { Name = "device", ParentId = rootSystemDirectory.Id },
                new DirectoryFolder() { Name = "logs", ParentId = rootSystemDirectory.Id },
                new DirectoryFolder() { Name = "network", ParentId = rootSystemDirectory.Id },
                new DirectoryFolder() { Name = "programs", ParentId = rootSystemDirectory.Id }
            };

            rootTempDirectory.Entities = new()
            {
                new DirectoryFolder() { Name = "logs", ParentId = rootTempDirectory.Id }
            };

            DirectoryFolder userDirectory = new() { Name = "user", ParentId = rootUsersDirectory.Id };
            rootUsersDirectory.Entities = new()
            {
                userDirectory
            };

            DirectoryFolder homeDirectory = new() { Name = "home", ParentId = userDirectory.Id };
            DirectoryFolder mailDirectory = new() { Name = "mail", ParentId = homeDirectory.Id };
            mailDirectory.Entities = new()
            {
                new DirectoryFile()
                {
                    Name = "welcome-to-terminal-os",
                    Extension = "mail",
                    Contents = "This is a mail file in Terminal OS. Welcome!",
                    ParentId = mailDirectory.Id
                }
            };

            homeDirectory.Entities = new() { mailDirectory };

            userDirectory.Entities = new()
            {
                new DirectoryFolder() { Name = "config", ParentId = userDirectory.Id },
                homeDirectory,
                new DirectoryFolder() { Name = "programs", ParentId = userDirectory.Id },
            };

            return new() { rootDirectory };
        }
    }
}
