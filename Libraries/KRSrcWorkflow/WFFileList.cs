using System;
using System.Collections.Generic;

namespace KRSrcWorkflow
{
	[Serializable]
	public class WFFileList : List<WFKeyValuePair<string, WFState>>
	{
		public new void Add(WFKeyValuePair<string, WFState> item)
		{
			base.Add(item);
		}

		public void Add(string filename, string state)
		{
			base.Add(new WFKeyValuePair<string, WFState>(filename, new WFState { Value = state }));
		}

		public void Add(string filename)
		{
			base.Add(new WFKeyValuePair<string, WFState>(filename, new WFState { Value = WFState.WFStateUnknown }));
		}
	}
}
