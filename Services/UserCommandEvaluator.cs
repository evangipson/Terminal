using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// An <see cref="Action"/> that will be invoked to view the permissions of an entity.
        /// </summary>
        public event Action<string> ViewPermissionsCommand;

        /// <summary>
        /// An <see cref="Action"/> that will be invoked to change the permissions of an entity.
        /// </summary>
        public event Action<string, string> ChangePermissionsCommand;

        private readonly List<UserCommand> _commandsThatNeedAdditionalArguments = new()
        {
            UserCommand.Color,
            UserCommand.ChangeDirectory,
            UserCommand.ViewFile,
            UserCommand.MakeFile,
            UserCommand.MakeDirectory,
            UserCommand.EditFile,
            UserCommand.ViewPermissions,
            UserCommand.ChangePermissions
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
            if (userCommand == UserCommand.ViewPermissions)
            {
                ViewPermissionsCommand?.Invoke(parsedTokens.Take(2).Last());
                return;
            }
            if (userCommand == UserCommand.ChangePermissions)
            {
                var fileNameAndPermissionSet = parsedTokens.Skip(1).Take(2);
                ChangePermissionsCommand?.Invoke(fileNameAndPermissionSet.FirstOrDefault(), fileNameAndPermissionSet.LastOrDefault());
                return;
            }
            if (userCommand == UserCommand.Date)
            {
                SimpleMessageCommand?.Invoke(DateTime.UtcNow.AddYears(250).ToLongDateString());
                return;
            }
            if (userCommand == UserCommand.Time)
            {
                SimpleMessageCommand?.Invoke(DateTime.UtcNow.AddYears(250).ToLongTimeString());
                return;
            }
            if (userCommand == UserCommand.Now)
            {
                var now = DateTime.UtcNow.AddYears(250);
                SimpleMessageCommand?.Invoke(string.Join(", ", now.ToLongTimeString(), now.ToLongDateString()));
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
            var allCommands = UserCommandService.GetAllCommands();
            return typeOfHelp switch
            {
                UserCommand.Help => GetOutputFromTokens(allCommands["help"]),
                UserCommand.Exit => GetOutputFromTokens(allCommands["exit"]),
                UserCommand.Color => GetOutputFromTokens(allCommands["color"]),
                UserCommand.Save => GetOutputFromTokens(allCommands["save"]),
                UserCommand.Commands => GetOutputFromTokens(allCommands["commands"]),
                UserCommand.ChangeDirectory => GetOutputFromTokens(allCommands["change"]),
                UserCommand.ListDirectory => GetOutputFromTokens(allCommands["list"]),
                UserCommand.ViewFile => GetOutputFromTokens(allCommands["view"]),
                UserCommand.MakeFile => GetOutputFromTokens(allCommands["makefile"]),
                UserCommand.MakeDirectory => GetOutputFromTokens(allCommands["makedirectory"]),
                UserCommand.EditFile => GetOutputFromTokens(allCommands["edit"]),
                UserCommand.ListHardware => GetOutputFromTokens(allCommands["listhardware"]),
                UserCommand.ViewPermissions => GetOutputFromTokens(allCommands["viewpermissions"]),
                UserCommand.ChangePermissions => GetOutputFromTokens(allCommands["changepermissions"]),
                UserCommand.Date => GetOutputFromTokens(allCommands["date"]),
                UserCommand.Time => GetOutputFromTokens(allCommands["time"]),
                UserCommand.Now => GetOutputFromTokens(allCommands["now"]),
                _ => string.Empty
            };
        }

        private static string GetOutputFromTokens(Dictionary<string, string> outputTokens) => $"\n{string.Join("\n\n", outputTokens.Select(token => string.Join('\n', token.Key, token.Value)))}\n\n";
    }
}
