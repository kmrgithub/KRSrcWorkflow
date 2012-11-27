using System;

namespace Pst2Eml
{
	public class PSTMessageQueue<T> 
#if !__USE_MSMQ
		: System.Collections.Generic.Queue<T>
#endif
	{
#if !__USE_MSMQ
		private static Object _lockobj = new object();
#endif

		public void Dequeue(ref T t)
		{
			t = default(T);
#if !__USE_MSMQ
			System.Threading.Monitor.Enter(_lockobj);
#endif
			if (this.Count > 0)
			{
				System.Messaging.Message mes = null;
				try
				{
					mes = this.MessageQueue.Receive(new TimeSpan(0, 0, 3));
					t = (T)mes.Body;
				}
				finally
				{
				}
			}
#if !__USE_MSMQ
			System.Threading.Monitor.Exit(_lockobj);
#endif
		}

#if __USE_MSMQ
		public System.Messaging.MessageQueue MessageQueue { get; private set; }
		public PSTMessageQueue(string path)
		{
			this.MessageQueue = null;
			try
			{
				this.MessageQueue = new System.Messaging.MessageQueue(path);
			}
			catch (Exception ex)
			{
				this.MessageQueue = null;
			}
//			if (System.Messaging.MessageQueue.Exists(path))
				//creates an instance MessageQueue, which points 
				//to the already existing MyQueue
//				this.MessageQueue = new System.Messaging.MessageQueue(path);
//			else
				//creates a new private queue called MyQueue 
			if (this.MessageQueue == null)
				this.MessageQueue = System.Messaging.MessageQueue.Create(path);

			this.MessageQueue.Formatter = new System.Messaging.XmlMessageFormatter(new Type[] { typeof(T) });
		}

		public T Dequeue()
		{
			T body = default(T);
			System.Messaging.Message mes = null;
			try
			{
				mes = this.MessageQueue.Receive(new TimeSpan(0, 0, 3));
				body = (T)mes.Body;
			}
			finally
			{
			}

			return body;
		}

		public void Enqueue(object obj)
		{
			this.MessageQueue.Send(obj);
		}

		public int Count
		{
			get
			{
				int cnt = 1;
				try
				{
					this.MessageQueue.Peek(new TimeSpan(0));
					cnt = 1;
				}
				catch (System.Messaging.MessageQueueException e)
				{
					if(e.MessageQueueErrorCode == System.Messaging.MessageQueueErrorCode.IOTimeout)
						cnt = 0;
				}

//				this.MessageQueue.MessageReadPropertyFilter = new System.Messaging.MessagePropertyFilter(); // { AdministrationQueue = false, ArrivedTime = false, CorrelationId = false, Priority = false, ResponseQueue = false, SentTime = false, Body = false, Label = false, Id = false };
//				this.MessageQueue.MessageReadPropertyFilter.ClearAll();
//				cnt = this.MessageQueue.GetAllMessages().Length;
//				this.MessageQueue.MessageReadPropertyFilter = new System.Messaging.MessagePropertyFilter() { Body = true };
				return cnt;
			}
		}
#else
		public PSTMessageQueue(string path) : base()
		{
		}
#endif
	}
}
