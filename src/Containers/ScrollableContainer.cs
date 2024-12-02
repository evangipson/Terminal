using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

using Terminal.Audio;
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
        private readonly Random _random = new(DateTime.UtcNow.GetHashCode());

        private UserInput _userInput;
        private Theme _defaultUserInputTheme;
        private ConfigService _configService;
        private DirectoryService _directoryService;
        private PersistService _persistService;
        private NetworkService _networkService;
        private HardDriveSounds _hardDriveSounds;
        private TurnOnSounds _turnOnSounds;
        private StyleBoxEmpty _emptyStyleBox = new();
        private FileInput _fileInput;
        private Tween _tween;
        private bool _shouldShowLoadingMessages = true;
        private int _bootTime = 0;

        public override void _Ready()
        {
            _directoryService = GetNode<DirectoryService>(ServicePathConstants.DirectoryServicePath);
            _configService = GetNode<ConfigService>(ServicePathConstants.ConfigServicePath);
            _persistService = GetNode<PersistService>(ServicePathConstants.PersistServicePath);
            _networkService = GetNode<NetworkService>(ServicePathConstants.NetworkServicePath);
            _hardDriveSounds = GetNode<HardDriveSounds>(HardDriveSounds.AbsolutePath);
            _turnOnSounds = GetNode<TurnOnSounds>(TurnOnSounds.AbsolutePath);
            _defaultUserInputTheme = GD.Load<Theme>(ThemePathConstants.MonospaceFontThemePath);
            _userInput = GetNode<UserInput>("UserInput");
            _fileInput = GetNode<FileInput>("%FileInput");
            _fileInput.SaveFileCommand += SaveFileCommandResponse;
            _fileInput.CloseFileCommand += CloseFileCommandResponse;

            UpdateThemeFontSize(_configService.FontSize);
            _configService.OnFontSizeConfigChange += UpdateThemeFontSize;
        }

        public override void _Draw()
        {
            if(_shouldShowLoadingMessages)
            {
                _ = ShowLoadingMessages();
                _shouldShowLoadingMessages = false;
            }
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

        private async Task ShowLoadingMessages()
        {
            _turnOnSounds.PlayTurnOnSound();

            _bootTime += 1200;
            await Task.Delay(1200);

            FadeInScreen();

            _ = _hardDriveSounds.PlayHardDriveSounds(4, 10, 100, 400);

            CreateResponse("                                               ___ ");
            CreateResponse("                                     _____________ ");
            CreateResponse("                      ____________________________ ");
            CreateResponse(" _________________________________________________ ");
            CreateResponse(@" _____ ___ ___ __ __ _ __  _  __  _      __    __  ");
            CreateResponse(@"|_   _| __| _ \  V  | |  \| |/  \| |    /__\ /' _/ ");
            CreateResponse(@"  | | | _|| v / \_/ | | | ' | /\ | |_  | \/ |`._`. ");
            CreateResponse(@"  |_| |___|_|_\_| |_|_|_|\__|_||_|___|  \__/ |___/ ");
            CreateResponse(" _________________________________________________ ");
            CreateResponse("                                     _____________ \n\n");

            _bootTime += 1800;
            await Task.Delay(1800);

            await ShowDotsResponse(10, 25, "BOOT INITIALIZING");
            CreateResponse($"\nDOT (C) 2197 Motherboard");
            CreateResponse($"BIOS Date {DateTime.UtcNow.AddYears(250).ToShortDateString()}\n\n");
            List<string> hardwareToBoot = ["motherboard", "processor", "memory", "input", "storage", "display"];
            var builder = new StringBuilder();
            List<string> hardwareBootOutput = [];
            foreach (var hardware in hardwareToBoot)
            {
                var hardwareInfo = GetHardwareDeviceInfo(hardware);
                foreach (var info in hardwareInfo)
                {
                    if(info.Key.Equals("name", StringComparison.OrdinalIgnoreCase) && builder.Length > 0)
                    {
                        hardwareBootOutput.Add($"{builder.ToString().TrimEnd()}");
                        builder.Clear();
                    }

                    var outputString = info.Key.Equals("name") || info.Key.Equals("manufacturer")
                        ? $"{info.Value}{(info.Key.Equals("manufacturer") ? "\n" : "")}"
                        : $"{info.Key.ToUpper()} {info.Value}";

                    builder.Append($"{outputString} ");
                }
            }

            _ = _hardDriveSounds.PlayHardDriveSounds();
            await ShowHardwareBootMessages(hardwareBootOutput);

            _ = _hardDriveSounds.PlayHardDriveSounds();
            ClearScreen();
            CreateResponse($"DOT Personal Computer Terminal OS Version {ProjectSettings.GetSetting("application/config/version")}\n\n");
            AddNewUserInput();
        }

        private async Task ShowHardwareBootMessages(List<string> hardwareBootMessages)
        {
            foreach (var message in hardwareBootMessages)
            {
                CreateResponse(message);
                var nextBootTime = GetNextBootTime(50, 500);
                _ = _hardDriveSounds.PlayHardDriveSounds(1, Math.Min(nextBootTime / 10, 15));
                await Task.Delay(GetNextBootTime(50, 500));
                _ = _hardDriveSounds.PlayHardDriveSounds();
                await ShowDotsResponse();
            }

            CreateResponse("\nBOOT COMPLETE");
            CreateResponse($"{_bootTime / 1000.0:F5} SECONDS ELAPSED\n");
            _ = _hardDriveSounds.PlayHardDriveSounds();
            await ShowDotsResponse(6, 11, "LOADING USER TERMINAL ", 250, 500);
        }

        private async Task ShowDotsResponse(int minDots = 3, int maxDots = 20, string initialText = ".", int minDotTime = 25, int maxDotTime = 200)
        {
            CreateResponse($"{initialText}");

            var dotsToDraw = _random.Next(minDots, maxDots);
            var lastResponse = GetLastLabel();
            foreach (var _ in Enumerable.Range(0, dotsToDraw))
            {
                var nextDotWait = GetNextBootTime(minDotTime, maxDotTime);
                await Task.Delay(nextDotWait);
                lastResponse.Text += ".";
            }
        }

        private Label GetLastLabel() => GetChild<Label>(GetChildCount() - 1);

        private void ClearScreen()
        {
            // Delete the children, besides the last one, if they aren't already deleted.
            var childrenWithoutInputProcessingTextEdits = GetChildren()
                .Where(IsInstanceValid)
                .Where(child =>
                {
                    return (child is TextEdit && child.IsProcessingInput() == false) || child is not TextEdit;
                });
            foreach (var child in childrenWithoutInputProcessingTextEdits)
            {
                child.QueueFree();
            }

            // Just hide the last child, if it's a TextEdit, because autocomplete uses the
            // last TextEdit. It will be removed during the next ClearScreen().
            if(GetChildren().Where(child => child is TextEdit).LastOrDefault() is not TextEdit lastInputToHide)
            {
                return;
            }
            lastInputToHide.Visible = false;
        }

        private void UpdateThemeFontSize(int fontSize) => _defaultUserInputTheme.DefaultFontSize = fontSize;

        private int GetNextBootTime(int min, int max)
        {
            var nextTime = _random.Next(min, max);
            _bootTime += nextTime;
            return nextTime;
        }

        private void FadeInScreen()
        {
            Modulate = Colors.Transparent;
            _tween = CreateTween().SetTrans(Tween.TransitionType.Cubic);
            _tween.TweenProperty(this, "modulate", Colors.White, _random.Next(3000, 5000) / 1000.0);
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
                Name = $"UserInput",
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
            newUserInput.NetworkCommand += NetworkCommandResponse;
            newUserInput.ClearScreen += ClearScreen;

            AddChild(newUserInput);
            newUserInput.Owner = this;

            EmitSignal(SignalName.ChildEnteredTree);
        }

        private void CreateResponse(string message)
        {
            var randomChanceForLoadingNoise = _random.Next(20) > 16;
            if(randomChanceForLoadingNoise)
            {
                _ = _hardDriveSounds.PlayHardDriveSounds();
            }

            var commandResponse = new Label()
            {
                Name = $"Response",
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
                Name = $"ListDirectoryResponse",
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

        private List<KeyValuePair<string, string>> GetHardwareDeviceInfo(string deviceName)
        {
            var deviceDirectory = _directoryService.GetRootDirectory().FindDirectory("system").FindDirectory("device");
            return deviceDirectory.Entities
                .Where(entity => entity.IsDirectory && entity.Name.Equals(deviceName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(entity =>
                {
                    var hardwareEntities = entity.Entities
                        .Where(entity => !entity.IsDirectory)
                        .Select(hardware => new KeyValuePair<string, DirectoryEntity>(entity.Name, hardware));

                    return hardwareEntities.SelectMany(hwe =>
                    {
                        var hardwareInfoLines = hwe.Value.Contents.Split('\n');
                        return hardwareInfoLines.Select(hwel =>
                        {
                            var nameAndValue = hwel.Split(':');

                            if (nameAndValue.Length != 2)
                            {
                                return new KeyValuePair<string, string>();
                            }

                            return new KeyValuePair<string, string>(nameAndValue.First(), nameAndValue.Last());
                        });
                    });
                })
                .Where(x => x.Key != default && x.Value != default)
                .ToList();
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

            List<Tuple<string, string, string>> hardwareColumnsOutput =
            [
                new("Device", "Class", "Manufacturer"),
                new("------", "-----", "------------"),
                .. hardwareResponse.SelectMany(hwr =>
                {
                    var device = $"{hwr.Key}";
                    return hwr.Value.Select(hwri => new Tuple<string, string, string>(
                        device,
                        hwri.First(x => x.Contains("name:")).Split(':').Last().Trim(),
                        hwri.First(x => x.Contains("manufacturer:")).Split(':').Last().Trim()
                    ));
                }).ToList(),
            ];

            //var deviceList = hardwareResponse.Select(hwr => $"{hwr.Key}\n{string.Concat(hwr.Key.Select(hwrc => '-'))}\n{string.Join("\n\n", hwr.Value.Select(hwri => string.Join('\n', hwri)))}");
            CreateResponse(string.Join("\n", hardwareColumnsOutput.Select(hardwareOutputTuple => $"{hardwareOutputTuple.Item1,-15}{hardwareOutputTuple.Item2,-15}{hardwareOutputTuple.Item3}")));
        }

        private void NetworkCommandResponse()
        {
            var networkDirectory = _directoryService.GetRootDirectory().FindDirectory("system").FindDirectory("network");
            var networkResponse = networkDirectory.Entities.Where(entity => !entity.IsDirectory).Select(entity =>
            {
                var networkEntityList = entity.Contents.Split('\n');
                return new KeyValuePair<string, List<string>>(entity.Name, [.. networkEntityList]);
            }).ToList();

            List<Tuple<string, string, string, string>> networkColumnsOutput =
            [
                new("Name", "Device", "Address (ipv6)", "Address (ipv8)"),
                new("----", "------", "--------------", "--------------"),
                .. networkResponse.Select(nri =>
                {
                    var name = $"{nri.Key}";
                    var device = $"{nri.Value.First(value => value.Contains("device:")).Replace("device:", string.Empty).Trim()}";
                    var ipv6 = $"{nri.Value.First(value => value.Contains("ipv6:")).Replace("ipv6:", string.Empty).Trim()}";
                    var ipv8 = $"{nri.Value.First(value => value.Contains("ipv8:")).Replace("ipv8:", string.Empty).Trim()}";
                    return new Tuple<string, string, string, string>(name, device, ipv6, ipv8);
                }).ToList(),
            ];

            CreateResponse(string.Join("\n", networkColumnsOutput.Select(networkOutput => $"{networkOutput.Item1,-10}{networkOutput.Item2,-10}{networkOutput.Item3,-24}{networkOutput.Item4}")));
        }
    }
}