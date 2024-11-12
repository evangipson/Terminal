using System.Linq;
using System;
using Godot;

using Terminal.Enums;
using Terminal.Models;
using Terminal.Constants;
using System.Collections.Generic;

namespace Terminal.Services
{
    /// <summary>
    /// A global singleton that is responsible for manging autocompletion of partial user prompts.
    /// </summary>
    public partial class AutoCompleteService : Node
    {
        /// <summary>
        /// Invoked when the auto-complete phrase generation was not successful.
        /// <para>
        /// Will continue to run unless unsubscribed after running the method.
        /// </para>
        /// </summary>
        public event Action<string> OnInvalidAutocomplete;

        /// <summary>
        /// Invoked when an auto-completed phrase is generated successfully.
        /// <para>
        /// Will continue to run unless unsubscribed after running the method.
        /// </para>
        /// </summary>
        public event Action<string> OnAutocomplete;

        private static readonly List<UserCommand> _validAutoCompleteCommands = new()
        {
            UserCommand.ChangeDirectory,
            UserCommand.ViewFile,
            UserCommand.EditFile,
            UserCommand.ViewPermissions,
            UserCommand.ChangePermissions,
            UserCommand.ListDirectory,
            UserCommand.DeleteFile,
            UserCommand.DeleteDirectory
        };

        private DirectoryService _directoryService;
        private UserCommandService _userCommandService;
        private DirectoryEntity _autocompletedEntity;
        private string _partialPath;

        public override void _Ready()
        {
            _directoryService = GetNode<DirectoryService>(ServicePathConstants.DirectoryServicePath);
            _userCommandService = GetNode<UserCommandService>(ServicePathConstants.UserCommandServicePath);
        }

        /// <summary>
        /// Auto-completes an incomplete phrase entered by the user.
        /// </summary>
        /// <param name="currentCommand">
        /// The current user's terminal input to attempt to auto-complete.
        /// </param>
        public void AutocompletePhrase(string currentCommand)
        {
            var inputWithoutDirectory = currentCommand.Replace(_userCommandService.GetCommandPrompt(), string.Empty).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var userCommand = UserCommandService.EvaluateToken(inputWithoutDirectory.FirstOrDefault());
            var pathToSearch = inputWithoutDirectory.Skip(1).LastOrDefault() ?? string.Empty;
            if (!_validAutoCompleteCommands.Contains(userCommand))
            {
                OnInvalidAutocomplete?.Invoke(null);
                return;
            }

            // find out what type of search the user is attempting to autocomplete
            bool isRootSearch = pathToSearch.Equals(TerminalCharactersConstants.Separator.ToString());
            bool isRelativeSearch = inputWithoutDirectory.Length >= 1 && !pathToSearch.EndsWith('/') && !pathToSearch.StartsWith('/');
            bool isAbsoluteSearch = inputWithoutDirectory.Length == 2 && !string.IsNullOrEmpty(pathToSearch) && !pathToSearch.EndsWith('/') && pathToSearch.StartsWith('/');

            bool isShallowRelativeSearch = isRelativeSearch && !string.IsNullOrEmpty(pathToSearch) && !pathToSearch.Trim('/').Contains('/');
            bool isDeepRelativeSearch = isRelativeSearch && !string.IsNullOrEmpty(pathToSearch) && pathToSearch.Trim('/').Contains('/');
            bool isDeepAbsoluteSearch = isAbsoluteSearch && pathToSearch.Trim('/').Contains('/');

            bool isCompleteAbsolutePath = !isRootSearch && pathToSearch.StartsWith('/') && pathToSearch.EndsWith('/');
            bool isCompleteRelativePath = !pathToSearch.StartsWith('/') && pathToSearch.EndsWith('/');

            // listen to the last part of the path, if there are multiple slashes in the path
            // i.e.: /system/co -> [hit tab] -> /system/config
            if (isDeepRelativeSearch || isDeepAbsoluteSearch)
            {
                _partialPath = pathToSearch.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
            }
            // allow autocomplete from the root
            // i.e.: / -> [hit tab] -> /system/
            else if (isRootSearch)
            {
                _partialPath = string.Empty;
            }
            // listen to the first part of an absolute path
            // i.e.: /sy -> [hit tab] -> /system/
            else if (isAbsoluteSearch)
            {
                _partialPath = pathToSearch.TrimStart('/');
            }
            // listen to the first part of a relative path
            // i.e.: from /system -> con -> [hit tab] -> config/
            else if (isShallowRelativeSearch)
            {
                _partialPath = pathToSearch;
            }
            // create new result if the result is already matched or not present
            // i.e.: from /system -> config/ -> [hit tab] -> device/
            else if (string.IsNullOrEmpty(pathToSearch))
            {
                _partialPath = string.Empty;
            }

            // determine which directory the autocomplete search begins from
            DirectoryEntity directoryToSearch = (isAbsoluteSearch || isRootSearch || isCompleteAbsolutePath)
                ? _directoryService.GetRootDirectory()
                : _directoryService.GetCurrentDirectory();
            if (isDeepAbsoluteSearch)
            {
                var directoryWithoutPartialPath = string.Join('/', pathToSearch.Split('/', StringSplitOptions.RemoveEmptyEntries).SkipLast(1));
                directoryToSearch = _directoryService.GetAbsoluteDirectory($"/{directoryWithoutPartialPath}");
            }
            if (isDeepRelativeSearch)
            {
                var directoryWithoutPartialPath = string.Join('/', pathToSearch.Split('/', StringSplitOptions.RemoveEmptyEntries).SkipLast(1));
                directoryToSearch = _directoryService.GetRelativeDirectory(directoryWithoutPartialPath);
            }

            // filter autocomplete results down to the partial path defined by the user
            var filteredEntities = string.IsNullOrEmpty(_partialPath)
                ? directoryToSearch.Entities
                : directoryToSearch.Entities.Where(entity => entity.Name.StartsWith(_partialPath));

            // if there are no autocomplete results and the user is searching with a partial path,
            // list the directory contents then fill up the next input with the current command.
            if (!filteredEntities.Any())
            {
                OnInvalidAutocomplete?.Invoke(_directoryService.GetAbsoluteDirectoryPath(directoryToSearch));
                OnAutocomplete?.Invoke(currentCommand);
                return;
            }

            // qualify autocomplete results to be a file or folder unless the command is agnostic
            if (userCommand != UserCommand.ViewPermissions && userCommand != UserCommand.ChangePermissions)
            {
                filteredEntities = filteredEntities.Where(entity => entity.IsDirectory == (userCommand == UserCommand.ChangeDirectory || userCommand == UserCommand.ListDirectory || userCommand == UserCommand.DeleteDirectory));
            }

            DirectoryEntity matchingEntity = filteredEntities.FirstOrDefault();
            if (_autocompletedEntity != null)
            {
                // wrap the autocomplete results
                matchingEntity = filteredEntities.SkipWhile(p => p.Name != _autocompletedEntity.Name).Skip(1).FirstOrDefault()
                    ?? filteredEntities.FirstOrDefault();
            }

            // determine which path to show as a result
            var autoCompletedPath = (isAbsoluteSearch || isRootSearch || isCompleteAbsolutePath)
                ? _directoryService.GetAbsoluteEntityPath(matchingEntity)
                : _directoryService.GetRelativeEntityPath(matchingEntity);

            _autocompletedEntity = matchingEntity;
            OnAutocomplete?.Invoke(string.Concat(_userCommandService.GetCommandPrompt(), $"{inputWithoutDirectory.FirstOrDefault()} {autoCompletedPath}"));
        }
    }
}
