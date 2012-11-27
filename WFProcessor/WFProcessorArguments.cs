using System;
using System.Linq;

namespace WFProcessor
{
	public class WFProcessorArguments
	{
		public string [] Arguments { get; set; }
		public string Assembly { get; set; }
		public string Src { get; set; }
		public string Target { get; set; }
		public string Configfile { get; set; }
		public int NumThreads { get; set; }
		public bool StartAsService { get; set; }

		public string ProcessorConfig { get; set; } 
		public string AssemblyCache { get; set; }
		public string InputQueue { get; set; }
		public string InputQueueType { get; set; }
		public string InputQueueHost { get; set; }
		public string OutputQueue { get; set; }
		public string OutputQueueType { get; set; }
		public string OutputQueueHost { get; set; }
		public string ProcessorAssembly { get; set; }
		public string ProcessorType { get; set; }
		public string ProcessorDataType { get; set; }
		public bool Usage { get; set; }
		public bool Install { get; set; }
		public bool Remove { get; set; }
		public string ConfigXml { get; set; }

		public WFProcessorArguments(string[] args)
		{
			this.Arguments = args;
			this.AssemblyCache = System.IO.Directory.GetCurrentDirectory();
			this.Assembly = string.Empty;
			this.Src = string.Empty;
			this.Target = string.Empty;
			this.Configfile = string.Empty;
			this.NumThreads = 1;
			this.StartAsService = false;
			this.InputQueue = string.Empty;
			this.InputQueueHost = string.Empty;
			this.InputQueueType = string.Empty;
			this.OutputQueue = string.Empty;
			this.OutputQueueHost = string.Empty;
			this.OutputQueueType = string.Empty;
			this.ProcessorAssembly = string.Empty;
			this.ProcessorDataType = string.Empty;
			this.ProcessorType = string.Empty;
			this.Usage = false;
			this.Install = false;
			this.Remove = false;
			this.ConfigXml = string.Empty;

			for (int i = 0; i < args.Length; i++)
			{
				switch (args[i].ToUpper())
				{
					case "-CONFIGXML":
						this.ConfigXml = args[i + 1];
						break;

					case "-CONFIGFILE":
						this.Configfile = args[i + 1];
						break;

					case "-ASSEMBLY":
						this.Assembly = args[i + 1];
						break;

					case "-NUMTHREADS":
						this.NumThreads = Convert.ToInt32(args[i + 1]);
						break;

					case "-SRC":
						this.Src = args[i + 1];
						break;

					case "-TARGET":
						this.Target = args[i + 1];
						break;

					case "-INSTALL":
						this.Install = true;
						return;

					case "-REMOVE":
						this.Remove = true;
						return;

					case "-S":
						this.StartAsService = true;
						break;

					case "-?":
					case "-h":
					case "-USAGE":
						this.Usage = true;
						return;

					case "-ASSEMBLYCACHE":
						this.AssemblyCache = args[i + 1];
						break;

					case "-INPUTQUEUE":
						this.InputQueue = args[i + 1];
						break;

					case "-INPUTQUEUETYPE":
						this.InputQueueType = args[i + 1];
						break;

					case "-INPUTQUEUEHOST":
						this.InputQueueHost = args[i + 1];
						break;

					case "-OUTPUTQUEUE":
						this.OutputQueue = args[i + 1];
						break;

					case "-OUTPUTQUEUETYPE":
						this.OutputQueueType = args[i + 1];
						break;

					case "-OUTPUTQUEUEHOST":
						this.OutputQueueHost = args[i + 1];
						break;

					case "-PROCESSORASSEMBLY":
						this.ProcessorAssembly = args[i + 1];
						break;

					case "-PROCESSORTYPE":
						this.ProcessorType = args[i + 1];
						break;

					case "-PROCESSORDATATYPE":
						this.ProcessorDataType = args[i + 1];
						break;
				}
			}

			if (args.Length == 0)
				this.Usage = true;
		}

		public override string ToString()
		{
			return this.Arguments.Aggregate((x, y) => (x.StartsWith("-") ? x : string.Format(@"""{0}""", x)) + " " + (y.StartsWith("-") ? y : string.Format(@"""{0}""", y)));
		}
	}
}
