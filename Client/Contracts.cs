using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Contracts
{
	public class Joined
	{
		public DateTime JoinedTime { get; set; }
		public string? User { get; set; }
	}

	public class Left
	{
		public DateTime LeftTime { get; set; }
		public string? User { get; set; }
	}

	public class Message
	{
		public DateTime MessageTime { get; set; }
		public string? User { get; set; }
		public string? Text { get; set; }
	}
}
