using System;
using System.Text;
using System.Threading.Tasks;
using Libs.Terminal;
using Client.Structs;

namespace Client.Program
{
	public static class EventListener
	{
		// to-do: async event handling to receive events from client->OnMessageReceived, so we can pass data to GUI
		public static async Task Listen(Client client)
		{
			while (true)
			{
				bool eventHandled = false;

				TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
				Program.client.OnMessageReceived += (sender, args) =>
				{
					if (!eventHandled)
					{
						eventHandled = HandleEvent(sender, args);
					}
				};

				await Task.Delay(1000);
			}
		}

		static bool HandleEvent(object sender, EventArgs args)
		{
			Terminal.Print(sender + " " + args);

			return true;
		}
	}
}
