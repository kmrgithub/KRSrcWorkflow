using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;

#if USE_PSTSDK
using pstsdk.layer.pst;
using pstsdk.definition.pst;
using pstsdk.definition.pst.folder;
using pstsdk.definition.pst.message;
using pstsdk.definition.util.primitives;
#elif USE_ASPOSE
using Aspose.Network.Outlook.Pst;
#elif USE_REDEMPTION
using Redemption;
#elif USE_ISYS
using ISYS.DocumentFilters;
#endif

namespace Pst2Eml
{
	[Serializable]
	public abstract class PSTObjectParser<T> : InputOutputThreadObjectHandler<PSTProcessingResult>
	{
		public PSTObjectParser() : this(string.Empty, 
#if USE_REDEMPTION
			string.Empty
#elif USE_ASPOSE
			string.Empty
#elif USE_ISYS
			string.Empty
#elif USE_PSTSDK
			0
#endif			
			, string.Empty)
		{
		}

		protected abstract PSTProcessingResult ProcessQueueObjectImplementation();
//		protected abstract void ProcessQueueObjectImplementation(List<InputOutputThreadObjectHandler<T>> outputobjects, InputOutputMessageQueue<T> inqueue, InputOutputMessageQueue<T> outqueue);
#if USE_PSTSTORE
		public
#if USE_PSTSDK
			IPst
#elif USE_ASPOSE
			PersonalStorage
#elif USE_ISYS
			Extractor
#elif USE_REDEMPTION
			RDOSession
			//			RDOPstStore
#endif
			PstStore { get; set; }
#endif
		public string PSTFile { get; set; }
		public string ExportDirectory { get; set; }
		public
#if USE_PSTSDK
			NodeID
#elif USE_ASPOSE
			string
#elif USE_ISYS
			string
#elif USE_REDEMPTION
			string
#endif
		EntryID { get; set; }

		public string ConvertEntryIdToString()
		{
#if USE_ASPOSE
			char[] c = new char[this.EntryID.Length * 2];
			byte b;
			for (int i = 0; i < this.EntryID.Length; ++i)
			{
				b = ((byte)(this.EntryID[i] >> 4));
				c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);
				b = ((byte)(this.EntryID[i] & 0xF));
				c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
			}
			return new string(c);
#else
			return this.EntryID.ToString();
#endif
		}

		public PSTObjectParser(string pstfile,
#if USE_PSTSDK
			NodeID
#elif USE_ASPOSE
			string
#elif USE_ISYS
			string
#else
			string
#endif
			entryid,
			string exportdir)
		{
			this.PSTFile = pstfile;
			this.EntryID = entryid;
			this.ExportDirectory = exportdir;
		}

		public override string ToString()
		{
			return string.Format(this.GetType().ToString() + ": PSTFile={0} EntryID={1} ExportDirectory={2}", this.PSTFile, this.EntryID, this.ExportDirectory);
		}

		public PSTProcessingResult ProcessQueueObject() //InputOutputMessageQueue<T> inqueue, InputOutputMessageQueue<T> outqueue)
		{
			List<InputOutputThreadObjectHandler<T>> outputobjects = new List<InputOutputThreadObjectHandler<T>>();
			int start = System.Environment.TickCount;

#if USE_PSTSTORE
			Monitor.Enter(this.PstStore);
#endif
			Logger.NLogger.Info("{0}: Processing {1} {2}", DateTime.Now.ToString("HHmmss"), this.GetType().ToString(), this.EntryID);
			PSTProcessingResult result = ProcessQueueObjectImplementation(); //outputobjects, inqueue, outqueue);

#if USE_PSTSTORE
			Monitor.Exit(this.PstStore);
#endif

			Logger.NLogger.Info(DateTime.Now.ToString("HHmmss") + ": " + this.GetType().ToString() + ".ProcessQueueObject: " + (System.Environment.TickCount - start));

			return result;
		}
	}
}
