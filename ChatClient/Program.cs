using System;
using System.Text;
using System.Threading.Tasks;
using Libs.Terminal;
using Client;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using EasyNetQ;
using EasyNetQ.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using EasyNetQ.DI;
using Client.UserData;

namespace Client.Program
{
	class Program
	{
		static bool debug = true;

		public static User user = new User("NULL", "NULL");
		public static Host host = new Host("localhost", 15672);
		public static Client client = new Client(user, host);

		public static async Task Connect()
		{
			Task listen;
			Task connect;

			host = new Host(Client.defaultHost, Client.defaultPort);

			client = new Client(user, host);

			listen = client.Listen();
			connect = client.Connect();

			await Task.WhenAll(listen, connect);
		}

		public static async Task Initialize()
		{


			if (debug)
			{
				await Authenticate.ValidateUser();

				await Connect();
			}
			else
			{
				bool validHost = false;
				bool validUser = false;

				while (!validHost)
				{
					validHost = await Authenticate.ValidateHost(); 
				}

				while (!validUser)
				{
					//validUser = await ValidateUser();
				}

				if (validUser && validHost)
				{
					client = new Client(user, host);

					await Connect();
				}
			}
		}

		public static int Main(string[] args)
		{
			// As we are running this task async we need to await the final result of all subsequent async tasks.
			Initialize().GetAwaiter().GetResult();
			return (0);
		}
	}

	
}
