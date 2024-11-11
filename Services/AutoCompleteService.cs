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
        public event Action OnInvalidAutocomplete;

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
            UserCommand.ChangePermissions
        };

        private DirectoryService _directoryService;
        private UserCommandService _userCommandService;
        private DirectoryEntity _autocompletedEntity;
        private string _partialPath;

        // Called when the node enters the scene tree for the first time.
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
            var inputWithoutDirectory = currentCommand.Replace(_userCommandService.GetCommandPrompt(), string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var userCommand = UserCommandService.EvaluateToken(inputWithoutDirectory.FirstOrDefault());
            var pathToSearch = inputWithoutDirectory.LastOrDefault();
            if (!_validAutoCompleteCommands.Contains(userCommand) || string.IsNullOrEmpty(pathToSearch))
            {
                OnInvalidAutocomplete?.Invoke();
                return;
            }

            // maintain the last input from the user during autocomplete
            var isAbsoluteSearch = pathToSearch.StartsWith('/');
            var isDeepRelativeSearch = !isAbsoluteSearch && pathToSearch.Trim('/').Contains('/');
            var isDeepAbsoluteSearch = isAbsoluteSearch && pathToSearch.Trim('/').Contains('/');
            if (!pathToSearch.EndsWith('/') && (isDeepRelativeSearch || isDeepAbsoluteSearch))
            {
                _partialPath = pathToSearch.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
            }
            else if (!pathToSearch.EndsWith('/') && isAbsoluteSearch)
            {
                _partialPath = isAbsoluteSearch ? pathToSearch.TrimStart('/') : pathToSearch;
            }

            // determine which directory the autocomplete search begins from
            DirectoryEntity directoryToSearch = isAbsoluteSearch ? _directoryService.GetRootDirectory() : _directoryService.GetCurrentDirectory();
            if (isDeepAbsoluteSearch)
            {
                var directoryWithoutPartialPath = string.Join('/', pathToSearch.Split('/', StringSplitOptions.RemoveEmptyEntries).SkipLast(1));
                directoryToSearch = _directoryService.GetAbsoluteDirectory(directoryWithoutPartialPath);
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

            // if there are no autocomplete results and the user is searching with a partial path, do nothing
            if (!filteredEntities.Any())
            {
                return;
            }

            // fill in the results of the autocomplete search, folders for change directory, and files for view or edit file
            if (userCommand == UserCommand.ChangeDirectory || userCommand == UserCommand.ViewFile || userCommand == UserCommand.EditFile)
            {
                filteredEntities = filteredEntities.Where(entity => entity.IsDirectory == (userCommand == UserCommand.ChangeDirectory));
            }

            DirectoryEntity matchingEntity = filteredEntities.FirstOrDefault();
            if (_autocompletedEntity != null)
            {
                // wrap the autocomplete results
                matchingEntity = filteredEntities.SkipWhile(p => p.Name != _autocompletedEntity.Name).Skip(1).FirstOrDefault() ?? filteredEntities.FirstOrDefault();
            }

            // determine which path to show as a result
            var autoCompletedPath = isAbsoluteSearch
                ? _directoryService.GetAbsoluteEntityPath(matchingEntity)
                : _directoryService.GetRelativeEntityPath(matchingEntity);

            _autocompletedEntity = matchingEntity;
            OnAutocomplete?.Invoke(string.Concat(_userCommandService.GetCommandPrompt(), $"{inputWithoutDirectory.FirstOrDefault()} {autoCompletedPath}"));
        }
    }
}
