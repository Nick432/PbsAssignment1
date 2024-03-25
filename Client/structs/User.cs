using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Structs
{
	public class User
	{
		public string username = "";
		public string address = "";
		public bool validUser = false;
		public Channel currentChannel;

		public User(string username, string address)
		{
			this.username = username;
			this.address = address;
		}

		public void SetChannel(Channel channel)
		{
			this.currentChannel = channel;
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
}
