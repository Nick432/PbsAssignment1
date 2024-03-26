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

		public static string defaultExchange = "amq.fanout";
		public static string defaultExchangeType = "fanout";
		public static string defaultRoom = "room";
		public static string defaultQueue = string.Empty;
		public static bool defaultDurability = true;

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

		// Events


		public async Task Listen()
		{
			using (connection = factory.CreateConnection())
			using (channel = connection.CreateModel())
			{
				Terminal.Print($"Listening to [{this.host.host}]...");
				EventingBasicConsumer consumer = new EventingBasicConsumer(channel);

				user.currentQueue = channel.QueueDeclare().QueueName;

				channel.ExchangeDeclare(exchange: defaultExchange, type: defaultExchangeType, durable: defaultDurability);
				try
				{
					//channel.QueueDeclare(queue: defaultQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
					channel.QueueBind(queue: user.currentQueue, exchange: defaultExchange, routingKey: "");
				}
				catch
				{
					ErrorMessage error = new ErrorMessage($"Error binding queue: {defaultQueue}: declaring new queue...");
					Terminal.Print(error.ToString());

					
				}
				

				consumer.Received += (model, ea) =>
				{
					Message message = Message.Receive(ea.Body.ToArray());

					if (message.sender.username != user.username)
					{
						Terminal.EraseLine();
						Terminal.Print(message.FormatMessage());

						Message.ChatField(user, host);
					}
				};
				channel.BasicConsume(queue: defaultQueue, autoAck: false, consumer: consumer);
				await Task.Delay(-1);
			};
		}

		public async Task Connect()
		{
			factory = new ConnectionFactory() { HostName = this.host.host};

			using (connection = factory.CreateConnection())
			using (channel = connection.CreateModel())
			{
				channel.ExchangeDeclare(exchange: defaultExchange, type: defaultExchangeType, durable: defaultDurability);
				//channel.QueueBind(queue: user.currentQueue, exchange: defaultExchange, routingKey: "");

				string routingKey = "";
				Channel messageRoom = new Channel("General");

				user.SetChannel(messageRoom);
				Message.Joined(user, channel, routingKey, messageRoom);

				while (true)
				{
					await Task.Delay(50);

					Message.ChatField(user, host);
					Message.HandleInput(channel, user, routingKey);
				}
			};

		}
	}
}