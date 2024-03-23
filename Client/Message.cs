using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Client
{
	public class Message
	{
		public string message;
		Encoding encoding;
		User sender;
		public static char field = (char)11;
		public static char record = (char)12;

		public Message(string message, Encoding encoding, User sender)
		{
			this.encoding = encoding;
			this.sender = sender;
			this.message = FormatMessage(message);
		}


		public Message(byte[] messageBytes)
		{
			string? input = Encoding.UTF8.GetString(messageBytes);
		}

		
		public string Serialize(string message)
		{
			//$sam:$encoding.utf8:%message;
			//$sam:$encoding.utf8:#3:#6:$message;
			
			return new string($"{sender.username}{field}{this.encoding}{field}{this.message}{record}");
		}

		public string FormatMessage(string message)
		{
			string formattedMessage = $"[{sender.username}]: {message}";

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
