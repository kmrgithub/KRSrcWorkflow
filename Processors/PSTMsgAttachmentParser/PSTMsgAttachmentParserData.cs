using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSTMsgAttachmentParser
{
	public class PSTMsgAttachmentParserData : PSTMsgParser.PSTMsgParserData
	{
		public PSTMsgAttachmentParserData()
			: base()
		{
			this.SaveAsTypes = SaveAsType.None;
			this.SaveAttachments = true;
		}
	}
}
