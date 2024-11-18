using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private static readonly List<Permission> _permissionsDisplayMap = new()
        {
            Permission.UserRead,
            Permission.UserWrite,
            Permission.UserExecutable,
            Permission.AdminRead,
            Permission.AdminWrite,
            Permission.AdminExecutable
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
            var stringBuilder = new StringBuilder();
            foreach (var permission in _permissionsDisplayMap)
            {
                if (permissions.Contains(permission))
                {
                    stringBuilder.Append('1');
                    continue;
                }
                stringBuilder.Append('0');
            }

            return stringBuilder.ToString();
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
        private static List<Permission> GetPermissionFromInput(string newPermissionsSet)
        {
            if (string.IsNullOrEmpty(newPermissionsSet))
            {
                return null;
            }

            var newPermissionChars = newPermissionsSet.ToCharArray();
            if (newPermissionChars.Length != 6)
            {
                return null;
            }

            List<Permission> newPermissions = new();
            for (var i = newPermissionChars.Length - 1; i >= 0; i--)
            {
                if (newPermissionChars[i] != '1')
                {
                    continue;
                }

                newPermissions.Add(_permissionsDisplayMap.ElementAt(i));
            };

            if(!newPermissions.Any())
            {
                newPermissions.Add(Permission.None);
            }

            return newPermissions;
        }
    }
}
