using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Management;
using System.Runtime.InteropServices;
using System.Configuration;

using ProcessorManagement.Interfaces;
using ProcessorManagement.Data;

namespace ProcessorController
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public partial class ProcessorController : ServiceBase, IProcessorController
	{
		private static readonly string ProcessorControllerHostDefault = System.Net.Dns.GetHostName();
		private static readonly string ProcessorControllerWCFPortDefault = "8002";

		private enum ConsoleCtrlEvent
		{
			CTRL_C = 0,
			CTRL_BREAK = 1,
			CTRL_CLOSE = 2,
			CTRL_LOGOFF = 5,
			CTRL_SHUTDOWN = 6
		}

		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		static extern bool GenerateConsoleCtrlEvent(ConsoleCtrlEvent sigevent, int dwProcessGroupId);

		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		static extern bool AttachConsole(int dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		static extern bool FreeConsole();		

		private ServiceHost ServiceHost { get; set; }
		private string Repository { get; set; }
		private string ProcessorControllerHost { get; set; }
		private string ProcessorControllerWCFPort { get; set; }

		public ProcessorController(string repository)
			: base()
		{
			this.Repository = repository;
		}

		public ProcessorController()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			this.ProcessorControllerHost = ConfigurationManager.AppSettings["ProcessorControllerHost"] != null ? ConfigurationManager.AppSettings["ProcessorControllerHost"] : ProcessorController.ProcessorControllerHostDefault;
			this.ProcessorControllerWCFPort = ConfigurationManager.AppSettings["ProcessorControllerWCFPort"] != null ? ConfigurationManager.AppSettings["ProcessorControllerWCFPort"] : ProcessorController.ProcessorControllerWCFPortDefault;

			try
			{
				this.ServiceHost = new ServiceHost(this, new Uri(string.Format("net.tcp://{0}:{1}/ProcessorControllerWCF", this.ProcessorControllerHost, this.ProcessorControllerWCFPort)));
				this.ServiceHost.AddServiceEndpoint(typeof(IProcessorController), new NetTcpBinding(), "");

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
			if (this.ServiceHost != null)
				this.ServiceHost.Close();
		}

		public void StartService(string[] args)
		{
			this.OnStart(args);
		}

		public void StopService()
		{
			this.OnStop();
		}

		public bool StopProcessor(string pid)
		{
			bool retval = AttachConsole(Convert.ToInt32(pid));
			if (retval == true)
			{
				Console.WriteLine("quit");
				FreeConsole();
			}
//			(new ManagementObjectSearcher(string.Format("select * from Win32_Process where ProcessId = {0}", pid))).Get().Cast<ManagementObject>().ToList().ForEach(x => GenerateConsoleCtrlEvent(ConsoleCtrlEvent.CTRL_C, (int)((uint)x["SessionId"])));
			Process process = Process.GetProcessById(Convert.ToInt32(pid));
			if (process != null)
			{
				process.Kill();
//				System.IO.StreamWriter wr = process.StandardInput;
//				wr.WriteLine("\x3");
//				wr.Flush();
//				wr.Close();
			}
//			GenerateConsoleCtrlEvent(ConsoleCtrlEvent.CTRL_C, Convert.ToInt32(pid));
			return true;
		}

		public bool StartProcessor()
		{
			return true;
		}

		public ProcessorDataList ListProcessors()
		{
			ProcessorDataList pdl = new ProcessorDataList();
			foreach (ProcessorData pd in (new ManagementObjectSearcher("select * from Win32_Process where Name like 'WFProcessor%'")).Get().Cast<ManagementObject>().Select(x => new ProcessorData() { ProcessorName = (string)x["Name"], Location = System.Net.Dns.GetHostName(), CommandLine = (string)x["CommandLine"], Pid = ((uint)x["ProcessId"]).ToString() }))
				pdl.Add(pd);
			return pdl; // (new ManagementObjectSearcher("select * from Win32_Process where Name like 'WFProcessor%'")).Get().Cast<ManagementObject>().Select(x => new ProcessorData() { ProcessorName = (string)x["Name"], Location = System.Net.Dns.GetHostName(), CommandLine = (string)x["CommandLine"], Pid = ((uint)x["ProcessId"]).ToString() }).ToList();
//			return Process.GetProcesses().Where(x => x.ProcessName == "WFProcessor").Select(x => new ProcessorData() { ProcessorName = x.ProcessName, Location = x.MachineName, CommandLine = string.Empty, Pid = x.Id.ToString() }).ToList();
		}
	}
}
