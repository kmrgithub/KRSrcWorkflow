using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KRSrcWorkflow.Config
{
	public class WFMappingSrc : WFMappingTarget
	{
		public WFMappingSrc(string src, Type srctype)
			: base(src, srctype, WFMappingTargetTypes.WFSrc)
		{
		}
	}
}
