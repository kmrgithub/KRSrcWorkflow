using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace ConfigurationServer
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string [] args)
		{
			bool serviceMode = false;
			ConfigurationServer cs = new ConfigurationServer();

			try
			{
				ServiceController svcControl = new ServiceController("ConfigurationServer");
				if (svcControl != null)
					serviceMode = (svcControl.Status == ServiceControllerStatus.StartPending);
			}
			catch (Exception)
			{
			}

			if (serviceMode)
			{
				ServiceBase[] ServicesToRun;
				ServicesToRun = new ServiceBase[] 
				{
					cs 
				};
				ServiceBase.Run(ServicesToRun);
			}
			else
			{
				cs.StartService(args);

				Console.CancelKeyPress += delegate
				{
					cs.StopService();
					Environment.Exit(0);
				};

				Thread.CurrentThread.Join();
			}
		}
	}
}
