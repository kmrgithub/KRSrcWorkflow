using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.InteropServices;

using ProcessorManagement.Interfaces;
using ProcessorManagement.Data;

namespace ProcessorControllerTest
{
	class ProcessorControllerTest
	{
		static void Main(string[] args)
		{
			IProcessorController proccontroller = ChannelFactory<IProcessorController>.CreateChannel(new WSDualHttpBinding(), new EndpointAddress(new Uri(string.Format("http://{0}:8001/ProcessorControllerWCF", "10.3.2.35"))));
			List<ProcessorData> ProcessorList = proccontroller.ListProcessors();
			ProcessorList.ForEach(x => Console.WriteLine(x.ToString()));
			Console.CancelKeyPress += delegate
			{
				Environment.Exit(0);
			};
			ProcessorList.ForEach(x => proccontroller.StopProcessor(x.Pid));
			System.Threading.Thread.CurrentThread.Join();
		}
	}
}
