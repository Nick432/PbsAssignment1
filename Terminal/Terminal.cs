using System;

namespace Libs.Terminal
{
	public static class Terminal
	{
		public static void Print(string message)
		{
			Console.WriteLine(message);
		}

		public static void Write(string message)
		{
			Console.Write(message);
		}

		public static void Print(params object[] args)
		{
			string output = "";

			for (int i = 0; i < args.Length; i++)
			{
				output += args[i].ToString();
			}

			Print(output);
		}

		public static string ReadLine()
		{
			string? input = "";

			input = Console.ReadLine();

			if (input == null)
				input = "";

			return input;
		}

		public static string Prompt(string message)
		{
			Console.Write(message);
			string? input = "";

			input = Console.ReadLine();

			if (input == null)
				input = "";
			return input;
		}
	}
}