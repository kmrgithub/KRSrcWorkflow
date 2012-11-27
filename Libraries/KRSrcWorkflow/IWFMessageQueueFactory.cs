using System;
using System.Collections.Generic;
using System.Messaging;

using KRSrcWorkflow.Interfaces;
using KRSrcWorkflow.Abstracts;
using KRSrcWorkflow.MessageQueueImplementations;
using RabbitMQ.Client;

namespace KRSrcWorkflow
{
	public class IWFMessageQueueFactory<T, U>
//		where T : IWFMessageQueue<U>
		where U : ProcessorData
//		where U : WFProcessor
	{
		public static IWFMessageQueue<U> CreateWFMessageQueue(string ipaddress, int port, string queuename, WFMessageQueueType queuetype)
		{
//			if (typeof(T) == typeof(Queue<U>))
//				return new WFMessageQueue_Queue<U>();
//			else if (typeof(T) == typeof(MessageQueue))
//				return new WFMessageQueue_MessageQueue<U>(ipaddress, queuename);
//			else 
			if (typeof(T) == typeof(QueueingBasicConsumer))
				return new WFMessageQueue_RabbitMQ<U>(ipaddress, port, queuename, queuetype);

			return null;
		}

		public static IWFMessageQueue<U> CreateWFMessageQueue(string ipaddress, string queuename)
		{
			return CreateWFMessageQueue(ipaddress, 0, queuename, WFMessageQueueType.None);
		}

		//host@10.3.2.35:queue@pstqueue:queuetype@rabbitmq
		public static IWFMessageQueue<U> CreateWFMessageQueue(string config, WFMessageQueueType messagequeuetype)
		{
			string queuehost = System.Net.Dns.GetHostName();
			string queuetype = "msmq";
			string queuename = string.Empty;

			string[] splitdata = config.Split(new char[] { ':' });
			foreach (string split in splitdata)
			{
				string[] elemdata = split.Split(new char[] { '@' });
				if (elemdata.Length == 2)
				{
					switch (elemdata[0])
					{
						case "host":
							queuehost = elemdata[1];
							break;
						case "queue":
							queuename = elemdata[1];
							break;
						case "queuetype":
							queuetype = elemdata[1];
							break;
					}
				}
			}

			string ipaddress = string.Empty;
			int port = 0;
			if (WFUtilities.SetHostAndIPAddress(queuehost, ref ipaddress))
			{
				Type type = null;

				switch (queuetype)
				{
					case ".netqueue":
						type = typeof(System.Collections.Queue);
						break;

					case "msmq":
						type = typeof(MessageQueue);
						break;

					case "rabbitmq":
						type = typeof(QueueingBasicConsumer);
						port = 5672;
						break;

					default:
						break;
				}
			}

			return CreateWFMessageQueue(ipaddress, port, queuename, messagequeuetype);
		}
	}
}
