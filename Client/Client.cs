using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Libs.Terminal;
using Client.Structs;

namespace Client
{
	public class Connection
	{
		// Connection Variables
		public static ConnectionFactory c_Factory = new ConnectionFactory();

		public static IConnection? _iconnection;
		public static IModel? _ichannel;

		Host host;
		public User user;

		public Connection(Host host, User user)
		{
			this.host = host;
			this.user = user;

			Initialize();
		}

		void Initialize()
		{
			c_Factory = new ConnectionFactory()
			{
				HostName = this.host.host
			};
		}
	}

	public class Client
	{
		public static string apiUrl = "http://localhost:15672/api/users/";

		public static string defaultHost = "localhost";
		public static int defaultPort = 15672;
		
		User user;
		Host host;

		static ConnectionFactory factory = new ConnectionFactory();
		static IConnection? connection;
		static IModel? channel;

		public Client(User user, Host host)
		{
			this.user = new User(user.username, host.host);
			this.host = host;
		}

		public async Task Listen()
		{
			using (connection = factory.CreateConnection())
			using (channel = connection.CreateModel())
			{
				Terminal.Print($"Listening to [{this.host.host}]...");
				EventingBasicConsumer consumer = new EventingBasicConsumer(channel);

				consumer.Received += (model, ea) =>
				{
					Message message = Message.Receive(ea.Body.ToArray());

					if (message.channel.name == user.currentChannel.name)
					{
						Terminal.Print(message.FormatMessage());
					}
				};
				channel.BasicConsume(queue: "room", autoAck: true, consumer: consumer);
				await Task.Delay(-1);
			};
		}

		public async Task Connect()
		{
			factory = new ConnectionFactory() { HostName = this.host.host};

			using (connection = factory.CreateConnection())
			using (channel = connection.CreateModel())
			{
				channel.ExchangeDeclare(exchange: "direct_logs", type: "direct");

				string routingKey = "chatMessage";
				Channel messageRoom = new Channel("General");

				user.SetChannel(messageRoom);
				Message.Joined(user, channel, routingKey, messageRoom);

				while (true)
				{
					await Task.Delay(1000);
					Terminal.Write($"[{user.currentChannel.name}][{user.username}]: ");
					string chatMessage = Terminal.ReadLine();

					Message message = new Message(chatMessage, Encoding.UTF8, user, messageRoom);

					message.Send(channel, routingKey);
					
				}
			};

		}


	}
}