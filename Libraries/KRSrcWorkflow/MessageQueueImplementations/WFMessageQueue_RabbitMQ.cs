//#define __USE_BASICGET

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using KRSrcWorkflow.Interfaces;
using KRSrcWorkflow.Abstracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KRSrcWorkflow.MessageQueueImplementations
{
	public class WFMessageQueue_RabbitMQ<T> : WFMessageQueue<T> where T : ProcessorData
	{
		private static readonly string Exchange = "krsrcworkflow";
		private static BinaryFormatter binaryFormatter = new BinaryFormatter();
#if !__USE_BASICGET
		private QueueingBasicConsumer _consumer = null;
#endif
		private IModel _model = null;
		private QueueDeclareOk _queue = null;
		private string IPAddress { get; set; }
		private int Port { get; set; }
		private string QueueName { get; set; }
		private string RoutingKey { get; set; }

		public WFMessageQueue_RabbitMQ(string ipaddress, int port, string queuename, WFMessageQueueType queuetype)
		{
			this.IPAddress = ipaddress;
			this.Port = port;
			this.QueueName = queuename;
			this.RoutingKey = this.QueueName + "consumer";

			ConnectionFactory factory = new ConnectionFactory();
			factory.HostName = this.IPAddress;
			factory.Port = this.Port;
			factory.Protocol = Protocols.FromEnvironment();
			IConnection connection = factory.CreateConnection();
			_model = connection.CreateModel();
			_model.ExchangeDeclare(WFMessageQueue_RabbitMQ<T>.Exchange, ExchangeType.Direct);
//			_model.QueueBind(this.QueueName, WFMessageQueue_RabbitMQ<T>.Exchange, this.RoutingKey);
			if ((queuetype & WFMessageQueueType.Consumer) == WFMessageQueueType.Consumer)
			{
				_queue = _model.QueueDeclare(this.QueueName, false, false, false, null);
				_model.QueueBind(this.QueueName, WFMessageQueue_RabbitMQ<T>.Exchange, this.RoutingKey);
#if !__USE_BASICGET
				_consumer = new QueueingBasicConsumer(_model);
				_model.BasicConsume(this.QueueName, true, _consumer);
#endif
			}
//			else
//			{
//				_model.BasicPublish(WFMessageQueue_RabbitMQ<T>.Exchange, this.QueueName, null, System.Text.Encoding.Unicode.GetBytes("hello"));
//				_queue = _model.QueueDeclarePassive(this.QueueName);
//			}
		}

		public override void Dequeue(ref byte[] t)
		{
#if __USE_BASICGET
			BasicGetResult result = _model.BasicGet(this.QueueName, true);
			if (result.Body != null && result.Body.Length > 0)
			{
				MemoryStream ms = new MemoryStream();
				ms.Write(result.Body, 0, result.Body.Length);
				ms.Seek(0, SeekOrigin.Begin);
				t = (byte[])binaryFormatter.Deserialize(ms);
			}
#else
			object u = null;
			_consumer.Queue.Dequeue((int)_timespan.TotalMilliseconds, out u);
			if (u != null)
			{
				byte[] body = ((BasicDeliverEventArgs)u).Body;
//				t = (T)u;
				if (body != null)
				{
					MemoryStream ms = new MemoryStream();
					ms.Write(body, 0, body.Length);
					ms.Seek(0, SeekOrigin.Begin);
					t = (byte [])binaryFormatter.Deserialize(ms);
				}
			}
#endif
			else
				t = default(byte[]);
		}

		public override void Enqueue(byte[] t)
		{
			MemoryStream ms = new MemoryStream();
			binaryFormatter.Serialize(ms, t);
			_model.BasicPublish(WFMessageQueue_RabbitMQ<T>.Exchange, this.RoutingKey, null, ms.ToArray());
//			_consumer.Queue.Enqueue((object)t);
		}

		public override int Count
		{
			get
			{
				int cnt = 0;
				try
				{
#if __USE_BASICGET
					QueueDeclareOk queue = _model.QueueDeclarePassive(this.QueueName);
					cnt = (int)queue.MessageCount;
#else
					cnt = 1;
#endif
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
				return string.Format(@"{0} {1}:{2}\{3}", this.GetType().Name, this.IPAddress, this.Port, this.QueueName);
			}
		}
	}
}
