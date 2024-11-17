using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Enums;
using Terminal.Factories;
using Terminal.Models;

namespace Terminal.Constants
{
    public static class DirectoryConstants
    {
        /// <summary>
        /// The separator character for lines of help information, written into the executable files.
        /// </summary>
        public const string HelpLineSeparator = "\n---\n";

        /// <summary>
        /// The separator character for keys and values of help information, written into the executable files.
        /// </summary>
        public const string HelpKeyValueSeparator = ":=:";

        /// <summary>
        /// A list of keys and values of all default commands for the terminal, where the key is the command name,
        /// and the value is a collection of help information for that command.
        /// </summary>
        public static readonly Dictionary<string, Dictionary<string, string>> AllDefaultCommands = new()
        {
            ["exit"] = new()
            {
                ["COMMAND"] = "exit",
                ["REMARKS"] = "Exits Terminal OS.",
            },
            ["help"] = new()
            {
                ["COMMAND"] = "help",
                ["REMARKS"] = "Displays help about Terminal OS commands.",
                ["EXAMPLES"] = "help commands    : Display information about the terminal commands."
            },
            ["color"] = new()
            {
                ["COMMAND"] = "color",
                ["REMARKS"] = "Changes the color of the terminal output.\nColor information is stored in the color config file at /system/config/color.conf.",
                ["EXAMPLES"] = $"color green    : Change the terminal output to green.",
                ["COLORS"] = "@@@@"
            },
            ["save"] = new()
            {
                ["COMMAND"] = "save",
                ["REMARKS"] = "Saves the state of the terminal."
            },
            ["commands"] = new()
            {
                ["COMMAND"] = "commands",
                ["REMARKS"] = "Displays information about the terminal commands. Use help [command] to get more information about each command.",
                ["COMMANDS"] = "$$$$"
            },
            ["list"] = new()
            {
                ["COMMAND"] = "ls [list]",
                ["REMARKS"] = "Lists contents of a directory.",
                ["EXAMPLES"] = "ls        : List the contents of the current directory.\nls /system : List the contents of the \"/system\" directory."
            },
            ["change"] = new()
            {
                ["COMMAND"] = "cd [change] [changedir]",
                ["REMARKS"] = "Changes directory.",
                ["EXAMPLES"] = $"cd ~    : Change directory to the default home directory for the current user."
            },
            ["view"] = new()
            {
                ["COMMAND"] = "vw [view]",
                ["REMARKS"] = "View the contents of a file.",
                ["EXAMPLES"] = "view file.ext    : List the contents of the file.ext file."
            },
            ["makefile"] = new()
            {
                ["COMMAND"] = "mf [makefile]",
                ["REMARKS"] = "Make a file.",
                ["EXAMPLES"] = "mf new.txt    : Creates a blank file called 'new.txt' in the current directory."
            },
            ["makedirectory"] = new()
            {
                ["COMMAND"] = "md [makedir] [makedirectory]",
                ["REMARKS"] = "Make a directory.",
                ["EXAMPLES"] = "md newdir    : Creates an empty directory called 'newdir' in the current directory."
            },
            ["edit"] = new()
            {
                ["COMMAND"] = "edit",
                ["REMARKS"] = "Edit a file.",
                ["EXAMPLES"] = "edit new.txt    : Edits the 'new.txt' file in the current directory."
            },
            ["listhardware"] = new()
            {
                ["COMMAND"] = "lhw [listhardware]",
                ["REMARKS"] = "View a list of hardware for the system."
            },
            ["viewpermissions"] = new()
            {
                ["COMMAND"] = "vp [viewperm] [viewpermissions]",
                ["REMARKS"] = "View the permissions of a file or directory.",
                ["FORMAT"] = "Permission sets are 6 bits in order: \"admin executable\", \"admin write\", \"admin read\", \"user executable\", \"user write\", and \"user read\". If no bits are set, the permissions are \"none\".",
                ["EXAMPLE SETS"] = "111111: \"admin executable\", \"admin write\", \"admin read\", \"user executable\", \"user write\", and \"user read\".\n000000: \"none\".",
                ["EXAMPLES"] = "vp new.txt    : Shows the permissions for the 'new.txt' file in the current directory."
            },
            ["changepermissions"] = new()
            {
                ["COMMAND"] = "chp [changeperm] [changepermissions]",
                ["REMARKS"] = "Changes the permissions of a file or directory.",
                ["FORMAT"] = "Permission sets are 6 bits in order: \"admin executable\", \"admin write\", \"admin read\", \"user executable\", \"user write\", and \"user read\". If no bits are set, the permissions are \"none\".",
                ["EXAMPLE SETS"] = "111111: \"admin executable\", \"admin write\", \"admin read\", \"user executable\", \"user write\", and \"user read\".\n000000: \"none\".",
                ["EXAMPLES"] = "chp new.txt 010100    : Updates the permissions for the 'new.txt' file to \"admin write\" and \"user executable\"."
            },
            ["date"] = new()
            {
                ["COMMAND"] = "date",
                ["REMARKS"] = "View the current date."
            },
            ["time"] = new()
            {
                ["COMMAND"] = "time",
                ["REMARKS"] = "View the current time."
            },
            ["now"] = new()
            {
                ["COMMAND"] = "now [dt] [datetime] [current]",
                ["REMARKS"] = "View the current date and time."
            },
            ["network"] = new()
            {
                ["COMMAND"] = "net [network]",
                ["REMARKS"] = "View current networking information.",
                ["ARGUMENTS"] = "-a [--active]: Show active networks.\n-v8 [--ipv8]: Show ipv8 addresses.",
                ["EXAMPLES"] = "net -a -v8    : Show the ipv8 addresses for active networks."
            },
            ["deletefile"] = new()
            {
                ["COMMAND"] = "df [deletefile]",
                ["REMARKS"] = "Deletes a file.",
                ["EXAMPLES"] = "df new.txt    : Deletes the new.txt file."
            },
            ["deletedirectory"] = new()
            {
                ["COMMAND"] = "dd [deletedirectory]",
                ["REMARKS"] = "Deletes a folder.",
                ["ARGUMENTS"] = "-r [--recursive]: Deletes a folder and all child files and folders.",
                ["EXAMPLES"] = "dd newdir    : Deletes the \"newdir\" folder.\ndd newdir -r : Deletes the \"newdir\" folder and all files and folders inside of it."
            },
            ["ping"] = new()
            {
                ["COMMAND"] = "ping",
                ["REMARKS"] = "Network utility used to test a host's reachability.",
                ["ARGUMENTS"] = "-v6 [--ipv6]: Ping the address using ipv6.\n-v8 [--ipv8]: Ping the address using ipv8.",
                ["EXAMPLES"] = "ping 12525         : Pings the \"12525\" ipv6 or ipv8 address 5 times.\nping 127.0.0.1 -v6 : Pings the \"127.0.0.1\" ipv6 address 5 times."
            },
            ["movefile"] = new()
            {
                ["COMMAND"] = "mf [movefile]",
                ["REMARKS"] = "Moves a file to a destination folder.",
                ["EXAMPLES"] = "mf new-file.txt /users/user/    : Moves the \"new-file.txt\" file in the current directory to the \"/users/user/\" folder."
            },
            ["movedirectory"] = new()
            {
                ["COMMAND"] = "md [movedir] [movedirectory]",
                ["REMARKS"] = "Moves a folder and all files and folders within to a destination folder.",
                ["EXAMPLES"] = "md olddir /users/user/newdir    : Moves the \"olddir\" folder in the current directory to the \"/users/user/newdir\" folder."
            },
            ["makeuser"] = new()
            {
                ["COMMAND"] = "mu [makeuser]",
                ["REMARKS"] = "Makes a new user, with all the default files and folders, in /users/.",
                ["EXAMPLES"] = "mu newuser    : Makes a user with the name \"newuser\"."
            }
        };

