using System;
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
			if (tokens.SingleOrDefault(token => token.Equals("help", StringComparison.OrdinalIgnoreCase)) != null)
			{
				return UserCommand.Help;
			}

			if (tokens.SingleOrDefault(token => token.Equals("color", StringComparison.OrdinalIgnoreCase)) != null)
			{
				return UserCommand.Color;
			}

			if (tokens.SingleOrDefault(token => token.Equals("save", StringComparison.OrdinalIgnoreCase)) != null)
			{
				return UserCommand.Save;
			}

			if (tokens.SingleOrDefault(token => token.Equals("commands", StringComparison.OrdinalIgnoreCase)) != null)
			{
				return UserCommand.Commands;
			}

			if (tokens.SingleOrDefault(token => token.Equals("exit", StringComparison.OrdinalIgnoreCase)) != null)
			{
				return UserCommand.Exit;
			}

			return UserCommand.Unknown;
		}

		public static List<string> ParseInputToTokens(string userInput)
		{
			return userInput.Split(' ').ToList();
		}
	}
}
