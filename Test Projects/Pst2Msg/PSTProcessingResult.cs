using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NLog;

namespace Pst2Eml
{
	public class PSTProcessingResult
	{
		public T GetProcessingObject<T>()
		{
			T t = default(T);

			if (!this.PSTProcessedObjectType.Equals(typeof(T).Name))
				return t;

			System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
			using (System.IO.StringReader reader = new System.IO.StringReader(this.PSTProcessedObject))
			{
				try
				{
					t = (T)serializer.Deserialize(reader);
				}
				catch (Exception ex)
				{
					Logger.NLogger.ErrorException(string.Empty, ex);
				}
				return t;
			}
		}
		public void SetProcessingObject<T>(T t)
		{
			this.PSTProcessedObject = string.Empty;
			System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
			using (System.IO.StringWriter writer = new System.IO.StringWriter())
			{
				serializer.Serialize(writer, t);

				this.PSTProcessedObject = writer.ToString();
				this.PSTProcessedObjectType = typeof(T).Name;
			}
		}

		public bool IsSuccessful { get; set; }
		public string Filename { get; set; }
		public string PSTProcessedObject { get; set; }
		public string PSTProcessedObjectType { get; set; }

		public PSTProcessingResult()
		{
			this.IsSuccessful = false;
			this.Filename = string.Empty;
			this.PSTProcessedObject = null;
			this.PSTProcessedObjectType = string.Empty;
		}
	}
}
