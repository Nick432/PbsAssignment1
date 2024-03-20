using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
	public class Message
	{
		string message;
		Encoding encoding;
		User sender;

		public Message(string message, Encoding encoding, User sender)
		{
			this.encoding = encoding;
			this.sender = sender;
			this.message = FormatMessage(message);
		}

		public string FormatMessage(string message)
		{
			string formattedMessage = $"[{sender.username}] {message}";

			return formattedMessage;
		}

		public byte[] Encoded()
		{
			byte[] encodedBytes = encoding.GetBytes(message);

			return encodedBytes;
		}

		public void Send()
		{

		}
	}
}
