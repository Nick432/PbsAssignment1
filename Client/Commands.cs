using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Libs.Terminal;
using Client.Structs;
using Client.UserData;

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
		public int args = 0;
		public Type dataType;



		public Command(string name, Action action, string args = "default")
		{
			this.name = name;
			this.action = action;
			this.shortcut = name.Replace(" ", "").ToLower();
			this.arg = args;
		}

		public Command(string name, Action<string> action, int numArgs, Type type, string args = "default")
		{
			this.name = name;
			this.action_arg = action;
			this.shortcut = name.Replace(" ", "").ToLower();
			this.arg = args;
			this.args = numArgs;
			this.dataType = type;
		}

		public void Invoke()
		{
			if (this.action != null)
				this.action.Invoke();
		}

		public void Invoke(string arg)
		{
			if (this.action_arg != null)
				this.action_arg.Invoke(arg);
		}
	}
	public static class Commands
	{
		public static char commandChar = '/';
		// dictionary for commands in here

		static Command[] commands =
		{
			new Command("Help", Help),
			new Command("Change Room", ChangeRoom, 1, typeof(string)),
			new Command("Change Name", ChangeName, 1, typeof(string)),
			new Command("Exit", Exit)
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

			if (input == "")
				return false;

			if ((char)input[0] == commandChar)
			{				
				string inputCommand = input[1..];

				string[] inputargs = inputCommand.Split(" ").ToArray();

				Command command = Commands.GetCommand(inputargs[0]);

				if (command != null)
				{
					if (command.args == 0)
					{
						command.Invoke();
					}
					else
					{
						command.Invoke(inputargs[1]);
					}
					
					return true;
					
				}
				else
				{
					ErrorMessage error = new ErrorMessage($"{inputCommand} is not recognized as a command");
					Terminal.Print(error.ToString());
				}
			}

			return false;
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
			Client.user.SetChannel(new Channel(room));
			Terminal.Print("Changed Room to: ", room);
		}

		public static void ChangeName(string newName)
		{
			Client.user.ChangeNickname(newName);
			Terminal.Print("Changed nickname to: ", newName);
		}

		public static void Exit()
		{
			Terminal.Print("Exit Command");
			Client.user.Disconnect();
		}
	}
}
