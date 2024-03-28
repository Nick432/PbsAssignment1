using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using RabbitMQ.Client;
using Libs.Terminal;
using Client.UserData;

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

		bool isBroadcast = false;

		public Message(string message, Encoding encoding, User sender, Channel channel, bool isBroadcast = false)
		{
			this.encoding = encoding;
			this.sender = sender;
			this.message = message;
			this.channel = channel;
			this.timeStamp = DateTime.Now;
			this.isBroadcast = isBroadcast;
		}

		public Message(byte[] messageBytes)
		{
			string[] arr = DataEncoding.Decode(messageBytes);
			this.encoding = Encoding.UTF8;
			channel = new Channel(arr[1]);
			sender = new User(arr[2], "localhost");
			this.isBroadcast = bool.Parse(arr[3]);
			message = arr[4];
			
			this.timeStamp = DateTime.Now;
		}

		public string Serialize(string message)
		{
			string serializedString = new string(
				$"{this.encoding}{field}"	+ 
				$"{channel.name}{field}"	+ 
				$"{sender.username}{field}" + 
				$"{this.isBroadcast}{field}" +
				$"{this.message}");

			return serializedString;
		}

		public string FormatMessage(string message)
		{

			string formattedMessage = "";

			if (!this.isBroadcast)
			{
				formattedMessage = $"[{channel.name}][{sender.nickname}]: {message}";
			}
			else
			{
				formattedMessage = $"[{sender.nickname}] has {message}";
			}

			return formattedMessage;
		}

		public string FormatMessage()
		{
			return FormatMessage(this.message);
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

		public void Send(IModel channel)
		{
			string serialized = Serialize(this.message);
			if (channel != null)
			{
				channel.BasicPublish(exchange: Client.defaultExchange,
					routingKey: sender.routingKey,
					basicProperties: null,
					body: this.Encoded(serialized));
			}
		}

		public static Message Receive(byte[] messageBytes)
		{
			Message message = new Message(messageBytes);

			return message;
		}

		public static void Joined(User user, IModel channel, Channel chatChannel)
		{
			Message joined = new Message($"joined the channel", Encoding.UTF8, user, chatChannel, true);

			joined.Send(channel);
		}

		public static void Left(User user, IModel channel, Channel chatChannel)
		{
			Message left = new Message($"left the channel", Encoding.UTF8, user, chatChannel, true);

			left.Send(channel);
		}

		public static void ChangedName(User user, IModel channel, Channel chatChannel, string oldName)
		{
			Message changedName = new Message($"changed their nickname to {user.nickname}", Encoding.UTF8, user, chatChannel, true);

			changedName.Send(channel);
		}

		public static void ChatField(User user, Host host)
		{
			Terminal.Write($"[{user.currentChannel.name}][{user.nickname}]:");
		}

		public static void HandleInput(IModel channel, User user)
		{
			string chatMessage = Terminal.ReadLine();
			 
			if (Commands.HandleCommandInput(chatMessage) == false)
			{
				Message message = new Message(chatMessage, Encoding.UTF8, user, user.currentChannel);
				message.Send(channel);
			}
		}
	}

	// Below is potential expansion of an announcement system that goes beyond current project scope.
	public class Broadcast
	{
		public enum BroadcastType
		{
			ANNOUNCEMENT_JOIN,
			ANNOUNCEMENT_LEAVE,
			ANNOUNCEMENT_CHANGENAME
		}
	}
}