using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Libs.Terminal;

namespace Client
{
	public class Host
	{
		public string host = "localhost";
		public bool validHost = false;
		int port = 0;

		public Host(string host, int port)
		{
			this.host = host;
			this.port = port;
		}

		public async Task<bool> Validate()
		{
			Terminal.Print($"Attempting connection to {host}:{port}...");
			bool validated = false;

			using (HttpClient client = new HttpClient())
			{
				try
				{
					HttpResponseMessage response = await client.GetAsync(this.FormatHost());
					if (response.IsSuccessStatusCode)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				catch (Exception ex)
				{
					ErrorMessage error = new ErrorMessage("Error finding host: " + ex.Message);
					Console.WriteLine(error.ToString());
				}
			}
			return validated;
		}

		public string FormatHost()
		{
			return new string($"http://{this.host}:{port}/");
		}
	}

	public class User
	{
		public string username = "";
		public string address = "";
		public bool validUser = false;

		public User(string username, string address)
		{
			this.username = username;
			this.address = address;
		}

		public async Task<bool> Validate()
		{
			bool validated = false;

			string base64Auth = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{username}:guest"));

			using (HttpClient client = new HttpClient())
			{
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64Auth);

				try
				{
					HttpResponseMessage response = await client.GetAsync(Client.apiUrl); 

					if (response.IsSuccessStatusCode)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				catch (Exception ex)
				{
					ErrorMessage error = new ErrorMessage("Error validating user: " + ex.Message);
					Console.WriteLine(error.ToString());
				}
			}
			return validated;
		}
	}

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

					Terminal.Print(message.message);
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

				Message.Joined(user, channel, routingKey);

				while (true)
				{
					Console.Write($"[{user.username}]: ");
					string chatMessage = Terminal.ReadLine();

					Message message = new Message(chatMessage, Encoding.UTF8, user);

					message.Send(channel, routingKey);
					await Task.Delay(100);
				}
			};

		}


	}
}