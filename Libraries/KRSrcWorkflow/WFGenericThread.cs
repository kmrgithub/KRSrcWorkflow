using System;
using System.Threading;

using KRSrcWorkflow.Interfaces;

namespace KRSrcWorkflow
{
	public abstract class WFGenericThread<T> // where T : IWFObjectProcessor
	{
		protected ManualResetEvent InterruptProcessingEvent { get; set; }
		public ManualResetEvent ThreadExitEvent { get; private set; }
		protected int ThreadId { get; set; }
		protected IWFMessageQueue<T> InputQueue { get; set; }

		protected WFGenericThread()
			: this(null, default(IWFMessageQueue<T>), 0, true)
		{
		}

		protected WFGenericThread(ManualResetEvent interruptprocessingevent, IWFMessageQueue<T> inputqueue, int threadid) : this(interruptprocessingevent, inputqueue, threadid, false)
		{
		}

		protected WFGenericThread(ManualResetEvent interruptprocessingevent, IWFMessageQueue<T> inputqueue, int threadid, bool defaultconstructor)
		{
			this.ThreadExitEvent = new ManualResetEvent(false);
			this.InterruptProcessingEvent = interruptprocessingevent;
			this.ThreadId = threadid;
			this.InputQueue = inputqueue;

			if (defaultconstructor == false)
			{
				if (this.InterruptProcessingEvent == null)
					throw new Exception("WFGenericThread Exception: InterruptProcessingEvent is null");

				if (this.InputQueue == null)
					throw new Exception("WFGenericThread Exception: InputQueue is null");
			}
		}

		public void Run(Object threadContext)
		{
			WFLogger.NLogger.Info("WFThread started for type {0} on queue: {1}", this.GetType().FullName, this.InputQueue.Path);
			while (true)
			{
				T inputobject = default(T);

				this.InputQueue.Dequeue(ref inputobject);
				if (inputobject != null)
				{
					WFLogger.NLogger.Info("Dequeue object of type: {0} on queue: {1}", inputobject.GetType().FullName, this.InputQueue.Path);
//					if (this is WFGenericThread<T>)
//						inputobject.Process();
//					else 
					RunHandler(inputobject);
				}
				if (this.InterruptProcessingEvent.WaitOne(1))
				{
					WFLogger.NLogger.Info("InterruptProcessingEvent set for WFThread type: {0}", this.GetType().FullName);
					break;
				}
			}
			WFLogger.NLogger.Info("WFThread type: {0} setting thread exit event.", this.GetType().FullName);
			this.ThreadExitEvent.Set();
			WFLogger.NLogger.Info("WFThread type: {0} exiting.", this.GetType().FullName);
		}

//		public abstract void Process(T t);
		public abstract void RunHandler(T t);
	}

	public class WFGenericThread<T, TU> : WFGenericThread<T> where T : IWFObjectProcessor<TU> //, IWFObjectProcessor
	{
		protected IWFMessageQueue<TU> OutputQueue { get; set; }

		public WFGenericThread(ManualResetEvent interruptprocessingevent, IWFMessageQueue<T> inputqueue, IWFMessageQueue<TU> outputqueue, int threadid)
			: base(interruptprocessingevent, inputqueue, threadid)
		{
			this.OutputQueue = outputqueue;

//			if (this.OutputQueue == null)
//				throw new Exception("WFGenericThread Exception: OutputQueue is null");
		}

		public sealed override void RunHandler(T t)
		{
			TU u = ((IWFObjectProcessor<TU>)t).Process();
			if (u != null && this.OutputQueue != null)
			{
				WFLogger.NLogger.Info("Enqueue object of type: {0} on queue: {1}", u.GetType().FullName, this.OutputQueue.Path);
				this.OutputQueue.Enqueue(u);
			}
		}
	}

	public class WFQueueConnector<T, TU> : WFGenericThread<T, TU> where T : IWFObjectProcessor<TU>
	{
		public WFQueueConnector(ManualResetEvent interruptprocessingevent, IWFMessageQueue<T> inputqueue, IWFMessageQueue<TU> outputqueue, int threadid)
			: base(interruptprocessingevent, inputqueue, outputqueue, threadid)
		{
			if (this.OutputQueue == null)
				throw new Exception("WFQueueConnector Exception: OutputQueue is null");
		}
	}

	public class WFQueueSink<T> : WFGenericThread<T> where T : IWFObjectProcessor
	{
		public sealed override void RunHandler(T t)
		{
			((IWFObjectProcessor)t).Process();
		}
	}
}
