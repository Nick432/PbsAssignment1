using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Libs.Terminal;
using Client.Structs;
using Client.UserData;

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

		public static User user = new User("NULL", "NULL");
		public static Host host = new Host(defaultHost, defaultPort);

		static ConnectionFactory factory = new ConnectionFactory();
		static IConnection? connection;
		static IModel? channel;

		bool running = true;
		bool quitting = false;

		CancellationTokenSource cts = new CancellationTokenSource();
		public Client(User _user, Host _host)
		{
			user = new User(_user.username, host.host);
			host = _host;
		}

		// Events

		public event EventHandler? OnMessageReceived;

		protected virtual void MessageReceived(EventArgs e, Message message)
		{
			OnMessageReceived?.Invoke(message, e);
		}

		public async Task Listen(CancellationToken cancelToken)
		{
			using (connection = factory.CreateConnection())
			using (channel = connection.CreateModel())
			{
				Terminal.Print($"Listening to [{host.host}]...");
				EventingBasicConsumer consumer = new EventingBasicConsumer(channel);

				user.currentQueue = channel.QueueDeclare().QueueName;
				user.rabbitChannel = channel;

				channel.ExchangeDeclare(exchange: defaultExchange, type: defaultExchangeType, durable: defaultDurability);
				try
				{
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

					MessageReceived(EventArgs.Empty, message); // fire message event so ChatClient front-end can subscribe to events and handle messages accordingly. (To-do)

					if (message.channel.name == user.currentChannel.name)
					{
						if (message.sender.username != user.username)
						{
							Terminal.EraseLine();
							Terminal.Print(message.FormatMessage());

							Message.ChatField(user, host);
						}
					}
				};
				channel.BasicConsume(queue: defaultQueue, autoAck: false, consumer: consumer);
				await Task.Delay(-1, cancelToken);
			};
		}

		public async Task Connect(CancellationToken cancelToken)
		{
			factory = new ConnectionFactory() { HostName = host.host};

			using (connection = factory.CreateConnection())
			using (channel = connection.CreateModel())
			{
				channel.ExchangeDeclare(exchange: defaultExchange, type: defaultExchangeType, durable: defaultDurability);

				user.SetRoutingKey("");
				user.currentClient = this;
				user.rabbitChannel = channel;
				user.rabbitConnection = connection;

				Channel messageRoom = new Channel("General");

				user.SetChannel(messageRoom);
				
				Terminal.Print($"Type your messages below. Use {Commands.commandChar}help for a list of commands.");

				while (!cancelToken.IsCancellationRequested)
				{
					if (quitting)
					{
						break;
					}
					await Task.Delay(50, cancelToken);

					Message.ChatField(user, host);
					Message.HandleInput(channel, user);
				}
				return;
			};
		}

		public void Disconnect()
		{
			running = false;
			quitting = true;
		}
	}
}