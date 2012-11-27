using System;

using KRSrcWorkflow.Interfaces.Wcf;

namespace WFManager
{
	public class TrackingData
	{
		public bool Notified { get; set; }
		public Guid Guid { get; set; }
		public string Filename { get; set; }
		public IWFClientProcessing WFClient { get; set; }

		public TrackingData(Guid guid, string filename, IWFClientProcessing wfclient)
		{
			this.Guid = guid;
			this.Filename = filename;
			this.Notified = false;
			this.WFClient = wfclient;
		}

		public TrackingData(Guid guid, string filename) :
			this(guid, filename, null)
		{
		}

		public TrackingData(Guid guid) :
			this(guid, string.Empty, null)
		{
		}

		public override string  ToString()
		{
	 	 return string.Format("Guid={0}  Filename={1}  Notified={2}", this.Guid.ToString(), this.Filename, this.Notified.ToString());
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(Object obj)
		{
			TrackingData t = (TrackingData)obj;
			if(t.Filename == string.Empty)
				return t.Guid == this.Guid;
			else if (t.Guid == Guid.Empty)
				return t.Filename == this.Filename;
			return t.Guid == this.Guid && t.Filename == this.Filename;
		}
	}
}
