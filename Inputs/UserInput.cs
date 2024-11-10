using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

using Terminal.Audio;
using Terminal.Constants;
using Terminal.Enums;
using Terminal.Models;
using Terminal.Services;

namespace Terminal.Inputs
{
    public partial class UserInput : TextEdit
    {
        [Signal]
        public delegate void EvaluatedEventHandler();

        [Signal]
        public delegate void KnownCommandEventHandler(string message);

        [Signal]
        public delegate void ColorCommandEventHandler(string colorName);

        [Signal]
        public delegate void SaveCommandEventHandler();

        [Signal]
        public delegate void ListDirectoryCommandEventHandler();

        [Signal]
        public delegate void ChangeDirectoryCommandEventHandler(string newDirectory);

        [Signal]
        public delegate void MakeFileCommandEventHandler(string fileName);

        [Signal]
        public delegate void MakeDirectoryCommandEventHandler(string directoryName);

        [Signal]
        public delegate void EditFileCommandEventHandler(string fileName);

        [Signal]
        public delegate void ListHardwareCommandEventHandler();

        [Signal]
        public delegate void ViewPermissionsCommandEventHandler(string entityName);

        [Signal]
        public delegate void ChangePermissionsCommandEventHandler(string entityName, string newPermissionSet);


        private static readonly List<UserCommand> _validAutoCompleteCommands = new()
        {
            UserCommand.ChangeDirectory,
            UserCommand.ViewFile,
            UserCommand.EditFile
        };

        private KeyboardSounds _keyboardSounds;
        private PersistService _persistService;
        private UserCommandEvaluator _userCommandEvaluator;
        private bool _hasFocus = false;
        private int _commandMemoryIndex;
        private DirectoryEntity _autocompletedEntity;
        private string _partialPath;

        public override void _Ready()
        {
            _persistService = GetNode<PersistService>(ServicePathConstants.PersistServicePath);
            _keyboardSounds = GetTree().Root.GetNode<KeyboardSounds>(KeyboardSounds.AbsolutePath);
            _commandMemoryIndex = _persistService.CommandMemory.Count;

            _userCommandEvaluator = new();
            _userCommandEvaluator.SimpleMessageCommand += CreateSimpleTerminalResponse;
            _userCommandEvaluator.SaveProgressCommand += SaveProgress;
            _userCommandEvaluator.ExitCommand += Exit;
            _userCommandEvaluator.ColorChangeCommand += ChangeColor;
            _userCommandEvaluator.ChangeDirectoryCommand += ChangeDirectory;
            _userCommandEvaluator.ListDirectoryCommand += ListDirectory;
            _userCommandEvaluator.ViewFileCommand += ViewFile;
            _userCommandEvaluator.MakeFileCommand += MakeFile;
            _userCommandEvaluator.MakeDirectoryCommand += MakeDirectory;
            _userCommandEvaluator.EditFileCommand += EditFile;
            _userCommandEvaluator.ListHardwareCommand += ListHardware;
            _userCommandEvaluator.ViewPermissionsCommand += ViewPermissions;
            _userCommandEvaluator.ChangePermissionsCommand += ChangePermissions;

            Text = GetDirectoryWithPrompt();
            SetCaretColumn(Text.Length);
            SetCaretLine(1);
        }

