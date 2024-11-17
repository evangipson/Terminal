using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

using Terminal.Audio;
using Terminal.Constants;
using Terminal.Enums;
using Terminal.Extensions;
using Terminal.Models;
using Terminal.Services;

namespace Terminal.Inputs
{
    /// <summary>
    /// A <see cref="TextEdit"/> <see cref="Node"/> managed in Godot that accepts and parses user commands.
    /// </summary>
    public partial class UserInput : TextEdit
    {
        /// <summary>
        /// The <see cref="Signal"/> that broadcasts whenever a command is done evaluting.
        /// <para>
        /// Typically will load a new <see cref="UserInput"/> into the console, and ensure the previous
        /// <see cref="UserInput"/> is cleaned up and can't be further modified with input.
        /// </para>
        /// </summary>
        [Signal]
        public delegate void EvaluatedEventHandler();

        /// <summary>
        /// The <see cref="Signal"/> that broadcasts whenever a simple response is added to the console.
        /// </summary>
        /// <param name="message">
        /// The response to show in the console.
        /// </param>
        [Signal]
        public delegate void KnownCommandEventHandler(string message);

        /// <summary>
        /// The <see cref="Signal"/> that broadcasts when the <see cref="UserCommand.Color"/> command is invoked.
        /// </summary>
        /// <param name="colorName">
        /// The name of the <see cref="Color"/> to change the console to.
        /// </param>
        [Signal]
        public delegate void ColorCommandEventHandler(string colorName);

        /// <summary>
        /// The <see cref="Signal"/> that broadcasts when the <see cref="UserCommand.Save"/> command is invoked.
        /// </summary>
        [Signal]
        public delegate void SaveCommandEventHandler();

        /// <summary>
        /// The <see cref="Signal"/> that broadcasts when the <see cref="UserCommand.ListDirectory"/> command is invoked.
        /// </summary>
        /// <param name="directoryToList">
        /// An optional directory to list, if not listing the contents of the current directory.
        /// </param>
        [Signal]
        public delegate void ListDirectoryCommandEventHandler(string directoryToList);

        /// <summary>
        /// The <see cref="Signal"/> that broadcasts when the <see cref="UserCommand.ChangeDirectory"/> command is invoked.
        /// </summary>
        /// <param name="newDirectory">
        /// The name of the new current directory.
        /// </param>
        [Signal]
        public delegate void ChangeDirectoryCommandEventHandler(string newDirectory);

        /// <summary>
        /// The <see cref="Signal"/> that broadcasts when the <see cref="UserCommand.MakeFile"/> command is invoked.
        /// </summary>
        /// <param name="fileName">
        /// The name of the new file to make.
        /// </param>
        [Signal]
        public delegate void MakeFileCommandEventHandler(string fileName);

        /// <summary>
        /// The <see cref="Signal"/> that broadcasts when the <see cref="UserCommand.MakeDirectory"/> command is invoked.
        /// </summary>
        /// <param name="directoryName">
        /// The name of the new directory to make.
        /// </param>
        [Signal]
        public delegate void MakeDirectoryCommandEventHandler(string directoryName);

        /// <summary>
        /// The <see cref="Signal"/> that broadcasts when the <see cref="UserCommand.EditFile"/> command is invoked.
        /// </summary>
        /// <param name="fileName">
        /// The name of the file to edit.
        /// </param>
        [Signal]
        public delegate void EditFileCommandEventHandler(string fileName);

        /// <summary>
        /// The <see cref="Signal"/> that broadcasts when the <see cref="UserCommand.ListHardware"/> command is invoked.
        /// </summary>
        [Signal]
        public delegate void ListHardwareCommandEventHandler();

        /// <summary>
        /// The <see cref="Signal"/> that broadcasts when the <see cref="UserCommand.ViewPermissions"/> command is invoked.
        /// </summary>
        /// <param name="entityName">
        /// The name of the file or folder to view the permissions of.
        /// </param>
        [Signal]
        public delegate void ViewPermissionsCommandEventHandler(string entityName);

