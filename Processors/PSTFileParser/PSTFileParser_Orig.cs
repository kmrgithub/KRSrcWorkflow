using System;
using System.Globalization;
using SFWorkflow;
using SFWorkflow.Abstracts;

namespace PSTFileParser
{
	[Serializable]
	public class PSTFileParser : WFProcessor
	{
		public override WFState Run()
		{
			WFState retval = new WFState();
			try
			{
				retval.Value = WFState.WFStateFail;
				pstsdk.definition.pst.IPst rdopststore = new pstsdk.layer.pst.Pst(this.FileToProcess);
				foreach (pstsdk.definition.pst.message.IMessage msg in rdopststore.Messages)
					this.OutputFiles.Add(msg.Node.Value.ToString(CultureInfo.InvariantCulture));
				rdopststore.Dispose();
				retval.Value = WFState.WFStateSuccess;
			}
			catch (Exception ex)
			{
				WFLogger.NLogger.ErrorException(String.Format("Message extraction for PST file: {0} failed.", this.FileToProcess), ex);
			}

			return retval;
		}
	}
}
