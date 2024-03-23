using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using RabbitMQ.Client;

namespace Client
{
	public class Message
	{
		public string message;
		Encoding encoding;
		public User sender;
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

		public void Send(IModel channel, string routingKey)
		{
			channel.BasicPublish(exchange: "direct_logs",
				routingKey: routingKey,
				basicProperties: null,
				body: this.Encoded());
		}

		public static Message Receive(byte[] messageBytes)
		{
			Message message = new Message(messageBytes);


			return message;
		}

		public static void Joined(User user, IModel channel, string routingKey)
		{
			Message joined = new Message($"Joined {DateTime.Now}", Encoding.UTF8, user);
			joined.Send(channel, routingKey);
		}
	}
}