        /// <summary>
        /// The <see cref="Signal"/> that broadcasts when the <see cref="UserCommand.ChangePermissions"/> command is invoked.
        /// </summary>
        /// <param name="entityName">
        /// The name of the file or folder to change the permissions of.
        /// </param>
        /// <param name="newPermissionSet">
        /// A <see langword="string"/> represntation of the new permission set for the file or folder with the provided <paramref name="entityName"/>.
        /// </param>
        [Signal]
        public delegate void ChangePermissionsCommandEventHandler(string entityName, string newPermissionSet);

        /// <summary>
        /// The <see cref="Signal"/> that broadcasts when the <see cref="UserCommand.Network"/> command is invoked.
        /// </summary>
        [Signal]
        public delegate void NetworkCommandEventHandler();

        /// <summary>
        /// The <see cref="Signal"/> that broadcasts when the <see cref="UserCommand.DeleteFile"/> command is invoked.
        /// </summary>
        /// <param name="fileName">
        /// The name of the file to delete.
        /// </param>
        [Signal]
        public delegate void DeleteFileCommandEventHandler(string fileName);

        /// <summary>
        /// The <see cref="Signal"/> that broadcasts when the <see cref="UserCommand.DeleteDirectory"/> command is invoked.
        /// </summary>
        /// <param name="directoryName">
        /// The name of the directory to delete.
        /// </param>
        /// <param name="arguments">
        /// A list of arguments provided with the <see cref="UserCommand.DeleteDirectory"/> command.
        /// </param>
        [Signal]
        public delegate void DeleteDirectoryCommandEventHandler(string directoryName, string[] arguments);

        private static readonly List<UserCommand> _commandsThatNeedAdditionalArguments = new()
        {
            UserCommand.Color,
            UserCommand.ChangeDirectory,
            UserCommand.ViewFile,
            UserCommand.MakeFile,
            UserCommand.MakeDirectory,
            UserCommand.EditFile,
            UserCommand.ViewPermissions,
            UserCommand.ChangePermissions,
            UserCommand.DeleteFile,
            UserCommand.DeleteDirectory,
            UserCommand.Ping,
            UserCommand.MoveFile,
            UserCommand.MoveDirectory,
            UserCommand.MakeUser,
            UserCommand.DeleteUser,
            UserCommand.MakeGroup,
            UserCommand.DeleteGroup,
            UserCommand.AddUserToGroup,
            UserCommand.DeleteUserFromGroup
        };

        private static readonly List<UserCommand> _interactiveResponseCommands = new()
        {
            UserCommand.EditFile,
            UserCommand.Ping
        };

        private KeyboardSounds _keyboardSounds;
        private PersistService _persistService;
        private UserCommandService _userCommandService;
        private DirectoryService _directoryService;
        private AutoCompleteService _autoCompleteService;
        private NetworkService _networkService;
        private bool _hasFocus = false;
        private int _commandMemoryIndex;

        public override void _Ready()
        {
            _userCommandService = GetNode<UserCommandService>(ServicePathConstants.UserCommandServicePath);
            _directoryService = GetNode<DirectoryService>(ServicePathConstants.DirectoryServicePath);
            _persistService = GetNode<PersistService>(ServicePathConstants.PersistServicePath);
            _autoCompleteService = GetNode<AutoCompleteService>(ServicePathConstants.AutoCompleteServicePath);
            _networkService = GetNode<NetworkService>(ServicePathConstants.NetworkServicePath);
            _keyboardSounds = GetTree().Root.GetNode<KeyboardSounds>(KeyboardSounds.AbsolutePath);
            _commandMemoryIndex = _persistService.CommandMemory.Count;

            _autoCompleteService.OnInvalidAutocomplete += ListDirectoryAndCreateNewInput;
            _autoCompleteService.OnAutocomplete += FillInputWithAutocompletedPhrase;
            _networkService.OnShowNetwork += ShowNetworkResponse;
            _networkService.OnPing += PingResponse;
            _networkService.OnPingDone += FinishPing;

            Text = _userCommandService.GetCommandPrompt();
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

            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if(!keyEvent.Echo)
                {
                    CallDeferred("PlayKeyboardSound");
                }

                // disallow select all and cut all
                if (keyEvent.IsCommandOrControlPressed() && (keyEvent.Keycode == Key.A || keyEvent.Keycode == Key.X))
                {
                    GetTree().Root.SetInputAsHandled();
                    return;
                }

                // allow control+c to stop in-flight ping command
                if (keyEvent.ShiftPressed && keyEvent.Keycode == Key.Tab)
                {
                    _autoCompleteService.AutocompletePhrase(Text, previousResult: true);
                    return;
                }
            }

