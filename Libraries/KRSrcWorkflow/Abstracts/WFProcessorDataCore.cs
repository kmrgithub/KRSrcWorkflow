using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KRSrcWorkflow.Abstracts
{
	abstract class WFProcessorDataCore
	{
		public string WorkflowState { get; set; }

		WFProcessorDataCore()
		{
			this.WorkflowState = string.Empty;
		}
	}
}
