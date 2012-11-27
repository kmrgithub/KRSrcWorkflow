using System;

using NLog;

namespace Pst2Eml
{
	public class InputOutputMessageQueue<T> 
#if !__USE_MSMQ
		: System.Collections.Generic.Queue<T>
#endif
	{
		public enum InputOutputMessageQueueTypes { Server, Client };
		public InputOutputMessageQueueTypes InputOutputMessageQueueType { get; private set; }
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
				Logger.NLogger.Info("Receiving type {0}", typeof(T).Name);
#if __USE_MSMQ
				System.Messaging.Message mes = null;
				try
				{
					mes = this.MessageQueue.Receive(new TimeSpan(0, 0, 3));
					t = (T)mes.Body;
				}
				catch (Exception ex)
				{
					Logger.NLogger.ErrorException(string.Empty, ex);
				}
				finally
				{
				}
#else
				t = (T)base.Dequeue();
#endif
			}
#if !__USE_MSMQ
			System.Threading.Monitor.Exit(_lockobj);
#endif
		}

#if __USE_MSMQ
		public System.Messaging.MessageQueue MessageQueue { get; private set; }

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
			Logger.NLogger.Info("Sending type {0}", obj.ToString());
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

//				this.MessageQueue.MessageReadPropertyFilter = new System.Messaging.MessagePropertyFilter( AdministrationQueue = false, ArrivedTime = false, CorrelationId = false, Priority = false, ResponseQueue = false, SentTime = false, Body = false, Label = false, Id = false };
//				this.MessageQueue.MessageReadPropertyFilter.ClearAll();
//				cnt = this.MessageQueue.GetAllMessages().Length;
//				this.MessageQueue.MessageReadPropertyFilter = new System.Messaging.MessagePropertyFilter() { Body = true };
				return cnt;
			}
		}
#endif
		public InputOutputMessageQueue(string path)
			: this(path, InputOutputMessageQueue<T>.InputOutputMessageQueueTypes.Client, false)
		{
		}

		public InputOutputMessageQueue(string path, InputOutputMessageQueue<T>.InputOutputMessageQueueTypes queuetype, bool purge)
		{
#if __USE_MSMQ
			this.InputOutputMessageQueueType = queuetype;
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
			if ((this.MessageQueue == null || !this.MessageQueue.CanWrite) && this.InputOutputMessageQueueType == InputOutputMessageQueue<T>.InputOutputMessageQueueTypes.Server)
				this.MessageQueue = System.Messaging.MessageQueue.Create(path);

			if (this.MessageQueue != null)
				this.MessageQueue.Formatter = new System.Messaging.XmlMessageFormatter(new Type[] { typeof(T) });

			if (queuetype == InputOutputMessageQueue<T>.InputOutputMessageQueueTypes.Server && purge)
				this.MessageQueue.Purge();
#endif
		}
	}
}
