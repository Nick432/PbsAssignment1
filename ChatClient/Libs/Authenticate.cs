using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Libs.Terminal;
using Client.UserData;

namespace Client.Program
{
	public static class Authenticate
	{
		public static async Task<bool> ValidateUser()
		{
			int attempt = 0;
			bool valid = false;

			while (!valid && attempt < ClientConfig.MaxLoginAttempts)
			{
				string Username = Terminal.Prompt("Enter your username:");

				Program.user = new User(Username, "NULL");
				Terminal.Print($"Authenticating...");
				Program.user.validUser = await Program.user.Validate();

				if (Program.user.validUser)
				{
					valid = true;
				}
				else
				{
					attempt++;
				}
			}

			if (!valid)
			{
				return false;
			}
			Terminal.Print($"User {Program.user.username} authenticated.");
			return true;
		}

		public static async Task<bool> ValidateHost()
		{
			bool valid = false;
			int attempt = 0;

			while (!valid && attempt < ClientConfig.MaxLoginAttempts)
			{
				string hostName = "";
				int port = 0;
				string input = Terminal.Prompt("Enter a host (e.g. localhost):");

				if (input == "")
				{
					hostName = Client.defaultHost;
					port = Client.defaultPort;
				}
				else
				{
					string[] subString = input.Split(':');
					hostName = subString[0];
					port = int.Parse(subString[1]);

					if (port == 0)
					{
						port = Client.defaultPort;
					}
				}

				Program.host = new Host(hostName, port);

				Terminal.Print($"Connecting to {Program.host.host}:{Program.host.port}...");
				Program.host.validHost = await Program.host.Validate();

				valid = Program.host.validHost;

				if (!valid)
				{
					Terminal.Print($"Invalid host. Attempt: {attempt + 1}/{ClientConfig.MaxLoginAttempts}");
					attempt++;
				}
				else
				{
					break;
				}
			}
			return Program.host.validHost;
		}
	}
}
