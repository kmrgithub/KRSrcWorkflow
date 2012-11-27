using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace ProcessorManager
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			bool serviceMode = false;
			ProcessorManager pm = new ProcessorManager();

			try
			{
				ServiceController svcControl = new ServiceController("ProcessorManager");
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
					pm 
				};
				ServiceBase.Run(ServicesToRun);
			}
			else
			{
				pm.StartService(args);

				Console.CancelKeyPress += delegate
				{
					pm.StopService();
					Environment.Exit(0);
				};

				Thread.CurrentThread.Join();
			}
		}
	}
}
