using System;
using System.ServiceProcess;
using System.Threading;
using System.Diagnostics;

namespace ProcessorController
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string [] args)
		{
			string repository = System.AppDomain.CurrentDomain.BaseDirectory;
			for(int i = 0; i < args.Length; i++)
			{
				switch (args[i].ToUpper())
				{
					case "-REPOSITORY":
						repository = args[i + 1];
						break;
				}
			}

			bool serviceMode = false;
			ProcessorController pc = new ProcessorController(repository);

			try
			{
				ServiceController svcControl = new ServiceController("ProcessorController");
				if (svcControl != null)
					serviceMode = (svcControl.Status == ServiceControllerStatus.StartPending);
			}
			catch(Exception)
			{
			}

			if (serviceMode)
			{
				ServiceBase[] ServicesToRun;
				ServicesToRun = new ServiceBase[] 
				{
					pc 
				};
				ServiceBase.Run(ServicesToRun);
			}
			else
			{
				pc.StartService(args);

				Console.CancelKeyPress += delegate
				{
					pc.StopService();
					Environment.Exit(0);
				};

				Thread.CurrentThread.Join();
			}
		}
	}
}
