using System;

using KRSrcWorkflow;
using KRSrcWorkflow.Abstracts;

namespace WFFileType
{
	[Serializable]
	public class WFFileType : Processor<WFFileTypeData>
	{
		public WFFileType()
			: base()
		{
		}

		public override void Process(WFFileTypeData data)
		{
			data.WFState.Value = WFState.WFStateFail;
			data.WFState.Value = KRSrcWorkflow.WFFileType.GetFileType(data.DocumentToProcess).ToString();
//			if (data.WFState.Value.StartsWith("Ole"))
//				data.WFState.Value = "Ole";
			data.OutputDocuments.Add(data.DocumentToProcess, data.WFState.Value);
			data.WFState.Value = WFState.WFStateSuccess;
		}
	}
}
