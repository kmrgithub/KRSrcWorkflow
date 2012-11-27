using System;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Reflection;
using System.Linq;

namespace KRSrcWorkflow.Abstracts
{
	[Serializable]
	public class ProcessorData : MarshalByRefObject, ICloneable
	{
		private static readonly string StateRootElement = "ProcessorDataState";
		private static readonly string XmlHeader = @"<?xml version=""1.0""?>";
		public static ProcessorData CreateInstance(Type type, string doctoprocess, Guid parenttrackingid, string exportdirectory)
		{
			ProcessorData processor = null;

			try
			{
				processor = (ProcessorData)Activator.CreateInstance(type, new object[0]);
				processor.DocumentToProcess = doctoprocess;
				processor.ParentTrackingId = parenttrackingid;
				processor.ExportDirectory = exportdirectory;

			}
			catch (Exception ex)
			{
				processor = null;
				WFLogger.NLogger.ErrorException("ERROR: ProcessorData.CreateInstance failed!", ex);
			}

			return processor;
		}

		public string DocumentToProcess { get; set; }
		public string ExportDirectory { get; set; }
		public WFFileList OutputDocuments { get; set; }
		public WFState WFState { get; set; }
		
		public Guid TrackingId { get; set; }
		public Guid ParentTrackingId { get; set; }

		public string State { get; set; }

		public string TypeName { get; set; }
		public string TypeFullName { get; set; }

		public DateTime EnqueueTime { get; set; }
		public DateTime DequeueTime { get; set; }

		public DateTime ProcessStartTime { get; set; }
		public DateTime ProcessEndTime { get; set; }

		public ProcessorData()
		{
			this.TypeFullName = this.GetType().FullName;
			this.TypeName = this.GetType().Name;
			this.OutputDocuments = new WFFileList();
			this.WFState = new WFState();
			this.State = string.Format(@"{0}<{1}></{2}>", ProcessorData.XmlHeader, ProcessorData.StateRootElement, ProcessorData.StateRootElement);
			this.DocumentToProcess = string.Empty;
			this.TrackingId = Guid.NewGuid();
			this.ParentTrackingId = this.TrackingId;
			this.ExportDirectory = string.Empty;
			this.EnqueueTime = DateTime.UtcNow;
			this.DequeueTime = this.EnqueueTime;
			this.ProcessStartTime = DateTime.UtcNow;
			this.ProcessEndTime = this.ProcessStartTime;
		}

		public void AddOutputDocument(string document)
		{
			this.OutputDocuments.Add(document);
		}

		private string XmlSerializeType<T>(T value)
		{
			try
			{
				XmlSerializer serializer = new XmlSerializer(typeof(T));
				StringWriter writer = new StringWriter();
				serializer.Serialize(writer, value);
				return writer.ToString();
			}
			catch (Exception ex)
			{
                WFLogger.NLogger.ErrorException("ERROR: ProcessorData.CreateInstance failed!", ex);
            }

			return string.Empty;
		}

		public T GetProperty<T>(string name)
		{
			return this.GetProperty<T>(name, this.GetType().Name);
		}

		public T GetProperty<T>(string name, string type)
		{
			return this.GetProperty<T>(name, type, null);
		}

		public T GetProperty<T>(string name, Func<XmlDocument, T> converter)
		{
			return this.GetProperty<T>(name, this.GetType().Name, null);
		}

		public T GetProperty<T>(string name, string type, Func<XmlDocument, T> converter)
		{
			try
			{
				type = string.IsNullOrEmpty(type) ? this.GetType().Name : type;

				// get node for this instance of ProcessorData
				XmlDocument xmldoc = new XmlDocument();
				xmldoc.LoadXml(this.State);
				XmlNode xmlnode = xmldoc.DocumentElement.SelectSingleNode(string.Format(@"/{0}/{1}/{2}", ProcessorData.StateRootElement, type, name)); //GetElementById(this.GetType().Name);
				if (xmlnode != null)
				{
					string xml = string.Format(@"{0}{1}", ProcessorData.XmlHeader, xmlnode.InnerXml);
					if (converter != null)
					{
						XmlDocument xmldocvalue = new XmlDocument();
						xmldocvalue.LoadXml(xml);
						return converter(xmldocvalue);
					}
					else
						return (T)(new XmlSerializer(typeof(T))).Deserialize(new System.IO.StringReader(xml));
				}
			}
			catch (Exception ex)
			{
                WFLogger.NLogger.ErrorException("ERROR: GetProperty failed!", ex);
            }

			return default(T);
		}

		public void SetProperty<T>(string name, T value)
		{
			SetProperty<T>(name, value, null);
		}

		private void SetProperty<T>(string name, T value, Func<T, XmlDocument> converter)
		{
			XmlDocument xmldocvalue = null;
			if (converter != null)
				xmldocvalue = converter(value);
			else
			{
//				XmlSerializer serializer = new XmlSerializer(typeof(T));
//				StringWriter writer = new StringWriter();
//				serializer.Serialize(writer, value);
				xmldocvalue = new XmlDocument();
				xmldocvalue.LoadXml(XmlSerializeType<T>(value)); //writer.ToString());
			}

			// get node for this instance of ProcessorData
			XmlDocument xmldoc = new XmlDocument();
			xmldoc.LoadXml(this.State);

			// get the xmlnode for this ProcessorData type
			XmlNode xmlnodetype = xmldoc.DocumentElement.SelectSingleNode(string.Format(@"/{0}/{1}", ProcessorData.StateRootElement, this.GetType().Name));
			if (xmlnodetype == null)
			{
				XmlElement elem = xmldoc.CreateElement(this.GetType().Name);
				xmldoc.DocumentElement.AppendChild(elem);
				xmlnodetype = xmldoc.DocumentElement.SelectSingleNode(string.Format(@"/{0}/{1}", ProcessorData.StateRootElement, this.GetType().Name));
			}

			// get xmlnode for name under this ProcessorData type
			XmlNode xmlnode = xmldoc.DocumentElement.SelectSingleNode(string.Format(@"/{0}/{1}/{2}", ProcessorData.StateRootElement, this.GetType().Name, name)); //GetElementById(this.GetType().Name);
			if (xmlnode == null)
			{
				XmlElement elem = xmldoc.CreateElement(name);
				xmlnodetype.AppendChild(elem);
				elem.InnerXml = xmldocvalue.DocumentElement.OuterXml;
			}
			else
				xmlnode.InnerXml = xmldocvalue.DocumentElement.OuterXml;

			this.State = xmldoc.OuterXml;
		}

		public override string ToString()
		{
			return base.ToString();
		}

		public object Clone()
		{
			ProcessorData procdata = new ProcessorData();
			procdata.DequeueTime = this.DequeueTime;
			procdata.DocumentToProcess = this.DocumentToProcess;
			procdata.EnqueueTime = this.EnqueueTime;
			procdata.ExportDirectory = this.ExportDirectory;
			procdata.ParentTrackingId = this.ParentTrackingId;
			procdata.ProcessEndTime = this.ProcessEndTime;
			procdata.ProcessStartTime = this.ProcessStartTime;
			procdata.State = this.State;
			procdata.TrackingId = this.TrackingId;
//			procdata.TypeFullName = this.TypeFullName;
//			procdata.TypeName = this.TypeName;
			procdata.WFState = this.WFState;
			procdata.OutputDocuments.AddRange(this.OutputDocuments);

			return procdata;
		}
	}
}
