using System;
using System.Linq;
using System.Reflection;
using System.Messaging;
using System.Collections.Generic;
using System.IO;

using KRSrcWorkflow.Interfaces;
using KRSrcWorkflow.Abstracts;
using RabbitMQ.Client;

namespace KRSrcWorkflow.Config
{
	// assembly@AssemblyDll:type@TypeName:queue@QueueName:host@HostOrIp:queuetype@(rabbitmq,.netqueue,msmq)
	public class WFTargetData
	{
		public string AssemblyDll { get; set; }
		public string AssemblyType { get; set; }
		public string QueueName { get; set; }
		public string QueueHost { get; set; }
		public string QueueType { get; set; }

		public WFTargetData(string data)
		{
			this.AssemblyDll = string.Empty; // "KRSrcWorkflow.dll";
			this.AssemblyType = string.Empty; // "ProcessorData";
			this.QueueName = string.Empty;
			this.QueueHost = System.Net.Dns.GetHostName();
			this.QueueType = "msmq";

			if (!data.Contains("@"))
				data = "type@" + data;

			string[] splitdata = data.Split(new char[] { ':' });
			foreach (string split in splitdata)
			{
				string[] elemdata = split.Split(new char[] { '@' });
				if (elemdata.Length == 2)
				{
					switch (elemdata[0])
					{
						case "assembly":
							this.AssemblyDll = elemdata[1];
							break;
						case "type":
							this.AssemblyType = elemdata[1];
							break;
						case "host":
							this.QueueHost = elemdata[1];
							break;
						case "queue":
							this.QueueName = elemdata[1];
							break;
						case "queuetype":
							this.QueueType = elemdata[1];
							break;
					}
				}
			}
		}
	}

	// assembly@AssemblyDll:type@TypeName:queue@QueueName:host@HostOrIp
	public class WFTarget
	{
		private static readonly string CreateWFMessageQueue = "CreateWFMessageQueue";
		private static Dictionary<string, IWFMessageQueue> TargetQueues { get; set; }

		private string AssemblyPath { get; set; }
		public Type AssemblyType { get; private set; }
//		public object GenericQueue { get; private set; }
//		public System.Messaging.MessageQueue MessageQueue { get; private set; }
		public IWFMessageQueue MessageQueue { get; private set; }
		public object AssemblyTypeInstance { get; set; }

		public override string ToString()
		{
			return this.AssemblyType.Name;
		}

		public WFTarget() : this(string.Empty)
		{
		}

		public WFTarget(string data)
			: this(data, string.Empty)
		{
		}

		public WFTarget(string data, string assemblycache)
			: this(new WFTargetData(data), assemblycache)
		{
		}

		Assembly currentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			//This handler is called only when the common language runtime tries to bind to the assembly and fails.
			//Load the assembly from the specified path.
			Assembly MyAssembly = Assembly.LoadFrom(this.AssemblyPath + @"\" + (new AssemblyName(args.Name)).Name + ".dll");

			//Return the loaded assembly.
			return MyAssembly;
		}

