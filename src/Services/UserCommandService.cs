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
    /// A global singleton that manages evaluating user commands.
    /// </summary>
    public partial class UserCommandService : Node
    {
        private DirectoryService _directoryService;

        public override void _Ready()
        {
            _directoryService = GetNode<DirectoryService>(ServicePathConstants.DirectoryServicePath);
        }

        private List<DirectoryEntity> AllCommandFiles
        {
            get
            {
                var commands = _directoryService.GetRootDirectory()
                    .FindDirectory("system/programs").Entities
                    .Where(entity => !entity.IsDirectory && (entity.Permissions.Contains(Permission.UserExecutable) || entity.Permissions.Contains(Permission.AdminExecutable)))
                    .ToList();

                UpdateSystemCommandsFileWithAllCommands(commands.Select(command => command.Name));
                return commands;
            }
        }

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
        public static List<string> ParseInputToTokens(string userInput) => [.. userInput.Split(' ')];

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
            "df" or "deletefile" => UserCommand.DeleteFile,
            "dd" or "deletedir" or "deletedirectory" => UserCommand.DeleteDirectory,
            "ping" => UserCommand.Ping,
            "mvf" or "movefile" => UserCommand.MoveFile,
            "mvd" or "movedir" or "movedirectory" => UserCommand.MoveDirectory,
            "mu" or "makeuser" => UserCommand.MakeUser,
            "du" or "deleteuser" => UserCommand.DeleteUser,
            "mg" or "makegroup" => UserCommand.MakeGroup,
            "dg" or "deletegroup" => UserCommand.DeleteGroup,
            "aug" or "adduser" or "addusertogroup" => UserCommand.AddUserToGroup,
            "dug" or "removeuser" or "deleteuserfromgroup" => UserCommand.DeleteUserFromGroup,
            "vg" or "viewgroup" => UserCommand.ViewGroup,
            "cls" or "clear" or "clearscreen" => UserCommand.ClearScreen,
            "deletesave" => UserCommand.DeleteSave,
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
                    var helpLines = command.Contents.Split(DirectoryConstants.HelpLineSeparator, StringSplitOptions.RemoveEmptyEntries);
                    var parsedHelpLines = helpLines.Select(line => line.Remove(line.Length - 1).Remove(0, 1));
                    var separatedLines = parsedHelpLines.Select(line =>
                    {
                        var keyValuePair = line.Split(DirectoryConstants.HelpKeyValueSeparator, StringSplitOptions.RemoveEmptyEntries);
                        if (keyValuePair.Length != 2)
                        {
                            return default;
                        }

                        return new KeyValuePair<string, string>(keyValuePair.First(), string.Concat(keyValuePair.Skip(1)));
                    }).Where(line => line.Key != default && line.Value != default);

                    return new KeyValuePair<string, Dictionary<string, string>>(command.Name, new Dictionary<string, string>(separatedLines));
                });

                return new Dictionary<string, Dictionary<string, string>>(parsedCommandFiles);
            }
        }

        /// <summary>
        /// Evalutes a <see cref="UserCommand.Help"/> command, and uses the provided <paramref name="typeOfHelp"/>
        /// and <paramref name="userHelpContext"/> to fully qualify what type of help command to display.
        /// </summary>
        /// <param name="typeOfHelp">
        /// An optional parsed second argument of the "help" command, such as "save" in the command <c>help <b>save</b></c>.
        /// </param>
        /// <param name="userHelpContext">
        /// An optional second argument of the "help" command for a user-generated program that could not be parsed, such
        /// as "userprogram" in the command <c>help <b>userprogram</b></c>.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> filled with helpful information about the provided <paramref name="typeOfHelp"/>
        /// or <paramref name="userHelpContext"/>.
        /// </returns>
        public string EvaluateHelpCommand(UserCommand typeOfHelp = UserCommand.Help, string userHelpContext = null)
        {
            if (!string.IsNullOrEmpty(userHelpContext) && AllCommands.TryGetValue(userHelpContext, out Dictionary<string, string> helpContext))
            {
                return GetOutputFromTokens(helpContext);
            }

            return typeOfHelp switch
            {
                UserCommand.Help => GetOutputFromTokens(AllCommands["help"]),
                UserCommand.Exit => GetOutputFromTokens(AllCommands["exit"]),
                UserCommand.Color => GetOutputFromTokens(AllCommands["color"]),
                UserCommand.Save => GetOutputFromTokens(AllCommands["save"]),
                UserCommand.Commands => GetOutputFromTokens(AllCommands["commands"]),
                UserCommand.ChangeDirectory => GetOutputFromTokens(AllCommands["change"]),
                UserCommand.ListDirectory => GetOutputFromTokens(AllCommands["list"]),
                UserCommand.ViewFile => GetOutputFromTokens(AllCommands["view"]),
                UserCommand.MakeFile => GetOutputFromTokens(AllCommands["makefile"]),
                UserCommand.MakeDirectory => GetOutputFromTokens(AllCommands["makedirectory"]),
                UserCommand.EditFile => GetOutputFromTokens(AllCommands["edit"]),
                UserCommand.ListHardware => GetOutputFromTokens(AllCommands["listhardware"]),
                UserCommand.ViewPermissions => GetOutputFromTokens(AllCommands["viewpermissions"]),
                UserCommand.ChangePermissions => GetOutputFromTokens(AllCommands["changepermissions"]),
                UserCommand.Date => GetOutputFromTokens(AllCommands["date"]),
                UserCommand.Time => GetOutputFromTokens(AllCommands["time"]),
                UserCommand.Now => GetOutputFromTokens(AllCommands["now"]),
                UserCommand.Network => GetOutputFromTokens(AllCommands["network"]),
                UserCommand.DeleteFile => GetOutputFromTokens(AllCommands["deletefile"]),
                UserCommand.DeleteDirectory => GetOutputFromTokens(AllCommands["deletedirectory"]),
                UserCommand.Ping => GetOutputFromTokens(AllCommands["ping"]),
                UserCommand.MoveFile => GetOutputFromTokens(AllCommands["movefile"]),
                UserCommand.MoveDirectory => GetOutputFromTokens(AllCommands["movedirectory"]),
                UserCommand.MakeUser => GetOutputFromTokens(AllCommands["makeuser"]),
                UserCommand.DeleteUser => GetOutputFromTokens(AllCommands["deleteuser"]),
                UserCommand.MakeGroup => GetOutputFromTokens(AllCommands["makegroup"]),
                UserCommand.DeleteGroup => GetOutputFromTokens(AllCommands["deletegroup"]),
                UserCommand.AddUserToGroup => GetOutputFromTokens(AllCommands["addusertogroup"]),
                UserCommand.DeleteUserFromGroup => GetOutputFromTokens(AllCommands["deleteuserfromgroup"]),
                UserCommand.ViewGroup => GetOutputFromTokens(AllCommands["viewgroup"]),
                UserCommand.ClearScreen => GetOutputFromTokens(AllCommands["clearscreen"]),
                UserCommand.DeleteSave => GetOutputFromTokens(AllCommands["deletesave"]),
                _ => string.Empty
            };
        }

        /// <summary>
        /// Gets the directory path and the prompt character, shown to the user before each command input.
        /// </summary>
        /// <returns>
        /// The current absolute directory with the <see cref="TerminalCharactersConstants.Prompt"/>, suffixed with a space.
        /// </returns>
        public string GetCommandPrompt() => $"{_directoryService.GetCurrentDirectoryPath()} {TerminalCharactersConstants.Prompt} ";

        private void UpdateSystemCommandsFileWithAllCommands(IEnumerable<string> newCommandNames)
        {
            var commandsExecutableFile = _directoryService.GetRootDirectory().FindDirectory("system/programs").FindFile("commands");
            if (commandsExecutableFile == null)
            {
                GD.Print("Could not find the \"/system/programs/commands\" executable file.");
                return;
            }

            var commandsContentsMinusReplacement = commandsExecutableFile.Contents.Split($"[COMMANDS{DirectoryConstants.HelpKeyValueSeparator}").First();
            var sortedCommands = string.Join(", ", newCommandNames.OrderBy(command => command).Select(command => command));
            commandsExecutableFile.Contents = string.Concat(commandsContentsMinusReplacement, DirectoryConstants.HelpLineSeparator, "[COMMANDS", DirectoryConstants.HelpKeyValueSeparator, sortedCommands, "]");
        }

        private static string GetOutputFromTokens(Dictionary<string, string> outputTokens) =>
            $"\n{string.Join("\n\n", outputTokens.Select(token => string.Join('\n', token.Key, token.Value)))}\n\n";
    }
}
