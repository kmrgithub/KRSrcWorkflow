using System;

namespace KRSrcWorkflow
{
	[Serializable]
	public class WFState
	{
		public static string WFStateSuccess = "Success";
		public static string WFStateFail = "Fail";
		public static string WFStateComplete = "Complete";
		public static string WFStateUnknown = "Unknown";

		public string Value { get; set; }

		public WFState(string state)
		{
			this.Value = state;
		}

		public WFState()
		{
			this.Value = string.Empty;
		}

		public override string ToString()
		{
			return this.Value;
		}
	}
}