		public WFTarget(WFTargetData wftargetdata, string assemblycache)
		{
			if (WFTarget.TargetQueues == null)
				WFTarget.TargetQueues = new Dictionary<string, IWFMessageQueue>();

			if (wftargetdata.AssemblyType != string.Empty && wftargetdata.AssemblyDll != string.Empty)
			{
				this.AssemblyPath = Directory.GetCurrentDirectory();

//				if (wftargetdata.AssemblyType != "ProcessorData" && !string.IsNullOrEmpty(assemblycache))
				if (!string.IsNullOrEmpty(assemblycache))
					this.AssemblyPath = string.Format(@"{0}\{1}", assemblycache, Path.GetFileNameWithoutExtension(wftargetdata.AssemblyDll));

				Assembly assembly = string.IsNullOrEmpty(wftargetdata.AssemblyDll) ? Assembly.GetExecutingAssembly() : Assembly.LoadFrom(this.AssemblyPath + @"\" + wftargetdata.AssemblyDll);
				if (assembly != null)
				{
					//The AssemblyResolve event is called when the common language runtime tries to bind to the assembly and fails.
					AppDomain currentDomain = AppDomain.CurrentDomain;
					currentDomain.AssemblyResolve += new ResolveEventHandler(currentDomain_AssemblyResolve);
					try
					{
						this.AssemblyType = assembly.GetType(assembly.GetTypes().Where(x => (x.Name.ToUpper() == wftargetdata.AssemblyType.ToUpper()) || (x.FullName.ToUpper() == wftargetdata.AssemblyType.ToUpper())).Select(x => x.FullName).FirstOrDefault());
						this.AssemblyTypeInstance = assembly.CreateInstance(this.AssemblyType.FullName);
					}
					catch (Exception ex)
					{
						throw new Exception("", ex);
					}
					finally
					{
						currentDomain.AssemblyResolve -= new ResolveEventHandler(currentDomain_AssemblyResolve);
					}
				}
				else
					throw new Exception("");
			}

			if (wftargetdata.QueueName != string.Empty)
			{
				string ip = string.Empty;
				if (WFUtilities.SetHostAndIPAddress(wftargetdata.QueueHost, ref ip))
				{
					Type type = null;
					int port = 0;
					WFMessageQueueType publisherorconsumer = WFMessageQueueType.None;

					switch (wftargetdata.QueueType)
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
							publisherorconsumer = WFMessageQueueType.Publisher;
							if (this is WFSrc)
								publisherorconsumer = WFMessageQueueType.Consumer;
							break;

						default:
							break;
					}

					if (WFTarget.TargetQueues.ContainsKey(wftargetdata.QueueName))
						this.MessageQueue = WFTarget.TargetQueues[wftargetdata.QueueName];
					else
					{
						Type genericqueuetype = typeof(IWFMessageQueueFactory<,>).MakeGenericType(new[] { type, this.AssemblyType == null ? typeof(ProcessorData) : this.AssemblyType });
						this.MessageQueue = (IWFMessageQueue)genericqueuetype.GetMethod(WFTarget.CreateWFMessageQueue, new object[] { ip, port, wftargetdata.QueueName, publisherorconsumer }.Select(p => p.GetType()).ToArray()).Invoke(null, new object[] { ip, port, wftargetdata.QueueName, publisherorconsumer });
						WFTarget.TargetQueues[wftargetdata.QueueName] = this.MessageQueue;
					}
					//					genericqueuetype = typeof(WFMessageQueue<>).MakeGenericType(new[] { this.AssemblyType });
					//					this.GenericQueue = Activator.CreateInstance(genericqueuetype, new object[] { @"FormatName:Direct=TCP:" + ip + @"\Private$\" + queuename });
					//					this.MessageQueue = (System.Messaging.MessageQueue)genericqueuetype.GetProperty("MessageQueue").GetGetMethod().Invoke(this.GenericQueue, new object[0]);
				}
				else
					throw new Exception("");
			}
		}

        private IWFMessageQueue GetOrCreateLocalQueue(string queuename, string ip)
	    {
#if DEBUG
            string fullyQualified = WFTarget.SanitizeQueue(queuename);
            if (!System.Messaging.MessageQueue.Exists(fullyQualified))
            {
                System.Messaging.MessageQueue.Create(fullyQualified);
            }
#endif 
	        Type genericqueuetype = typeof(IWFMessageQueueFactory<,>).MakeGenericType(new[] { typeof (System.Messaging.MessageQueue), this.AssemblyType });
	        return (IWFMessageQueue)genericqueuetype.GetMethod("CreateWFMessageQueue").Invoke(null, new object[] {ip, queuename});
	    }

        private static string SanitizeQueue(string name)
        {
            return (name.IndexOf('\\') > 0) ? name : string.Format(".\\Private$\\{0}", name);
        }
	}
}
