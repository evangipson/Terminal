using System.Linq;
using Godot;
using Terminal.Services;

namespace Terminal.Inputs
{
	public partial class UserInput : TextEdit
	{
		[Signal]
		public delegate void EvaluatedEventHandler();

		[Signal]
		public delegate void KnownCommandEventHandler(string message);

		private KeyboardSounds _keyboardSounds;

		private string _directory;

		private bool _hasFocus = false;

		public override void _Ready()
		{
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

			if (Input.IsPhysicalKeyPressed(Key.Backspace) || Input.IsPhysicalKeyPressed(Key.Left) || Input.IsPhysicalKeyPressed(Key.Up) || Input.IsPhysicalKeyPressed(Key.Home))
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
			var inputWithoutDirectory = Text.Replace(_directory, string.Empty).Trim();
			var parsedTokens = UserCommandService.ParseInputToTokens(inputWithoutDirectory);
			var command = UserCommandService.EvaluateUserInput(inputWithoutDirectory);
			if (command == Enums.UserCommand.Exit)
			{
				GetTree().Quit();
				return;
			}

			if (command == Enums.UserCommand.Help && parsedTokens.Count == 1)
			{
				EmitSignal(SignalName.KnownCommand, @"
TOPIC
    Terminal OS help system

SHORT DESCRIPTION
    Displays help about Terminal OS commands.

EXAMPLES:
    help exit   : Display information about the ""exit"" command.
");
			}

			if (command == Enums.UserCommand.Help && parsedTokens.Count == 2 && parsedTokens.Last().Equals("exit", System.StringComparison.OrdinalIgnoreCase))
			{
				EmitSignal(SignalName.KnownCommand, @"
COMMAND
    exit
	
REMARKS
    Exits Terminal OS.
");
			}

			if (command == Enums.UserCommand.Unknown && !parsedTokens.All(token => string.IsNullOrEmpty(token)))
			{
				EmitSignal(SignalName.KnownCommand, $"\"{parsedTokens.First()}\" is an unknown command");
			}

			EmitSignal(SignalName.Evaluated);
			SetProcessInput(false);
			GetTree().Root.SetInputAsHandled();
		}
	}
}
