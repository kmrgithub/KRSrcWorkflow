using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KRSrcWorkflow.Config
{
	public class WFSrcData : WFTargetData
	{
		public WFSrcData(string data)
			: base(data)
		{
		}
	}

	public class WFSrc : WFTarget
	{
		public WFSrc() : this(string.Empty)
		{
		}

		public WFSrc(string data)
			: this(data, string.Empty)
		{
		}

		public WFSrc(string data, string assemblycache)
			: this(new WFSrcData(data), assemblycache)
		{
		}

		public WFSrc(WFSrcData wfsrcdata, string assemblycache)
			: base(wfsrcdata, assemblycache)
		{
		}
	}
}
