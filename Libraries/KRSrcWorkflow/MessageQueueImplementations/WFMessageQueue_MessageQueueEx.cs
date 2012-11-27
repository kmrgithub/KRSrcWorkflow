using System;
using System.Messaging;
using System.Threading;

using SFWorkflow.Interfaces;
using SFWorkflow.Abstracts;

namespace SFWorkflow.MessageQueueImplementations
{
	public class WFMessageQueue_MessageQueueEx<T> : WFMessageQueue<T>
{
		private MessageQueue _queue = null;

		public WFMessageQueue_MessageQueueEx(string ipaddress, string queuename)
			: this(String.Format("FormatName:Direct=TCP:{0}\\Private$\\{1}", ipaddress, queuename))
		{
		}

		private WFMessageQueue_MessageQueueEx(string path)
		{
			_queue = new MessageQueue(path);

			_queue.Formatter = new XmlMessageFormatter(new[] { typeof(T) });
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
