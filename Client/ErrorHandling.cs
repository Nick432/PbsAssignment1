using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
	internal class ErrorHandling
	{

	}

	public class ErrorMessage
	{
		string message;
		DateTimeOffset timestamp;

		public ErrorMessage(string message, DateTimeOffset timestamp)
		{
			this.message = message;
			this.timestamp = timestamp;
		}

		public ErrorMessage(string message)
		{
			this.message = message;
			this.timestamp = DateTimeOffset.Now;
		}

		public override string ToString()
		{
			return new string($"ERROR: {this.timestamp.ToLocalTime()}: {this.message}");
		}
	}
}