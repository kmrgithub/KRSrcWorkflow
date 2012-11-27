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
	public partial class PSTFolderParser : PSTObjectParser<PSTFolderParser>
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
			> ProcessedFolders { get; set; }

#if USE_ISYS
		public SubFile SubFile { get; set; }
#endif
		public OutputFilesList OutputFiles { get; set; }
		public PSTFolderParser(string pstfile,
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
			: base(pstfile, entryid, exportdir)
		{
			this.OutputFiles = new OutputFilesList();
			this.ProcessedFolders = new List<
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

		public PSTFolderParser()
			: this(string.Empty,
#if USE_PSTSDK
			0
#else
			string.Empty
#endif
			, string.Empty)
		{
		}

#if !USE_REDEMPTION && !USE_PSTSDK && !USE_ASPOSE && !USE_ISYS
		protected override void ProcessQueueObjectImplementation(List<InputOutputThreadObjectHandler> outputobjects, Queue<InputOutputThreadObjectHandler> queue)
		{
		}
#endif
	}
}
