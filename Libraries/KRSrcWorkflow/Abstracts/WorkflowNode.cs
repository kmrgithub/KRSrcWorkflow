using System;
using System.Threading;

using KRSrcWorkflow;
using KRSrcWorkflow.Interfaces;

namespace KRSrcWorkflow.Abstracts
{
	public abstract class WorkflowNode<T, TU> //: IProcessor<T, TU>
	{
		IProcessor<T, TU> Processor { get; set; }
		protected ManualResetEvent InterruptProcessingEvent { get; set; }
		public ManualResetEvent ThreadExitEvent { get; private set; }
		IWFMessageQueue<T> InputQueue { get; set; }
		IWFMessageQueue<TU> OutputQueue { get; set; }

		protected WorkflowNode()
			: this(default(IProcessor<T, TU>), default(IWFMessageQueue<T>), default(IWFMessageQueue<TU>), null, true)
		{
		}

		public WorkflowNode(IProcessor<T, TU> processor, IWFMessageQueue<T> inputqueue, IWFMessageQueue<TU> outputqueue, ManualResetEvent interruptprocessingevent)
			: this(processor, inputqueue, outputqueue, interruptprocessingevent, false)
		{
		}

		private WorkflowNode(IProcessor<T, TU> processor, IWFMessageQueue<T> inputqueue, IWFMessageQueue<TU> outputqueue, ManualResetEvent interruptprocessingevent, bool defaultconstructor)
		{
			this.Processor = processor;
			this.ThreadExitEvent = new ManualResetEvent(false);
			this.InterruptProcessingEvent = interruptprocessingevent;
			this.InputQueue = inputqueue;
			this.OutputQueue = outputqueue;

			if (defaultconstructor == false)
			{
				if (this.InterruptProcessingEvent == null)
					throw new Exception("WorkflowNode Exception: InterruptProcessingEvent is null");

				if (this.InputQueue == null)
					throw new Exception("WorkflowNode Exception: InputQueue is null");

//				if (this.OutputQueue == null)
//					throw new Exception("WorkflowNode Exception: OutputQueue is null");

				if (this.Processor == null)
					throw new Exception("WorkflowNode Exception: Processor is null");
			}
			ThreadPool.QueueUserWorkItem(this.Run);
		}

		public void Run(Object threadcontext)
		{
			while(true)
			{
				T inputobject = default(T);
				if (this.InputQueue != null)
				{
					this.InputQueue.Dequeue(ref inputobject);
					if (inputobject != null)
					{
						TU tu = this.Processor.ProcessEx(inputobject);
						if (tu != null && this.OutputQueue != null)
							this.OutputQueue.Enqueue(tu);
					}
				}
				if (this.InterruptProcessingEvent != null)
				{
					if (this.InterruptProcessingEvent.WaitOne(1))
					{
						WFLogger.NLogger.Info("InterruptProcessingEvent set for WFThread type: {0}", this.GetType().FullName);
						break;
					}
				}
				else
					Thread.Sleep(1);
			}
			WFLogger.NLogger.Info("WorkflowNode type: {0} setting thread exit event.", this.GetType().FullName);
			this.ThreadExitEvent.Set();
			WFLogger.NLogger.Info("WorkflowNode type: {0} exiting.", this.GetType().FullName);
		}

//		public abstract TU Process(T t);
	}
}
