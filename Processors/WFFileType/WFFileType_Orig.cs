using System;

using SFWorkflow;
using SFWorkflow.Abstracts;

namespace WFFileType
{
	[Serializable]
	public class WFFileType : WFProcessor
	{
		public WFFileType()
			: base()
		{
		}

		public override WFState Run()
		{
			WFState retval = new WFState(WFState.WFStateFail);

			retval.Value = SFWorkflow.WFFileType.GetFileType(this.FileToProcess).ToString();
			if (retval.Value.StartsWith("Ole"))
				retval.Value = "Ole";

			this.OutputFiles.Add(this.FileToProcess);

			return retval;
		}
	}
}
