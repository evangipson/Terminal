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
    /// A global singleton that is responsible for managing users.
    /// </summary>
    public partial class UserService : Node
    {
        private DirectoryService _directoryService;

        public override void _Ready()
        {
            _directoryService = GetNode<DirectoryService>(ServicePathConstants.DirectoryServicePath);
        }

        /// <summary>
        /// Makes a user in the <c>/users/</c> folder.
        /// </summary>
        /// <param name="arguments">
        /// A collection of arguments that should contain the new user name.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> containing the status of the user creation.
        /// </returns>
        public string MakeUser(IEnumerable<string> arguments)
        {
            var userActionInvalidMessage = GetActionIsInvalidMessage(false, arguments, "user", "users");
            if (!string.IsNullOrEmpty(userActionInvalidMessage))
            {
                return userActionInvalidMessage;
            }

            var userName = arguments.FirstOrDefault();
            var rootUsersDirectory = _directoryService.GetRootDirectory().FindDirectory("users");

            rootUsersDirectory.Entities.Add(DirectoryConstants.GetDefaultUserDirectory(rootUsersDirectory, userName));
            return $"\"{userName}\" user created.";
        }

        /// <summary>
        /// Deletes a user from the <c>/users/</c> folder.
        /// </summary>
        /// <param name="arguments">
        /// A collection of arguments that should contain the user name.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> containing the status of the user deletion.
        /// </returns>
        public string DeleteUser(IEnumerable<string> arguments)
        {
            var userActionInvalidMessage = GetActionIsInvalidMessage(true, arguments, "user", "users");
            if (!string.IsNullOrEmpty(userActionInvalidMessage))
            {
                return userActionInvalidMessage;
            }

            var userName = arguments.FirstOrDefault();
            var rootUsersDirectory = _directoryService.GetRootDirectory().FindDirectory("users");
            var userDirectoryToDelete = rootUsersDirectory.FindDirectory(userName);

            rootUsersDirectory.Entities.Remove(userDirectoryToDelete);
            return $"\"{userName}\" user removed.";
        }

        /// <summary>
        /// Creates a user group in the <c>/users/groups/</c> folder.
        /// </summary>
        /// <param name="arguments">
        /// A collection of arguments that should contain the new user group name.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> containing the status of the user group creation.
        /// </returns>
        public string MakeGroup(IEnumerable<string> arguments)
        {
            var groupActionInvalidMessage = GetActionIsInvalidMessage(false, arguments, "user group", "users/groups");
            if (!string.IsNullOrEmpty(groupActionInvalidMessage))
            {
                return groupActionInvalidMessage;
            }

            var groupName = arguments.FirstOrDefault();
            var rootUserGroupsDirectory = _directoryService.GetRootDirectory().FindDirectory("users/groups");

            rootUserGroupsDirectory.Entities.Add(new DirectoryFolder()
            {
                Name = groupName,
                ParentId = rootUserGroupsDirectory.Id,
                Permissions = [Permission.AdminRead, Permission.AdminWrite, Permission.UserRead, Permission.UserWrite]
            });
            return $"\"{groupName}\" user group created.";
        }

        /// <summary>
        /// Deletes a user group from the <c>/users/groups/</c> folder.
        /// </summary>
        /// <param name="arguments">
        /// A collection of arguments that should contain the user group name.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> containing the status of the user group deletion.
        /// </returns>
        public string DeleteGroup(IEnumerable<string> arguments)
        {
            var groupActionInvalidMessage = GetActionIsInvalidMessage(true, arguments, "user group", "users/groups");
            if (!string.IsNullOrEmpty(groupActionInvalidMessage))
            {
                return groupActionInvalidMessage;
            }

            var groupName = arguments.FirstOrDefault();
            var rootUserGroupsDirectory = _directoryService.GetRootDirectory().FindDirectory($"users/groups");
            var groupDirectory = rootUserGroupsDirectory.FindDirectory(groupName);

            rootUserGroupsDirectory.Entities.Remove(groupDirectory);
            return $"\"{groupName}\" user group removed.";
        }

        /// <summary>
        /// Adds a user to a user group in the <c>/users/groups/</c> folder.
        /// </summary>
        /// <param name="arguments">
        /// A collection of arguments that should contain the user and user group name.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> containing the status of the user group addition.
        /// </returns>
        public string AddUserToGroup(IEnumerable<string> arguments)
        {
            var groupActionInvalidMessage = GetGroupActionIsInvalidMessage(false, arguments);
            if (!string.IsNullOrEmpty(groupActionInvalidMessage))
            {
                return groupActionInvalidMessage;
            }

            var userName = arguments.FirstOrDefault();
            var groupName = arguments.LastOrDefault();
            var rootUserGroupsDirectory = _directoryService.GetRootDirectory().FindDirectory($"users/groups/{groupName}");
            var groupDirectory = rootUserGroupsDirectory.FindDirectory(groupName);

            DirectoryFile userGroupFile = new()
            {
                Name = userName,
                ParentId = groupDirectory.Id,
                Permissions = [Permission.AdminRead, Permission.AdminWrite, Permission.UserRead, Permission.UserWrite]
            };
            groupDirectory.Entities.Add(userGroupFile);
            return $"\"{userName}\" added to the \"{groupName}\" user group.";
        }

        /// <summary>
        /// Deletes a user from a user group in the <c>/users/groups/</c> folder.
        /// </summary>
        /// <param name="arguments">
        /// A collection of arguments that should contain the user and user group name.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> containing the status of the user group removal.
        /// </returns>
        public string DeleteUserFromGroup(IEnumerable<string> arguments)
        {
            var groupActionInvalidMessage = GetGroupActionIsInvalidMessage(true, arguments);
            if (!string.IsNullOrEmpty(groupActionInvalidMessage))
            {
                return groupActionInvalidMessage;
            }

            var userName = arguments.FirstOrDefault();
            var groupName = arguments.LastOrDefault();
            var rootUserGroupsDirectory = _directoryService.GetRootDirectory().FindDirectory($"users/groups/{groupName}");
            var groupDirectory = rootUserGroupsDirectory.FindDirectory(groupName);

            var userFileInGroup = groupDirectory.FindFile(userName);
            groupDirectory.Entities.Remove(userFileInGroup);
            return $"\"{userName}\" removed from the \"{groupName}\" user group.";
        }

        /// <summary>
        /// Returns a list of all users in the <paramref name="groupName"/> user group.
        /// </summary>
        /// <param name="groupName">
        /// The name of the user group to get the users for.
        /// </param>
        /// <returns>
        /// A list of users from the provided <paramref name="groupName"/> user group.
        /// </returns>
        public string ViewUserGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                return "Attempted to view a user group, but user group name was not provided.";
            }

            var rootUserGroupsDirectory = _directoryService.GetRootDirectory().FindDirectory("users/groups");
            if (rootUserGroupsDirectory == null)
            {
                return $"Attempted to view the \"{groupName}\" user group, but there was no root /users/ directory.";
            }

            var groupDirectory = rootUserGroupsDirectory.FindDirectory(groupName);
            if (groupDirectory == null)
            {
                return $"\"{groupName}\" user group does not exist.";
            }

            var usersInGroup = groupDirectory.Entities.Where(entity => !entity.IsDirectory);
            if (!usersInGroup.Any())
            {
                return $"No users in the \"{groupName}\" user group.";
            }

            return string.Join('\n', $"{groupName} users", "═".Repeat($"{groupName} users".Length), string.Join(", ", usersInGroup));
        }

        private string GetActionIsInvalidMessage(bool forRemoval, IEnumerable<string> arguments, string targetEntity, string parentFolderPath)
        {
            var action = forRemoval ? "remove" : "make";
            var entityName = arguments.FirstOrDefault();
            if (string.IsNullOrEmpty(entityName))
            {
                return $"Attempted to {action} a {targetEntity}, but {targetEntity} name was not provided.";
            }

            var rootEntityDirectory = _directoryService.GetRootDirectory().FindDirectory(parentFolderPath);
            if (rootEntityDirectory == null)
            {
                return $"Attempted to {action} the \"{entityName}\" {targetEntity}, but there was no {parentFolderPath} directory.";
            }

            var entityDirectory = rootEntityDirectory.FindDirectory(entityName);
            if (forRemoval && entityDirectory == null)
            {
                return $"Attempted to {action} the \"{entityName}\" {targetEntity}, but it does not exist.";
            }
            if (!forRemoval && entityDirectory != null)
            {
                return $"Attempted to {action} the new {targetEntity} \"{entityName}\", but it already existed.";
            }

            return string.Empty;
        }

        private string GetGroupActionIsInvalidMessage(bool forRemoval, IEnumerable<string> arguments)
        {
            var command = forRemoval ? "removeuserfromgroup" : "addusertogroup";
            if (arguments.Count() != 2)
            {
                return $"\"{command}\" takes 2 arguments, use \"help {command}\" to see an example.";
            }

            var action = forRemoval ? "remove" : "make";
            var actionVerb = forRemoval ? "from" : "to";
            var userName = arguments.FirstOrDefault();
            if (string.IsNullOrEmpty(userName))
            {
                return $"Attempted to {action} a user {actionVerb} a user group, but user name was not provided.";
            }

            var groupName = arguments.LastOrDefault();
            if (string.IsNullOrEmpty(groupName))
            {
                return $"Attempted to {action} a user {actionVerb} a user group, but user group name was not provided.";
            }

            var rootUserGroupsDirectory = _directoryService.GetRootDirectory().FindDirectory("users/groups");
            if (rootUserGroupsDirectory == null)
            {
                return $"Attempted to {action} the \"{userName}\" user {actionVerb} the \"{groupName}\" user group, but there was no /users/groups/ directory.";
            }

            var groupDirectory = rootUserGroupsDirectory.FindDirectory(groupName);
            if (groupDirectory == null)
            {
                return $"\"{groupName}\" user group does not exist.";
            }

            var userFileInGroup = groupDirectory.FindFile(userName);
            if (forRemoval && userFileInGroup == null)
            {
                return $"\"{userName}\" is not part of the \"{groupName}\" user group.";
            }
            if (!forRemoval && userFileInGroup != null)
            {
                return $"\"{userName}\" is already a part of the \"{groupName}\" user group.";
            }

            return string.Empty;
        }
    }
}
