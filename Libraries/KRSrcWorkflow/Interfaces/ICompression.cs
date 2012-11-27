using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KRSrcWorkflow.Interfaces
{
	public interface ICompression
	{
		string Decompress(byte [] buffer);
		byte [] Compress(string data);
	}
}
