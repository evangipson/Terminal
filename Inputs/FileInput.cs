using Godot;

using Terminal.Constants;
using Terminal.Containers;
using Terminal.Models;
using Terminal.Services;

namespace Terminal.Inputs
{
    public partial class FileInput : TextEdit
    {
        [Signal]
        public delegate void SaveFileCommandEventHandler(string fileName, bool closeEditor, string saveMessage = null);

        [Signal]
        public delegate void CloseFileCommandEventHandler(string closeMessage);

        private PersistService _persistService;
        private Theme _defaultUserInputTheme;
        private ScrollableContainer _scrollableContainer;
        private VBoxContainer _fileEditorContainer;
        private RichTextLabel _saveLabel;
        private RichTextLabel _exitLabel;
        private RichTextLabel _saveAndExitLabel;
        private string _currentFileName;

        public override void _Ready()
        {
            _persistService = GetNode<PersistService>(ServicePathConstants.PersistServicePath);
            _defaultUserInputTheme = GD.Load<Theme>(ThemePathConstants.MonospaceFontThemePath);
            _scrollableContainer = GetNode<ScrollableContainer>("%ScrollableContainer");
            _fileEditorContainer = GetParent<VBoxContainer>();
            _saveLabel = GetNode<RichTextLabel>("%SaveLabel");
            _exitLabel = GetNode<RichTextLabel>("%ExitLabel");
            _saveAndExitLabel = GetNode<RichTextLabel>("%SaveAndExitLabel");
        }

        public override void _Input(InputEvent @event)
        {
            if (!HasFocus())
            {
                return;
            }

            if(@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (!keyEvent.IsCommandOrControlPressed())
                {
                    return;
                }

                if (keyEvent.Keycode == Key.S)
                {
                    EmitSignal(SignalName.SaveFileCommand, _currentFileName, false);
                    return;
                }

                if (keyEvent.Keycode == Key.W)
                {
                    EmitSignal(SignalName.CloseFileCommand, $"'{_currentFileName}' not saved.");
                    return;
                }

                if (keyEvent.Keycode == Key.X)
                {
                    EmitSignal(SignalName.SaveFileCommand, _currentFileName, true, $"'{_currentFileName}' saved.");
                    return;
                }
            }
        }

        public void OpenFileEditor(DirectoryEntity file)
        {
            Text = file.Contents;
            _currentFileName = file.ToString();
            SetProcessInput(true);
            SetLabelTheme();
            _scrollableContainer.Visible = false;
            _fileEditorContainer.Visible = true;
            GrabFocus();
        }

        public void SaveFile(DirectoryEntity file, bool closeEditor)
        {
            file.Contents = Text;
            if(closeEditor)
            {
                CloseFileEditor();
            }
        }

        public void CloseFileEditor()
        {
            _scrollableContainer.Visible = true;
            _fileEditorContainer.Visible = false;
            SetProcessInput(false);
            Text = string.Empty;
            _currentFileName = string.Empty;
        }

        private void SetLabelTheme()
        {
            AddThemeColorOverride("font_color", _persistService.CurrentColor);
            _saveLabel.AddThemeColorOverride("default_color", _persistService.CurrentColor);
            _exitLabel.AddThemeColorOverride("default_color", _persistService.CurrentColor);
            _saveAndExitLabel.AddThemeColorOverride("default_color", _persistService.CurrentColor);
        }
    }
}
