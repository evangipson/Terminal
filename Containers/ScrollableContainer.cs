using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

using Terminal.Constants;
using Terminal.Enums;
using Terminal.Extensions;
using Terminal.Inputs;
using Terminal.Models;
using Terminal.Services;

namespace Terminal.Containers
{
    /// <summary>
    /// A <see cref="VBoxContainer"/> <see cref="Node"/> managed in Godot that is the wrapper for the console and the file editor.
    /// </summary>
    public partial class ScrollableContainer : VBoxContainer
    {
        private UserInput _userInput;
        private Theme _defaultUserInputTheme;
        private DirectoryService _directoryService;
        private PersistService _persistService;
        private NetworkService _networkService;
        private StyleBoxEmpty _emptyStyleBox = new();
        private FileInput _fileInput;

        public override void _Ready()
        {
            _directoryService = GetNode<DirectoryService>(ServicePathConstants.DirectoryServicePath);
            _persistService = GetNode<PersistService>(ServicePathConstants.PersistServicePath);
            _networkService = GetNode<NetworkService>(ServicePathConstants.NetworkServicePath);
            _defaultUserInputTheme = GD.Load<Theme>(ThemePathConstants.MonospaceFontThemePath);
            _userInput = GetNode<UserInput>("UserInput");
            _fileInput = GetNode<FileInput>("%FileInput");
            _fileInput.SaveFileCommand += SaveFileCommandResponse;
            _fileInput.CloseFileCommand += CloseFileCommandResponse;

            AddNewUserInput();
        }

        public override void _Input(InputEvent @event)
        {
            if (!@event.IsPressed())
            {
                return;
            }

            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // allow control+c to stop in-flight ping command
                if (keyEvent.IsCommandOrControlPressed() && keyEvent.Keycode == Key.C)
                {
                    _networkService.InterruptPing();
                    return;
                }
            }
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
                AutowrapMode = TextServer.AutowrapMode.Arbitrary,
                MouseFilter = MouseFilterEnum.Ignore
            };

            newUserInput.AddThemeColorOverride("font_color", _persistService.CurrentColor);
            newUserInput.AddThemeColorOverride("caret_color", _persistService.CurrentColor);
            newUserInput.AddThemeConstantOverride("outline_size", 0);
            newUserInput.AddThemeConstantOverride("caret_width", 8);
            newUserInput.AddThemeStyleboxOverride("normal", _emptyStyleBox);
            newUserInput.AddThemeStyleboxOverride("focus", _emptyStyleBox);

            newUserInput.Evaluated += AddNewUserInput;
            newUserInput.KnownCommand += CreateResponse;
            newUserInput.ListDirectoryCommand += ListDirectoryCommandResponse;
            newUserInput.EditFileCommand += EditFileCommandResponse;
            newUserInput.ListHardwareCommand += ListHardwareCommandResponse;
            newUserInput.ViewPermissionsCommand += ViewPermissionsCommandResponse;
            newUserInput.ChangePermissionsCommand += ChangePermissionsCommandResponse;
            newUserInput.NetworkCommand += NetworkCommandResponse;

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
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                MouseFilter = MouseFilterEnum.Ignore
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

