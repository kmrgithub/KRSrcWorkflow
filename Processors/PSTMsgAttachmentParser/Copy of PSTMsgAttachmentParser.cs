using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using pstsdk.definition.util.primitives;

namespace PSTMsgAttachmentParser
{
	public class PSTMsgAttachmentParser : PSTMsgParser.PSTMsgParser
	{
		public PSTMsgAttachmentParser()
			: base()
		{
			this.SaveAsTypes = SaveAsType.None;
			this.SaveAttachments = true;
		}
	}
}
