using System;
using System.Collections.Generic;
using System.Linq;

using Terminal.Constants;
using Terminal.Enums;

namespace Terminal.Services
{
    /// <summary>
    /// An evaluator that will evaluate user input and invoke a command.
    /// </summary>
    public class UserCommandEvaluator
    {
        /// <summary>
        /// An <see cref="Action"/> that will be invoked to create a simple response in the terminal.
        /// </summary>
        public event Action<string> SimpleMessageCommand;

        /// <summary>
        /// An <see cref="Action"/> that will be invoked to save progress.
        /// </summary>
        public event Action SaveProgressCommand;

        /// <summary>
        /// An <see cref="Action"/> that will be invoked to exit the terminal.
        /// </summary>
        public event Action ExitCommand;

        /// <summary>
        /// An <see cref="Action"/> that will be invoked to change the terminal color.
        /// </summary>
        public event Action<string> ColorChangeCommand;

        /// <summary>
        /// An <see cref="Action"/> that will be invoked to change the current directory.
        /// </summary>
        public event Action<string> ChangeDirectoryCommand;

        /// <summary>
        /// An <see cref="Action"/> that will be invoked to list the contents of the current directory.
        /// </summary>
        public event Action ListDirectoryCommand;

        /// <summary>
        /// An <see cref="Action"/> that will be invoked to view the contents of a file.
        /// </summary>
        public event Action<string> ViewFileCommand;

        /// <summary>
        /// An <see cref="Action"/> that will be invoked to create a new file.
        /// </summary>
        public event Action<string> MakeFileCommand;

        /// <summary>
        /// An <see cref="Action"/> that will be invoked to create a new folder.
        /// </summary>
        public event Action<string> MakeDirectoryCommand;

        /// <summary>
        /// An <see cref="Action"/> that will be invoked to launch the file editor and edit an existing file.
        /// </summary>
        public event Action<string> EditFileCommand;

        /// <summary>
        /// An <see cref="Action"/> that will be invoked to list the hardware of the system.
        /// </summary>
        public event Action ListHardwareCommand;

        private readonly List<UserCommand> _commandsThatNeedAdditionalArguments = new()
        {
            UserCommand.Color,
            UserCommand.ChangeDirectory,
            UserCommand.ViewFile,
            UserCommand.MakeFile,
            UserCommand.MakeDirectory,
            UserCommand.EditFile
        };

        /// <summary>
        /// Evalutes the provided <paramref name="command"/>, and invokes an <see langword="event"/> if it's successfully evaluated.
        /// </summary>
        /// <param name="command">
        /// The command to evaluate and invoke an <see langword="event"/> for.
        /// </param>
        public void EvaluateCommand(string command)
        {
            var parsedTokens = UserCommandService.ParseInputToTokens(command);
            var userCommand = UserCommandService.EvaluateUserInput(command);
            var unqualifiedCommand = _commandsThatNeedAdditionalArguments.Contains(userCommand) && parsedTokens.Count == 1;
            if (userCommand == UserCommand.Help || userCommand == UserCommand.Commands || unqualifiedCommand)
            {
                var helpContextToken = userCommand;
                if (parsedTokens.Count == 2 && UserCommandService.EvaluateToken(parsedTokens.Take(2).Last()) != UserCommand.Unknown)
                {
                    var parsedContextToken = UserCommandService.EvaluateToken(parsedTokens.Take(2).Last());
                    helpContextToken = parsedContextToken == UserCommand.Unknown ? UserCommand.Help : parsedContextToken;
                }

                SimpleMessageCommand?.Invoke(EvaluateHelpCommand(helpContextToken));
                return;
            }
            if (userCommand == UserCommand.ChangeDirectory)
            {
                ChangeDirectoryCommand?.Invoke(parsedTokens.Take(2).Last());
                return;
            }
            if (userCommand == UserCommand.ListDirectory)
            {
                ListDirectoryCommand?.Invoke();
                return;
            }
            if (userCommand == UserCommand.ViewFile)
            {
                ViewFileCommand?.Invoke(parsedTokens.Take(2).Last());
                return;
            }
            if (userCommand == UserCommand.MakeFile)
            {
                MakeFileCommand?.Invoke(parsedTokens.Take(2).Last());
                return;
            }
            if (userCommand == UserCommand.MakeDirectory)
            {
                MakeDirectoryCommand?.Invoke(parsedTokens.Take(2).Last());
                return;
            }
            if (userCommand == UserCommand.EditFile)
            {
                EditFileCommand?.Invoke(parsedTokens.Take(2).Last());
                return;
            }
            if (userCommand == UserCommand.ListHardware)
            {
                ListHardwareCommand?.Invoke();
                return;
            }
            if (userCommand == UserCommand.Color)
            {
                ColorChangeCommand?.Invoke(parsedTokens.Take(2).Last());
                return;
            }
            if (userCommand == UserCommand.Save)
            {
                SaveProgressCommand?.Invoke();
                return;
            }
            if (userCommand == UserCommand.Exit)
            {
                ExitCommand?.Invoke();
                return;
            }

            SimpleMessageCommand?.Invoke($"\"{parsedTokens.First()}\" is an unknown command. Use \"commands\" to get a list of available commands.");
        }