            List<string> entityList = entities.OrderBy(entity => $"{entity}").Select(entity =>
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

        private void ListDirectoryCommandResponse(string directoryToList)
        {
            if(string.IsNullOrEmpty(directoryToList))
            {
                CreateListDirectoryResponse(_directoryService.GetCurrentDirectory().Entities);
                return;
            }

            if(directoryToList.Equals(TerminalCharactersConstants.Separator.ToString()))
            {
                CreateListDirectoryResponse(_directoryService.GetRootDirectory().Entities);
                return;
            }

            var absoluteDirectoryToList = _directoryService.GetRootDirectory().FindDirectory(directoryToList.TrimEnd('/'));
            if(absoluteDirectoryToList == null)
            {
                CreateResponse($"The directory {directoryToList} does not exist.");
                return;
            }

            CreateListDirectoryResponse(absoluteDirectoryToList.Entities);
        }

        private void EditFileCommandResponse(string fileName)
        {
            var existingFile = _directoryService.GetRelativeFile(fileName);
            if (existingFile == null)
            {
                GD.PrintErr($"Attempted to edit \"{fileName}\" file, but it does not exist.");
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

            var existingFile = _directoryService.GetRelativeFile(fileName);
            if (existingFile == null)
            {
                GD.PrintErr($"No file with the name '{fileName}' can be saved.");
                return;
            }

            _fileInput.SaveFile(existingFile, closeEditor);
            if (!string.IsNullOrEmpty(saveMessage))
            {
                CreateResponse(saveMessage);
            }
            AddNewUserInput();
        }

        private void CloseFileCommandResponse(string closeMessage)
        {
            _fileInput.CloseFileEditor();
            if (!string.IsNullOrEmpty(closeMessage))
            {
                CreateResponse(closeMessage);
            }
            AddNewUserInput();
        }

        private void ListHardwareCommandResponse()
        {
            var deviceDirectory = _directoryService.GetRootDirectory().FindDirectory("system").FindDirectory("device");
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

            List<Tuple<string, string, string>> hardwareColumnsOutput = new()
            {
                new("Device", "Class", "Manufacturer"),
                new("------", "-----", "------------")
            };
            hardwareColumnsOutput.AddRange(hardwareResponse.SelectMany(hwr =>
            {
                var device = $"{hwr.Key}";
                return hwr.Value.Select(hwri => new Tuple<string, string, string>(
                    device,
                    hwri.First(x => x.Contains("name:")).Split(':').Last().Trim(),
                    hwri.First(x => x.Contains("manufacturer:")).Split(':').Last().Trim()
                ));
            }).ToList());

            //var deviceList = hardwareResponse.Select(hwr => $"{hwr.Key}\n{string.Concat(hwr.Key.Select(hwrc => '-'))}\n{string.Join("\n\n", hwr.Value.Select(hwri => string.Join('\n', hwri)))}");
            CreateResponse(string.Join("\n", hardwareColumnsOutput.Select(hardwareOutputTuple => $"{hardwareOutputTuple.Item1,-15}{hardwareOutputTuple.Item2,-15}{hardwareOutputTuple.Item3}")));
        }

        private void ViewPermissionsCommandResponse(string entityName)
        {
            var entity = entityName.StartsWith('/')
                ? _directoryService.GetAbsoluteFile(entityName) ?? _directoryService.GetAbsoluteDirectory(entityName.TrimEnd('/'))
                : _directoryService.GetRelativeFile(entityName) ?? _directoryService.GetRelativeDirectory(entityName.TrimEnd('/'));

            if (entityName == "/" || entityName == "root")
            {
                entity = _directoryService.GetRootDirectory();
            }

            if (entity == null)
            {
                CreateResponse($"No folder or file with the name \"{entityName}\" exists.");
                return;
            }

            CreateResponse(PermissionsService.GetPermissionDisplay(entity.Permissions));
        }

        private void ChangePermissionsCommandResponse(string entityName, string newPermissionSet)
        {
            var entity = entityName.StartsWith('/')
                ? _directoryService.GetAbsoluteFile(entityName) ?? _directoryService.GetAbsoluteDirectory(entityName.TrimEnd('/'))
                : _directoryService.GetRelativeFile(entityName) ?? _directoryService.GetRelativeDirectory(entityName.TrimEnd('/'));

            if (entityName == "/" || entityName == "root")
            {
                entity = _directoryService.GetRootDirectory();
            }

            if (entity == null)
            {
                CreateResponse($"No folder or file with the name \"{entityName}\" exists.");
                return;
            }

            var newPermissions = PermissionsService.GetPermissionFromInput(newPermissionSet);
            if (newPermissions == null)
            {
                CreateResponse($"Permissions set \"{newPermissionSet}\" was in an incorrect format. Permission sets are 6 bits (011011).");
                return;
            }

            entity.Permissions = newPermissions;
            CreateResponse($"\"{entityName}\" permissions updated to {PermissionsService.GetPermissionDisplay(entity.Permissions)}");
        }

        private void NetworkCommandResponse()
        {
            var networkDirectory = _directoryService.GetRootDirectory().FindDirectory("system").FindDirectory("network");
            var networkResponse = networkDirectory.Entities.Where(entity => !entity.IsDirectory).Select(entity =>
            {
                var networkEntityList = entity.Contents.Split('\n');
                return new KeyValuePair<string, List<string>>(entity.Name, networkEntityList.ToList());
            }).ToList();

            List<Tuple<string, string, string, string>> networkColumnsOutput = new()
            {
                new("Name", "Device", "Address (ipv6)", "Address (ipv8)"),
                new("----", "------", "--------------", "--------------")
            };
            networkColumnsOutput.AddRange(networkResponse.Select(nri =>
            {
                var name = $"{nri.Key}";
                var device = $"{nri.Value.First(value => value.Contains("device:")).Replace("device:", string.Empty).Trim()}";
                var ipv6 = $"{nri.Value.First(value => value.Contains("ipv6:")).Replace("ipv6:", string.Empty).Trim()}";
                var ipv8 = $"{nri.Value.First(value => value.Contains("ipv8:")).Replace("ipv8:", string.Empty).Trim()}";
                return new Tuple<string, string, string, string>(name, device, ipv6, ipv8);
            }).ToList());

            CreateResponse(string.Join("\n", networkColumnsOutput.Select(networkOutput => $"{networkOutput.Item1,-10}{networkOutput.Item2,-10}{networkOutput.Item3,-24}{networkOutput.Item4}")));
        }
    }
}