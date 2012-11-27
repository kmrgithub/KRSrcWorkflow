using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Pst2Eml
{
	public interface InputOutputThreadObjectHandler<U>
	{
		U ProcessQueueObject();
	}

	public class InputOutputThread<T, U>
	{
		private InputOutputMessageQueue<T> InputQueue { get; set; }
		private InputOutputMessageQueue<U> OutputQueue { get; set; }
		private ManualResetEvent InterruptProcessingEvent { get; set; }
		public ManualResetEvent ThreadExitEvent { get; private set; }
		private int ThreadId { get; set; }

		public InputOutputThread(ManualResetEvent interruptprocessingevent, InputOutputMessageQueue<T> inputqueue, int threadid)
			: this(interruptprocessingevent, inputqueue, null, threadid)
		{
		}

		public InputOutputThread(ManualResetEvent interruptprocessingevent, InputOutputMessageQueue<T> inputqueue)
			: this(interruptprocessingevent, inputqueue, null, 0)
		{
		}

		public InputOutputThread(ManualResetEvent interruptprocessingevent, InputOutputMessageQueue<T> inputqueue, InputOutputMessageQueue<U> outputqueue, int threadid)
		{
			this.ThreadExitEvent = new ManualResetEvent(false);
			this.InputQueue = inputqueue;
			this.OutputQueue = outputqueue;
			this.InterruptProcessingEvent = interruptprocessingevent;
			this.ThreadId = threadid;
		}

		public void ThreadPoolCallback(Object threadContext)
		{
			while (true && (this.InputQueue != null))
			{
				T inputobject = default(T);
				this.InputQueue.Dequeue(ref inputobject);
				if (inputobject != null)
				{
					InputOutputThreadObjectHandler<U> handler = inputobject as InputOutputThreadObjectHandler<U>;
					if (handler != null)
					{
						U u = ((InputOutputThreadObjectHandler<U>)inputobject).ProcessQueueObject();
						if (u != null && this.OutputQueue != null)
							this.OutputQueue.Enqueue(u);
					}
				}
				if (this.InterruptProcessingEvent != null)
				{
					if (this.InterruptProcessingEvent.WaitOne(100))
					{
						if (this.InputQueue.Count == 0)
							break;
					}
				}
			}
			this.ThreadExitEvent.Set();
		}
	}
}
