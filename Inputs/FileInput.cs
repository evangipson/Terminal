using Godot;

using Terminal.Constants;
using Terminal.Containers;
using Terminal.Models;
using Terminal.Services;

namespace Terminal.Inputs
{
    /// <summary>
    /// A <see cref="TextEdit"/> <see cref="Node"/> managed in Godot that allows users to edit the contents of files.
    /// </summary>
    public partial class FileInput : TextEdit
    {
        /// <summary>
        /// The <see cref="Signal"/> that broadcasts when a file is saved in the file editor.
        /// </summary>
        /// <param name="fileName">
        /// The name of the file being saved in the file editor.
        /// </param>
        /// <param name="closeEditor">
        /// A flag the determines if the file is being closed, used to enable a "save" and "save & exit" flow.
        /// </param>
        /// <param name="saveMessage">
        /// The message to show the user when the file has been saved.
        /// </param>
        [Signal]
        public delegate void SaveFileCommandEventHandler(string fileName, bool closeEditor, string saveMessage = null);

        /// <summary>
        /// The <see cref="Signal"/> that broadcasts when the file editor is closed.
        /// </summary>
        /// <param name="closeMessage">
        /// The message to show the user when the file editor closes.
        /// </param>
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

        /// <summary>
        /// Opens the provided <paramref name="file"/> in the file editor by hiding the console and showing the file editor.
        /// <para>
        /// Also ensures the user is focused on the file editor when it opens, and that the file editor is populated with
        /// the contents of the provided <paramref name="file"/>.
        /// </para>
        /// </summary>
        /// <param name="file">
        /// The <see cref="DirectoryEntity"/> to load into the file editor when it opens.
        /// </param>
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

        /// <summary>
        /// Persists the contents of the file editor into the provided <paramref name="file"/>.
        /// </summary>
        /// <param name="file">
        /// The file to persist the file editor content into.
        /// </param>
        /// <param name="closeEditor">
        /// A flag that determines if the file is being closed.
        /// </param>
        public void SaveFile(DirectoryEntity file, bool closeEditor)
        {
            file.Contents = Text;
            if(closeEditor)
            {
                CloseFileEditor();
            }
        }

        /// <summary>
        /// Closes the file editor by hiding it, and shows the console.
        /// <para>
        /// Also clears out the content of the file editor and removes any references to the last open file.
        /// </para>
        /// </summary>
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
