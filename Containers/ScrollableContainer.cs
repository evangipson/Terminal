using Godot;
using Terminal.Constants;
using Terminal.Inputs;
using Terminal.Services;

namespace Terminal.Containers
{
	public partial class ScrollableContainer : VBoxContainer
	{
		private UserInput _userInput;
		private Theme _defaultUserInputTheme;
		private StyleBoxEmpty _emptyStyleBox;
		private PersistService _persistService;

		public override void _Ready()
		{
			_persistService = GetNode<PersistService>("/root/PersistService");
			_userInput = GetNode<UserInput>("UserInput");
			_defaultUserInputTheme = GD.Load<Theme>("res://Themes/monospace-font-theme.tres");
			_emptyStyleBox = new StyleBoxEmpty();
			AddNewUserInput();
		}

		private void AddNewUserInput()
		{
			var newUserInput = new UserInput()
			{
				ContextMenuEnabled = false,
				SelectingEnabled = false,
				DeselectOnFocusLossEnabled = false,
				DragAndDropSelectionEnabled = false,
				MiddleMousePasteEnabled = false,
				WrapMode = TextEdit.LineWrappingMode.Boundary,
				CaretBlink = true,
				CaretMoveOnRightClick = false,
				CaretMultiple = false,
				Name = $"UserInput-{GetChildCount()}",
				GrowHorizontal = GrowDirection.End,
				GrowVertical = GrowDirection.End,
				ScrollFitContentHeight = true,
				Theme = _defaultUserInputTheme,
				Text = string.Empty,
				FocusMode = FocusModeEnum.All,
				AutowrapMode = TextServer.AutowrapMode.Arbitrary
			};

			newUserInput.AddThemeColorOverride("font_color", _persistService.CurrentColor);
			newUserInput.AddThemeColorOverride("caret_color", _persistService.CurrentColor);
			newUserInput.AddThemeConstantOverride("outline_size", 0);
			newUserInput.AddThemeConstantOverride("caret_width", 8);
			newUserInput.AddThemeStyleboxOverride("normal", _emptyStyleBox);
			newUserInput.AddThemeStyleboxOverride("focus", _emptyStyleBox);

			newUserInput.Evaluated += AddNewUserInput;
			newUserInput.KnownCommand += CreateResponse;
			newUserInput.ColorCommand += ColorCommandResponse;
			newUserInput.SaveCommand += SaveCommandResponse;
			AddChild(newUserInput);
			newUserInput.Owner = this;

			EmitSignal(SignalName.ChildEnteredTree);
		}

		private void ColorCommandResponse(string colorName)
		{
			_persistService.CurrentColor = colorName switch
			{
				"blue" => ColorConstants.TerminalBlue,
				"purple" => ColorConstants.TerminalPurple,
				"orange" => ColorConstants.TerminalOrange,
				"red" => ColorConstants.TerminalRed,
				"teal" => ColorConstants.TerminalTeal,
				_ => ColorConstants.TerminalGreen
			};

			CreateResponse($"Color updated to {colorName}.");
		}

		private void SaveCommandResponse()
		{
			_persistService.SaveGame();
			CreateResponse("Progress saved.");
		}

		private void CreateResponse(string message)
		{
			var commandResponse = new Label()
			{
				Name = $"Response-{GetChildCount()}",
				GrowHorizontal = GrowDirection.End,
				GrowVertical = GrowDirection.End,
				Theme = _defaultUserInputTheme,
				Text = message,
				FocusMode = FocusModeEnum.None,
				AutowrapMode = TextServer.AutowrapMode.WordSmart
			};

			commandResponse.AddThemeColorOverride("font_color", _persistService.CurrentColor);
			commandResponse.AddThemeColorOverride("caret_color", _persistService.CurrentColor);
			commandResponse.AddThemeConstantOverride("outline_size", 0);
			commandResponse.AddThemeConstantOverride("caret_width", 8);
			commandResponse.AddThemeStyleboxOverride("normal", _emptyStyleBox);
			commandResponse.AddThemeStyleboxOverride("focus", _emptyStyleBox);

			AddChild(commandResponse);
			commandResponse.Owner = this;

			EmitSignal(SignalName.ChildEnteredTree);
		}
	}
}