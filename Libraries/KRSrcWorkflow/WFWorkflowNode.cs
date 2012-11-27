using System;
using System.Threading;

using KRSrcWorkflow.Abstracts;
using KRSrcWorkflow.Interfaces;

namespace KRSrcWorkflow
{
	public class WFWorkflowNode<T> : WorkflowNode<T, ProcessorData>
	{
		public WFWorkflowNode(IProcessor<T, ProcessorData> processor, IWFMessageQueue<T> inputqueue, IWFMessageQueue<ProcessorData> outputqueue, ManualResetEvent interruptprocessingevent)
			: base(processor, inputqueue, outputqueue, interruptprocessingevent)
		{
		}
	}
}
