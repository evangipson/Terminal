using System;
using System.Linq;
using Godot;
using Godot.Collections;
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

		private KeyboardSounds _keyboardSounds;
		private PersistService _persistService;
		private string _directory;
		private bool _hasFocus = false;
		private int _commandMemoryIndex = 0;

		public override void _Ready()
		{
			_persistService = GetNode<PersistService>("/root/PersistService");
			_keyboardSounds = GetTree().Root.GetNode<CanvasLayer>("Root").GetNode<KeyboardSounds>("/root/Root/KeyboardSounds");

			SetDirectory();
			SetCaretColumn(Text.Length);
			SetCaretLine(1);

			GD.Print($"user input '{Name}' ready");
		}

		public override void _Draw()
		{
			if(!_hasFocus)
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
				_keyboardSounds.PlayKeyboardSound();
			}

			if (Input.IsPhysicalKeyPressed(Key.Enter))
			{
				EvaluateCommand();
				return;
			}

			if(Input.IsPhysicalKeyPressed(Key.Up))
			{
				_commandMemoryIndex += 1;
				if (_commandMemoryIndex > _persistService.CommandMemory.Count - 1)
				{
					_commandMemoryIndex = 0;
				}
				Text = $"{_directory} {_persistService.CommandMemory.ElementAtOrDefault(_commandMemoryIndex)}";
				SetCaretColumn(Text.Length);
				GetTree().Root.SetInputAsHandled();
				return;
			}

			if (Input.IsPhysicalKeyPressed(Key.Down))
			{
				_commandMemoryIndex -= 1;
				if (_commandMemoryIndex < 0)
				{
					_commandMemoryIndex = _persistService.CommandMemory.Count - 1;
				}
				Text = $"{_directory} {_persistService.CommandMemory.ElementAtOrDefault(_commandMemoryIndex)}";
				SetCaretColumn(Text.Length);
			}

			if (Input.IsPhysicalKeyPressed(Key.Backspace) || Input.IsPhysicalKeyPressed(Key.Left) || Input.IsPhysicalKeyPressed(Key.Home))
			{
				var caretPosition = GetCaretColumn();
				if(caretPosition <= _directory.Length + 1)
				{
					GetTree().Root.SetInputAsHandled();
					return;
				}
			}
		}

		private void SetDirectory(string directory = null)
		{
			_directory = (directory ?? "~>");
			Text = $"{_directory} ";
		}

		private void EvaluateCommand()
		{
			var inputWithoutDirectory = Text.Replace(_directory, string.Empty).Trim(' ');
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
					["COLORS"] = "green (default), blue, purple, orange, red, teal",
					["EXAMPLES"] = "color blue\t: Change the terminal output to blue."
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
					["COMMANDS"] = "exit, save, color, help, commands"
				};
				EmitSignal(SignalName.KnownCommand, GetOutputFromTokens(outputTokens));
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
	}
}
