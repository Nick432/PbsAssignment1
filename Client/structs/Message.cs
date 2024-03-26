using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using RabbitMQ.Client;
using Libs.Terminal;

namespace Client.Structs
{
	public class Message
	{
		public string message;
		Encoding encoding;
		public User sender;
		public static char field = (char)11;
		public static char record = (char)12;
		public Channel channel;
		DateTime timeStamp;

		public Message(string message, Encoding encoding, User sender, Channel channel)
		{
			this.encoding = encoding;
			this.sender = sender;
			this.message = message;
			this.channel = channel;
			this.timeStamp = DateTime.Now;
		}

		public Message(byte[] messageBytes)
		{
			string[] arr = DataEncoding.Decode(messageBytes);
			this.encoding = Encoding.UTF8;
			channel = new Channel(arr[1]);
			sender = new User(arr[2], "localhost");
			message = arr[3];
			this.timeStamp = DateTime.Now;
		}

		public string Serialize(string message)
		{
			string serializedString = new string(
				$"{this.encoding}{field}"	+ 
				$"{channel.name}{field}"	+ 
				$"{sender.username}{field}" + 
				$"{this.message}");

			return serializedString;
		}

		public string FormatMessage(string message)
		{
			string formattedMessage = $"[{channel.name}][{sender.username}]: {message}";

			return formattedMessage;
		}

		public string FormatMessage()
		{
			string formattedMessage = $"[{this.channel.name}][{this.sender.username}]: {this.message}";

			return formattedMessage;
		}

		public byte[] Encoded()
		{
			byte[] encodedBytes = encoding.GetBytes(message);

			return encodedBytes;
		}

		public byte[] Encoded(string input)
		{
			byte[] encodedBytes = encoding.GetBytes(input);

			return encodedBytes;
		}

		public void Send(IModel channel, string routingKey)
		{
			string serialized = Serialize(this.message);
			channel.BasicPublish(exchange: Client.defaultExchange,
				routingKey: routingKey,
				basicProperties: null,
				body: this.Encoded(serialized));
		}

		public static Message Receive(byte[] messageBytes)
		{
			Message message = new Message(messageBytes);


			return message;
		}

		public static void Joined(User user, IModel channel, string routingKey, Channel chatChannel)
		{
			Message joined = new Message($"Joined", Encoding.UTF8, user, chatChannel);

			joined.Send(channel, routingKey);
		}

		public static void ChatField(User user, Host host)
		{
			Terminal.Write($"[{user.currentChannel.name}][{user.username}]:");
		}

		public static void HandleInput(IModel channel, User user, string routingKey)
		{
			string chatMessage = Terminal.ReadLine();

			if (!Commands.HandleCommandInput(chatMessage))
			{
				Message message = new Message(chatMessage, Encoding.UTF8, user, user.currentChannel);
				message.Send(channel, routingKey);
			}
		}
	}

	public class Broadcast
	{
		User user;
		public string announcement = "";
		public Broadcast(User user)
		{
			this.user = user;
		}

		public void Announce()
		{
			// send announcement here
		}

	}

	public class JoinedBroadcast : Broadcast
	{
		
		public JoinedBroadcast(User user) : base(user)
		{
			base.announcement = "Joined";
		}
	}

	public class LeftBroadcast : Broadcast
	{
		public LeftBroadcast(User user) : base(user)
		{
			base.announcement = "Left";
		}
	}
}
