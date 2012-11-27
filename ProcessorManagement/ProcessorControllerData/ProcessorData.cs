using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcessorManagement.Data
{
	public class ProcessorData
	{
		public string ProcessorName { get; set; }
		public string CommandLine { get; set; }
		public string Pid { get; set; }
		public string Location { get; set; }

		public string InputQueue { get; set; }
		public string OutputQueue { get; set; }
		public string InputQueueType { get; set; }
		public string OutputQueueType { get; set; }
		public string InputQueueHost { get; set; }
		public string OutputQueueHost { get; set; }
		public string ProcessorAssembly { get; set; }
		public string ProcessorType { get; set; }
		public string ProcessorDataType { get; set; }

		public ProcessorData()
		{
			this.ProcessorName = string.Empty;
			this.Pid = string.Empty;
			this.CommandLine = string.Empty;
			this.Location = string.Empty;
		}

		public override string ToString()
		{
			return this.ToString(0);
		}

		public string ToString(int offset)
		{
			return string.Format("{0}ProcessorName: {1}({2})\n{3}Location: {4}\n{5}CommandLine: {6}",
				Enumerable.Repeat(" ", offset).DefaultIfEmpty(string.Empty).Aggregate((x, y) => x + y),
				this.ProcessorName,
				this.Pid,
				Enumerable.Repeat(" ", offset).DefaultIfEmpty(string.Empty).Aggregate((x, y) => x + y),
				this.Location,
				Enumerable.Repeat(" ", offset).DefaultIfEmpty(string.Empty).Aggregate((x, y) => x + y),
				this.CommandLine);
		}
	}

	public class ProcessorDataList : List<ProcessorData>
	{
		public override string ToString()
		{
			return this.ToString(0);
		}

		public string ToString(int offset)
		{
			return this.Select(x => x.ToString(offset)).Aggregate((x, y) => Enumerable.Repeat(" ", offset).DefaultIfEmpty(string.Empty).Aggregate((a, b) => a + b) + x + "\n" + y);
		}
	}
}
