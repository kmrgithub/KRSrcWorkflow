using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KRSrcWorkflow.Config
{
	public class WFProcessorData : WFTargetData
	{
		public WFProcessorData(string data)
			: base(data)
		{
		}
	}

	public class WFProcessor : WFTarget
	{
		public WFProcessor() 
			: this(string.Empty)
		{
		}

		public WFProcessor(string data)
			: this(data, string.Empty)
		{
		}

		public WFProcessor(string data, string assemblycache)
			: this(new WFProcessorData(data), assemblycache)
		{
		}

		public WFProcessor(WFProcessorData wfprocessordata, string assemblycache)
			: base(wfprocessordata, assemblycache)
		{
		}
	}
}
