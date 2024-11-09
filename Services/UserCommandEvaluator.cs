using System;
using System.Collections.Generic;
using System.Linq;

using Terminal.Constants;
using Terminal.Enums;

namespace Terminal.Services
{
    public class UserCommandEvaluator
    {
        public event Action<string> SimpleMessageCommand;
        public event Action SaveProgressCommand;
        public event Action ExitCommand;
        public event Action<string> ColorChangeCommand;
        public event Action<string> ChangeDirectoryCommand;
        public event Action ListDirectoryCommand;
        public event Action<string> ViewFileCommand;
		public event Action<string> MakeFileCommand;
		public event Action<string> MakeDirectoryCommand;

		private readonly List<UserCommand> _commandsThatNeedAdditionalArguments = new()
		{
			UserCommand.Color,
			UserCommand.ChangeDirectory,
			UserCommand.ViewFile,
			UserCommand.MakeFile,
			UserCommand.MakeDirectory
		};

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
            if(userCommand == UserCommand.ChangeDirectory)
            {
                ChangeDirectoryCommand?.Invoke(parsedTokens.Take(2).Last());
                return;
            }
            if(userCommand == UserCommand.ListDirectory)
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
                    ["COMMANDS"] = "help, commands, ls, cd, view, exit, save, color"
                }),
                UserCommand.ChangeDirectory => GetOutputFromTokens(new()
                {
                    ["COMMAND"] = "cd",
                    ["REMARKS"] = "Changes directory.",
                    ["EXAMPLES"] = $"cd ~    : Change directory to the default home directory for the current user."
                }),
                UserCommand.ListDirectory => GetOutputFromTokens(new()
                {
                    ["COMMAND"] = "ls",
                    ["REMARKS"] = "Lists contents of a directory.",
                    ["EXAMPLES"] = "ls    : List the contents of the current directory."
                }),
                UserCommand.ViewFile => GetOutputFromTokens(new()
                {
                    ["COMMAND"] = "view",
                    ["REMARKS"] = "View the contents of a file.",
                    ["EXAMPLES"] = "view file.ext    : List the contents of the file.ext file."
                }),
				UserCommand.MakeFile => GetOutputFromTokens(new()
				{
					["COMMAND"] = "mf",
					["REMARKS"] = "Make a file.",
					["EXAMPLES"] = "make new.txt    : Creates a blank file called 'new.txt' in the current directory."
				}),
				UserCommand.MakeDirectory => GetOutputFromTokens(new()
				{
					["COMMAND"] = "md",
					["REMARKS"] = "Make a directory.",
					["EXAMPLES"] = "md newdir    : Creates an empty directory called 'newdir' in the current directory."
				}),
				_ => string.Empty
            };
        }
        private static string GetOutputFromTokens(Dictionary<string, string> outputTokens) => string.Join('\n', outputTokens.Select(token => string.Join('\n', token.Key, $"    {token.Value}")));
    }
}
