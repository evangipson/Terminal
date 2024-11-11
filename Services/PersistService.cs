using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using Dictionary = Godot.Collections.Dictionary;

using Terminal.Constants;
using Terminal.Models;

namespace Terminal.Services
{
    /// <summary>
    /// A global singleton that is responsible for saving and loading the game.
    /// <para>
    /// Also provides access to the <see cref="CurrentColor"/>, <see cref="ExecutableColor"/>,
    /// and <see cref="CommandMemory"/>.
    /// </para>
    /// </summary>
    public partial class PersistService : Node
    {
        /// <summary>
        /// The current color of the console.
        /// <para>
        /// Defaults to <see cref="ColorConstants.TerminalGreen"/>.
        /// </para>
        /// </summary>
        public Color CurrentColor = ColorConstants.TerminalGreen;

        /// <summary>
        /// The current color of executable files for the console.
        /// <para>
        /// Defaults to <see cref="ColorConstants.TerminalTeal"/>.
        /// </para>
        /// </summary>
        public Color ExecutableColor = ColorConstants.TerminalTeal;

        /// <summary>
        /// A list of previous user commands.
        /// </summary>
        public LinkedList<string> CommandMemory = new();

        private readonly string _path = ProjectSettings.GlobalizePath("user://");
        private readonly string _fileName = "savegame.json";
        private readonly int _commandMemoryLimit = 10;

        private DirectoryService _directoryService;

        public override void _Ready()
        {
            _directoryService = GetNode<DirectoryService>(ServicePathConstants.DirectoryServicePath);
            LoadGame();
        }

        /// <summary>
        /// Adds the provided <paramref name="command"/> to the <see cref="CommandMemory"/>.
        /// <para>
        /// Retires the oldest command if <see cref="CommandMemory"/> is full.
        /// </para>
        /// </summary>
        /// <param name="command">
        /// The command to add to <see cref="CommandMemory"/>
        /// </param>
        public void AddCommandToMemory(string command)
        {
            if (CommandMemory.Count > _commandMemoryLimit)
            {
                CommandMemory.RemoveFirst();
                CommandMemory.AddLast(command);
                return;
            }

            CommandMemory.AddLast(command);
        }

        /// <summary>
        /// Saves the game state to a JSON file.
        /// </summary>
        public void SaveGame()
        {
            Dictionary saveData = new()
            {
                ["TerminalColor"] = CurrentColor.ToHtml(false),
                ["ExecutableColor"] = ExecutableColor.ToHtml(false),
                ["CommandMemory"] = string.Join(',', CommandMemory),
                ["FileSystem"] = JsonSerializer.Serialize(_directoryService.FileSystem)
            };

            var saveDataJson = Json.Stringify(saveData);
            SaveDataToFile(_path, _fileName, saveDataJson);
        }

        private void LoadGame()
        {
            var saveDataJson = LoadDataFromFile(_path, _fileName);
            if (string.IsNullOrEmpty(saveDataJson))
            {
                return;
            }

            Json jsonLoader = new();
            Error error = jsonLoader.Parse(saveDataJson);
            if (error != Error.Ok)
            {
                GD.PrintErr("Unable to parse save game.");
                return;
            }

            Dictionary loadedData = (Dictionary)jsonLoader?.Data;
            if (loadedData?.ContainsKey("TerminalColor") == true)
            {
                CurrentColor = (Color)loadedData["TerminalColor"];
            }
            if (loadedData?.ContainsKey("ExecutableColor") == true)
            {
                ExecutableColor = (Color)loadedData["ExecutableColor"];
            }
            if (loadedData?.ContainsKey("CommandMemory") == true && !string.IsNullOrEmpty(loadedData["CommandMemory"].ToString()))
            {
                CommandMemory = new LinkedList<string>(loadedData["CommandMemory"].ToString().Split(','));
            }
            if (loadedData?.ContainsKey("FileSystem") == true && !string.IsNullOrEmpty(loadedData["FileSystem"].ToString()))
            {
                _directoryService.FileSystem = JsonSerializer.Deserialize<FileSystem>(loadedData["FileSystem"].ToString());
            }
        }

        private static void SaveDataToFile(string path, string fileName, string data)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var saveFileLocation = Path.Join(path, fileName);
            try
            {
                File.WriteAllText(saveFileLocation, data);
            }
            catch (Exception exception)
            {
                GD.PrintErr("Could not write save game file: ", exception.Message);
            }
        }

        private static string LoadDataFromFile(string path, string fileName)
        {
            if (!Directory.Exists(path))
            {
                return string.Empty;
            }

            var saveFileLocation = Path.Join(path, fileName);
            try
            {
                return File.ReadAllText(saveFileLocation);
            }
            catch (Exception exception)
            {
                GD.PrintErr("Could not read save game file: ", exception.Message);
                return string.Empty;
            }
        }
    }
}