            EvaluateKeyboardInput().Invoke();
        }

        private void EvaluateCommand()
        {
            _commandMemoryIndex = 0;

            var inputWithoutDirectory = Text.Replace(_userCommandService.GetCommandPrompt(), string.Empty).Trim(' ');
            _persistService.AddCommandToMemory(inputWithoutDirectory);
            RouteCommand(inputWithoutDirectory).Invoke();

            var command = UserCommandService.EvaluateUserInput(inputWithoutDirectory);

            // for commands that have interactive responses, don't immediately create a new input, unless the user is getting more
            // help/info about the command, e.g.: just typing in "edit" instead of qualifying with a filename as well.
            if (!_interactiveResponseCommands.Contains(command) || (_interactiveResponseCommands.Contains(command) && inputWithoutDirectory.Split(' ').Length == 1))
            {
                EmitSignal(SignalName.Evaluated);
                UnsubscribeAndStopInput();
            }

            // if we are in an interactive command, just ignore the "enter" used to trigger it and wait for the command logic
            // to create you the new input when it's needed (i.e.: with "edit" or "ping").
            if(command == UserCommand.Ping && inputWithoutDirectory.Split(' ').Length > 1)
            {
                StopInput();
                ReleaseFocus();
            }
            if(command == UserCommand.EditFile && inputWithoutDirectory.Split(' ').Length > 1)
            {
                ReleaseFocus();
                UnsubscribeFromEvents();
            }
        }

        private void UnsubscribeAndStopInput()
        {
            StopInput();
            UnsubscribeFromEvents();
        }

        private void StopInput()
        {
            SetProcessInput(false);
            GetTree().Root.SetInputAsHandled();
        }

        private void UnsubscribeFromEvents()
        {
            // unsubscribe from the auto-complete events to prevent old inputs from getting autocompleted.
            _autoCompleteService.OnInvalidAutocomplete -= ListDirectoryAndCreateNewInput;
            _autoCompleteService.OnAutocomplete -= FillInputWithAutocompletedPhrase;
            _networkService.OnShowNetwork -= ShowNetworkResponse;
            _networkService.OnPing -= PingResponse;
            _networkService.OnPingDone -= FinishPing;
        }

        private Action RouteCommand(string command)
        {
            var parsedTokens = UserCommandService.ParseInputToTokens(command);
            var userCommand = UserCommandService.EvaluateUserInput(command);
            if(_commandsThatNeedAdditionalArguments.Contains(userCommand) && parsedTokens.Count == 1)
            {
                return QualifyAndEvaluateHelpCommand(userCommand);
            }

            return userCommand switch
            {
                UserCommand.Help or UserCommand.Commands => QualifyAndEvaluateHelpCommand(userCommand, parsedTokens),
                UserCommand.ChangeDirectory => () => ChangeDirectory(parsedTokens.Take(2).Last()),
                UserCommand.ListDirectory => () => ListDirectory(parsedTokens.Skip(1).Take(1).FirstOrDefault()),
                UserCommand.ViewFile => () => ViewFile(parsedTokens.Take(2).Last()),
                UserCommand.MakeFile => () => MakeFile(parsedTokens.Take(2).Last()),
                UserCommand.MakeDirectory => () => MakeDirectory(parsedTokens.Take(2).Last()),
                UserCommand.EditFile => () => EditFile(parsedTokens.Take(2).Last()),
                UserCommand.ListHardware => () => ListHardware(),
                UserCommand.ViewPermissions => () => ViewPermissions(parsedTokens.Take(2).Last()),
                UserCommand.ChangePermissions => () => ChangePermissions(parsedTokens.Skip(1).Take(2).FirstOrDefault(), parsedTokens.Skip(1).Take(2).LastOrDefault()),
                UserCommand.Date => () => CreateSimpleTerminalResponse(DateTime.UtcNow.AddYears(250).ToLongDateString()),
                UserCommand.Time => () => CreateSimpleTerminalResponse(DateTime.UtcNow.AddYears(250).ToLongTimeString()),
                UserCommand.Now => () => CreateSimpleTerminalResponse(string.Join(", ", DateTime.UtcNow.AddYears(250).ToLongTimeString(), DateTime.UtcNow.AddYears(250).ToLongDateString())),
                UserCommand.Network => () => _networkService.ShowNetworkInformation(parsedTokens.Skip(1)),
                UserCommand.Color => () => ChangeColor(parsedTokens.Take(2).Last()),
                UserCommand.Save => () => SaveProgress(),
                UserCommand.Exit => () => Exit(),
                UserCommand.DeleteFile => () => DeleteFile(parsedTokens.Take(2).Last()),
                UserCommand.DeleteDirectory => () => DeleteDirectory(parsedTokens.Take(2).Last(), parsedTokens.Skip(2)),
                UserCommand.MoveFile => () => MoveFile(parsedTokens.Skip(1)),
                UserCommand.MoveDirectory => () => MoveDirectory(parsedTokens.Skip(1)),
                UserCommand.Ping => () => StartPing(parsedTokens.Take(2).Last(), parsedTokens.Skip(2)),
                UserCommand.MakeUser => () => MakeUser(parsedTokens.Skip(1)),
                UserCommand.DeleteUser => () => DeleteUser(parsedTokens.Skip(1)),
                UserCommand.MakeGroup => () => MakeGroup(parsedTokens.Skip(1)),
                UserCommand.DeleteGroup => () => DeleteGroup(parsedTokens.Skip(1)),
                UserCommand.AddUserToGroup => () => AddUserToGroup(parsedTokens.Skip(1)),
                UserCommand.DeleteUserFromGroup => () => DeleteUserFromGroup(parsedTokens.Skip(1)),
                _ => () => CreateSimpleTerminalResponse($"\"{parsedTokens.First()}\" is an unknown command. Use \"commands\" to get a list of available commands.")
            };
        }

