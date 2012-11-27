using System;

namespace KRSrcWorkflow.Abstracts.Wcf
{
	public abstract class WFClientAsync : Interfaces.Wcf.IWFClientProcessing
	{
		public abstract void ProcessingAsync(Guid guid, string filename, uint depth, Guid parentguid);
		public abstract void CompletedAsync(Guid guid, string filename, uint depth, Guid parentguid);
		public abstract void CompletedExAsync(Guid guid, WFState state);

		public void Processing(Guid guid, string filename, uint depth, Guid parentguid)
		{
			Action<Guid, string, uint, Guid> cd = ProcessingAsync;
			cd.BeginInvoke(guid, filename, depth, parentguid, cd.EndInvoke, null);
		}

		public void Completed(Guid guid, string filename, uint depth, Guid parentguid)
		{
			Action<Guid, string, uint, Guid> cd = CompletedAsync;
			cd.BeginInvoke(guid, filename, depth, parentguid, cd.EndInvoke, null);
		}

		public void CompletedEx(Guid guid, WFState state)
		{
			Action<Guid, WFState> cd = CompletedExAsync;
			cd.BeginInvoke(guid, state, cd.EndInvoke, null);
		}
	}
}
