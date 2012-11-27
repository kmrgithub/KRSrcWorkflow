using System;
using System.Reflection;
using System.ServiceModel;

using KRSrcWorkflow.Interfaces;
using KRSrcWorkflow.Interfaces.Wcf;

namespace KRSrcWorkflow.Abstracts
{
	[Serializable]
	public abstract class WFProcessor : IDisposable, IWFProcessor, IWFObjectProcessor<WFProcessingResult>, IWFClientAdmin
	{
		public static WFProcessor CreateInstance(Type type, string filetoprocess, Guid parenttrackingid, string exportdirectory)
		{
			WFProcessor processor = null;

			try
			{
				processor = (WFProcessor)Activator.CreateInstance(type, new object[0]);
				processor.FileToProcess = filetoprocess;
				processor.ParentTrackingId = parenttrackingid;
				processor.ExportDirectory = exportdirectory;

			}
			catch (Exception ex)
			{
				processor = null;
				WFLogger.NLogger.ErrorException("ERROR: WFProcessor.CreateInstance failed!", ex);
			}

			return processor;
		}

		public abstract WFState Run();

		[System.Xml.Serialization.XmlIgnore]
		public bool OutputFilesSpecified { get; set; }

		[System.Xml.Serialization.XmlIgnore]
		private string IPAddress { get; set; }

		[System.Xml.Serialization.XmlIgnore]
		private uint Port { get; set; }

		[System.Xml.Serialization.XmlIgnore]
		private Interfaces.Wcf.IWFManagerAdmin WFManagerAdminProxy { get; set; }

		public WFFileList OutputFiles { get; set; }
		public string FileToProcess { get; set; }
		public Guid TrackingId { get; set; }
		public Guid ParentTrackingId { get; set; }
		public string ExportDirectory { get; set; }

//		private MethodInfo SetProcessedObject { get; set; }

		public void Interrupt()
		{
		}

		protected WFProcessor() : this(8000)
		{
		}

		protected WFProcessor(uint port)
		{
			this.OutputFilesSpecified = true;
			this.OutputFiles = new WFFileList();
			this.FileToProcess = string.Empty;
			this.TrackingId = Guid.NewGuid();
			this.ParentTrackingId = this.TrackingId;
			this.WFManagerAdminProxy = null;
			this.ExportDirectory = string.Empty;
			this.IPAddress = string.Empty;
			this.Port = port;

//			try
//			{
//				this.SetProcessedObject = typeof(WFProcessingResult).GetMethod("SetProcessedObject").MakeGenericMethod(new [] { this.GetType() });
//			}
//			catch (Exception ex)
//			{
//				throw new Exception(String.Format("No method named SetProcessedObject for type: {0}", this.GetType().FullName), ex); 
//			}

#if false
			string host = System.Net.Dns.GetHostName();
			string ipaddress = string.Empty;
			if (WFUtilities.SetHostAndIPAddress(host, ref ipaddress))
			{
				this.IPAddress = ipaddress;
				this.Port = port;
			}
#endif
		}

		public WFProcessingResult Process()
		{
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

			WFProcessingResult result = new WFProcessingResult();

			if (this.FileToProcess != string.Empty)
			{
				WFLogger.NLogger.Debug("Start processing file {0}", this.FileToProcess);
				result.State.Value = Run().Value;
				WFLogger.NLogger.Debug("End processing file {0}", this.FileToProcess);
			}
			else
				WFLogger.NLogger.Error("DocumentToProcess is not set for type: {0}", this.GetType().FullName);
			result.EndTime = DateTime.Now;

#if true
			result.ProcessedObject = this;
#else
			this.OutputFilesSpecified = false;
			try
			{
				this.SetProcessedObject.Invoke(result, new object[] { this });
			}
			catch (Exception ex)
			{
				WFLogger.NLogger.ErrorException(String.Format("SetProcessedObject invocation failed for type: {0}", this.GetType().FullName), ex);
			}
//			result.GetType().GetMethod("SetProcessedObject").MakeGenericMethod(new Type[] { this.GetType() }).Invoke(this, new object[] { this });
			this.OutputFilesSpecified = true;
#endif

			return result;
		}

		public void Dispose()
		{
		}
	}
}
