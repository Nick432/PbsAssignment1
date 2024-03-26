using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Libs.Terminal;
using Client.Structs;

namespace Client
{
	public class Command
	{
		public string name;
		public string shortcut;
		public string syntax = "";
		string? arg = "";
		Action? action;
		Action<string>? action_arg;



		public Command(string name, Action action, string args = "default")
		{
			this.name = name;
			this.action = action;
			this.shortcut = name.Replace(" ", "").ToLower();
			this.arg = args;
		}

		public Command(string name, Action<string> action, string args = "default")
		{
			this.name = name;
			this.action_arg = action;
			this.shortcut = name.Replace(" ", "").ToLower();
			this.arg = args;
		}

		public void Invoke()
		{
			if (this.action != null)
				this.action.Invoke();
		}

		public void Invoke(string arg)
		{
			if (this.action_arg != null)
				this.action_arg.Invoke(this.arg);
		}
	}
	public static class Commands
	{
		public static char commandChar = '/';
		// dictionary for commands in here

		static Command[] commands =
		{
			new Command("Help", Help),
			new Command("Change Room", ChangeRoom),
			new Command("Change Name", ChangeName)
		};

		public static Command? GetCommand(string command)
		{
			for (int i = 0; i < commands.Length; i++)
			{
				Command cmd = commands[i];
				if (cmd.shortcut == command)
				{
					return cmd;
				}
			}
			return null;
		}

		public static string[] GetArguments(string input)
		{
			return input.Split(" ").ToArray();
		}
		public static bool HandleCommandInput(string input)
		{
			bool isCommand = false;

			if ((char)input[0] == commandChar)
			{
				isCommand = true;
				
				string inputCommand = input[1..];

				string[] inputargs = inputCommand.Split(" ").ToArray();

				Command command = Commands.GetCommand(inputCommand);

				if (command != null)
				{
					command.Invoke(inputargs[1]);
				}
				else
				{
					ErrorMessage error = new ErrorMessage($"{inputCommand} is not recognized as a command");
					Terminal.Print(error.ToString());
				}
			}

			return isCommand;
		}


		public static void Help()
		{
			string helpText = "";
			for (int i = 0; i < commands.Length; i++)
			{
				helpText += $"\t{commandChar}{commands[i].shortcut} - {commands[i].name}\n";
			}

			Terminal.Print(new string('-', 5), "Chat Help", new string('-', 5), "\n", helpText);
		}

		public static void ChangeRoom(string room)
		{
			Terminal.Print("Change Room Command", room);

		}

		public static void ChangeName(string newName)
		{
			Terminal.Print("Change Name Command", newName);
		}

		public static void Exit()
		{
			Terminal.Print("Exit Command");
		}
	}
}
