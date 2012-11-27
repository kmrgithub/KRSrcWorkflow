using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using RabbitMQ.Client;

namespace RabbitMQTest
{
	class Program
	{
		static object lockobject = new object();

		static void Main(string[] args)
		{
			var connectionFactory = new ConnectionFactory();
			IConnection connection = connectionFactory.CreateConnection();
			IModel channel = connection.CreateModel();
			channel.ExchangeDeclare("direct-exchange-example", ExchangeType.Direct);
			channel.QueueDeclare("logs", false, false, false, null);
			channel.QueueBind("logs", "direct-exchange-example", "");

			ThreadPool.QueueUserWorkItem(x =>
			{
				while (true)
				{
					string value = Guid.NewGuid().ToString();
					byte[] message = Encoding.UTF8.GetBytes(value);
					Console.WriteLine("Writing " + value);
//					lock (lockobject)
					{
						channel.BasicPublish("direct-exchange-example", "", null, message);
					}
					Thread.Sleep(500);
				}
			});

//			ThreadPool.QueueUserWorkItem(x =>
//			{
				//				var connectionFactory = new ConnectionFactory();
				//				IConnection connection = connectionFactory.CreateConnection();
				//				IModel channel = connection.CreateModel();
//				while (true)
//				{
//					QueueDeclareOk queue = null;
//					lock (lockobject)
//					{
//						queue = channel.QueueDeclarePassive("logs");
//					}
//					Console.WriteLine("Count: " + queue.MessageCount);
//					Thread.Sleep(2000);
//				}
//			});

#if true
			ThreadPool.QueueUserWorkItem(x =>
			{
				//				var connectionFactory = new ConnectionFactory();
//				IConnection connection = connectionFactory.CreateConnection();
//				IModel channel = connection.CreateModel();
//				channel.ExchangeDeclare("direct-exchange-example", ExchangeType.Direct);

//				channel.QueueDeclare("logs", false, false, false, null);
//				channel.QueueBind("logs", "direct-exchange-example", "");
//				var consumer = new QueueingBasicConsumer(channel);
//				BasicGetResult result = channel.BasicGet("logs", true); // BasicConsume("logs", true, consumer);
//				Console.WriteLine(result.MessageCount);

				while (true)
				{
					QueueDeclareOk queue = channel.QueueDeclarePassive("logs");
					if (queue.MessageCount > 0)
					{
						BasicGetResult result = null;
						while ((result = channel.BasicGet("logs", true)).MessageCount != 0)
						{
							Console.WriteLine("MessageCount: " + result.MessageCount + "  Body: " + Encoding.UTF8.GetString(result.Body));
						}
					}
					Thread.Sleep(5000);
				}
			});
#endif

			Thread.CurrentThread.Join();
		}
	}
}
