using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

using Terminal.Audio;
using Terminal.Constants;
using Terminal.Enums;
using Terminal.Extensions;
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
        [Signal]
        public delegate void ListDirectoryCommandEventHandler();

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

            if (@event is InputEventKey && @event.IsPressed())
            {
                CallDeferred("PlayKeyboardSound");
            }

            EvaluateKeyboardInput()?.Invoke();
        }

        private void EvaluateCommand()
        {
            _commandMemoryIndex = 0;

            var inputWithoutDirectory = Text.Replace(_userCommandService.GetCommandPrompt(), string.Empty).Trim(' ');
            _persistService.AddCommandToMemory(inputWithoutDirectory);
            RouteCommand(inputWithoutDirectory)?.Invoke();

            EmitSignal(SignalName.Evaluated);
            SetProcessInput(false);
            GetTree().Root.SetInputAsHandled();

            // unsubscribe from the auto-complete events to prevent old inputs from getting autocompleted.
            _autoCompleteService.OnInvalidAutocomplete -= ListDirectoryAndCreateNewInput;
            _autoCompleteService.OnAutocomplete -= FillInputWithAutocompletedPhrase;
            _networkService.OnShowNetwork -= ShowNetworkResponse;
        }

        private Action RouteCommand(string command)
        {
            var parsedTokens = UserCommandService.ParseInputToTokens(command);
            var userCommand = UserCommandService.EvaluateUserInput(command);
            var unqualifiedCommand = _commandsThatNeedAdditionalArguments.Contains(userCommand) && parsedTokens.Count == 1;
            return userCommand switch
            {
                UserCommand.Help or UserCommand.Commands => QualifyAndEvaluateHelpCommand(userCommand, parsedTokens),
                UserCommand.ChangeDirectory => () => ChangeDirectory(parsedTokens.Take(2).Last()),
                UserCommand.ListDirectory => () => ListDirectory(),
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
                _ when unqualifiedCommand => QualifyAndEvaluateHelpCommand(userCommand, parsedTokens),
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
            var file = _directoryService.GetCurrentDirectory().FindFile(fileName);
            if (file?.Permissions.Contains(Permission.UserWrite) != true)
            {
                EmitSignal(SignalName.KnownCommand, $"Insufficient permissions to edit the \"{fileName}\" file.");
                return;
            }

            EmitSignal(SignalName.EditFileCommand, fileName);
        }

        private void ListHardware() => EmitSignal(SignalName.ListHardwareCommand);

        private void ViewPermissions(string entityName) => EmitSignal(SignalName.ViewPermissionsCommand, entityName);

        private void ChangePermissions(string entityName, string newPermissionsSet) => EmitSignal(SignalName.ChangePermissionsCommand, entityName, newPermissionsSet);

        private void ShowNetworkResponse(string networkResponse) => EmitSignal(SignalName.KnownCommand, networkResponse);

        private void ListDirectoryAndCreateNewInput()
        {
            EmitSignal(SignalName.ListDirectoryCommand);
            EmitSignal(SignalName.Evaluated);
            GetTree().Root.SetInputAsHandled();
        }

        private void FillInputWithAutocompletedPhrase(string phrase)
        {
            Text = phrase;
            SetCaretColumn(Text.Length);
            GetTree().Root.SetInputAsHandled();
        }

        private void UpdateCommandMemory(bool forPreviousCommand)
        {
            _commandMemoryIndex = forPreviousCommand
                ? (_commandMemoryIndex - 1 + _persistService.CommandMemory.Count) % _persistService.CommandMemory.Count
                : (_commandMemoryIndex + 1) % _persistService.CommandMemory.Count;

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

        private Action QualifyAndEvaluateHelpCommand(UserCommand userCommand, List<string> parsedTokens)
        {
            var commandContext = parsedTokens.Take(2).Last();
            var helpContextToken = userCommand;
            if (parsedTokens.Count == 2 && UserCommandService.EvaluateToken(commandContext) != UserCommand.Unknown)
            {
                var parsedContextToken = UserCommandService.EvaluateToken(commandContext);
                helpContextToken = parsedContextToken == UserCommand.Unknown ? UserCommand.Help : parsedContextToken;
            }

            return () => CreateSimpleTerminalResponse(_userCommandService.EvaluateHelpCommand(helpContextToken, commandContext));
        }
    }
}
