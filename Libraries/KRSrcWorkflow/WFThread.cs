using System.Threading;

using KRSrcWorkflow.Interfaces;

namespace KRSrcWorkflow
{
	public class WFThread<T> : WFGenericThread<T, WFProcessingResult> where T : IWFObjectProcessor<WFProcessingResult> //, IWFObjectProcessor
	{
		public WFThread(ManualResetEvent interruptprocessingevent, IWFMessageQueue<T> inputqueue, IWFMessageQueue<KRSrcWorkflow.WFProcessingResult> outputqueue, int threadid)
			: base(interruptprocessingevent, inputqueue, outputqueue, threadid)
		{
		}
	}
}
