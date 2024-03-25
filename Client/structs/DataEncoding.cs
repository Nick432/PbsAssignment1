using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Structs
{
	public static class DataEncoding
	{
		public static Encoding GetEncoding(byte[] byteArray)
		{
			// To-do - just use UTF-8 as default
			return Encoding.UTF8;
		}
		public static string[] Decode(byte[] message)
		{
			Encoding encoding = GetEncoding(message);

			string decoded = encoding.GetString(message);
			string[] arr = decoded.Split(Message.field);

			return arr;
		}
	}
}
