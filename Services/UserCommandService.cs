using System.Collections.Generic;
using System.Linq;

using Terminal.Constants;
using Terminal.Enums;

namespace Terminal.Services
{
    /// <summary>
    /// A <see langword="static"/> service that manages evaluating user commands.
    /// </summary>
    public static class UserCommandService
    {
        /// <summary>
        /// Used to list all available commands in <see cref="GetAllCommands"/>.
        /// <para>
        /// Mirrors all values of <see cref="UserCommand"/>.
        /// </para>
        /// </summary>
        private static readonly List<string> _allCommandKeys = new()
        {
            "exit",
            "help",
            "color",
            "save",
            "commands",
            "list",
            "change",
            "view",
            "makefile",
            "makedirectory",
            "edit",
            "listhardware",
            "viewpermissions",
            "changepermissions"
        };

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
        public static Dictionary<string, Dictionary<string, string>> GetAllCommands() => new()
        {
            ["exit"] = new()
            {
                ["COMMAND"] = "exit",
                ["REMARKS"] = "Exits Terminal OS.",
            },
            ["help"] = new()
            {
                ["COMMAND"] = "help",
                ["REMARKS"] = "Displays help about Terminal OS commands.",
                ["EXAMPLES"] = "help commands    : Display information about the terminal commands."
            },
            ["color"] = new()
            {
                ["COMMAND"] = "color",
                ["REMARKS"] = "Changes the color of the terminal output.",
                ["COLORS"] = string.Join(", ", ColorConstants.TerminalColors.Keys),
                ["EXAMPLES"] = $"color {ColorConstants.TerminalColors.Keys.First()}    : Change the terminal output to {ColorConstants.TerminalColors.Keys.First()}."
            },
            ["save"] = new()
            {
                ["COMMAND"] = "save",
                ["REMARKS"] = "Saves the state of the terminal."
            },
            ["commands"] = new()
            {
                ["COMMAND"] = "commands",
                ["REMARKS"] = "Displays information about the terminal commands. Use help [command] to get more information about each command.",
                ["COMMANDS"] = string.Join(", ", _allCommandKeys)
            },
            ["list"] = new()
            {
                ["COMMAND"] = "ls [list]",
                ["REMARKS"] = "Lists contents of a directory."
            },
            ["change"] = new()
            {
                ["COMMAND"] = "cd [change] [changedir]",
                ["REMARKS"] = "Changes directory.",
                ["EXAMPLES"] = $"cd ~    : Change directory to the default home directory for the current user."
            },
            ["view"] = new()
            {
                ["COMMAND"] = "vw [view]",
                ["REMARKS"] = "View the contents of a file.",
                ["EXAMPLES"] = "view file.ext    : List the contents of the file.ext file."
            },
            ["makefile"] = new()
            {
                ["COMMAND"] = "mf [makefile]",
                ["REMARKS"] = "Make a file.",
                ["EXAMPLES"] = "mf new.txt    : Creates a blank file called 'new.txt' in the current directory."
            },
            ["makedirectory"] = new()
            {
                ["COMMAND"] = "md [makedir] [makedirectory]",
                ["REMARKS"] = "Make a directory.",
                ["EXAMPLES"] = "md newdir    : Creates an empty directory called 'newdir' in the current directory."
            },
            ["edit"] = new()
            {
                ["COMMAND"] = "edit",
                ["REMARKS"] = "Edit a file.",
                ["EXAMPLES"] = "edit new.txt    : Edits the 'new.txt' file in the current directory."
            },
            ["listhardware"] = new()
            {
                ["COMMAND"] = "lhw [listhardware]",
                ["REMARKS"] = "View a list of hardware for the system."
            },
            ["viewpermissions"] = new()
            {
                ["COMMAND"] = "vp [viewperm] [viewpermissions]",
                ["REMARKS"] = "View the permissions of a file or directory.",
                ["FORMAT"] = "Permission sets are 6 bits in order: \"admin executable\", \"admin write\", \"admin read\", \"user executable\", \"user write\", and \"user read\". If no bits are set, the permissions are \"none\".",
                ["EXAMPLE SETS"] = "111111: \"admin executable\", \"admin write\", \"admin read\", \"user executable\", \"user write\", and \"user read\".\n000000: \"none\".",
                ["EXAMPLES"] = "vp new.txt    : Shows the permissions for the 'new.txt' file in the current directory."
            },
            ["changepermissions"] = new()
            {
                ["COMMAND"] = "chp [changeperm] [changepermissions]",
                ["REMARKS"] = "Changes the permissions of a file or directory.",
                ["FORMAT"] = "Permission sets are 6 bits in order: \"admin executable\", \"admin write\", \"admin read\", \"user executable\", \"user write\", and \"user read\". If no bits are set, the permissions are \"none\".",
                ["EXAMPLE SETS"] = "111111: \"admin executable\", \"admin write\", \"admin read\", \"user executable\", \"user write\", and \"user read\".\n000000: \"none\".",
                ["EXAMPLES"] = "chp new.txt 010100    : Updates the permissions for the 'new.txt' file to \"admin write\" and \"user executable\"."
            },
            ["date"] = new()
            {
                ["COMMAND"] = "date",
                ["REMARKS"] = "View the current date.",
                ["EXAMPLES"] = "date    : Prints the current date."
            },
            ["time"] = new()
            {
                ["COMMAND"] = "time",
                ["REMARKS"] = "View the current time.",
                ["EXAMPLES"] = "time    : Prints the current time."
            },
            ["now"] = new()
            {
                ["COMMAND"] = "now [dt] [datetime] [current]",
                ["REMARKS"] = "View the current date and time.",
                ["EXAMPLES"] = "now    : Prints the current date and time."
            }
        };
    }
}
