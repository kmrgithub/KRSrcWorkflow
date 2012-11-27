using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KRSrcWorkflow.Interfaces;

namespace KRSrcWorkflow.CompressionImplementations
{
	class NoCompression : ICompression
	{
		public byte[] Compress(string data)
		{
			return System.Text.UnicodeEncoding.Unicode.GetBytes(data);
		}

		public string Decompress(byte[] buffer)
		{
			return System.Text.UnicodeEncoding.Unicode.GetString(buffer);
		}
	}
}
