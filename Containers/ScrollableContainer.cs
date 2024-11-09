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
        private PersistService _persistService;
        private StyleBoxEmpty _emptyStyleBox = new();

        public override void _Ready()
        {
            _persistService = GetNode<PersistService>(ServicePathConstants.PersistServicePath);
            _defaultUserInputTheme = GD.Load<Theme>(ThemePathConstants.MonospaceFontThemePath);
            _userInput = GetNode<UserInput>("UserInput");
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
            newUserInput.ListDirectoryCommand += ListDirectoryCommandResponse;
            newUserInput.ChangeDirectoryCommand += ChangeDirectoryCommandResponse;
            newUserInput.MakeFileCommand += MakeFileCommandResponse;
            newUserInput.MakeDirectoryCommand += MakeDirectoryCommandResponse;

            AddChild(newUserInput);
            newUserInput.Owner = this;

            EmitSignal(SignalName.ChildEnteredTree);
        }

        private void ColorCommandResponse(string colorName)
        {
            if (!ColorConstants.TerminalColors.TryGetValue(colorName, out Color newColor))
            {
                CreateResponse($"Invalid color option. Possible color options are: {string.Join(", ", ColorConstants.TerminalColors.Keys)}.");
                return;
            }

            _persistService.CurrentColor = newColor;
            CreateResponse($"Color updated to {colorName}.");
        }

        private void SaveCommandResponse()
        {
            _persistService.SaveGame();
            CreateResponse("Progress saved.");
        }

        private void ListDirectoryCommandResponse()
        {
            CreateResponse(string.Join(' ', _persistService.GetCurrentDirectory().Entities));
        }

        private void ChangeDirectoryCommandResponse(string newDirectory)
        {
            if (newDirectory.Equals("."))
            {
                return;
            }

            if (newDirectory.Equals(".."))
            {
                var parentDirectory = _persistService.GetParentDirectory(_persistService.GetCurrentDirectory());
                _persistService.SetCurrentDirectory(parentDirectory);
                return;
            }

            if (newDirectory.Equals("~"))
            {
                _persistService.SetCurrentDirectory(_persistService.GetHomeDirectory());
                return;
            }

            if (newDirectory.Equals("root"))
            {
                _persistService.SetCurrentDirectory(_persistService.GetRootDirectory());
                return;
            }

            _persistService.SetCurrentDirectory(newDirectory);
        }

        private void MakeFileCommandResponse(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                CreateResponse("File can't be made without a name.");
                return;
            }

            var existingFile = _persistService.GetFile(fileName);
            if (existingFile != null)
            {
                CreateResponse($"File with the name of '{fileName}' already exists.");
                return;
            }

            _persistService.CreateFile(fileName);
            CreateResponse($"New file '{fileName}' created.");
        }

        private void MakeDirectoryCommandResponse(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
            {
                CreateResponse("Directory can't be made without a name.");
                return;
            }

            var existingDirectory = _persistService.GetDirectory(directoryName);
            if (existingDirectory != null)
            {
                CreateResponse($"Directory with the name of '{directoryName}' already exists.");
                return;
            }

            _persistService.CreateDirectory(directoryName);
            CreateResponse($"New directory '{directoryName}' created.");
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