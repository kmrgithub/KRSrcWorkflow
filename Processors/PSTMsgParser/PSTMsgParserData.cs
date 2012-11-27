using System;
using System.Collections.Generic;
using System.IO;

using pstsdk.definition.util.primitives;
using pstsdk.layer.pst;
using pstsdk.definition.pst;

using KRSrcWorkflow;
using KRSrcWorkflow.Abstracts;
using KRSrcWorkflow.CustomAttributes;

namespace PSTMsgParser
{
	[Serializable]
	public class PSTMsgParserData : ProcessorData
	{
		[Flags]
		public enum SaveAsType { None = 0, Eml = 1, Html = 2, Text = 4, Xml = 8, Msg = 16, Rtf = 32, All = Eml | Html | Text | Msg | Rtf };

        [KRSrcWorkflowAttribute(ProcessorDataType = "PSTFileParserData")]
		public string PSTFile { get; set; }
//		public string ExportDirectory { get; set; }
//		private NodeID EntryID { get; set; }
		public SaveAsType SaveAsTypes { get; set; }
		public bool SaveAttachments { get; set; }
		public bool SaveEmbeddedMsgs { get; set; }
		public string FolderPath { get; set; }
		public bool Pst2MsgCompatible { get; set; }

		public PSTMsgParserData(string pstfile, NodeID entryid, string exportdir)
		{
			this.SaveAsTypes = SaveAsType.Msg; // | SaveAsType.Xml | SaveAsType.Html;
			this.SaveAttachments = false;
			if(!string.IsNullOrEmpty(pstfile))
				this.PSTFile = pstfile;
			this.ExportDirectory = exportdir;
			this.SaveEmbeddedMsgs = false;
			this.FolderPath = string.Empty;
			this.Pst2MsgCompatible = false;
		}

		public PSTMsgParserData()
			: this(string.Empty, 0, string.Empty)
		{
		}
	}
}
