using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

using Terminal.Constants;
using Terminal.Enums;
using Terminal.Extensions;
using Terminal.Models;

namespace Terminal.Services
{
    /// <summary>
    /// A singleton service that manages evaluating user commands.
    /// </summary>
    public partial class UserCommandService : Node
    {
        private DirectoryService _directoryService;

        public override void _Ready()
        {
            _directoryService = GetNode<DirectoryService>(ServicePathConstants.DirectoryServicePath);
            UpdateSystemCommandsFileWithAllCommands();
        }

        private List<DirectoryEntity> AllCommandFiles => _directoryService.GetRootDirectory()
            .FindDirectory("system/programs").Entities
            .Where(entity => !entity.IsDirectory && (entity.Permissions.Contains(Permission.UserExecutable) || entity.Permissions.Contains(Permission.AdminExecutable)))
            .ToList();

        /// <summary>
        /// Parses the provided <paramref name="userInput"/> into tokens, and evalutes the first one.
        /// </summary>
        /// <param name="userInput">
        /// The user input to evaluate.
        /// </param>
        /// <returns>
        /// The first command from the provided <paramref name="userInput"/>.
        /// </returns>
        public static UserCommand EvaluateUserInput(string userInput)
        {
            var tokens = ParseInputToTokens(userInput);
            var firstToken = tokens.FirstOrDefault()?.ToLowerInvariant();
            return EvaluateToken(firstToken);
        }

        /// <summary>
        /// Takes a user input and splits it into a collection by spaces.
        /// </summary>
        /// <param name="userInput">
        /// The user input to parse into tokens.
        /// </param>
        /// <returns>
        /// A list of <see langword="string"/> tokens from the provided <paramref name="userInput"/>.
        /// </returns>
        public static List<string> ParseInputToTokens(string userInput) => userInput.Split(' ').ToList();

        /// <summary>
        /// Evaluates the provided <paramref name="token"/> and returns a <see cref="UserCommand"/>.
        /// </summary>
        /// <param name="token">
        /// The <see langword="string"/> to get a <see cref="UserCommand"/> from.
        /// </param>
        /// <returns>
        /// A <see cref="UserCommand"/> based off the provided <paramref name="token"/>.
        /// </returns>
        public static UserCommand EvaluateToken(string token) => token switch
        {
            "help" => UserCommand.Help,
            "color" => UserCommand.Color,
            "save" => UserCommand.Save,
            "commands" => UserCommand.Commands,
            "exit" => UserCommand.Exit,
            "ls" or "list" or "listdir" => UserCommand.ListDirectory,
            "cd" or "change" or "changedir" => UserCommand.ChangeDirectory,
            "vw" or "view" => UserCommand.ViewFile,
            "mf" or "makefile" => UserCommand.MakeFile,
            "md" or "makedirectory" or "makedir" => UserCommand.MakeDirectory,
            "edit" => UserCommand.EditFile,
            "lhw" or "listhardware" => UserCommand.ListHardware,
            "vp" or "viewperm" or "viewpermissions" => UserCommand.ViewPermissions,
            "chp" or "changeperm" or "changepermissions" => UserCommand.ChangePermissions,
            "date" => UserCommand.Date,
            "time" => UserCommand.Time,
            "now" or "datetime" or "dt" or "current" => UserCommand.Now,
            "network" or "net" => UserCommand.Network,
            _ => UserCommand.Unknown
        };

        /// <summary>
        /// Gets all user commands supplemented with additional information about each command.
        /// <para>
        /// Mirrors all values of <see cref="UserCommand"/>.
        /// </para>
        /// </summary>
        /// <returns>
        /// A collection of key value pairs, where the key is a <see langword="string"/> representation
        /// of a user command, and the value is a key value pair of information about that command.
        /// </returns>
        public Dictionary<string, Dictionary<string, string>> AllCommands
        {
            get
            {
                var parsedCommandFiles = AllCommandFiles.Select(command =>
                {
                    var helpLines = command.Contents.Split('\n');
                    var parsedHelpLines = helpLines.Select(line => line.TrimStart('[').TrimEnd(']'));
                    var separatedLines = parsedHelpLines.Select(line =>
                    {
                        var keyValuePair = line.Split(':', StringSplitOptions.RemoveEmptyEntries);
                        if (keyValuePair.Length != 2)
                        {
                            return default;
                        }

                        return new KeyValuePair<string, string>(line.Split(':').First(), line.Split(':').Last());
                    }).Where(line => line.Key != default && line.Value != default);

                    return new KeyValuePair<string, Dictionary<string, string>>(command.Name, new Dictionary<string, string>(separatedLines));
                });

                return new Dictionary<string, Dictionary<string, string>>(parsedCommandFiles);
            }
        }

        private void UpdateSystemCommandsFileWithAllCommands()
        {
            var commandsExecutableFile = _directoryService.GetRootDirectory().FindDirectory("system/programs").FindFile("commands");
            if (commandsExecutableFile == null)
            {
                GD.Print("Could not find the \"/system/programs/commands\" executable file.");
                return;
            }

            var newCommandList = commandsExecutableFile.Contents.Replace("$$$$", string.Join(", ", AllCommandFiles.Select(command => command.Name)));
            commandsExecutableFile.Contents = newCommandList;
        }
    }
}
