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
			var userCommand = firstToken switch
			{
				"help" => UserCommand.Help,
				"color" => UserCommand.Color,
				"save" => UserCommand.Save,
				"commands" => UserCommand.Commands,
				"exit" => UserCommand.Exit,
				_ => UserCommand.Unknown
			};

			return userCommand;
		}

		public static List<string> ParseInputToTokens(string userInput) => userInput.Split(' ').ToList();
	}
}
