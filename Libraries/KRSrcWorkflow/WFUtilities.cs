using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Serialization;

namespace KRSrcWorkflow
{
	public class WFUtilities
	{
		public static bool CanBeXmlSerialized(Type objectToTest)
		{
			Type[] interfaces = objectToTest.GetInterfaces();
			bool containsIXmlSerializable = interfaces.Contains(typeof(IXmlSerializable));

			return containsIXmlSerializable || objectToTest.IsSerializable;
		}

		public static uint GetNextDirectoryNumber(string basedir)
		{
			uint idx = 0;

			for (uint i = 0; i < uint.MaxValue; i++)
			{
				if (!System.IO.Directory.Exists(string.Format("{0}\\{1}", basedir, i)))
				{
					idx = i;
					break;
				}
			}

			return idx;
		}

		public static bool SetHostAndIPAddress(string host, ref string ipaddress)
		{
			// assume host is not an ipaddress
			try
			{
				List<System.Net.IPAddress> addresslist = System.Net.Dns.GetHostAddresses(host).Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToList();
				if (addresslist.Any())
					ipaddress = addresslist[0].ToString();
			}
			catch (Exception)
			{
				ipaddress = string.Empty;
			}
			// GetHostAdddresses may have failed because host is passed in as ipaddress
			if (ipaddress.Equals(string.Empty))
			{
				// first determine if host is form a.b.c.d
				if (IsValidIP(host))
				{
					ipaddress = host;
					try
					{
						host = System.Net.Dns.GetHostEntry(ipaddress).HostName;
					}
					catch (Exception)
					{
						ipaddress = string.Empty;
					}
				}
				else
				{
					ipaddress = string.Empty;
				}
			}

			return !ipaddress.Equals(string.Empty);
		}

		private static bool IsValidIP(string addr)
		{
			//create our match pattern
			const string pattern = @"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$";

			//create our Regular Expression object
			System.Text.RegularExpressions.Regex check = new System.Text.RegularExpressions.Regex(pattern);

			//boolean variable to hold the status
			bool valid = false;

			//check to make sure an ip address was provided
			if (!addr.Equals(string.Empty))
			{
				//address provided so use the IsMatch Method
				//of the Regular Expression object
				valid = check.IsMatch(addr, 0);
			}
			//return the results
			return valid;
		}

		public static byte [] CompressString(string text)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(text);
			var memoryStream = new MemoryStream();
			using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
			{
				gZipStream.Write(buffer, 0, buffer.Length);
			}

			memoryStream.Position = 0;

			var compressedData = new byte[memoryStream.Length];
			memoryStream.Read(compressedData, 0, compressedData.Length);

			var gZipBuffer = new byte[compressedData.Length + 4];
			Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
			Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
			return gZipBuffer;
		}

		public static string CompressStringBase64(string text)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(text);
			var memoryStream = new MemoryStream();
			using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
			{
				gZipStream.Write(buffer, 0, buffer.Length);
			}

			memoryStream.Position = 0;

			var compressedData = new byte[memoryStream.Length];
			memoryStream.Read(compressedData, 0, compressedData.Length);

			var gZipBuffer = new byte[compressedData.Length + 4];
			Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
			Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
			return Convert.ToBase64String(gZipBuffer);
		}

		public static string Decompress(byte[] gZipBuffer)
		{
			using (var memoryStream = new MemoryStream())
			{
				int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
				memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

				var buffer = new byte[dataLength];

				memoryStream.Position = 0;
				using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
				{
					gZipStream.Read(buffer, 0, buffer.Length);
				}

				return Encoding.UTF8.GetString(buffer);
			}
		}

		public static string DecompressStringBase64(string compressedText)
		{
			byte[] gZipBuffer = Convert.FromBase64String(compressedText);
			using (var memoryStream = new MemoryStream())
			{
				int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
				memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

				var buffer = new byte[dataLength];

				memoryStream.Position = 0;
				using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
				{
					gZipStream.Read(buffer, 0, buffer.Length);
				}

				return Encoding.UTF8.GetString(buffer);
			}
		}
	}
}
