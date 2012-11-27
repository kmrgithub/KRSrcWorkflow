using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Reflection;

using KRSrcWorkflow.Abstracts;
using KRSrcWorkflow.Interfaces.Wcf;
using KRSrcWorkflow.CustomAttributes;

namespace KRSrcWorkflow
{
	public class WFManagerProxy
	{
		IWFManagerProcessing Proxy { get; set; }

		public WFManagerProxy(string ipaddress, uint port, object callbackclass)
		{
			//IncludeExceptionDetailInFaults
			this.Proxy = DuplexChannelFactory<IWFManagerProcessing>.CreateChannel(callbackclass, new WSDualHttpBinding(), new EndpointAddress(new Uri(string.Format("http://{0}:{1}/WFManagerWCF/WFManagerWCF", ipaddress, port))));
		}

		public IWFManagerProcessing GetProxy()
		{
			return this.Proxy;
		}

		public Guid Execute(ProcessorData procdata)
		{
			Guid guid = Guid.Empty;

			if (this.Proxy != null)
			{
				// need to set property table for all properties having KRSrcWorkflow attribute
                foreach (PropertyInfo pi in procdata.GetType().GetProperties().Where(x => x.GetCustomAttributes(false).Count(y => y.GetType().Name == typeof(KRSrcWorkflowAttribute).Name) > 0))
				{
					MethodInfo method = procdata.GetType().GetMethod("SetProperty");
					MethodInfo generic = method.MakeGenericMethod(pi.PropertyType);
					generic.Invoke(procdata, new object[] { pi.Name, pi.GetValue(procdata, null)});

					Console.WriteLine(pi.Name);
				}

				guid = this.Proxy.Execute((ProcessorData)procdata.Clone());
			}

			return guid;
		}
	}
}
