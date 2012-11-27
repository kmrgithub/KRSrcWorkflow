using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Threading;
using System.Configuration;
using System.ServiceModel;

using ConfigurationServer.Interfaces;
using ConfigurationServer.Data;
using ProcessorManagement.Interfaces;

namespace ProcessorManager
{
	public partial class ProcessorManager : ServiceBase
	{
		private static readonly string ConfigurationServerHostDefault = System.Net.Dns.GetHostName();
		private static readonly string ConfigurationServerWCFPortDefault = "8001";
		private static readonly string ProcessorControllerWCFPortDefault = "8002";
		private string ConfigurationServerHost { get; set; }
		private string ConfigurationServerWCFPort { get; set; }
		private string ProcessorControllerWCFPort { get; set; }

		public ProcessorManager()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			this.ConfigurationServerHost = ConfigurationManager.AppSettings["ConfigurationServerHost"] != null ? ConfigurationManager.AppSettings["ConfigurationServerHost"] : ProcessorManager.ConfigurationServerHostDefault;
			this.ConfigurationServerWCFPort = ConfigurationManager.AppSettings["ConfigurationServerWCFPort"] != null ? ConfigurationManager.AppSettings["ConfigurationServerWCFPort"] : ProcessorManager.ConfigurationServerWCFPortDefault;
			this.ProcessorControllerWCFPort = ConfigurationManager.AppSettings["ProcessorControllerWCFPort"] != null ? ConfigurationManager.AppSettings["ProcessorControllerWCFPort"] : ProcessorManager.ProcessorControllerWCFPortDefault;

			ThreadPool.QueueUserWorkItem(x =>
			{
				LocationDataDictionary prevlocations = new LocationDataDictionary();
				while (true)
				{
					try
					{
						IConfigurationServer configserver = ChannelFactory<IConfigurationServer>.CreateChannel(new NetTcpBinding(), new EndpointAddress(new Uri(string.Format("net.tcp://{0}:{1}/ConfigurationServerWCF", this.ConfigurationServerHost, this.ConfigurationServerWCFPort))));
						LocationDataDictionary currlocations = configserver.GetLocations();
						Console.WriteLine(currlocations.ToString());
						foreach (string location in currlocations.Keys)
						{
							try
							{
								IProcessorController proccontroller = ChannelFactory<IProcessorController>.CreateChannel(new NetTcpBinding(), new EndpointAddress(new Uri(string.Format("net.tcp://{0}:{1}/ProcessorControllerWCF", location, this.ProcessorControllerWCFPort))));
								ProcessorManagement.Data.ProcessorDataList pdl = proccontroller.ListProcessors();

								Console.WriteLine(pdl.ToString());
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex.Message);
							}

						}
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
					}

					Thread.Sleep(60000);
				}
			});
		}

		protected override void OnStop()
		{
		}

		public void StartService(string[] args)
		{
			this.OnStart(args);
		}

		public void StopService()
		{
			this.OnStop();
		}

	}
}
