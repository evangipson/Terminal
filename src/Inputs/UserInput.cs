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
        /// The <see cref="Signal"/> that broadcasts when the <see cref="UserCommand.ListDirectory"/> command is invoked.
        /// </summary>
        /// <param name="directoryToList">
        /// An optional directory to list, if not listing the contents of the current directory.
        /// </param>
        [Signal]
        public delegate void ListDirectoryCommandEventHandler(string directoryToList);

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
        /// The <see cref="Signal"/> that broadcasts when the <see cref="UserCommand.Network"/> command is invoked.
        /// </summary>
        [Signal]
        public delegate void NetworkCommandEventHandler();

        /// <summary>
        /// The <see cref="Signal"/> that broadcasts when the <see cref="UserCommand.ClearScreen"/> command is invoked.
        /// </summary>
        [Signal]
        public delegate void ClearScreenEventHandler();

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
            UserCommand.DeleteUserFromGroup,
            UserCommand.ViewGroup
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
        private ConfigService _configService;
        private AutoCompleteService _autoCompleteService;
        private NetworkService _networkService;
        private UserService _userService;
        private PermissionsService _permissionsService;
        private bool _hasFocus = false;
        private int _commandMemoryIndex;

        public override void _Ready()
        {
            _userCommandService = GetNode<UserCommandService>(ServicePathConstants.UserCommandServicePath);
            _directoryService = GetNode<DirectoryService>(ServicePathConstants.DirectoryServicePath);
            _configService = GetNode<ConfigService>(ServicePathConstants.ConfigServicePath);
            _persistService = GetNode<PersistService>(ServicePathConstants.PersistServicePath);
            _autoCompleteService = GetNode<AutoCompleteService>(ServicePathConstants.AutoCompleteServicePath);
            _networkService = GetNode<NetworkService>(ServicePathConstants.NetworkServicePath);
            _userService = GetNode<UserService>(ServicePathConstants.UserServicePath);
            _permissionsService = GetNode<PermissionsService>(ServicePathConstants.PermissionsServicePath);
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
                UnsubscribeAndStopInput();
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
                UserCommand.ChangeDirectory => () => CreateSimpleTerminalResponse(_directoryService.ChangeDirectory(parsedTokens.Take(2).Last())),
                UserCommand.ListDirectory => () => ListDirectory(parsedTokens.Skip(1).Take(1).FirstOrDefault()),
                UserCommand.ViewFile => () => CreateSimpleTerminalResponse(_directoryService.ViewFile(parsedTokens.Take(2).Last())),
                UserCommand.MakeFile => () => CreateSimpleTerminalResponse(_directoryService.CreateFile(parsedTokens.Take(2).Last())),
                UserCommand.MakeDirectory => () => CreateSimpleTerminalResponse(_directoryService.CreateDirectory(parsedTokens.Take(2).Last())),
                UserCommand.EditFile => () => EditFile(parsedTokens.Take(2).Last()),
                UserCommand.ListHardware => () => ListHardware(),
                UserCommand.ViewPermissions => () => CreateSimpleTerminalResponse(_permissionsService.ViewPermissions(parsedTokens.Take(2).Last())),
                UserCommand.ChangePermissions => () => CreateSimpleTerminalResponse(_permissionsService.ChangePermissions(parsedTokens.Skip(1).Take(2).FirstOrDefault(), parsedTokens.Skip(1).Take(2).LastOrDefault())),
                UserCommand.Date => () => CreateSimpleTerminalResponse(DateTime.UtcNow.AddYears(250).ToLongDateString()),
                UserCommand.Time => () => CreateSimpleTerminalResponse(DateTime.UtcNow.AddYears(250).ToLongTimeString()),
                UserCommand.Now => () => CreateSimpleTerminalResponse(string.Join(", ", DateTime.UtcNow.AddYears(250).ToLongTimeString(), DateTime.UtcNow.AddYears(250).ToLongDateString())),
                UserCommand.Network => () => _networkService.ShowNetworkInformation(parsedTokens.Skip(1)),
                UserCommand.Color => () => CreateSimpleTerminalResponse(ChangeColor(parsedTokens.Take(2).Last())),
                UserCommand.Save => () => CreateSimpleTerminalResponse(SaveProgress()),
                UserCommand.Exit => () => Exit(),
                UserCommand.DeleteFile => () => CreateSimpleTerminalResponse(_directoryService.DeleteFile(parsedTokens.Take(2).Last())),
                UserCommand.DeleteDirectory => () => CreateSimpleTerminalResponse(_directoryService.DeleteDirectory(parsedTokens.Take(2).Last(), parsedTokens.Skip(2))),
                UserCommand.MoveFile => () => CreateSimpleTerminalResponse(_directoryService.MoveFile(parsedTokens.Skip(1))),
                UserCommand.MoveDirectory => () => CreateSimpleTerminalResponse(_directoryService.MoveDirectory(parsedTokens.Skip(1))),
                UserCommand.Ping => () => StartPing(parsedTokens.Take(2).Last(), parsedTokens.Skip(2)),
                UserCommand.MakeUser => () => CreateSimpleTerminalResponse(_userService.MakeUser(parsedTokens.Skip(1))),
                UserCommand.DeleteUser => () => CreateSimpleTerminalResponse(_userService.DeleteUser(parsedTokens.Skip(1))),
                UserCommand.MakeGroup => () => CreateSimpleTerminalResponse(_userService.MakeGroup(parsedTokens.Skip(1))),
                UserCommand.DeleteGroup => () => CreateSimpleTerminalResponse(_userService.DeleteGroup(parsedTokens.Skip(1))),
                UserCommand.AddUserToGroup => () => CreateSimpleTerminalResponse(_userService.AddUserToGroup(parsedTokens.Skip(1))),
                UserCommand.DeleteUserFromGroup => () => CreateSimpleTerminalResponse(_userService.DeleteUserFromGroup(parsedTokens.Skip(1))),
                UserCommand.ViewGroup => () => CreateSimpleTerminalResponse(_userService.ViewUserGroup(parsedTokens.Take(2).Last())),
                UserCommand.ClearScreen => () => EmitSignal(SignalName.ClearScreen),
                UserCommand.DeleteSave => () => CreateSimpleTerminalResponse(_persistService.DeleteSaveGame()),
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

        private void CreateSimpleTerminalResponse(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                GD.Print("Attempted to create a simple terminal response, but the message provided was null or empty.");
                return;
            }

            EmitSignal(SignalName.KnownCommand, message);
        }

        private string SaveProgress()
        {
            _persistService.SaveGame();
            return "Progress saved.";
        }

        private void Exit() => GetTree().Quit();

        private string ChangeColor(string newColorName)
        {
            if (!_configService.Colors.TryGetValue(newColorName, out Color newColor))
            {
                var sortedColors = string.Join(", ", _configService.Colors.Keys.OrderBy(key => key).Select(key => key));
                return $"Invalid color option. Possible color options are: {sortedColors}.";
            }

            if (!_configService.ExecutableColors.TryGetValue(newColorName, out Color executableColor))
            {
                return $"Unable to find executable color mapping for '{newColorName}'.";
            }

            _persistService.CurrentColor = newColor;
            _persistService.ExecutableColor = executableColor;
            return $"Color updated to {newColorName}.";
        }

        private void ListDirectory(string directoryToList = null) => EmitSignal(SignalName.ListDirectoryCommand, directoryToList);

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

        private void StartPing(string address, IEnumerable<string> arguments) => _networkService.StartPingResponse(address, arguments);

        private void PingResponse(string pingMessage) => CreateSimpleTerminalResponse(pingMessage);

        private void FinishPing(string message)
        {
            EmitSignal(SignalName.KnownCommand, message);
            EmitSignal(SignalName.Evaluated);
            UnsubscribeAndStopInput();
        }
    }
}
