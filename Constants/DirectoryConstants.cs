﻿using System;
using System.Collections.Generic;
using System.Linq;

using Terminal.Enums;
using Terminal.Models;
using Terminal.Services;

namespace Terminal.Constants
{
    /// <summary>
    /// A <see langword="static"/> collection of constant values for managing a directory.
    /// </summary>
    public static class DirectoryConstants
    {
        private static readonly List<Permission> _adminReadWritePermissions = new() { Permission.AdminRead, Permission.AdminWrite };
        private static readonly List<Permission> _userReadWritePermissions = new() { Permission.UserRead, Permission.UserWrite };
        private static readonly List<Permission> _userExecutablePermissions = new() { Permission.UserRead, Permission.UserExecutable };

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
            DirectoryFolder rootDirectory = new() { Name = DirectorySeparator, IsRoot = true, Permissions = _adminReadWritePermissions };
            DirectoryFolder rootSystemDirectory = new() { Name = "system", ParentId = rootDirectory.Id, Permissions = _adminReadWritePermissions };
            DirectoryFolder rootUsersDirectory = new() { Name = "users", ParentId = rootDirectory.Id, Permissions = _adminReadWritePermissions };
            DirectoryFolder rootTempDirectory = new() { Name = "temp", ParentId = rootDirectory.Id, Permissions = _adminReadWritePermissions };
            rootDirectory.Entities = new()
            {
                rootSystemDirectory,
                rootUsersDirectory,
                rootTempDirectory
            };

            // Fill the "/system/programs/" folder with all commands as executable files
            DirectoryFolder systemProgramsDirectory = new() { Name = "programs", ParentId = rootSystemDirectory.Id, Permissions = _adminReadWritePermissions };
            systemProgramsDirectory.Entities = UserCommandService.GetAllCommands().Select(command => new DirectoryEntity()
            {
                Name = command.Key,
                Contents = string.Join("\n", command.Value.Select(commandInfo => $"[\"{commandInfo.Key}\": \"{commandInfo.Value}\"]")),
                ParentId = systemProgramsDirectory.Id,
                Permissions = _userExecutablePermissions
            }).ToList();

            DirectoryEntity systemDeviceDirectory = GetDefaultSystemDeviceDirectory(rootSystemDirectory.Id);

            DirectoryFolder systemNetworkDirectory = new() { Name = "network", ParentId = rootSystemDirectory.Id, Permissions = _adminReadWritePermissions };
            systemNetworkDirectory.Entities = new()
            {
                new DirectoryFile()
                {
                    Name = "ethernet",
                    Contents = "device:eth-0\ncapacity:1073741824\nactive:true\nipv6:2bae::a93c::dd1e::8ane\nipv8:zEw-F_92!#2A3(3j",
                    ParentId = systemNetworkDirectory.Id,
                    Permissions = _adminReadWritePermissions
                },
                new DirectoryFile()
                {
                    Name = "loopback",
                    Contents = "device:local-0\ncapacity:0\nactive:true\nipv6:fe02::29aa::39ba::f12e\nipv8:a!9v(J#M8W*E3@inld",
                    ParentId = systemNetworkDirectory.Id,
                    Permissions = _adminReadWritePermissions
                }
            };

            rootSystemDirectory.Entities = new()
            {
                new DirectoryFolder() { Name = "config", ParentId = rootSystemDirectory.Id, Permissions = _adminReadWritePermissions },
                systemDeviceDirectory,
                new DirectoryFolder() { Name = "logs", ParentId = rootSystemDirectory.Id, Permissions = _adminReadWritePermissions },
                systemNetworkDirectory,
                systemProgramsDirectory
            };

            rootTempDirectory.Entities = new()
            {
                new DirectoryFolder() { Name = "logs", ParentId = rootTempDirectory.Id, Permissions = _adminReadWritePermissions }
            };

            DirectoryFolder userDirectory = new() { Name = "user", ParentId = rootUsersDirectory.Id, Permissions = _userReadWritePermissions };
            rootUsersDirectory.Entities = new()
            {
                userDirectory
            };

            DirectoryFolder homeDirectory = new() { Name = "home", ParentId = userDirectory.Id, Permissions = _userReadWritePermissions };
            DirectoryFolder mailDirectory = new() { Name = "mail", ParentId = homeDirectory.Id, Permissions = _userReadWritePermissions };
            mailDirectory.Entities = new()
            {
                new DirectoryFile()
                {
                    Name = "welcome-to-terminal-os",
                    Extension = "mail",
                    Contents = "This is a mail file in Terminal OS. Welcome!",
                    ParentId = mailDirectory.Id,
                    Permissions = _userReadWritePermissions
                }
            };

