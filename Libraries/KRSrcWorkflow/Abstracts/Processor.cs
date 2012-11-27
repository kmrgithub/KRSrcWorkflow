using System;
using System.Reflection;
using System.ServiceModel;
using System.Linq;

using KRSrcWorkflow.Interfaces;
using KRSrcWorkflow.Interfaces.Wcf;

namespace KRSrcWorkflow.Abstracts
{
	public abstract class Processor<T> : IDisposable, IProcessor<T, ProcessorData>, IWFClientAdmin
		where T : ProcessorData
	{
		private string IPAddress { get; set; }
		private uint Port { get; set; }

		private Interfaces.Wcf.IWFManagerAdmin WFManagerAdminProxy { get; set; }

		protected Processor() : this(8000)
		{
		}

		protected Processor(uint port)
		{
			this.WFManagerAdminProxy = null;
			this.IPAddress = string.Empty;
			this.Port = port;

			string host = System.Net.Dns.GetHostName();
			string ipaddress = string.Empty;
			if (WFUtilities.SetHostAndIPAddress(host, ref ipaddress))
			{
				this.IPAddress = ipaddress;
				this.Port = port;
			}
#if false
			try
			{
				EndpointAddress ep = new EndpointAddress(new Uri(string.Format(@"http://{0}:{1}/WFManagerWCF/WFManagerWCF", this.IPAddress, this.Port)));//				, EndpointIdentity.CreateDnsIdentity("localhost"));
				this.WFManagerAdminProxy = DuplexChannelFactory<Interfaces.Wcf.IWFManagerAdmin>.CreateChannel(this, new WSDualHttpBinding(), ep);
				WFLogger.NLogger.Trace("Discovery {0}", string.Format(@"http://{0}:{1}/WFManagerWCF/WFManagerWCF", this.IPAddress, this.Port));
				this.WFManagerAdminProxy.Discovery();
				WFLogger.NLogger.Trace("Discovery {0} Leaving", string.Format(@"http://{0}:{1}/WFManagerWCF/WFManagerWCF", this.IPAddress, this.Port));
			}
			catch (Exception ex)
			{
				WFLogger.NLogger.ErrorException("ERROR: DuplexChannelFactory failed!", ex);
			}
#endif

		}

		public ProcessorData ProcessEx(T t)
		{
			WFLogger.NLogger.Debug("Start processing");
			t.ProcessStartTime = DateTime.UtcNow;
			Process(t);
			t.ProcessEndTime = DateTime.UtcNow;
			WFLogger.NLogger.Debug("End processing");

			t.TypeFullName = t.GetType().FullName;
			t.TypeName = t.GetType().Name;

			t.OutputDocuments.Where(x => x.Value.Value == WFState.WFStateUnknown).ToList().ForEach(x => x.Value.Value = t.WFState.Value);
	
			return t;
		}

		public virtual void Dispose()
		{
		}

		public void Interrupt()
		{
		}

		public abstract void Process(T t);
	}
}