        private Action EvaluateKeyboardInput() => true switch
        {
            _ when Input.IsPhysicalKeyPressed(Key.Enter) => () => EvaluateCommand(),
            _ when Input.IsPhysicalKeyPressed(Key.Tab) => () => _autoCompleteService.AutocompletePhrase(Text),
            _ when Input.IsPhysicalKeyPressed(Key.Up) => () => UpdateCommandMemory(forPreviousCommand: true),
            _ when Input.IsPhysicalKeyPressed(Key.Down) => () => UpdateCommandMemory(forPreviousCommand: false),
            _ when Input.IsPhysicalKeyPressed(Key.Backspace) => () => StopCursorAtPrompt(),
            _ when Input.IsPhysicalKeyPressed(Key.Left) => () => StopCursorAtPrompt(),
            _ when Input.IsPhysicalKeyPressed(Key.Home) => () => StopCursorAtPrompt(),
            _ => () => { }
        };

        private void PlayKeyboardSound() => _keyboardSounds.PlayKeyboardSound();

        private void CreateSimpleTerminalResponse(string message) => EmitSignal(SignalName.KnownCommand, message);

        private void SaveProgress() => EmitSignal(SignalName.SaveCommand);

        private void Exit() => GetTree().Quit();

        private void ChangeColor(string newColor) => EmitSignal(SignalName.ColorCommand, newColor);

