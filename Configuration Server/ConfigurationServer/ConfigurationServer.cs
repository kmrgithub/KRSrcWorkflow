using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Xml.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Configuration;

using ConfigurationServer.Interfaces;
using ConfigurationServer.Data;

namespace ConfigurationServer
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public partial class ConfigurationServer : ServiceBase, IConfigurationServer
	{
		private static readonly string ConfigurationServerHostDefault = System.Net.Dns.GetHostName();
		private static readonly string ConfigurationServerWCFPortDefault = "8001";
		private ServiceHost ServiceHost { get; set; }
		private WorkflowData WorkflowData { get; set; }
		private string ConfigurationServerHost { get; set; }
		private string ConfigurationServerWCFPort { get; set; }

		public ConfigurationServer()
		{
			this.WorkflowData = null;
			InitializeComponent();

			XmlSerializer mySerializer = new XmlSerializer(typeof(WorkflowData));
			// To read the file, create a FileStream.
			System.IO.FileStream myFileStream = new System.IO.FileStream("workflowdata.xml", System.IO.FileMode.Open);
			// Call the Deserialize method and cast to the object type.
			this.WorkflowData = (WorkflowData)mySerializer.Deserialize(myFileStream);
			myFileStream.Close();
			myFileStream.Dispose();
		}

		protected override void OnStart(string[] args)
		{
			this.ConfigurationServerHost = ConfigurationManager.AppSettings["ConfigurationServerHost"] != null ? ConfigurationManager.AppSettings["ConfigurationServerHost"] : ConfigurationServer.ConfigurationServerHostDefault;
			this.ConfigurationServerWCFPort = ConfigurationManager.AppSettings["ConfigurationServerWCFPort"] != null ? ConfigurationManager.AppSettings["ConfigurationServerWCFPort"] : ConfigurationServer.ConfigurationServerWCFPortDefault;

			try
			{
				this.ServiceHost = new ServiceHost(this, new Uri(string.Format("net.tcp://{0}:{1}/ConfigurationServerWCF", this.ConfigurationServerHost, this.ConfigurationServerWCFPort)));
				this.ServiceHost.AddServiceEndpoint(typeof(IConfigurationServer), new NetTcpBinding(), "");

				ServiceThrottlingBehavior throttling = new ServiceThrottlingBehavior { MaxConcurrentCalls = 1000, MaxConcurrentSessions = 1000 };
				this.ServiceHost.Description.Behaviors.Add(throttling);

//				ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
//				smb.HttpGetEnabled = true;
//				this.ServiceHost.Description.Behaviors.Add(smb);

				this.ServiceHost.Open();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
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

		public LocationDataDictionary GetLocations()
		{
			return this.WorkflowData.GetLocations();
		}
	}
}
