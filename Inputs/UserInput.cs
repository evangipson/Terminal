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

        [Signal]
        public delegate void NetworkCommandEventHandler();


        private static readonly List<UserCommand> _validAutoCompleteCommands = new()
        {
            UserCommand.ChangeDirectory,
            UserCommand.ViewFile,
            UserCommand.EditFile,
            UserCommand.ViewPermissions,
            UserCommand.ChangePermissions
        };

        private static readonly List<UserCommand> _commandsThatNeedAdditionalArguments = new()
        {
            UserCommand.Color,
            UserCommand.ChangeDirectory,
            UserCommand.ViewFile,
            UserCommand.MakeFile,
            UserCommand.MakeDirectory,
            UserCommand.EditFile,
            UserCommand.ViewPermissions,
            UserCommand.ChangePermissions
        };

        private KeyboardSounds _keyboardSounds;
        private PersistService _persistService;
        private UserCommandService _userCommandService;
        private DirectoryService _directoryService;
        private bool _hasFocus = false;
        private int _commandMemoryIndex;
        private DirectoryEntity _autocompletedEntity;
        private string _partialPath;

        public override void _Ready()
        {
            _userCommandService = GetNode<UserCommandService>(ServicePathConstants.UserCommandServicePath);
            _directoryService = GetNode<DirectoryService>(ServicePathConstants.DirectoryServicePath);
            _persistService = GetNode<PersistService>(ServicePathConstants.PersistServicePath);
            _keyboardSounds = GetTree().Root.GetNode<KeyboardSounds>(KeyboardSounds.AbsolutePath);
            _commandMemoryIndex = _persistService.CommandMemory.Count;

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

        /// <summary>
        /// Evalutes the provided <paramref name="command"/>, and invokes the correct method if it's successfully evaluated.
        /// </summary>
        /// <param name="command">
        /// The command to evaluate and invoke a method for.
        /// </param>
        public void RouteCommand(string command)
        {
            var parsedTokens = UserCommandService.ParseInputToTokens(command);
            var userCommand = UserCommandService.EvaluateUserInput(command);
            var unqualifiedCommand = _commandsThatNeedAdditionalArguments.Contains(userCommand) && parsedTokens.Count == 1;
            if (userCommand == UserCommand.Help || userCommand == UserCommand.Commands || unqualifiedCommand)
            {
                var helpContextToken = userCommand;
                if (parsedTokens.Count == 2 && UserCommandService.EvaluateToken(parsedTokens.Take(2).Last()) != UserCommand.Unknown)
                {
                    var parsedContextToken = UserCommandService.EvaluateToken(parsedTokens.Take(2).Last());
                    helpContextToken = parsedContextToken == UserCommand.Unknown ? UserCommand.Help : parsedContextToken;
                }

                CreateSimpleTerminalResponse(EvaluateHelpCommand(helpContextToken, parsedTokens.Take(2).Last()));
            }
            else if (userCommand == UserCommand.ChangeDirectory)
            {
                ChangeDirectory(parsedTokens.Take(2).Last());
            }
            else if (userCommand == UserCommand.ListDirectory)
            {
                ListDirectory();
            }
            else if (userCommand == UserCommand.ViewFile)
            {
                ViewFile(parsedTokens.Take(2).Last());
            }
            else if (userCommand == UserCommand.MakeFile)
            {
                MakeFile(parsedTokens.Take(2).Last());
            }
            else if (userCommand == UserCommand.MakeDirectory)
            {
                MakeDirectory(parsedTokens.Take(2).Last());
            }
            else if (userCommand == UserCommand.EditFile)
            {
                EditFile(parsedTokens.Take(2).Last());
            }
            else if (userCommand == UserCommand.ListHardware)
            {
                ListHardware();
            }
            else if (userCommand == UserCommand.ViewPermissions)
            {
                ViewPermissions(parsedTokens.Take(2).Last());
            }
            else if (userCommand == UserCommand.ChangePermissions)
            {
                var fileNameAndPermissionSet = parsedTokens.Skip(1).Take(2);
                ChangePermissions(fileNameAndPermissionSet.FirstOrDefault(), fileNameAndPermissionSet.LastOrDefault());
            }
            else if (userCommand == UserCommand.Date)
            {
                CreateSimpleTerminalResponse(DateTime.UtcNow.AddYears(250).ToLongDateString());
            }
            else if (userCommand == UserCommand.Time)
            {
                CreateSimpleTerminalResponse(DateTime.UtcNow.AddYears(250).ToLongTimeString());
            }
            else if (userCommand == UserCommand.Now)
            {
                var now = DateTime.UtcNow.AddYears(250);
                CreateSimpleTerminalResponse(string.Join(", ", now.ToLongTimeString(), now.ToLongDateString()));
            }
            else if (userCommand == UserCommand.Network)
            {
                Network();
            }
            else if (userCommand == UserCommand.Color)
            {
                ChangeColor(parsedTokens.Take(2).Last());
            }
            else if (userCommand == UserCommand.Save)
            {
                SaveProgress();
            }
            else if (userCommand == UserCommand.Exit)
            {
                Exit();
            }
            else
            {
                CreateSimpleTerminalResponse($"\"{parsedTokens.First()}\" is an unknown command. Use \"commands\" to get a list of available commands.");
            }
        }

        private string EvaluateHelpCommand(UserCommand? typeOfHelp = UserCommand.Help, string userHelpContext = null)
        {
            var allCommands = _userCommandService.AllCommands;
            if(allCommands.TryGetValue(userHelpContext, out Dictionary<string, string> helpContext))
            {
                return GetOutputFromTokens(helpContext);
            }

            return typeOfHelp switch
            {
                UserCommand.Help => GetOutputFromTokens(allCommands["help"]),
                UserCommand.Exit => GetOutputFromTokens(allCommands["exit"]),
                UserCommand.Color => GetOutputFromTokens(allCommands["color"]),
                UserCommand.Save => GetOutputFromTokens(allCommands["save"]),
                UserCommand.Commands => GetOutputFromTokens(allCommands["commands"]),
                UserCommand.ChangeDirectory => GetOutputFromTokens(allCommands["change"]),
                UserCommand.ListDirectory => GetOutputFromTokens(allCommands["list"]),
                UserCommand.ViewFile => GetOutputFromTokens(allCommands["view"]),
                UserCommand.MakeFile => GetOutputFromTokens(allCommands["makefile"]),
                UserCommand.MakeDirectory => GetOutputFromTokens(allCommands["makedirectory"]),
                UserCommand.EditFile => GetOutputFromTokens(allCommands["edit"]),
                UserCommand.ListHardware => GetOutputFromTokens(allCommands["listhardware"]),
                UserCommand.ViewPermissions => GetOutputFromTokens(allCommands["viewpermissions"]),
                UserCommand.ChangePermissions => GetOutputFromTokens(allCommands["changepermissions"]),
                UserCommand.Date => GetOutputFromTokens(allCommands["date"]),
                UserCommand.Time => GetOutputFromTokens(allCommands["time"]),
                UserCommand.Now => GetOutputFromTokens(allCommands["now"]),
                UserCommand.Network => GetOutputFromTokens(allCommands["network"]),
                _ => string.Empty
            };
        }

        private static string GetOutputFromTokens(Dictionary<string, string> outputTokens) => $"\n{string.Join("\n\n", outputTokens.Select(token => string.Join('\n', token.Key, token.Value)))}\n\n";

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
            DirectoryEntity directoryToSearch = isAbsoluteSearch ? _directoryService.GetRootDirectory() : _directoryService.GetCurrentDirectory();
            if(isDeepAbsoluteSearch)
            {
                var directoryWithoutPartialPath = string.Join('/', pathToSearch.Split('/', StringSplitOptions.RemoveEmptyEntries).SkipLast(1));
                directoryToSearch = _directoryService.GetAbsoluteDirectory(directoryWithoutPartialPath);
            }
            if(isDeepRelativeSearch)
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
                GetTree().Root.SetInputAsHandled();
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
            RouteCommand(inputWithoutDirectory);

            EmitSignal(SignalName.Evaluated);
            SetProcessInput(false);
            GetTree().Root.SetInputAsHandled();
        }

        private void PlayKeyboardSound() => _keyboardSounds.PlayKeyboardSound();

        private string GetDirectoryWithPrompt() => $"{_directoryService.GetCurrentDirectoryPath()} {TerminalCharactersConstants.Prompt} ";

        private void CreateSimpleTerminalResponse(string message) => EmitSignal(SignalName.KnownCommand, message);

        private void SaveProgress() => EmitSignal(SignalName.SaveCommand);

        private void Exit() => GetTree().Quit();

        private void ChangeColor(string newColor) => EmitSignal(SignalName.ColorCommand, newColor);

        private void ChangeDirectory(string newDirectory)
        {
            var directory = newDirectory.ToLowerInvariant() switch
            {
                ".." => _directoryService.GetParentDirectory(_directoryService.GetCurrentDirectory()),
                "." => _directoryService.GetCurrentDirectory(),
                "~" => _directoryService.GetHomeDirectory(),
                "root" or "/" => _directoryService.GetRootDirectory(),
                _ => newDirectory.StartsWith('/')
                    ? _directoryService.GetAbsoluteDirectory(newDirectory.TrimEnd('/'))
                    : _directoryService.GetRelativeDirectory(newDirectory.TrimEnd('/'))
            };

            if (directory == null)
            {
                EmitSignal(SignalName.KnownCommand, $"\"{newDirectory}\" is not a directory.");
                return;
            }

            if (!directory.Permissions.Contains(Permission.UserRead))
            {
                EmitSignal(SignalName.KnownCommand, $"Insufficient permissions to enter the \"{_directoryService.GetAbsoluteDirectoryPath(directory)}\" directory.");
                return;
            }

            _directoryService.SetCurrentDirectory(directory);
        }

        private void ListDirectory() => EmitSignal(SignalName.ListDirectoryCommand);

        private void ViewFile(string fileName)
        {
            var file = _directoryService.GetRelativeFile(fileName);
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
            var currentDirectory = _directoryService.GetCurrentDirectory();
            if (!currentDirectory.Permissions.Contains(Permission.UserWrite))
            {
                EmitSignal(SignalName.KnownCommand, $"Insufficient permissions to create a file in the current directory.");
                return;
            }

            EmitSignal(SignalName.MakeFileCommand, fileName);
        }

        private void MakeDirectory(string directoryName)
        {
            var currentDirectory = _directoryService.GetCurrentDirectory();
            if (!currentDirectory.Permissions.Contains(Permission.UserWrite))
            {
                EmitSignal(SignalName.KnownCommand, $"Insufficient permissions to create a directory in the current directory.");
                return;
            }

            EmitSignal(SignalName.MakeDirectoryCommand, directoryName);
        }

        private void EditFile(string fileName)
        {
            var currentDirectory = _directoryService.GetCurrentDirectory();
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

        private void Network() => EmitSignal(SignalName.NetworkCommand);
    }
}
