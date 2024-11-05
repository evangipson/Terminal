using System.Collections.Generic;
using System.Linq;
using Terminal.Models;

namespace Terminal.Constants
{
    public static class DirectoryConstants
    {
        public const string DirectorySeparator = "/";

        public const string TerminalPromptCharacter = ">";

        public static List<DirectoryEntity> GetDefaultDirectoryStructure()
        {
            DirectoryEntity rootDirectory = new() { Name = "/", IsDirectory = true, IsRoot = true };

            DirectoryEntity rootSystemDirectory = new() { Name = "system", IsDirectory = true, ParentId = rootDirectory.Id };
            DirectoryEntity rootUsersDirectory = new() { Name = "users", IsDirectory = true, ParentId = rootDirectory.Id };
            DirectoryEntity rootTempDirectory = new() { Name = "temp", IsDirectory = true, ParentId = rootDirectory.Id };

            rootDirectory.Entities = new() { rootSystemDirectory, rootUsersDirectory, rootTempDirectory };

            DirectoryEntity systemDirectory = rootDirectory.Entities.First(entity => entity.Name == "system");
            systemDirectory.Entities = new()
            {
                new() { Name = "config", IsDirectory = true, ParentId = systemDirectory.Id },
                new() { Name = "device", IsDirectory = true, ParentId = systemDirectory.Id },
                new() { Name = "logs", IsDirectory = true, ParentId = systemDirectory.Id },
                new() { Name = "network", IsDirectory = true, ParentId = systemDirectory.Id },
                new() { Name = "programs", IsDirectory = true, ParentId = systemDirectory.Id }
            };

            DirectoryEntity usersDirectory = rootDirectory.Entities.First(entity => entity.Name == "users");
            usersDirectory.Entities = new()
            {
                new() { Name = "user", IsDirectory = true, ParentId = usersDirectory.Id }
            };

            DirectoryEntity userDirectory = rootDirectory.Entities.First(entity => entity.Name == "users").Entities.First(entity => entity.Name == "user");
            userDirectory.Entities = new()
            {
                new() { Name = "config", IsDirectory = true, ParentId = userDirectory.Id },
                new() { Name = "home", IsDirectory = true, ParentId = userDirectory.Id },
                new() { Name = "programs", IsDirectory = true, ParentId = userDirectory.Id },
            };

            DirectoryEntity tempDirectory = rootDirectory.Entities.First(entity => entity.Name == "temp");
            tempDirectory.Entities = new()
            {
                new() { Name = "logs", IsDirectory = true, ParentId = tempDirectory.Id }
            };

            return new() { rootDirectory };
        }
    }
}
