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
		public delegate void UnknownCommandEventHandler(string message);

		private string _directory;

		private bool _hasFocus = false;

		public override void _Ready()
		{
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

			if (command == Enums.UserCommand.Unknown && !parsedTokens.All(token => string.IsNullOrEmpty(token)))
			{
				EmitSignal(SignalName.UnknownCommand, $"\"{parsedTokens.First()}\" is an unknown command");
			}

			EmitSignal(SignalName.Evaluated);
			SetProcessInput(false);
			GetTree().Root.SetInputAsHandled();
		}
	}
}