        private static readonly List<Permission> _adminReadWritePermissions = new() { Permission.AdminRead, Permission.AdminWrite };
        private static readonly List<Permission> _userReadWritePermissions = new() { Permission.AdminRead, Permission.AdminWrite, Permission.UserRead, Permission.UserWrite };
        private static readonly List<Permission> _userReadPermissions = new() { Permission.AdminRead, Permission.AdminWrite, Permission.UserRead };
        private static readonly List<Permission> _userExecutablePermissions = new() { Permission.AdminRead, Permission.AdminWrite, Permission.AdminExecutable, Permission.UserRead, Permission.UserExecutable };

        /// <summary>
        /// Gets the default directory structure of the file system, filled with all required system files and a user directory.
        /// </summary>
        /// <returns>
        /// A default list of <see cref="DirectoryEntity"/> used in the file system.
        /// </returns>
        public static List<DirectoryEntity> GetDefaultDirectoryStructure()
        {
            DirectoryFolder rootDirectory = new() { Name = TerminalCharactersConstants.Separator.ToString(), IsRoot = true, Permissions = _userReadPermissions };
            DirectoryFolder rootSystemDirectory = new() { Name = "system", ParentId = rootDirectory.Id, Permissions = _userReadPermissions };
            DirectoryFolder rootUsersDirectory = new() { Name = "users", ParentId = rootDirectory.Id, Permissions = _userReadPermissions };
            DirectoryFolder rootTempDirectory = new() { Name = "temp", ParentId = rootDirectory.Id, Permissions = _userReadPermissions };
            rootDirectory.Entities = new()
            {
                rootSystemDirectory,
                rootUsersDirectory,
                rootTempDirectory
            };

            DirectoryFolder systemConfigDirectory = new() { Name = "config", ParentId = rootSystemDirectory.Id, Permissions = _userReadPermissions };
            systemConfigDirectory.Entities = new()
            {
                new DirectoryFile()
                {
                    Name = "color",
                    Extension = "conf",
                    Contents = "green:377a1c\nblue:1c387a\nteal:1c677a\npurple:5e1c7a\norange:7a2f1c\nred:7a1c38",
                    ParentId = systemConfigDirectory.Id,
                    Permissions = _userReadWritePermissions
                }
            };

            DirectoryEntity systemDeviceDirectory = GetDefaultSystemDeviceDirectory(rootSystemDirectory.Id);
            DirectoryFolder systemNetworkDirectory = new() { Name = "network", ParentId = rootSystemDirectory.Id, Permissions = _userReadPermissions };
            systemNetworkDirectory.Entities = new()
            {
                new DirectoryFile()
                {
                    Name = "ethernet",
                    Contents = $"device:eth-0\ncapacity:1073741824\nactive:true\nipv6:{NetworkFactory.CreateIpAddressV6()}\nipv8:{NetworkFactory.CreateIpAddressV8()}",
                    ParentId = systemNetworkDirectory.Id,
                    Permissions = _adminReadWritePermissions
                },
                new DirectoryFile()
                {
                    Name = "loopback",
                    Contents = $"device:local-0\ncapacity:0\nactive:true\nipv6:{NetworkFactory.CreateIpAddressV6(loopback: true)}\nipv8:{NetworkFactory.CreateIpAddressV8(loopback: true)}",
                    ParentId = systemNetworkDirectory.Id,
                    Permissions = _adminReadWritePermissions
                }
            };

            // Fill the "/system/programs/" folder with all commands as executable files
            DirectoryFolder systemProgramsDirectory = new() { Name = "programs", ParentId = rootSystemDirectory.Id, Permissions = _userReadPermissions };
            systemProgramsDirectory.Entities = AllDefaultCommands.Select(command => new DirectoryEntity()
            {
                Name = command.Key,
                Contents = string.Join(HelpLineSeparator, command.Value.Select(commandInfo => $"[{commandInfo.Key}{HelpKeyValueSeparator}{commandInfo.Value}]")),
                ParentId = systemProgramsDirectory.Id,
                Permissions = _userExecutablePermissions
            }).ToList();

            rootSystemDirectory.Entities = new()
            {
                systemConfigDirectory,
                systemDeviceDirectory,
                new DirectoryFolder() { Name = "logs", ParentId = rootSystemDirectory.Id, Permissions = _userReadPermissions },
                systemNetworkDirectory,
                systemProgramsDirectory
            };

            rootTempDirectory.Entities = new()
            {
                new DirectoryFolder() { Name = "logs", ParentId = rootTempDirectory.Id, Permissions = _userReadPermissions }
            };

            DirectoryEntity userDirectory = GetDefaultUserDirectory(rootUsersDirectory, "user");
            var userHomeDirectory = userDirectory.Entities
                .Where(entity => entity.Name.Equals("home", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
            if(userHomeDirectory != null)
            {
                userHomeDirectory.IsHome = true;
            }

            rootUsersDirectory.Entities = new()
            {
                userDirectory
            };

            return new() { rootDirectory };
        }

        /// <summary>
        /// Gets the default user directory structure for <paramref name="userName"/>, and hooks up necessary entity links for all files and folders.
        /// </summary>
        /// <param name="rootUsersDirectory">
        /// The <c>/users/</c> directory, where the user will be made.
        /// </param>
        /// <param name="userName">
        /// The name of the new user.
        /// </param>
        /// <returns>
        /// A new user directory with all the default files and folders.
        /// </returns>
        public static DirectoryEntity GetDefaultUserDirectory(DirectoryEntity rootUsersDirectory, string userName)
        {
            DirectoryFolder userDirectory = new() { Name = userName, ParentId = rootUsersDirectory.Id, Permissions = _userReadWritePermissions };
            DirectoryFolder configDirectory = new() { Name = "config", ParentId = userDirectory.Id, Permissions = _userReadWritePermissions };
            configDirectory.Entities = new()
            {
                new DirectoryFile()
                {
                    Name = "user",
                    Extension = "conf",
                    Contents = "volume:100",
                    ParentId = configDirectory.Id,
                    Permissions = _userReadWritePermissions
                }
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
                configDirectory,
                homeDirectory,
                new DirectoryFolder() { Name = "programs", ParentId = userDirectory.Id, Permissions = _userReadWritePermissions },
            };

            return userDirectory;
        }

        private static DirectoryEntity GetDefaultSystemDeviceDirectory(Guid rootSystemDirectoryId)
        {
            DirectoryFolder systemDeviceDirectory = new() { Name = "device", ParentId = rootSystemDirectoryId, Permissions = _userReadPermissions };
            DirectoryFolder deviceDisplayDirectory = new() { Name = "display", ParentId = systemDeviceDirectory.Id, Permissions = _userReadPermissions };
            deviceDisplayDirectory.Entities = new()
            {
                new DirectoryFile()
                {
                    Name = "0",
                    Contents = "name:Monitor\nmanufacturer:Display Bois\nh_resolution:1920\nv_resolution:1200",
                    ParentId = deviceDisplayDirectory.Id,
                    Permissions = _userReadPermissions
                }
            };
            DirectoryFolder deviceInputDirectory = new() { Name = "input", ParentId = systemDeviceDirectory.Id, Permissions = _userReadPermissions };
            deviceInputDirectory.Entities = new()
            {
                new DirectoryFile()
                {
                    Name = "0",
                    Contents = "name:USB\nmanufacturer:FlashDrive Inc.\nsize:34359738368\nremaining:28154768992",
                    ParentId = deviceInputDirectory.Id,
                    Permissions = _userReadPermissions
                }
            };
            DirectoryFolder deviceMemoryDirectory = new() { Name = "memory", ParentId = systemDeviceDirectory.Id, Permissions = _userReadPermissions };
            deviceMemoryDirectory.Entities = new()
            {
                new DirectoryFile()
                {
                    Name = "0",
                    Contents = "name:L1CACHE\nmanufacturer:Notel\nsize:32768\nremaining:32768",
                    ParentId = deviceMemoryDirectory.Id,
                    Permissions = _userReadPermissions
                },
                new DirectoryFile()
                {
                    Name = "1",
                    Contents = "name:L2CACHE\nmanufacturer:Notel\nsize:6291456\nremaining:6291456",
                    ParentId = deviceMemoryDirectory.Id,
                    Permissions = _userReadPermissions
                },
                new DirectoryFile()
                {
                    Name = "2",
                    Contents = "name:DDR2\nmanufacturer:Memory Guys\nsize:1073741824\nremaining:855253756",
                    ParentId = deviceMemoryDirectory.Id,
                    Permissions = _userReadPermissions
                },
                new DirectoryFile()
                {
                    Name = "3",
                    Contents = "name:DDR2\nmanufacturer:Memory Guys\nsize:1073741824\nremaining:913745724",
                    ParentId = deviceMemoryDirectory.Id,
                    Permissions = _userReadPermissions
                },
            };
            DirectoryFolder deviceProcessorDirectory = new() { Name = "processor", ParentId = systemDeviceDirectory.Id, Permissions = _userReadPermissions };
            deviceProcessorDirectory.Entities = new()
            {
                new DirectoryFile()
                {
                    Name = "0",
                    Contents = "name:CPU\nmanufacturer:Notel\ncores:8\nspeed:2.2Ghz",
                    ParentId = deviceProcessorDirectory.Id,
                    Permissions = _userReadPermissions
                }
            };
            DirectoryFolder deviceStorageDirectory = new() { Name = "storage", ParentId = systemDeviceDirectory.Id, Permissions = _userReadPermissions };
            deviceStorageDirectory.Entities = new()
            {
                new DirectoryFile()
                {
                    Name = "0",
                    Contents = "name:SSD.M2\nmanufacturer:SolidStateTech\nsize:2199023255552\nremaining:1949015253411",
                    ParentId = deviceStorageDirectory.Id,
                    Permissions = _userReadPermissions
                },
                new DirectoryFile()
                {
                    Name = "1",
                    Contents = "name:SSD.M2\nmanufacturer:SolidStateTech\nsize:2199023255552\nremaining:2016489954243",
                    ParentId = deviceStorageDirectory.Id,
                    Permissions = _userReadPermissions
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
