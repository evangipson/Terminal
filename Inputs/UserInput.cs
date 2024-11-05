using System.Linq;
using Godot;
using Godot.Collections;

using Terminal.Audio;
using Terminal.Constants;
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

        private KeyboardSounds _keyboardSounds;
        private PersistService _persistService;
        private bool _hasFocus = false;
        private int _commandMemoryIndex;

        public override void _Ready()
        {
            _persistService = GetNode<PersistService>(ServicePathConstants.PersistServicePath);
            _keyboardSounds = GetTree().Root.GetNode<KeyboardSounds>(KeyboardSounds.AbsolutePath);
            _commandMemoryIndex = _persistService.CommandMemory.Count;

            SetInitialDirectory();
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

            if (Input.IsPhysicalKeyPressed(Key.Enter))
            {
                EvaluateCommand();
                return;
            }

            if (Input.IsPhysicalKeyPressed(Key.Up))
            {
                _commandMemoryIndex -= 1;
                if (_commandMemoryIndex < 0)
                {
                    _commandMemoryIndex = _persistService.CommandMemory.Count - 1;
                }
                Text = string.Join(' ', GetDirectoryWithPrompt(), _persistService.CommandMemory.ElementAtOrDefault(_commandMemoryIndex));
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
                Text = string.Join(' ', GetDirectoryWithPrompt(), _persistService.CommandMemory.ElementAtOrDefault(_commandMemoryIndex));
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

        private void SetInitialDirectory() => Text = GetDirectoryWithPrompt();

        private void EvaluateCommand()
        {
            var inputWithoutDirectory = Text.Replace(GetDirectoryWithPrompt(), string.Empty).Trim(' ');
            _persistService.AddCommandToMemory(inputWithoutDirectory);
            _commandMemoryIndex = 0;

            var parsedTokens = UserCommandService.ParseInputToTokens(inputWithoutDirectory);
            var command = UserCommandService.EvaluateUserInput(inputWithoutDirectory);
            if (command == Enums.UserCommand.Help && parsedTokens.All(token => token.Equals("help", System.StringComparison.OrdinalIgnoreCase)))
            {
                Dictionary<string, string> outputTokens = new()
                {
                    ["TOPIC"] = "Terminal OS help system.",
                    ["REMARKS"] = "Displays help about Terminal OS commands.",
                    ["EXAMPLES"] = "help commands\t: Display information about the terminal commands."
                };
                EmitSignal(SignalName.KnownCommand, GetOutputFromTokens(outputTokens));
            }
            if (command == Enums.UserCommand.Help && parsedTokens.Take(2).Last().Equals("exit", System.StringComparison.OrdinalIgnoreCase))
            {
                Dictionary<string, string> outputTokens = new()
                {
                    ["COMMAND"] = "exit",
                    ["REMARKS"] = "Exits Terminal OS.",
                };
                EmitSignal(SignalName.KnownCommand, GetOutputFromTokens(outputTokens));
            }
            if ((command == Enums.UserCommand.Help && parsedTokens.Take(2).Last().Equals("color", System.StringComparison.OrdinalIgnoreCase)) || (command == Enums.UserCommand.Color && parsedTokens.Count == 1))
            {
                Dictionary<string, string> outputTokens = new()
                {
                    ["COMMAND"] = "color",
                    ["REMARKS"] = "Changes the color of the terminal output.",
                    ["COLORS"] = string.Join(", ", ColorConstants.TerminalColors.Keys),
                    ["EXAMPLES"] = $"color {ColorConstants.TerminalColors.Keys.First()}\t: Change the terminal output to {ColorConstants.TerminalColors.Keys.First()}."
                };
                EmitSignal(SignalName.KnownCommand, GetOutputFromTokens(outputTokens));
            }
            if (command == Enums.UserCommand.Help && parsedTokens.Take(2).Last().Equals("save", System.StringComparison.OrdinalIgnoreCase))
            {
                Dictionary<string, string> outputTokens = new()
                {
                    ["COMMAND"] = "save",
                    ["REMARKS"] = "Saves the state of the terminal."
                };
                EmitSignal(SignalName.KnownCommand, GetOutputFromTokens(outputTokens));
            }
            if ((command == Enums.UserCommand.Help && parsedTokens.Take(2).Last().Equals("commands", System.StringComparison.OrdinalIgnoreCase)) || command == Enums.UserCommand.Commands)
            {
                Dictionary<string, string> outputTokens = new()
                {
                    ["COMMAND"] = "commands",
                    ["REMARKS"] = "Displays information about the terminal commands. Use help [command] to get more information about each command.",
                    ["COMMANDS"] = "help, commands, ls, cd, exit, save, color"
                };
                EmitSignal(SignalName.KnownCommand, GetOutputFromTokens(outputTokens));
            }
            if (command == Enums.UserCommand.Help && parsedTokens.Take(2).Last().Equals("cd", System.StringComparison.OrdinalIgnoreCase) || (command == Enums.UserCommand.ChangeDirectory && parsedTokens.Count == 1))
            {
                Dictionary<string, string> outputTokens = new()
                {
                    ["COMMAND"] = "cd",
                    ["REMARKS"] = "Changes directory.",
                    ["EXAMPLES"] = $"cd ~\t: Change directory to the default home directory for the current user."
                };
                EmitSignal(SignalName.KnownCommand, GetOutputFromTokens(outputTokens));
            }
            if (command == Enums.UserCommand.ChangeDirectory && parsedTokens.Count < 3)
            {
                EmitSignal(SignalName.ChangeDirectoryCommand, parsedTokens.Last());
            }
            if (command == Enums.UserCommand.Help && parsedTokens.Take(2).Last().Equals("ls", System.StringComparison.OrdinalIgnoreCase))
            {
                Dictionary<string, string> outputTokens = new()
                {
                    ["COMMAND"] = "ls",
                    ["REMARKS"] = "Lists contents of a directory.",
                    ["EXAMPLES"] = "ls\t: List the contents of the current directory."
                };
                EmitSignal(SignalName.KnownCommand, GetOutputFromTokens(outputTokens));
            }
            if (command == Enums.UserCommand.ListDirectory)
            {
                EmitSignal(SignalName.ListDirectoryCommand);
            }
            if ((command == Enums.UserCommand.Help && parsedTokens.Take(2).Last().Equals("ls", System.StringComparison.OrdinalIgnoreCase)) || (command == Enums.UserCommand.ViewFile && parsedTokens.Count == 1))
            {
                Dictionary<string, string> outputTokens = new()
                {
                    ["COMMAND"] = "view",
                    ["REMARKS"] = "View the contents of a file.",
                    ["EXAMPLES"] = "view file.ext\t: List the contents of the file.ext file."
                };
                EmitSignal(SignalName.KnownCommand, GetOutputFromTokens(outputTokens));
            }
            if (command == Enums.UserCommand.ViewFile && parsedTokens.Count == 2)
            {
                var fileName = parsedTokens.Take(2).Last();
                var file = _persistService.GetFile(fileName);
                EmitSignal(SignalName.KnownCommand, file?.Contents ?? $"\"{fileName}\" does not exist.");
            }
            if (command == Enums.UserCommand.Color && parsedTokens.Count == 2)
            {
                EmitSignal(SignalName.ColorCommand, parsedTokens.Last());
            }
            if (command == Enums.UserCommand.Save)
            {
                EmitSignal(SignalName.SaveCommand);
            }
            if (command == Enums.UserCommand.Unknown && !parsedTokens.All(token => string.IsNullOrEmpty(token)))
            {
                EmitSignal(SignalName.KnownCommand, $"\n\"{parsedTokens.First()}\" is an unknown command.\n");
            }
            if (command == Enums.UserCommand.Exit)
            {
                GetTree().Quit();
                return;
            }

            EmitSignal(SignalName.Evaluated);
            SetProcessInput(false);
            GetTree().Root.SetInputAsHandled();
        }

        private static string GetOutputFromTokens(Dictionary<string, string> outputTokens) => string.Join("\n\n", outputTokens.Select(token => string.Join('\n', token.Key, $"\t{token.Value}")));

        private void PlayKeyboardSound() => _keyboardSounds.PlayKeyboardSound();

        private string GetDirectoryWithPrompt() => $"{_persistService.GetCurrentDirectoryPath()} {DirectoryConstants.TerminalPromptCharacter} ";
    }
}
