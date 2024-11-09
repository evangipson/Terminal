using System.Collections.Generic;
using System.Linq;

using Terminal.Enums;

namespace Terminal.Services
{
    /// <summary>
    /// A <see langword="static"/> service that manages evaluating user commands.
    /// </summary>
    public static class UserCommandService
    {
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
            _ => UserCommand.Unknown
        };

        /// <summary>
        /// A list of human-readable commands available to the user.
        /// <para>
        /// Mirrors all values of <see cref="UserCommand"/>.
        /// </para>
        /// </summary>
        /// <returns>
        /// A <see langword="string"/> representation of every user command.
        /// </returns>
        public static List<string> GetAllCommands() => new()
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
            "edit"
        };
    }
}
