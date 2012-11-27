using System;
using System.Collections.Generic;

#if USE_PSTSDK
using pstsdk.definition.util.primitives;
#elif USE_ISYS
using ISYS.DocumentFilters;
#endif

namespace Pst2Eml
{
	[Serializable]
	public partial class PSTMsgParser : PSTObjectParser<PSTMsgParser>
	{
		public class OutputFilesList : List<string>
		{
			public new void Add(string item)
			{
#if __USE_MSMQ
				base.Add(item);
#endif
			}
		}

		List<
#if USE_PSTSDK
			NodeID
#elif USE_ASPOSE
			string
#elif USE_ISYS
			string
#elif USE_REDEMPTION
			string
#endif
			> ProcessedMsgs { get; set; }

		[Flags]
		public enum SaveAsType { None = 0, Eml = 1, Html = 2, Text = 4, Xml = 8, Msg = 16, Rtf = 32, All = Eml & Html & Text & Msg & Rtf};

		public SaveAsType SaveAsTypes { get; set; }
		public bool SaveAttachments { get; set; }
		public NodeID FolderId { get; set; }

#if USE_ISYS
		public SubFile SubFile { get; set; }
#endif
		public OutputFilesList OutputFiles { get; set; }
		public PSTMsgParser(string pstfile,
#if USE_PSTSDK
			NodeID
#elif USE_ASPOSE
			string
#elif USE_ISYS
			string
#elif USE_REDEMPTION
			string
#endif
			entryid,
			NodeID folderid,
			string exportdir)
			: base(pstfile, entryid, exportdir)
		{
			this.FolderId = folderid;
			this.OutputFiles = new OutputFilesList();
			this.SaveAsTypes = SaveAsType.Eml;
			this.SaveAttachments = false;
			this.ProcessedMsgs = new List<
#if USE_PSTSDK
			NodeID
#elif USE_ASPOSE
			string
#elif USE_ISYS
			string
#elif USE_REDEMPTION
			string
#endif
			>();
		}

		public PSTMsgParser()
			: this(string.Empty,
#if USE_PSTSDK
			0
#else
			string.Empty
#endif
			, string.Empty)
		{
		}

		public PSTMsgParser(string pstfile,
#if USE_PSTSDK
 NodeID
#elif USE_ASPOSE
			string
#elif USE_ISYS
			string
#elif USE_REDEMPTION
			string
#endif
			entryid,
			string exportdir)
			: this(pstfile, entryid, 0, exportdir)
		{
		}
#if !USE_REDEMPTION && !USE_PSTSDK && !USE_ASPOSE && !USE_ISYS
		protected override void ProcessQueueObjectImplementation(List<InputOutputThreadObjectHandler> outputobjects, Queue<InputOutputThreadObjectHandler> queue)
		{
		}
#endif
	}
}