        public override void _Draw()
        {
            if (!_hasFocus)
            {
                GrabFocus();
                _hasFocus = true;
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (!@event.IsPressed() || !HasFocus())
            {
                return;
            }

            if (@event is InputEventKey && @event.IsPressed())
            {
                CallDeferred("PlayKeyboardSound");
            }

            EvaluateKeyboardInput();
        }

        private void EvaluateKeyboardInput()
        {
            if (Input.IsPhysicalKeyPressed(Key.Enter))
            {
                EvaluateCommand();
                return;
            }

            if (Input.IsPhysicalKeyPressed(Key.Tab))
            {
                AutocompletePhrase();
                return;
            }

            if (Input.IsPhysicalKeyPressed(Key.Up))
            {
                _commandMemoryIndex -= 1;
                if (_commandMemoryIndex < 0)
                {
                    _commandMemoryIndex = _persistService.CommandMemory.Count - 1;
                }
                Text = string.Concat(GetDirectoryWithPrompt(), _persistService.CommandMemory.ElementAtOrDefault(_commandMemoryIndex));
                SetCaretColumn(Text.Length);
                GetTree().Root.SetInputAsHandled();
                return;
            }

            if (Input.IsPhysicalKeyPressed(Key.Down))
            {
                _commandMemoryIndex += 1;
                if (_commandMemoryIndex > _persistService.CommandMemory.Count - 1)
                {
                    _commandMemoryIndex = 0;
                }
                Text = string.Concat(GetDirectoryWithPrompt(), _persistService.CommandMemory.ElementAtOrDefault(_commandMemoryIndex));
                SetCaretColumn(Text.Length);
            }

            if (Input.IsPhysicalKeyPressed(Key.Backspace) || Input.IsPhysicalKeyPressed(Key.Left) || Input.IsPhysicalKeyPressed(Key.Home))
            {
                var caretPosition = GetCaretColumn();
                if (caretPosition <= GetDirectoryWithPrompt().Length)
                {
                    GetTree().Root.SetInputAsHandled();
                    return;
                }
            }
        }

        private void AutocompletePhrase()
        {
            var inputWithoutDirectory = Text.Replace(GetDirectoryWithPrompt(), string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var userCommand = UserCommandService.EvaluateToken(inputWithoutDirectory.FirstOrDefault());
            var pathToSearch = inputWithoutDirectory.LastOrDefault();
            if(!_validAutoCompleteCommands.Contains(userCommand) || string.IsNullOrEmpty(pathToSearch))
            {
                ListDirectory();
                EmitSignal(SignalName.Evaluated);
                GetTree().Root.SetInputAsHandled();
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
            DirectoryEntity directoryToSearch = isAbsoluteSearch ? _persistService.GetRootDirectory() : _persistService.GetCurrentDirectory();
            if(isDeepAbsoluteSearch)
            {
                var directoryWithoutPartialPath = string.Join('/', pathToSearch.Split('/', StringSplitOptions.RemoveEmptyEntries).SkipLast(1));
                directoryToSearch = _persistService.GetAbsoluteDirectory(directoryWithoutPartialPath);
            }
            if(isDeepRelativeSearch)
            {
                var directoryWithoutPartialPath = string.Join('/', pathToSearch.Split('/', StringSplitOptions.RemoveEmptyEntries).SkipLast(1));
                directoryToSearch = _persistService.GetRelativeDirectory(directoryWithoutPartialPath);
            }

            // filter autocomplete results down to the partial path defined by the user
            var filteredEntities = string.IsNullOrEmpty(_partialPath)
                ? directoryToSearch.Entities
                : directoryToSearch.Entities.Where(entity => entity.Name.StartsWith(_partialPath));

            // if there are no autocomplete results and the user is searching with a partial path, do nothing
            if (!filteredEntities.Any())
            {
                GetTree().Root.SetInputAsHandled();
                return;
            }

            // fill in the results of the autocomplete search, folders for change directory, and files otherwise
            var matchingEntity = filteredEntities.FirstOrDefault(entity => entity.IsDirectory == (userCommand == UserCommand.ChangeDirectory));
            if (_autocompletedEntity != null)
            {
                // wrap the autocomplete results
                matchingEntity = filteredEntities.SkipWhile(p => p.Name != _autocompletedEntity.Name).Skip(1).FirstOrDefault() ?? filteredEntities.FirstOrDefault();
            }

            // determine which path to show as a result, file or folder, based on the user command
            var autoCompletedPath = isAbsoluteSearch ? _persistService.GetAbsoluteDirectoryPath(matchingEntity) : _persistService.GetRelativeDirectoryPath(matchingEntity);
            if(userCommand != UserCommand.ChangeDirectory)
            {
                autoCompletedPath = $"{matchingEntity}";
            }
            Text = string.Concat(GetDirectoryWithPrompt(), $"{inputWithoutDirectory.FirstOrDefault()} {autoCompletedPath}");
            SetCaretColumn(Text.Length);
            _autocompletedEntity = matchingEntity;
            GetTree().Root.SetInputAsHandled();
        }

        private void EvaluateCommand()
        {
            _commandMemoryIndex = 0;

            var inputWithoutDirectory = Text.Replace(GetDirectoryWithPrompt(), string.Empty).Trim(' ');
            _persistService.AddCommandToMemory(inputWithoutDirectory);
            _userCommandEvaluator.EvaluateCommand(inputWithoutDirectory);

            EmitSignal(SignalName.Evaluated);
            SetProcessInput(false);
            GetTree().Root.SetInputAsHandled();
        }

        private void PlayKeyboardSound() => _keyboardSounds.PlayKeyboardSound();

        private string GetDirectoryWithPrompt() => $"{_persistService.GetCurrentDirectoryPath()} {DirectoryConstants.TerminalPromptCharacter} ";

        private void CreateSimpleTerminalResponse(string message) => EmitSignal(SignalName.KnownCommand, message);

        private void SaveProgress() => EmitSignal(SignalName.SaveCommand);

        private void Exit() => GetTree().Quit();

        private void ChangeColor(string newColor) => EmitSignal(SignalName.ColorCommand, newColor);

        private void ChangeDirectory(string newDirectory)
        {
            var directory = newDirectory.ToLowerInvariant() switch
            {
                ".." => _persistService.GetParentDirectory(_persistService.GetCurrentDirectory()),
                "." => _persistService.GetCurrentDirectory(),
                "~" => _persistService.GetHomeDirectory(),
                "root" or "/" => _persistService.GetRootDirectory(),
                _ => newDirectory.StartsWith('/')
                    ? _persistService.GetAbsoluteDirectory(newDirectory.TrimEnd('/'))
                    : _persistService.GetRelativeDirectory(newDirectory.TrimEnd('/'))
            };

            if (directory == null)
            {
                EmitSignal(SignalName.KnownCommand, $"\"{newDirectory}\" is not a directory.");
                return;
            }

            if (!directory.Permissions.Contains(Permission.UserRead))
            {
                EmitSignal(SignalName.KnownCommand, $"Insufficient permissions to enter the \"{_persistService.GetAbsoluteDirectoryPath(directory)}\" directory.");
                return;
            }

            _persistService.SetCurrentDirectory(directory);
        }

        private void ListDirectory() => EmitSignal(SignalName.ListDirectoryCommand);

        private void ViewFile(string fileName)
        {
            var file = _persistService.GetFile(fileName);
            if(file == null)
            {
                EmitSignal(SignalName.KnownCommand, $"\"{fileName}\" does not exist.");
                return;
            }

            if(file.Permissions.Contains(Permission.AdminExecutable) || file.Permissions.Contains(Permission.UserExecutable))
            {
                EmitSignal(SignalName.KnownCommand, $"\"{fileName}\" is an executable.");
                return;
            }

            if (!file.Permissions.Contains(Permission.UserRead))
            {
                EmitSignal(SignalName.KnownCommand, $"Insufficient permissions to view the \"{fileName}\" file.");
                return;
            }

            EmitSignal(SignalName.KnownCommand, file.Contents);
        }

        private void MakeFile(string fileName)
        {
            var currentDirectory = _persistService.GetCurrentDirectory();
            if (!currentDirectory.Permissions.Contains(Permission.UserWrite))
            {
                EmitSignal(SignalName.KnownCommand, $"Insufficient permissions to create a file in the current directory.");
                return;
            }

            EmitSignal(SignalName.MakeFileCommand, fileName);
        }

        private void MakeDirectory(string directoryName)
        {
            var currentDirectory = _persistService.GetCurrentDirectory();
            if (!currentDirectory.Permissions.Contains(Permission.UserWrite))
            {
                EmitSignal(SignalName.KnownCommand, $"Insufficient permissions to create a directory in the current directory.");
                return;
            }

            EmitSignal(SignalName.MakeDirectoryCommand, directoryName);
        }

        private void EditFile(string fileName)
        {
            var currentDirectory = _persistService.GetCurrentDirectory();
            if (!currentDirectory.Permissions.Contains(Permission.UserWrite))
            {
                EmitSignal(SignalName.KnownCommand, $"Insufficient permissions to edit the \"{fileName}\" file in the current directory.");
                return;
            }

            EmitSignal(SignalName.EditFileCommand, fileName);
        }

        private void ListHardware() => EmitSignal(SignalName.ListHardwareCommand);

        private void ViewPermissions(string entityName) => EmitSignal(SignalName.ViewPermissionsCommand, entityName);

        private void ChangePermissions(string entityName, string newPermissionsSet) => EmitSignal(SignalName.ChangePermissionsCommand, entityName, newPermissionsSet);
    }
}
