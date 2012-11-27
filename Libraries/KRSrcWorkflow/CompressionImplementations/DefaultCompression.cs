using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KRSrcWorkflow.Interfaces;

namespace KRSrcWorkflow.CompressionImplementations
{
	class DefaultCompression : ICompression
	{
		public byte[] Compress(string data)
		{
			return KRSrcWorkflow.WFUtilities.CompressString(data);
		}

		public string Decompress(byte[] buffer)
		{
			return WFUtilities.Decompress(buffer);
		}
	}
}
