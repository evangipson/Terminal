using Godot;
using Terminal.Inputs;

namespace Terminal.Containers
{
	public partial class ScrollableContainer : VBoxContainer
	{
		private UserInput _userInput;
		private Theme _defaultUserInputTheme;
		private StyleBoxEmpty _emptyStyleBox;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
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

			newUserInput.AddThemeColorOverride("font_color", new Color("#377a1c"));
			newUserInput.AddThemeColorOverride("caret_color", new Color("#377a1c"));
			newUserInput.AddThemeConstantOverride("outline_size", 0);
			newUserInput.AddThemeConstantOverride("caret_width", 8);
			newUserInput.AddThemeStyleboxOverride("normal", _emptyStyleBox);
			newUserInput.AddThemeStyleboxOverride("focus", _emptyStyleBox);

			newUserInput.Evaluated += AddNewUserInput;
			newUserInput.UnknownCommand += AddUnknownCommandResponse;
			AddChild(newUserInput);
			newUserInput.Owner = this;

			EmitSignal(SignalName.ChildEnteredTree);
		}

		private void AddUnknownCommandResponse(string message)
		{
			var commandResponse = new Label()
			{
				Name = $"Response-{GetChildCount()}",
				GrowHorizontal = GrowDirection.End,
				GrowVertical = GrowDirection.End,
				Theme = _defaultUserInputTheme,
				Text = message,
				FocusMode = FocusModeEnum.All,
				AutowrapMode = TextServer.AutowrapMode.Arbitrary
			};

			commandResponse.AddThemeColorOverride("font_color", new Color("#377a1c"));
			commandResponse.AddThemeColorOverride("caret_color", new Color("#377a1c"));
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