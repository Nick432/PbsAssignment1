using System;
using System.Text;
using Libs.Terminal;
using Client;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace Client.Program
{
	class Program
	{
		static Client client = new Client(0, "NULL");
		static User user = new User("NULL", "NULL");
		static Host host = new Host("localhost", 15672);

		static async Task<bool> ValidateHost()
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

				host = new Host(hostName, port);

				host.validHost = await host.Validate();

				valid = host.validHost;

				if (!valid)
				{
					Terminal.Print($"Invalid host. Attempt: {attempt+1}/{ClientConfig.MaxLoginAttempts}");
					attempt++;
				}
				else
				{
					break;
				}
			}
			return host.validHost;
		}

		static async Task<bool> ValidateUser()
		{
			int attempt = 0;
			bool valid = false;

			while (!valid && attempt < ClientConfig.MaxLoginAttempts)
			{
				string Username = Terminal.Prompt("Enter your username:");

				user = new User(Username, "NULL");

				user.validUser = await user.Validate();

				if (user.validUser)
				{
					valid = true;
				}
				else
				{
					Terminal.Print($"Invalid username. Attempt: {attempt+1}/{ClientConfig.MaxLoginAttempts}");
					attempt++;
				}
			}

			if (!valid)
			{
				return false;
			}

			return true;
		}

		static void Connect()
		{
			Terminal.Print("Connecting...");

			using (IModel? channel = client.connection.Channel())
			{
				if (channel == null)
				{
					return;
				}

				channel.ExchangeDeclare(exchange: "room", type: "topic", durable: true);

				EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
				string queueName = channel.QueueDeclare().QueueName;

				channel.DefaultConsumer = consumer;
				channel.QueueBind(queue: channel.QueueDeclare().QueueName, exchange: "room", routingKey: "");

				Terminal.Print($"[*] Started listening to room topic. Your username is: {client.connection.user.username}");

				while (channel.IsOpen)
				{

					consumer.Received += (model, ea) =>
					{
						byte[] body = ea.Body.ToArray();
						string message = Encoding.UTF8.GetString(body);

						Message receive = new Message(body);

						Terminal.Print(ea.RoutingKey, message);
						
					};
					channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);

					string message = Terminal.Prompt($"{user.username}: \b");

					if (message.ToLower() == "exit")
					{
						break;
					}

					Message chatMessage = new Message(message, Encoding.UTF8, user);

					channel.BasicPublish(exchange: string.Empty,
					 routingKey: "hello",
					 basicProperties: null,
					 body: chatMessage.Encoded());

					//Client.SendMessage(host, chatMessage);
					
				}
			}
		}

		public static async Task Initialize()
		{
			bool validHost = false;
			bool validUser = false;

			while (!validHost)
			{
				validHost = await ValidateHost(); 
			}

			while (!validUser)
			{
				validUser = await ValidateUser();
			}

			if (validUser && validHost)
			{
				client = new Client(0, user.username, host.host);
				Connect();
			}
		}
		public static int Main(string[] args)
		{
			Initialize().GetAwaiter().GetResult();

			return (0);
		}
	}
}