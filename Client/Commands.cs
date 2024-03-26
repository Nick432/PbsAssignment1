using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Libs.Terminal;

namespace Client
{
	public static class Commands
	{
		public static char commandChar = '/';
		// dictionary for commands in here
		
		public static bool HandleCommandInput(string input)
		{
			bool isCommand = false;

			if ((char)input[0] == commandChar)
			{
				isCommand = true;
				Terminal.Print(input);
			}

			return isCommand;
		}


		public static void Help()
		{

		}

		public static void ChangeRoom()
		{

		}

		public static void ChangeName()
		{

		}

		public static void Exit()
		{

		}
	}
}
