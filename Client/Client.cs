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
		ConnectionFactory c_Factory = new ConnectionFactory();

		IConnection? _iconnection;
		IModel? _ichannel;

		string hostName = "";
		string username = "";
		public User user;

		public Connection(string host, User user)
		{
			this.hostName = host;
			this.username = user.username;
			this.user = user;

			Initialize();
		}

		void Initialize()
		{
			c_Factory = new ConnectionFactory() 
			{ 
				HostName = this.hostName, 
				UserName = this.username 
			};
		}

		public IConnection Connect()
		{
			try
			{
				c_Factory.CreateConnection();
			}
			catch
			{

			}

			return c_Factory.CreateConnection();
		}

		public IModel? Channel(IConnection con)
		{
			return con.CreateModel();
		}

		public IModel? Channel()
		{
			try
			{
				_iconnection = c_Factory.CreateConnection();
				_ichannel = _iconnection.CreateModel();
			}
			catch (Exception ex)
			{
				ErrorMessage error = new ErrorMessage(ex.Message);

				Terminal.Print(error.ToString());
				Console.ReadKey();
			}

			return _ichannel;
		}
	}

	public class Client
	{
		public static string apiUrl = "http://localhost:15672/api/users/";

		public static string defaultHost = "localhost";
		public static int defaultPort = 15672;
		
		int clientID = 0;
		string clientName = "";
		User? user;
		public Connection connection;

		public Client(int clientID, string clientName, string host = "localhost")
		{
			this.clientID = clientID;
			this.clientName = clientName;

			user = new User(clientName, "localhost");

			connection = new Connection(host, user);
		}

		public Client(User user, string host = "localhost")
		{
			this.clientID = 0;
			this.clientName = user.username;

			user = new User(clientName, host);

			connection = new Connection(host, user);
		}

	}
}