        private void ChangeDirectory(string newDirectory)
        {
            var directory = newDirectory.ToLowerInvariant() switch
            {
                ".." => _directoryService.GetParentDirectory(_directoryService.GetCurrentDirectory()),
                "." or "./" => _directoryService.GetCurrentDirectory(),
                "~" => _directoryService.GetHomeDirectory(),
                "root" or "/" => _directoryService.GetRootDirectory(),
                _ => newDirectory.StartsWith(TerminalCharactersConstants.Separator)
                    ? _directoryService.GetAbsoluteDirectory(newDirectory.TrimEnd(TerminalCharactersConstants.Separator))
                    : newDirectory.Contains(TerminalCharactersConstants.HomeDirectory)
                        ? _directoryService.GetHomeDirectory().FindDirectory(newDirectory.Split(TerminalCharactersConstants.HomeDirectory).LastOrDefault()?.TrimEnd(TerminalCharactersConstants.Separator))
                        : _directoryService.GetRelativeDirectory(newDirectory.TrimEnd(TerminalCharactersConstants.Separator))
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

        private void ListDirectory(string directoryToList = null) => EmitSignal(SignalName.ListDirectoryCommand, directoryToList);

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
            if (string.IsNullOrEmpty(fileName))
            {
                EmitSignal(SignalName.KnownCommand, "File can't be edited without a name.");
                EmitSignal(SignalName.Evaluated);
                return;
            }

            var file = _directoryService.GetCurrentDirectory().FindFile(fileName);
            if (file == null)
            {
                EmitSignal(SignalName.KnownCommand, $"No file with the name '{fileName}' exists.");
                EmitSignal(SignalName.Evaluated);
                return;
            }

            if (file?.Permissions.Contains(Permission.UserWrite) != true)
            {
                EmitSignal(SignalName.KnownCommand, $"Insufficient permissions to edit the \"{fileName}\" file.");
                EmitSignal(SignalName.Evaluated);
                return;
            }

            EmitSignal(SignalName.EditFileCommand, fileName);
        }

        private void ListHardware() => EmitSignal(SignalName.ListHardwareCommand);

        private void ViewPermissions(string entityName) => EmitSignal(SignalName.ViewPermissionsCommand, entityName);

        private void ChangePermissions(string entityName, string newPermissionsSet) => EmitSignal(SignalName.ChangePermissionsCommand, entityName, newPermissionsSet);

        private void ShowNetworkResponse(string networkResponse) => EmitSignal(SignalName.KnownCommand, networkResponse);

        private void ListDirectoryAndCreateNewInput(string directoryToList = null)
        {
            EmitSignal(SignalName.ListDirectoryCommand, directoryToList);
            EmitSignal(SignalName.Evaluated);
            UnsubscribeAndStopInput();
        }

        private void FillInputWithAutocompletedPhrase(string phrase)
        {
            Text = phrase;
            SetCaretColumn(Text.Length);
            GetTree().Root.SetInputAsHandled();
        }

        private void UpdateCommandMemory(bool forPreviousCommand)
        {
            _commandMemoryIndex = _persistService.CommandMemory.Count switch
            {
                0 when forPreviousCommand => 0,
                > 0 when forPreviousCommand => (_commandMemoryIndex - 1 + _persistService.CommandMemory.Count) % _persistService.CommandMemory.Count,
                _ => (_commandMemoryIndex + 1) % _persistService.CommandMemory.Count,
            };

            Text = string.Concat(_userCommandService.GetCommandPrompt(), _persistService.CommandMemory.ElementAtOrDefault(_commandMemoryIndex));
            SetCaretColumn(Text.Length);
            GetTree().Root.SetInputAsHandled();
        }

        private void StopCursorAtPrompt()
        {
            if (GetCaretColumn() <= _userCommandService.GetCommandPrompt().Length)
            {
                GetTree().Root.SetInputAsHandled();
            }
        }

        private Action QualifyAndEvaluateHelpCommand(UserCommand userCommand, List<string> parsedTokens = null)
        {
            if (parsedTokens == null)
            {
                var unqualifiedHelpText = _userCommandService.EvaluateHelpCommand(userCommand);
                return () => CreateSimpleTerminalResponse(unqualifiedHelpText);
            }
;
            var additionalCommand = parsedTokens.Take(2).LastOrDefault();
            UserCommand parsedContextToken = UserCommandService.EvaluateToken(additionalCommand);
            var helpContextToken = parsedContextToken == UserCommand.Unknown ? UserCommand.Help : parsedContextToken;
            var helpText = _userCommandService.EvaluateHelpCommand(helpContextToken, parsedTokens.Take(2).LastOrDefault());
            return () => CreateSimpleTerminalResponse(helpText);
        }

        private void DeleteFile(string fileName)
        {
            var currentDirectory = _directoryService.GetCurrentDirectory();
            if (!currentDirectory.Permissions.Contains(Permission.UserWrite))
            {
                EmitSignal(SignalName.KnownCommand, $"Insufficient permissions to delete \"{fileName}\" in the current directory.");
                return;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                GD.PrintErr("File can't be deleted without a name.");
                return;
            }

            var existingFile = _directoryService.GetRelativeFile(fileName);
            if (existingFile == null)
            {
                EmitSignal(SignalName.KnownCommand, $"No file with the name of \"{fileName}\" exists.");
                return;
            }

            _directoryService.DeleteEntity(existingFile);
            EmitSignal(SignalName.KnownCommand, $"\"{fileName}\" deleted.");
        }

        private void DeleteDirectory(string directoryName, IEnumerable<string> arguments)
        {
            var currentDirectory = _directoryService.GetCurrentDirectory();
            if (!currentDirectory.Permissions.Contains(Permission.UserWrite))
            {
                EmitSignal(SignalName.KnownCommand, $"Insufficient permissions to delete \"{directoryName}\" in the current directory.");
                return;
            }

            if (string.IsNullOrEmpty(directoryName))
            {
                GD.PrintErr("Directory can't be deleted without a name.");
                return;
            }

            var existingDirectory = _directoryService.GetRelativeDirectory(directoryName);
            if (existingDirectory == null)
            {
                EmitSignal(SignalName.KnownCommand, $"No folder with the name of \"{directoryName}\" exists.");
                return;
            }

            var recurse = arguments.Contains("-r") || arguments.Contains("--recurse");
            if(recurse == false && existingDirectory.Entities.Any())
            {
                EmitSignal(SignalName.KnownCommand, $"Cannot delete the \"{directoryName}\" directory, it has files or folders in it.");
                return;
            }

            _directoryService.DeleteEntity(existingDirectory);
            EmitSignal(SignalName.KnownCommand, $"\"{directoryName}\" deleted.");
        }

        private void StartPing(string address, IEnumerable<string> arguments)
        {
            _networkService.StartPingResponse(address, arguments);
        }

        private void PingResponse(string pingMessage)
        {
            CreateSimpleTerminalResponse(pingMessage);
        }

        private void FinishPing(string message)
        {
            EmitSignal(SignalName.KnownCommand, message);
            EmitSignal(SignalName.Evaluated);
            UnsubscribeAndStopInput();
        }

        private void MoveFile(IEnumerable<string> arguments)
        {
            var fileName = arguments.FirstOrDefault();
            if(string.IsNullOrEmpty(fileName))
            {
                EmitSignal(SignalName.KnownCommand, $"Cannot move file without a file name.");
                return;
            }

            var destinationPath = arguments.Skip(1).FirstOrDefault();
            if (string.IsNullOrEmpty(destinationPath))
            {
                EmitSignal(SignalName.KnownCommand, $"Cannot move file without a destination.");
                return;
            }

            var fileToMove = _directoryService.GetRelativeFile(fileName);
            if(fileName.Trim('/').Contains('/'))
            {
                var fileNameWithoutPath = fileName.Split('/').LastOrDefault();
                var filePath = fileName.Split('/').SkipLast(1);
                fileToMove = _directoryService.GetRelativeDirectory(string.Join('/', filePath)).FindFile(fileNameWithoutPath);
            }
            if(fileToMove == null)
            {
                EmitSignal(SignalName.KnownCommand, $"Cannot move, file \"{fileName}\" does not exist in the current directory.");
                return;
            }

            var destinationDirectory = _directoryService.GetAbsoluteDirectory(destinationPath.TrimEnd('/'));
            if (destinationDirectory == null)
            {
                EmitSignal(SignalName.KnownCommand, $"Cannot move, destination folder \"{destinationDirectory}\" does not exist.");
                return;
            }

            _directoryService.MoveEntity(fileToMove, destinationDirectory);
            EmitSignal(SignalName.KnownCommand, $"\"{fileToMove}\" moved to \"{destinationDirectory}\".");
        }

        private void MoveDirectory(IEnumerable<string> arguments)
        {
            var directoryToMovePath = arguments.FirstOrDefault();
            if (string.IsNullOrEmpty(directoryToMovePath))
            {
                EmitSignal(SignalName.KnownCommand, $"Cannot move directory without a directory name.");
                return;
            }

            var destinationPath = arguments.Skip(1).FirstOrDefault();
            if (string.IsNullOrEmpty(destinationPath))
            {
                EmitSignal(SignalName.KnownCommand, $"Cannot move directory without a destination.");
                return;
            }

            var directoryToMove = _directoryService.GetRelativeDirectory(directoryToMovePath);
            if (directoryToMove == null)
            {
                EmitSignal(SignalName.KnownCommand, $"Cannot move, directory \"{directoryToMove}\" does not exist in the current directory.");
                return;
            }

            var destinationDirectory = _directoryService.GetAbsoluteDirectory(destinationPath);
            if (destinationDirectory == null)
            {
                EmitSignal(SignalName.KnownCommand, $"Cannot move, destination folder \"{destinationDirectory}\" does not exist.");
                return;
            }

            _directoryService.MoveEntity(directoryToMove, destinationDirectory);
            EmitSignal(SignalName.KnownCommand, $"\"{directoryToMove}\" moved to \"{destinationDirectory}\".");
        }

        private void MakeUser(IEnumerable<string> arguments)
        {
            var userName = arguments.FirstOrDefault();
            if(string.IsNullOrEmpty(userName))
            {
                GD.Print($"Attempted to make the new user, but user name was not provided.");
                return;
            }

            var rootUsersDirectory = _directoryService.GetRootDirectory().FindDirectory("users");
            if(rootUsersDirectory == null)
            {
                EmitSignal(SignalName.KnownCommand, $"Attempted to make the new user \"{userName}\", but there was no root /users/ directory.");
                return;
            }

            var newUserDirectoryExists = rootUsersDirectory.FindDirectory(userName) != null;
            if(newUserDirectoryExists)
            {
                EmitSignal(SignalName.KnownCommand, $"Attempted to make the new user \"{userName}\", but it already existed.");
                return;
            }

            var newUserDirectory = DirectoryConstants.GetDefaultUserDirectory(rootUsersDirectory, userName);
            rootUsersDirectory.Entities.Add(newUserDirectory);
            EmitSignal(SignalName.KnownCommand, $"\"{userName}\" user created.");
        }

        private void DeleteUser(IEnumerable<string> arguments)
        {
            var userName = arguments.FirstOrDefault();
            if(string.IsNullOrEmpty(userName))
            {
                GD.Print($"Attempted to delete a user, but user name was not provided.");
                return;
            }

            var rootUsersDirectory = _directoryService.GetRootDirectory().FindDirectory("users");
            if (rootUsersDirectory == null)
            {
                EmitSignal(SignalName.KnownCommand, $"Attempted to remove the \"{userName}\" user, but there was no root /users/ directory.");
                return;
            }

            var userDirectoryToRemove = rootUsersDirectory.FindDirectory(userName);
            if (userDirectoryToRemove == null)
            {
                EmitSignal(SignalName.KnownCommand, $"Attempted to remove the \"{userName}\" user, but it does not exist.");
                return;
            }

            rootUsersDirectory.Entities.Remove(userDirectoryToRemove);
            EmitSignal(SignalName.KnownCommand, $"\"{userName}\" user removed.");
        }

        private void MakeGroup(IEnumerable<string> arguments)
        {
            var groupName = arguments.FirstOrDefault();
            if (string.IsNullOrEmpty(groupName))
            {
                GD.Print($"Attempted to make a user group, but group name was not provided.");
                return;
            }

            var rootUserGroupsDirectory = _directoryService.GetRootDirectory().FindDirectory("users/groups");
            if (rootUserGroupsDirectory == null)
            {
                EmitSignal(SignalName.KnownCommand, $"Attempted to add the \"{groupName}\" user group, but there was no root /users/ directory.");
                return;
            }

            var groupDirectoryExists = rootUserGroupsDirectory.FindDirectory(groupName) != null;
            if (groupDirectoryExists)
            {
                EmitSignal(SignalName.KnownCommand, $"Attempted to make the new user group \"{groupName}\", but it already existed.");
                return;
            }

            DirectoryFolder newUserGroupFolder = new()
            {
                Name = groupName,
                ParentId = rootUserGroupsDirectory.Id,
                Permissions = new() { Permission.AdminRead, Permission.AdminWrite, Permission.UserRead, Permission.UserWrite }
            };
            rootUserGroupsDirectory.Entities.Add(newUserGroupFolder);
            EmitSignal(SignalName.KnownCommand, $"\"{groupName}\" user group created.");
        }

        private void DeleteGroup(IEnumerable<string> arguments)
        {
            var groupName = arguments.FirstOrDefault();
            if (string.IsNullOrEmpty(groupName))
            {
                GD.Print($"Attempted to delete a user group, but group name was not provided.");
                return;
            }

            var rootUserGroupsDirectory = _directoryService.GetRootDirectory().FindDirectory("users/groups");
            if (rootUserGroupsDirectory == null)
            {
                EmitSignal(SignalName.KnownCommand, $"Attempted to delete the \"{groupName}\" user group, but there was no root /users/ directory.");
                return;
            }

            var groupDirectory = rootUserGroupsDirectory.FindDirectory(groupName);
            if (groupDirectory == null)
            {
                EmitSignal(SignalName.KnownCommand, $"Attempted to delete the \"{groupName}\" user group, but it does not exist.");
                return;
            }

            rootUserGroupsDirectory.Entities.Remove(groupDirectory);
            EmitSignal(SignalName.KnownCommand, $"\"{groupName}\" user group removed.");
        }

        private void AddUserToGroup(IEnumerable<string> arguments)
        {
            if(arguments.Count() != 2)
            {
                EmitSignal(SignalName.KnownCommand, $"\"addusertogroup\" takes 2 arguments, use \"help addusertogroup\" to see an example.");
                return;
            }

            var userName = arguments.FirstOrDefault();
            if (string.IsNullOrEmpty(userName))
            {
                GD.Print($"Attempted to add a user to a group, but user name was not provided.");
                return;
            }

            var groupName = arguments.LastOrDefault();
            if (string.IsNullOrEmpty(groupName))
            {
                GD.Print($"Attempted to add a user to a group, but user group was not provided.");
                return;
            }

            var rootUserGroupsDirectory = _directoryService.GetRootDirectory().FindDirectory("users/groups");
            if (rootUserGroupsDirectory == null)
            {
                EmitSignal(SignalName.KnownCommand, $"Attempted to add the \"{userName}\" user to the \"{groupName}\" user group, but there was no root /users/ directory.");
                return;
            }

            var groupDirectory = rootUserGroupsDirectory.FindDirectory(groupName);
            if (groupDirectory == null)
            {
                EmitSignal(SignalName.KnownCommand, $"\"{groupName}\" user group does not exist.");
                return;
            }

            var userFileInGroup = groupDirectory.FindFile(userName);
            if (userFileInGroup != null)
            {
                EmitSignal(SignalName.KnownCommand, $"\"{userName}\" is already a part of the \"{groupName}\" user group.");
                return;
            }

            DirectoryFile userGroupFile = new()
            {
                Name = userName,
                ParentId = groupDirectory.Id,
                Permissions = new() { Permission.AdminRead, Permission.AdminWrite, Permission.UserRead, Permission.UserWrite }
            };
            groupDirectory.Entities.Add(userGroupFile);
            EmitSignal(SignalName.KnownCommand, $"\"{userName}\" added to the \"{groupName}\" user group.");
        }

        private void DeleteUserFromGroup(IEnumerable<string> arguments)
        {
            if (arguments.Count() != 2)
            {
                EmitSignal(SignalName.KnownCommand, $"\"deleteuserfromgroup\" takes 2 arguments, use \"help deleteuserfromgroup\" to see an example.");
                return;
            }

            var userName = arguments.FirstOrDefault();
            if (string.IsNullOrEmpty(userName))
            {
                GD.Print($"Attempted to remove a user from a group, but user name was not provided.");
                return;
            }

            var groupName = arguments.LastOrDefault();
            if (string.IsNullOrEmpty(groupName))
            {
                GD.Print($"Attempted to remove a user from a group, but user group was not provided.");
                return;
            }

            var rootUserGroupsDirectory = _directoryService.GetRootDirectory().FindDirectory("users/groups");
            if (rootUserGroupsDirectory == null)
            {
                EmitSignal(SignalName.KnownCommand, $"Attempted to remove the \"{userName}\" user from the \"{groupName}\" user group, but there was no root /users/ directory.");
                return;
            }

            var groupDirectory = rootUserGroupsDirectory.FindDirectory(groupName);
            if (groupDirectory == null)
            {
                EmitSignal(SignalName.KnownCommand, $"\"{groupName}\" user group does not exist.");
                return;
            }

            var userFileInGroup = groupDirectory.FindFile(userName);
            if (userFileInGroup == null)
            {
                EmitSignal(SignalName.KnownCommand, $"\"{userName}\" is not part of the \"{groupName}\" user group.");
                return;
            }

            groupDirectory.Entities.Remove(userFileInGroup);
            EmitSignal(SignalName.KnownCommand, $"\"{userName}\" removed from the \"{groupName}\" user group.");
        }
    }
}
