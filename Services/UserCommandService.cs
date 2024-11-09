using System.Collections.Generic;
using System.Linq;

using Terminal.Enums;

namespace Terminal.Services
{
    public static class UserCommandService
    {
        public static UserCommand EvaluateUserInput(string userInput)
        {
            var tokens = ParseInputToTokens(userInput);
            var firstToken = tokens.FirstOrDefault()?.ToLowerInvariant();
            return EvaluateToken(firstToken);
        }

        public static List<string> ParseInputToTokens(string userInput) => userInput.Split(' ').ToList();

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
    }
}
