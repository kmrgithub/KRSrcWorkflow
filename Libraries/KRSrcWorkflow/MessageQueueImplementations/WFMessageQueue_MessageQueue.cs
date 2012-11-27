using System;
using System.Messaging;
using System.Threading;

using KRSrcWorkflow.Interfaces;
using KRSrcWorkflow.Abstracts;

namespace KRSrcWorkflow.MessageQueueImplementations
{
	public class WFMessageQueue_MessageQueue<T> : WFMessageQueue<T> where T : ProcessorData //MessageQueue, IWFMessageQueue<T>, IWFMessageQueue
	{
		private MessageQueue _queue = null;

		public WFMessageQueue_MessageQueue(string ipaddress, string queuename)
			: this(String.Format("FormatName:Direct=TCP:{0}\\Private$\\{1}", ipaddress, queuename))
		{
		}

		private WFMessageQueue_MessageQueue(string path)
		{
			_queue = new MessageQueue(path);

			_queue.Formatter = new BinaryMessageFormatter();
//				new XmlMessageFormatter(new[] { typeof(T) });
		}

		public override void Dequeue(ref byte[] t)
		{
			Message mes = _queue.Receive(_timespan);
			if (mes != null && mes.Body != null)
				t = (byte[])mes.Body;
		}

		public override void Enqueue(byte[] t)
		{
			_queue.Send(t);
		}

		public override int Count
		{
			get
			{
				int cnt = 1;
				try
				{
					_queue.Peek(_timespan);
				}
				catch (Exception)
				{
					cnt = 0;
				}
				return cnt;
			}
		}

		public override string Path
		{
			get
			{
				return _queue.Path;
			}
		}
	}
}
