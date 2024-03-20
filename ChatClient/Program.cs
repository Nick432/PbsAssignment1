﻿using System;
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
		static Host host = new Host("localhost");

		static async Task<bool> ValidateHost()
		{
			bool valid = false;
			int attempt = 0;

			while (!valid && attempt < ClientConfig.MaxLoginAttempts)
			{
				string hostName = Terminal.Prompt("Enter a host (e.g. localhost):");

				host = new Host(hostName);

				host.validHost = await host.Validate();

				valid = host.validHost;

				if (!valid)
				{
					Terminal.Print($"Invalid username. Attempt: {attempt}/{ClientConfig.MaxLoginAttempts}");
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
					Terminal.Print($"Invalid username. Attempt: {attempt}/{ClientConfig.MaxLoginAttempts}");
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

				channel.ExchangeDeclare(exchange: "room", type: "topic");

				string queueName = channel.QueueDeclare().QueueName;
				channel.QueueBind(queue: channel.QueueDeclare().QueueName, exchange: "room", routingKey: "");

				EventingBasicConsumer consumer = new EventingBasicConsumer(channel);

				consumer.Received += (model, ea) =>
				{
					byte[] body = ea.Body.ToArray();
					string message = Encoding.UTF8.GetString(body);

					Terminal.Print(ea.RoutingKey, message);
				};

				channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

				Terminal.Print($"[*] Started listening to room topic. Your username is: {client.connection.user.username}");

				while (true)
				{
					string message = Terminal.Prompt("");

					if (message.ToLower() == "exit")
					{
						break;
					}

					byte[] messageBytes = Encoding.UTF8.GetBytes(message);
					channel.BasicPublish(exchange: "room", routingKey: "", basicProperties: null, body: messageBytes);
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

			if (validUser)
			{
				client = new Client(0, user.username, "localhost");
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