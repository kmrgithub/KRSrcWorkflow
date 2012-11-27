using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KRSrcWorkflow.Config
{
	public class WFProcessorStates : List<WFState>
	{
//		public new void Add(WFState wfstate)
//		{
//			WFState state = null;
//			if ((state = this.Find(x => x.State.Value == wfstate.State.Value)) != null)
//				this.Remove(state);
//			base.Add(wfstate);
//		}

		public WFProcessorStates()
		{
			this.Add(new WFState(KRSrcWorkflow.WFState.WFStateSuccess));
			this.Add(new WFState(KRSrcWorkflow.WFState.WFStateFail));
			this.Add(new WFState(KRSrcWorkflow.WFState.WFStateComplete));
		}
	}
}
