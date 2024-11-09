using System.Collections.Generic;
using System.Linq;
using Godot;

using Terminal.Audio;
using Terminal.Constants;
using Terminal.Enums;
using Terminal.Models;
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

        [Signal]
        public delegate void MakeFileCommandEventHandler(string fileName);

        [Signal]
        public delegate void MakeDirectoryCommandEventHandler(string directoryName);

        [Signal]
        public delegate void EditFileCommandEventHandler(string fileName);

        private KeyboardSounds _keyboardSounds;
        private PersistService _persistService;
        private UserCommandEvaluator _userCommandEvaluator;
        private bool _hasFocus = false;
        private int _commandMemoryIndex;
        private DirectoryEntity _autocompletedEntity;

        public override void _Ready()
        {
            _persistService = GetNode<PersistService>(ServicePathConstants.PersistServicePath);
            _keyboardSounds = GetTree().Root.GetNode<KeyboardSounds>(KeyboardSounds.AbsolutePath);
            _commandMemoryIndex = _persistService.CommandMemory.Count;

            _userCommandEvaluator = new();
            _userCommandEvaluator.SimpleMessageCommand += CreateSimpleTerminalResponse;
            _userCommandEvaluator.SaveProgressCommand += SaveProgress;
            _userCommandEvaluator.ExitCommand += Exit;
            _userCommandEvaluator.ColorChangeCommand += ChangeColor;
            _userCommandEvaluator.ChangeDirectoryCommand += ChangeDirectory;
            _userCommandEvaluator.ListDirectoryCommand += ListDirectory;
            _userCommandEvaluator.ViewFileCommand += ViewFile;
            _userCommandEvaluator.MakeFileCommand += MakeFile;
            _userCommandEvaluator.MakeDirectoryCommand += MakeDirectory;
            _userCommandEvaluator.EditFileCommand += EditFile;

            Text = GetDirectoryWithPrompt();
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

            EvaluateKeyboardInput();
        }

        private void EvaluateKeyboardInput()
        {
            if (Input.IsPhysicalKeyPressed(Key.Enter))
            {
                EvaluateCommand();
                return;
            }

            if (Input.IsPhysicalKeyPressed(Key.Tab))
            {
                AutocompletePhrase();
                return;
            }

            if (Input.IsPhysicalKeyPressed(Key.Up))
            {
                _commandMemoryIndex -= 1;
                if (_commandMemoryIndex < 0)
                {
                    _commandMemoryIndex = _persistService.CommandMemory.Count - 1;
                }
                Text = string.Concat(GetDirectoryWithPrompt(), _persistService.CommandMemory.ElementAtOrDefault(_commandMemoryIndex));
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
                Text = string.Concat(GetDirectoryWithPrompt(), _persistService.CommandMemory.ElementAtOrDefault(_commandMemoryIndex));
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

        private void AutocompletePhrase()
        {
            var inputWithoutDirectory = Text.Replace(GetDirectoryWithPrompt(), string.Empty).Split(' ');
            var userCommand = UserCommandService.EvaluateToken(inputWithoutDirectory.FirstOrDefault());

            DirectoryEntity matchingEntity = _persistService.GetCurrentDirectory().Entities.FirstOrDefault(entity =>
            {
                return (entity.IsDirectory == (userCommand == UserCommand.ChangeDirectory)) && entity.Name.Contains(inputWithoutDirectory.Last());
            });
            if (_autocompletedEntity != null)
            {
                matchingEntity = _persistService.GetCurrentDirectory().Entities.SkipWhile(p => p.Name != _autocompletedEntity.Name).Skip(1).FirstOrDefault()
                    ?? _persistService.GetCurrentDirectory().Entities.FirstOrDefault();
            }

            if (userCommand == UserCommand.ChangeDirectory || userCommand == UserCommand.ViewFile || userCommand == UserCommand.EditFile)
            {
                Text = string.Concat(GetDirectoryWithPrompt(), $"{inputWithoutDirectory.FirstOrDefault()} {matchingEntity}");
                SetCaretColumn(Text.Length);
                _autocompletedEntity = matchingEntity;
                GetTree().Root.SetInputAsHandled();
                return;
            }

            var matchingEntities = _persistService.GetCurrentDirectory().Entities.Where(entity => entity.Name.Contains(inputWithoutDirectory.Last()));
            if (matchingEntities != null)
            {
                EmitSignal(SignalName.KnownCommand, string.Join(' ', matchingEntities));
                EmitSignal(SignalName.Evaluated);
            }
            GetTree().Root.SetInputAsHandled();
        }

        private void EvaluateCommand()
        {
            _commandMemoryIndex = 0;

            var inputWithoutDirectory = Text.Replace(GetDirectoryWithPrompt(), string.Empty).Trim(' ');
            _persistService.AddCommandToMemory(inputWithoutDirectory);
            _userCommandEvaluator.EvaluateCommand(inputWithoutDirectory);

            EmitSignal(SignalName.Evaluated);
            SetProcessInput(false);
            GetTree().Root.SetInputAsHandled();
        }

        private void PlayKeyboardSound() => _keyboardSounds.PlayKeyboardSound();

        private string GetDirectoryWithPrompt() => $"{_persistService.GetCurrentDirectoryPath()} {DirectoryConstants.TerminalPromptCharacter} ";

        private void CreateSimpleTerminalResponse(string message) => EmitSignal(SignalName.KnownCommand, message);

        private void SaveProgress() => EmitSignal(SignalName.SaveCommand);

        private void Exit() => GetTree().Quit();

        private void ChangeColor(string newColor) => EmitSignal(SignalName.ColorCommand, newColor);

        private void ChangeDirectory(string newDirectory) => EmitSignal(SignalName.ChangeDirectoryCommand, newDirectory);

        private void ListDirectory() => EmitSignal(SignalName.ListDirectoryCommand);

        private void ViewFile(string fileName)
        {
            var file = _persistService.GetFile(fileName);
            EmitSignal(SignalName.KnownCommand, file?.Contents ?? $"\"{fileName}\" does not exist.");
        }

        private void MakeFile(string fileName) => EmitSignal(SignalName.MakeFileCommand, fileName);

        private void MakeDirectory(string directoryName) => EmitSignal(SignalName.MakeDirectoryCommand, directoryName);

        private void EditFile(string fileName) => EmitSignal(SignalName.EditFileCommand, fileName);
    }
}
