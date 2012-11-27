using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ConfigurationServer.Data
{
	public class LocationData
	{
		public QueueData InputQueue { get; set; }
		public QueueData OutputQueue { get; set; }
		public ProcessorData Processor { get; set; }
		public string AssemblyCache { get; set; }
		public uint NumInstances { get; set; }

		public LocationData()
		{
		}

		public override string ToString()
		{
			return this.ToString(0);
//			string.Format("Processor\n{0}\nAssemblyCache: {1}\nNumInstances: {2}\nInputQueue\n{3}\nOutputQueue\n{4}", this.Processor.ToString(), this.AssemblyCache, this.NumInstances, this.InputQueue.ToString(), this.OutputQueue.ToString());
		}

		public string ToString(int offset)
		{
			return string.Format("{0}Processor\n{1}\n{2}AssemblyCache: {3}\n{4}NumInstances: {5}\n{6}InputQueue\n{7}\n{8}OutputQueue\n{9}",
				Enumerable.Repeat(" ", offset).DefaultIfEmpty(string.Empty).Aggregate((x, y) => x + y), 
				this.Processor.ToString(offset + 1),
				Enumerable.Repeat(" ", offset + 1).DefaultIfEmpty(string.Empty).Aggregate((x, y) => x + y), 
				this.AssemblyCache,
				Enumerable.Repeat(" ", offset + 1).DefaultIfEmpty(string.Empty).Aggregate((x, y) => x + y), 
				this.NumInstances,
				Enumerable.Repeat(" ", offset + 1).DefaultIfEmpty(string.Empty).Aggregate((x, y) => x + y), 
				this.InputQueue.ToString(offset + 2),
				Enumerable.Repeat(" ", offset + 1).DefaultIfEmpty(string.Empty).Aggregate((x, y) => x + y), 
				this.OutputQueue.ToString(offset + 2));
		}
	}

	public class LocationDataDictionary : Dictionary<string, LocationDataList>
	{
		public override string ToString()
		{
			return this.Keys.Select(x => string.Format("Location: {0}\n{1}", x, this[x].ToString(1))).Aggregate((x, y) => x + "\n" + y);
		}
	}

	public class LocationDataList : List<LocationData>
	{
		public override string ToString()
		{
			return this.ToString(0);
//			return this.Select(x => x.ToString()).Aggregate((x, y) => x + "\n" + y);
		}

		public string ToString(int offset)
		{
			return this.Select(x => x.ToString(offset + 1)).Aggregate((x, y) => Enumerable.Repeat(" ", offset).DefaultIfEmpty(string.Empty).Aggregate((a, b) => a + b) + x + "\n" + y);
		}
	}

	[Serializable]
	public class WorkflowNodeList : List<WorkflowNodeList.WorkflowNode>
	{
		[Serializable]
		public class WorkflowNode
		{
			[XmlElement("Name")]
			public string Name { get; set; }
			[XmlElement("InputQueue")]
			public string InputQueue { get; set; }
			[XmlElement("Processor")]
			public string Processor { get; set; }
			[XmlElement("OutputQueue")]
			public string OutputQueue { get; set; }

			public WorkflowNode()
			{
			}
		}

		public WorkflowNodeList()
			: base()
		{
		}
	}

	[Serializable]
	public class ProcessorLocationData
	{
		[Serializable]
		[XmlType("ProcessorLocationData.ProcessorData")]
		public class ProcessorData
		{
			[XmlElement("Name")]
			public string Name { get; set; }
			[XmlElement("NumInstances")]
			public uint NumInstances { get; set; }
			public ProcessorData()
			{
				this.Name = string.Empty;
				this.NumInstances = 0;
			}
		}

		[Serializable]
		[XmlType("ProcessorLocationData.ProcessorDataList")]
		public class ProcessorDataList : List<ProcessorData>
		{
			public ProcessorDataList()
				: base()
			{
			}
		}

		[XmlElement("Host")]
		public string Host { get; set; }

		[XmlElement("Name")]
		public string Name { get; set; }

		[XmlArray("Processors")]
		[XmlArrayItem(ElementName = "Processor")]
		public ProcessorDataList Processors { get; set; }

		public ProcessorLocationData()
		{
			this.Host = string.Empty;
			this.Name = string.Empty;
			this.Processors = new ProcessorDataList();
		}
	}

	[Serializable]
	public class ProcessorLocationDataList : List<ProcessorLocationData>
	{
		public ProcessorLocationDataList()
			: base()
		{
		}
	}

	[Serializable]
	public class ProcessorData
	{
		[XmlElement("Name")]
		public string Name { get; set; }
		[XmlElement("ProcessorDataType")]
		public string ProcessorDataType { get; set; }
		[XmlElement("ProcessorType")]
		public string ProcessorType { get; set; }
		[XmlElement("Assembly")]
		public string Assembly { get; set; }
		[XmlIgnore]
		public string AssemblyCache { get; set; }

		public ProcessorData()
		{
			this.Name = string.Empty;
			this.ProcessorDataType = string.Empty;
			this.ProcessorType = string.Empty;
			this.Assembly = string.Empty;
			this.AssemblyCache = string.Empty;
		}

		public override string ToString()
		{
			return this.ToString(0);
//			string.Format("ProcessorType: {0}\nAssembly: {1}\nProcessorDataType: {2}\nAssemblyCache: {3}", this.ProcessorType, this.Assembly, this.ProcessorDataType, this.AssemblyCache);
		}

		public string ToString(int offset)
		{
			return string.Format("{0}ProcessorType: {1}\n{2}Assembly: {3}\n{4}ProcessorDataType: {5}\n{6}AssemblyCache: {7}",
				Enumerable.Repeat(" ", offset).DefaultIfEmpty(string.Empty).Aggregate((x, y) => x + y),
				this.ProcessorType,
				Enumerable.Repeat(" ", offset).DefaultIfEmpty(string.Empty).Aggregate((x, y) => x + y),
				this.Assembly,
				Enumerable.Repeat(" ", offset).DefaultIfEmpty(string.Empty).Aggregate((x, y) => x + y),
				this.ProcessorDataType,
				Enumerable.Repeat(" ", offset).DefaultIfEmpty(string.Empty).Aggregate((x, y) => x + y),
				this.AssemblyCache);
		}
	}

	[Serializable]
	public class ProcessorDataList : List<ProcessorData>
	{
		public ProcessorDataList()
			: base()
		{
		}
	}

	[Serializable]
	public class QueueData
	{
		[XmlElement("Name")]
		public string Name { get; set; }
		[XmlElement("Type")]
		public string Type { get; set; }
		[XmlElement("Host")]
		public string Host { get; set; }

		public QueueData()
		{
		}

		public override string ToString()
		{
			return this.ToString(0);
//			string.Format("QueueName: {0}\nQueueType: {1}\nQueueHost: {2}", this.Name, this.Type, this.Host);
		}
		public string ToString(int offset)
		{
			return string.Format("{0}QueueName: {1}\n{2}QueueType: {3}\n{4}QueueHost: {5}",
				Enumerable.Repeat(" ", offset).DefaultIfEmpty(string.Empty).Aggregate((x, y) => x + y),
				this.Name,
				Enumerable.Repeat(" ", offset).DefaultIfEmpty(string.Empty).Aggregate((x, y) => x + y),
				this.Type,
				Enumerable.Repeat(" ", offset).DefaultIfEmpty(string.Empty).Aggregate((x, y) => x + y),
				this.Host);
		}
	}

	[Serializable]
	public class QueueDataList : List<QueueData>
	{
		public QueueDataList()
			: base()
		{
		}
	}

	[Serializable]
	public class WorkflowData
	{
		[XmlAttribute("assembly_cache")]
		public string AssemblyCache { get; set; }

		[XmlArray("ProcessorLocations")]
		[XmlArrayItem(ElementName = "ProcessorLocation")]
		public ProcessorLocationDataList ProcessorLocations { get; set; }

		[XmlArray("Queues")]
		[XmlArrayItem(ElementName = "Queue")]
		public QueueDataList Queues { get; set; }

		[XmlArray("Processors")]
		[XmlArrayItem(ElementName = "Processor")]
		public ProcessorDataList Processors { get; set; }

		[XmlArray("WorkflowNodes")]
		[XmlArrayItem(ElementName = "WorkflowNode")]
		public WorkflowNodeList WorkflowNodes { get; set; }

		public WorkflowData()
		{
			this.AssemblyCache = string.Empty;
			this.ProcessorLocations = new ProcessorLocationDataList();
			this.Processors = new ProcessorDataList();
			this.Queues = new QueueDataList();
			this.WorkflowNodes = new WorkflowNodeList();
		}

		public LocationDataDictionary GetLocations()
		{
			LocationDataDictionary locationdata = new LocationDataDictionary();

			this.ProcessorLocations.ForEach(a =>
				{
					LocationDataList ldl = new LocationDataList();
					a.Processors.ForEach(b => ldl.Add(new LocationData()
					{
						AssemblyCache = this.AssemblyCache,
						NumInstances = b.NumInstances,
						InputQueue = this.Queues.Find(c => c.Name == this.WorkflowNodes.Find(d => d.Name == b.Name).InputQueue),
						OutputQueue = this.Queues.Find(c => c.Name == this.WorkflowNodes.Find(d => d.Name == b.Name).OutputQueue),
						Processor = this.Processors.Find(c => c.Name == this.WorkflowNodes.Find(d => d.Name == b.Name).Processor)
					}));
					locationdata[a.Host] = ldl;
				}
			);
//					locationdata[a.Host] = a.Processors.Select(b => new LocationData() { 
//							AssemblyCache = this.AssemblyCache, 
//							NumInstances = b.NumInstances, 
//							InputQueue = this.Queues.Find(c => c.Name == this.WorkflowNodes.Find(d => d.Name == b.Name).InputQueue), 
//							OutputQueue = this.Queues.Find(c => c.Name == this.WorkflowNodes.Find(d => d.Name == b.Name).OutputQueue),
 //							Processor = this.Processors.Find(c => c.Name == this.WorkflowNodes.Find(d => d.Name == b.Name).Processor)
	//					}).ToList());
			return locationdata;
		}

		public ProcessorData GetProcessor(string processorname)
		{
			return this.Processors.Where(x => x.Name == processorname).Select(x => new ProcessorData() {Name = x.Name, AssemblyCache = this.AssemblyCache, Assembly = x.Assembly, ProcessorDataType = x.ProcessorDataType, ProcessorType = x.ProcessorType}).FirstOrDefault();
		}
	}
}
