using System;

using KRSrcWorkflow.Abstracts;
using KRSrcWorkflow.CustomAttributes;

namespace PSTFileParser
{
	[Serializable]
	public class PSTFileParserData : ProcessorData
	{
        [KRSrcWorkflowAttribute]
		public string PSTFile { get; set; }
	}
}
