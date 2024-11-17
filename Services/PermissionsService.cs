using System.Collections.Generic;
using System.Linq;
using Godot;

using Terminal.Constants;
using Terminal.Enums;

namespace Terminal.Services
{
    /// <summary>
    /// A global singleton service that manages <see cref="Permission"/>.
    /// </summary>
    public partial class PermissionsService : Node
    {
        private static readonly Dictionary<Permission, int> _permissionsDisplayMap = new()
        {
            [Permission.None] = 0b000000,
            [Permission.UserRead] = 0b000001,
            [Permission.UserWrite] = 0b000010,
            [Permission.UserExecutable] = 0b000100,
            [Permission.AdminRead] = 0b001000,
            [Permission.AdminWrite] = 0b010000,
            [Permission.AdminExecutable] = 0b100000
        };

        private DirectoryService _directoryService;

        public override void _Ready()
        {
            _directoryService = GetNode<DirectoryService>(ServicePathConstants.DirectoryServicePath);
        }

        /// <summary>
        /// Views the permissions of the file or folder with the <paramref name="entityName"/> name.
        /// </summary>
        /// <param name="entityName">
        /// The name of the file or folder to view the permissions of.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> containing the permissions for the <paramref name="entityName"/>.
        /// </returns>
        public string ViewPermissions(string entityName)
        {
            var entity = entityName.StartsWith('/')
                ? _directoryService.GetAbsoluteFile(entityName) ?? _directoryService.GetAbsoluteDirectory(entityName.TrimEnd('/'))
                : _directoryService.GetRelativeFile(entityName) ?? _directoryService.GetRelativeDirectory(entityName.TrimEnd('/'));

            if (entityName == "/" || entityName == "root")
            {
                entity = _directoryService.GetRootDirectory();
            }

            if (entity == null)
            {
                return $"No folder or file with the name \"{entityName}\" exists.";
            }

            return GetPermissionDisplay(entity.Permissions);
        }

        /// <summary>
        /// Changes the permissions for the file or folder with the <paramref name="entityName"/> name.
        /// </summary>
        /// <param name="entityName">
        /// The name of the file or folder to change the permissions of.
        /// </param>
        /// <param name="newPermissionsSet">
        /// The new set of permissions for the <paramref name="entityName"/>.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> containing the status of the permissions change.
        /// </returns>
        public string ChangePermissions(string entityName, string newPermissionsSet)
        {
            var entity = entityName.StartsWith('/')
                ? _directoryService.GetAbsoluteFile(entityName) ?? _directoryService.GetAbsoluteDirectory(entityName.TrimEnd('/'))
                : _directoryService.GetRelativeFile(entityName) ?? _directoryService.GetRelativeDirectory(entityName.TrimEnd('/'));

            if (entityName == "/" || entityName == "root")
            {
                entity = _directoryService.GetRootDirectory();
            }

            if (entity == null)
            {
                return $"No folder or file with the name \"{entityName}\" exists.";
            }

            var newPermissions = GetPermissionFromInput(newPermissionsSet);
            if (newPermissions == null)
            {
                return $"Permissions set \"{newPermissionsSet}\" was in an incorrect format. Permission sets are 6 bits (011011).";
            }

            entity.Permissions = newPermissions;
            return $"\"{entityName}\" permissions updated to {GetPermissionDisplay(entity.Permissions)}";
        }

        /// <summary>
        /// Gets a display value for the provided <paramref name="permissions"/> collection.
        /// </summary>
        /// <param name="permissions">
        /// The collection of <see cref="Permission"/> to display.
        /// </param>
        /// <returns>
        /// A display value for the provided <paramref name="permissions"/>.
        /// </returns>
        public static string GetPermissionDisplay(IEnumerable<Permission> permissions)
        {
            int permissionDisplay = 0;
            foreach(var permission in permissions)
            {
                permissionDisplay += _permissionsDisplayMap[permission];
            }

            return string.Format("{0:b6}", (byte)permissionDisplay);
        }

        /// <summary>
        /// Gets a collection of <see cref="Permission"/> from the provided <paramref name="newPermissionsSet"/>.
        /// <para>
        /// Returns <see langword="null"/> if <paramref name="newPermissionsSet"/> is in an invalid format.
        /// </para>
        /// </summary>
        /// <param name="newPermissionsSet">
        /// The <see langword="string"/> to build the new permission set from.
        /// </param>
        /// <returns>
        /// A collection of <see cref="Permission"/> based on the provided <paramref name="newPermissionsSet"/>,
        /// assuming it's in the right format. Defaults to <see langword="null"/>.
        /// </returns>
        public static List<Permission> GetPermissionFromInput(string newPermissionsSet)
        {
            if(string.IsNullOrEmpty(newPermissionsSet))
            {
                return null;
            }

            var newPermissionChars = newPermissionsSet.ToCharArray();
            if(newPermissionChars.Length != 6)
            {
                return null;
            }

            return newPermissionChars.Select((permissionChar, index) =>
            {
                if (permissionChar != '1')
                {
                    return Permission.None;
                }

                return index switch
                {
                    5 => Permission.UserRead,
                    4 => Permission.UserWrite,
                    3 => Permission.UserExecutable,
                    2 => Permission.AdminRead,
                    1 => Permission.AdminWrite,
                    0 => Permission.AdminExecutable,
                    _ => Permission.None
                };
            }).Where(permission => permission != Permission.None).ToList();
        }
    }
}
