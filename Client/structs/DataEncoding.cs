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
			int i = -1;
			while (i++ < byteArray.Length)
			{
				char c = BitConverter.ToChar(byteArray, i);
			}


			return Encoding.UTF8;
		}
		public static string Decode(byte[] message)
		{
			Encoding encoding = GetEncoding(message);

			string decoded = "";


			return decoded;
		}
	}
}
