using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Dictionary = Godot.Collections.Dictionary;

using Terminal.Constants;

namespace Terminal.Services
{
	public partial class PersistService : Node
	{
		public Color CurrentColor = ColorConstants.TerminalGreen;
		public SortedSet<string> CommandMemory = new();

		private readonly string _path = ProjectSettings.GlobalizePath("user://");
		private readonly string _fileName = "savegame.json";
		private readonly int _commandMemoryLimit = 10;

		public override void _Ready() => LoadGame();

		public void AddCommandToMemory(string command)
		{
			if(CommandMemory.Count > _commandMemoryLimit)
			{
				CommandMemory.Remove(CommandMemory.First());
				CommandMemory.Add(command);
				return;
			}

			CommandMemory.Add(command);
		}

		public void LoadGame()
		{
			var saveDataJson = LoadDataFromFile(_path, _fileName);
			if(string.IsNullOrEmpty(saveDataJson))
			{
				return;
			}

			Json jsonLoader = new();
			Error error = jsonLoader.Parse(saveDataJson);
			if(error != Error.Ok)
			{
				GD.PrintErr("Unable to parse save game.");
				return;
			}

			Dictionary loadedData = (Dictionary)jsonLoader.Data;
			CurrentColor = (Color)loadedData["TerminalColor"];
			CommandMemory = new SortedSet<string>(loadedData["CommandMemory"].ToString().Split(','));
		}

		public void SaveGame()
		{
			Dictionary saveData = new()
			{
				["TerminalColor"] = CurrentColor.ToHtml(false),
				["CommandMemory"] = string.Join(',', CommandMemory)
			};

			var saveDataJson = Json.Stringify(saveData);
			SaveDataToFile(_path, _fileName, saveDataJson);
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
