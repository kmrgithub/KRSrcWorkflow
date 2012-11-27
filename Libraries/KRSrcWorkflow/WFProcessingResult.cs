using System;
using System.Xml.Serialization;
using KRSrcWorkflow.Abstracts;
using System.Runtime.Serialization;
using System.Reflection;

using KRSrcWorkflow.Interfaces;

namespace KRSrcWorkflow
{
	[Serializable]
	public class WFProcessingResult
	{
		private static T ProcessedObjectDeserialize<T>(string serializedprocessobject) where T : WFProcessor
		{
			T t = default(T);

			try
			{
				System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
				System.IO.StringReader reader = new System.IO.StringReader(KRSrcWorkflow.WFUtilities.DecompressStringBase64(serializedprocessobject));
				t = (T)serializer.Deserialize(reader);
			}
			catch (Exception ex)
			{
				WFLogger.NLogger.ErrorException(String.Format("Deserialize failed for type: ", typeof(T).FullName), ex);
			}

			return t;
		}

		private static string ProcessedObjectSerialize<T>(T t) where T : WFProcessor
		{
			System.IO.StringWriter writer = null;
			try
			{
				System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
				writer = new System.IO.StringWriter();
				serializer.Serialize(writer, t);
			}
			catch (Exception ex)
			{
				WFLogger.NLogger.ErrorException(String.Format("Serialize failed for type: ", typeof(T).FullName), ex);
			}

			return KRSrcWorkflow.WFUtilities.CompressStringBase64(writer.ToString());
		}

		private WFProcessor _processedobject = null;
		[XmlIgnore]
		public WFProcessor ProcessedObject
		{
			get
			{
				if (_processedobject == null)
					_processedobject = (WFProcessor)typeof(WFProcessingResult).GetMethod("ProcessedObjectDeserialize", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(new[] { Type.GetType(this.ProcessedObjectType) }).Invoke(this, new object[1] { this.ProcessedObjectSerialized });
				return _processedobject;
			}

			set
			{
				if (value != null)
				{
					value.OutputFiles.ForEach(x => { if (x.Value.Value == WFState.WFStateUnknown) x.Value.Value = this.State.Value; });
					this.ProcessedObjectSerialized = (string)typeof(WFProcessingResult).GetMethod("ProcessedObjectSerialize", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(new[] { value.GetType() }).Invoke(this, new object[] { value });
					this.ProcessedObjectType = value.GetType().AssemblyQualifiedName;
				}
				_processedobject = value;
			}
		}

		public string ProcessedObjectSerialized { get; set; }
		public string ProcessedObjectType { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public WFState State { get; set; }

		public WFFileList OutputFiles()
		{
			if (_processedobject != null)
				return _processedobject.OutputFiles;
			return new WFFileList();
		}

		public string Filename()
		{
			if (_processedobject != null)
				return _processedobject.FileToProcess;
			return string.Empty;
		}

		public WFProcessingResult()
		{
			this.ProcessedObject = null;
			this.ProcessedObjectType = string.Empty;
			this.State = new WFState() { Value = string.Empty };
			this.StartTime = DateTime.Now;
			this.EndTime = this.StartTime;
		}
	}
}
