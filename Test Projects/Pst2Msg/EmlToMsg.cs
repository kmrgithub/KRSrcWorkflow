using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

#if USE_PSTSDK
using pstsdk.definition.util.primitives;
#endif
using Redemption;

namespace Pst2Eml
{
	[Serializable]
	class EmlToMsg : InputOutputThreadObjectHandler
	{
		public string ExportDirectory { get; set; }
		public 
#if USE_PSTSDK
			//NodeID
			string
#elif USE_ASPOSE
			byte []
#else
			string 
#endif
			EntryID { get; set; }

		public EmlToMsg(
#if USE_PSTSDK
			//NodeID
			string
#elif USE_ASPOSE
			byte []
#else
			string 
#endif
				entryid,
			string exportdir)
		{
			this.EntryID = entryid;
			this.ExportDirectory = exportdir;
		}

		public override string ToString()
		{
			return string.Format("EmlToMsg: EntryID={0} ExportDirectory={1}", this.EntryID, this.ExportDirectory);
		}

		public List<InputOutputThreadObjectHandler> ProcessQueueObject(Queue<InputOutputThreadObjectHandler> inqueue, Queue<InputOutputThreadObjectHandler> outqueue)
		{
			int start = System.Environment.TickCount;
			try
			{
				RDOSessionClass rdosession = new RDOSessionClass();
				RDOMail msg = rdosession.CreateMessageFromMsgFile(this.ExportDirectory + @"\" + this.EntryID + ".msg", Redemption.rdoItemType.olMailItem, Redemption.rdoMsgFileFormat.mffDefault);
				msg.Import(this.ExportDirectory + @"\" + this.EntryID + ".eml", Redemption.rdoSaveAsType.olRFC822);
				msg.Save();
				msg = null;
				rdosession = null;
//				GC.Collect();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			Console.WriteLine(DateTime.Now.ToString("HHMMss") + ": EmlToMsg.ProcessQueueObject: " + (System.Environment.TickCount - start));

			return null;
		}
	}
}
