using System;
using System.Globalization;

using KRSrcWorkflow;
using KRSrcWorkflow.Abstracts;

using pstsdk.definition.pst.message;
using pstsdk.definition.pst;
using pstsdk.layer.pst;

namespace PSTFileParser
{
	public class PSTFileParser : Processor<PSTFileParserData>
	{
		public override void Process(PSTFileParserData data)
		{
			data.WFState.Value = WFState.WFStateFail;
			try
			{
				IPst rdopststore = new Pst(data.DocumentToProcess);
				foreach (IMessage msg in rdopststore.Messages)
					data.OutputDocuments.Add(msg.Node.Value.ToString());
				rdopststore.Dispose();
				data.PSTFile = data.DocumentToProcess;
			}
			catch (Exception ex)
			{
				WFLogger.NLogger.ErrorException(String.Format("Message extraction for PST file: {0} failed.", data.DocumentToProcess), ex);
			}
			data.WFState.Value = WFState.WFStateSuccess;
		}
	}
}
