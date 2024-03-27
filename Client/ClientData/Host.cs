using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.UserData
{
	public class Host
	{
		public string host = "localhost";
		public bool validHost = false;
		public int port = 0;

		public Host(string host, int port)
		{
			this.host = host;
			this.port = port;
		}

		public async Task<bool> Validate()
		{
			bool validated = false;

			using (HttpClient client = new HttpClient())
			{
				try
				{
					HttpResponseMessage response = await client.GetAsync(FormatHost());
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
			return new string($"http://{host}:{port}/");
		}
	}
}
