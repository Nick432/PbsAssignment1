﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using RabbitMQ.Client;

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
			//$sam:$encoding.utf8:%message;
			//$sam:$encoding.utf8:#3:#6:$message;
			
			return new string($"{this.encoding}{field}{channel.name}{field}{sender.username}{field}{this.message}");
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
			channel.BasicPublish(exchange: "direct_logs",
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
			Message joined = new Message($"Joined {DateTime.Now}", Encoding.UTF8, user, chatChannel);
			joined.Send(channel, routingKey);
		}
	}
}
