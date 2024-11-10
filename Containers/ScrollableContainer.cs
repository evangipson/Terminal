using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

using Terminal.Constants;
using Terminal.Inputs;
using Terminal.Models;
using Terminal.Services;

namespace Terminal.Containers
{
    public partial class ScrollableContainer : VBoxContainer
    {
        private UserInput _userInput;
        private Theme _defaultUserInputTheme;
        private PersistService _persistService;
        private StyleBoxEmpty _emptyStyleBox = new();
        private FileInput _fileInput;

        public override void _Ready()
        {
            _persistService = GetNode<PersistService>(ServicePathConstants.PersistServicePath);
            _defaultUserInputTheme = GD.Load<Theme>(ThemePathConstants.MonospaceFontThemePath);
            _userInput = GetNode<UserInput>("UserInput");
            _fileInput = GetNode<FileInput>("%FileInput");
            _fileInput.SaveFileCommand += SaveFileCommandResponse;
            _fileInput.CloseFileCommand += CloseFileCommandResponse;

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
            newUserInput.EditFileCommand += EditFileCommandResponse;
            newUserInput.ListHardwareCommand += ListHardwareCommandResponse;

            AddChild(newUserInput);
            newUserInput.Owner = this;

            EmitSignal(SignalName.ChildEnteredTree);
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

        private void CreateListDirectoryResponse(List<DirectoryEntity> entities)
        {
            var directoryResponse = new RichTextLabel()
            {
                Name = $"Response-{GetChildCount()}",
                GrowHorizontal = GrowDirection.End,
                GrowVertical = GrowDirection.End,
                FitContent = true,
                ScrollActive = false,
                ShortcutKeysEnabled = false,
                MouseFilter = MouseFilterEnum.Ignore,
                Theme = _defaultUserInputTheme,
                FocusMode = FocusModeEnum.None,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };

            List<string> entityList = entities.Select(entity =>
            {
                if (entity.Permissions.Contains(Permission.UserExecutable) || entity.Permissions.Contains(Permission.AdminExecutable))
                {
                    return $"[color=#{_persistService.ExecutableColor.ToHtml(false)}]{entity}[/color]";
                }

                return $"{entity}";
            }).ToList();
            directoryResponse.AppendText(string.Join(' ', entityList));
            directoryResponse.AddThemeColorOverride("default_color", _persistService.CurrentColor);

            AddChild(directoryResponse);
            directoryResponse.Owner = this;

            EmitSignal(SignalName.ChildEnteredTree);
        }

        private void ColorCommandResponse(string colorName)
        {
            if (!ColorConstants.TerminalColors.TryGetValue(colorName, out Color newColor))
            {
                CreateResponse($"Invalid color option. Possible color options are: {string.Join(", ", ColorConstants.TerminalColors.Keys)}.");
                return;
            }

            if(!ColorConstants.ColorToExecutableColorMap.TryGetValue(colorName, out Color executableColor))
            {
                GD.PrintErr($"Unable to find executable color mapping for '{colorName}'.");
                return;
            }

            _persistService.CurrentColor = newColor;
            _persistService.ExecutableColor = executableColor;
            CreateResponse($"Color updated to {colorName}.");
        }

        private void SaveCommandResponse()
        {
            _persistService.SaveGame();
            CreateResponse("Progress saved.");
        }

        private void ListDirectoryCommandResponse() => CreateListDirectoryResponse(_persistService.GetCurrentDirectory().Entities);

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

            if (newDirectory.Equals("root") || newDirectory.Equals(DirectoryConstants.DirectorySeparator))
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
                GD.PrintErr("File can't be made without a name.");
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
                GD.PrintErr("Directory can't be made without a name.");
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

        private void EditFileCommandResponse(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                GD.PrintErr("File can't be edited without a name.");
                return;
            }

            var existingFile = _persistService.GetFile(fileName);
            if (existingFile == null)
            {
                CreateResponse($"No file with the name '{fileName}' exists.");
                return;
            }

            _fileInput.OpenFileEditor(existingFile);
        }

        private void SaveFileCommandResponse(string fileName, bool closeEditor, string saveMessage)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                GD.PrintErr("File can't be saved without a name.");
                return;
            }

            var existingFile = _persistService.GetFile(fileName);
            if (existingFile == null)
            {
                GD.PrintErr($"No file with the name '{fileName}' can be saved.");
                return;
            }

            _fileInput.SaveFile(existingFile, closeEditor);
            if(!string.IsNullOrEmpty(saveMessage))
            {
                CreateResponse(saveMessage);
                AddNewUserInput();
            }
        }

        private void CloseFileCommandResponse(string closeMessage)
        {
            _fileInput.CloseFileEditor();
            if (!string.IsNullOrEmpty(closeMessage))
            {
                CreateResponse(closeMessage);
                AddNewUserInput();
            }
        }

        private void ListHardwareCommandResponse()
        {
            var deviceDirectory = _persistService.GetRootDirectory().FindDirectory("system").FindDirectory("device");
            var hardwareResponse = deviceDirectory.Entities.Where(entity => entity.IsDirectory).Select(entity =>
            {
                var hardwareEntities = entity.Entities
                    .Where(entity => !entity.IsDirectory)
                    .Select(hardware => new KeyValuePair<string, DirectoryEntity>(entity.Name, hardware));

                var hardwareEntityList = hardwareEntities.Select(hwe =>
                {
                    var hardwareInfoLines = hwe.Value.Contents.Split('\n');
                    return hardwareInfoLines.Select(hwel =>
                    {
                        var nameAndValue = hwel.Split(':');

                        if (nameAndValue.Length != 2)
                        {
                            return string.Empty;
                        }

                        return $"{nameAndValue.First()}: {nameAndValue.Last()}";
                    });
                }).ToList();

                return new KeyValuePair<string, List<IEnumerable<string>>>(entity.Name, hardwareEntityList);
            }).ToList();

            List<Tuple<string, string, string>> hardwareFourColumnOutput = new() { new("Device", "Class", "Manufacturer"), new("------", "----", "------------") };
            hardwareFourColumnOutput.AddRange(hardwareResponse.SelectMany(hwr =>
            {
                var device = $"{hwr.Key}";
                return hwr.Value.Select(hwri => new Tuple<string, string, string>(
                    device,
                    hwri.First(x => x.Contains("name:")).Split(':').Last().Trim(),
                    hwri.First(x => x.Contains("manufacturer:")).Split(':').Last().Trim()
                ));
            }).ToList());

            //var deviceList = hardwareResponse.Select(hwr => $"{hwr.Key}\n{string.Concat(hwr.Key.Select(hwrc => '-'))}\n{string.Join("\n\n", hwr.Value.Select(hwri => string.Join('\n', hwri)))}");
            CreateResponse(string.Join("\n", hardwareFourColumnOutput.Select(hardwareOutputTuple => $"{hardwareOutputTuple.Item1,-15}{hardwareOutputTuple.Item2,-15}{hardwareOutputTuple.Item3}")));
        }
    }
}