            homeDirectory.Entities = new() { mailDirectory };

            userDirectory.Entities = new()
            {
                new DirectoryFolder() { Name = "config", ParentId = userDirectory.Id, Permissions = _userReadWritePermissions },
                homeDirectory,
                new DirectoryFolder() { Name = "programs", ParentId = userDirectory.Id, Permissions = _userReadWritePermissions },
            };

            return new() { rootDirectory };
        }

        private static DirectoryEntity GetDefaultSystemDeviceDirectory(Guid rootSystemDirectoryId)
        {
            DirectoryFolder systemDeviceDirectory = new() { Name = "device", ParentId = rootSystemDirectoryId, Permissions = _adminReadWritePermissions };
            DirectoryFolder deviceDisplayDirectory = new() { Name = "display", ParentId = systemDeviceDirectory.Id, Permissions = _adminReadWritePermissions };
            deviceDisplayDirectory.Entities = new()
            {
                new DirectoryFile()
                {
                    Name = "0",
                    Contents = "name:Monitor\nmanufacturer:Display Bois\nh_resolution:1920\nv_resolution:1200",
                    ParentId = deviceDisplayDirectory.Id
                }
            };
            DirectoryFolder deviceInputDirectory = new() { Name = "input", ParentId = systemDeviceDirectory.Id, Permissions = _adminReadWritePermissions };
            deviceInputDirectory.Entities = new()
            {
                new DirectoryFile()
                {
                    Name = "0",
                    Contents = "name:USB\nmanufacturer:FlashDrive Inc.\nsize:34359738368\nremaining:28154768992",
                    ParentId = deviceInputDirectory.Id
                }
            };
            DirectoryFolder deviceMemoryDirectory = new() { Name = "memory", ParentId = systemDeviceDirectory.Id, Permissions = _adminReadWritePermissions };
            deviceMemoryDirectory.Entities = new()
            {
                new DirectoryFile()
                {
                    Name = "0",
                    Contents = "name:L1CACHE\nmanufacturer:Notel\nsize:32768\nremaining:32768",
                    ParentId = deviceMemoryDirectory.Id
                },
                new DirectoryFile()
                {
                    Name = "1",
                    Contents = "name:L2CACHE\nmanufacturer:Notel\nsize:6291456\nremaining:6291456",
                    ParentId = deviceMemoryDirectory.Id
                },
                new DirectoryFile()
                {
                    Name = "2",
                    Contents = "name:DDR2\nmanufacturer:Memory Guys\nsize:1073741824\nremaining:855253756",
                    ParentId = deviceMemoryDirectory.Id
                },
                new DirectoryFile()
                {
                    Name = "3",
                    Contents = "name:DDR2\nmanufacturer:Memory Guys\nsize:1073741824\nremaining:913745724",
                    ParentId = deviceMemoryDirectory.Id
                },
            };
            DirectoryFolder deviceProcessorDirectory = new() { Name = "processor", ParentId = systemDeviceDirectory.Id, Permissions = _adminReadWritePermissions };
            deviceProcessorDirectory.Entities = new()
            {
                new DirectoryFile()
                {
                    Name = "0",
                    Contents = "name:CPU\nmanufacturer:Notel\ncores:8\nspeed:2.2Ghz",
                    ParentId = deviceProcessorDirectory.Id
                }
            };
            DirectoryFolder deviceStorageDirectory = new() { Name = "storage", ParentId = systemDeviceDirectory.Id, Permissions = _adminReadWritePermissions };
            deviceStorageDirectory.Entities = new()
            {
                new DirectoryFile()
                {
                    Name = "0",
                    Contents = "name:SSD.M2\nmanufacturer:SolidStateTech\nsize:2199023255552\nremaining:1949015253411",
                    ParentId = deviceStorageDirectory.Id
                },
                new DirectoryFile()
                {
                    Name = "1",
                    Contents = "name:SSD.M2\nmanufacturer:SolidStateTech\nsize:2199023255552\nremaining:2016489954243",
                    ParentId = deviceStorageDirectory.Id
                }
            };

            systemDeviceDirectory.Entities = new()
            {
                deviceDisplayDirectory,
                deviceInputDirectory,
                deviceMemoryDirectory,
                deviceProcessorDirectory,
                deviceStorageDirectory
            };

            return systemDeviceDirectory;
        }
    }
}