        private static string EvaluateHelpCommand(UserCommand? typeOfHelp = UserCommand.Help)
        {
            return typeOfHelp switch
            {
                UserCommand.Help => GetOutputFromTokens(new()
                {
                    ["TOPIC"] = "Terminal OS help system.",
                    ["REMARKS"] = "Displays help about Terminal OS commands.",
                    ["EXAMPLES"] = "help commands    : Display information about the terminal commands."
                }),
                UserCommand.Exit => GetOutputFromTokens(new()
                {
                    ["COMMAND"] = "exit",
                    ["REMARKS"] = "Exits Terminal OS.",
                }),
                UserCommand.Color => GetOutputFromTokens(new()
                {
                    ["COMMAND"] = "color",
                    ["REMARKS"] = "Changes the color of the terminal output.",
                    ["COLORS"] = string.Join(", ", ColorConstants.TerminalColors.Keys),
                    ["EXAMPLES"] = $"color {ColorConstants.TerminalColors.Keys.First()}    : Change the terminal output to {ColorConstants.TerminalColors.Keys.First()}."
                }),
                UserCommand.Save => GetOutputFromTokens(new()
                {
                    ["COMMAND"] = "save",
                    ["REMARKS"] = "Saves the state of the terminal."
                }),
                UserCommand.Commands => GetOutputFromTokens(new()
                {
                    ["COMMAND"] = "commands",
                    ["REMARKS"] = "Displays information about the terminal commands. Use help [command] to get more information about each command.",
                    ["COMMANDS"] = string.Join(", ", UserCommandService.GetAllCommands())
                }),
                UserCommand.ChangeDirectory => GetOutputFromTokens(new()
                {
                    ["COMMAND"] = "cd [change] [changedir]",
                    ["REMARKS"] = "Changes directory.",
                    ["EXAMPLES"] = $"cd ~    : Change directory to the default home directory for the current user."
                }),
                UserCommand.ListDirectory => GetOutputFromTokens(new()
                {
                    ["COMMAND"] = "ls [list]",
                    ["REMARKS"] = "Lists contents of a directory.",
                    ["EXAMPLES"] = "ls    : List the contents of the current directory."
                }),
                UserCommand.ViewFile => GetOutputFromTokens(new()
                {
                    ["COMMAND"] = "vw [view]",
                    ["REMARKS"] = "View the contents of a file.",
                    ["EXAMPLES"] = "view file.ext    : List the contents of the file.ext file."
                }),
                UserCommand.MakeFile => GetOutputFromTokens(new()
                {
                    ["COMMAND"] = "mf [makefile]",
                    ["REMARKS"] = "Make a file.",
                    ["EXAMPLES"] = "make new.txt    : Creates a blank file called 'new.txt' in the current directory."
                }),
                UserCommand.MakeDirectory => GetOutputFromTokens(new()
                {
                    ["COMMAND"] = "md [makedir] [makedirectory]",
                    ["REMARKS"] = "Make a directory.",
                    ["EXAMPLES"] = "md newdir    : Creates an empty directory called 'newdir' in the current directory."
                }),
                UserCommand.EditFile => GetOutputFromTokens(new()
                {
                    ["COMMAND"] = "edit",
                    ["REMARKS"] = "Edit a file.",
                    ["EXAMPLES"] = "edit new.txt    : Edits the 'new.txt' file in the current directory."
                }),
                UserCommand.ListHardware => GetOutputFromTokens(new()
                {
                    ["COMMAND"] = "lhw [listhardware]",
                    ["REMARKS"] = "View a list of hardware for the system.",
                    ["EXAMPLES"] = "lhw    : List all the hardware for the system."
                }),
                _ => string.Empty
            };
        }
        private static string GetOutputFromTokens(Dictionary<string, string> outputTokens) => string.Join('\n', outputTokens.Select(token =>
        {
            return string.Join('\n', token.Key, $"    {token.Value}");
        }));
    }
}
