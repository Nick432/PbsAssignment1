using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Structs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Client.UserData
{
	public class User
	{
		public string username = "";
		public string nickname = "";
		public string address = "";
		public bool validUser = false;
		public Channel currentChannel;
		public string currentQueue = "";
		public string routingKey = "";

		public IModel rabbitChannel;
		public IConnection rabbitConnection;
		public Client currentClient;

		public User(string username, string address)
		{
			this.username = username;
			this.nickname = username;
			this.address = address;
		}

		public void SetRoutingKey(string routingKey)
		{
			this.routingKey = routingKey;
		}

		public void SetChannel(Channel channel)
		{
			if (this.currentChannel != null)
			{
				Message.Left(this, rabbitChannel, currentChannel);
			}

			currentChannel = channel;

			Message.Joined(this, rabbitChannel, currentChannel);
		}

		public void ChangeNickname(string newName)
		{
			string oldName = this.nickname;
			this.nickname = newName;

			Message.ChangedName(this, rabbitChannel, currentChannel, oldName);
		}

		public async Task<bool> Validate()
		{
			bool validated = false;

			string base64Auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:guest"));

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

		public void Disconnect()
		{
			Message.Left(this, rabbitChannel, currentChannel);

			this.currentClient.Disconnect();
		}
	}
}
