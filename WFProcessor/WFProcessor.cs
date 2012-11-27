using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;
using System.ServiceProcess;
using System.Diagnostics;
using System.ComponentModel;
using System.Management;
using System.Runtime.InteropServices;

using KRSrcWorkflow;
using KRSrcWorkflow.Abstracts;

namespace WFProcessor
{
	class WFProcessor
	{
		private enum ConsoleCtrlEvent
		{
			CTRL_C = 0,
			CTRL_BREAK = 1,
			CTRL_CLOSE = 2,
			CTRL_LOGOFF = 5,
			CTRL_SHUTDOWN = 6
		}

		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		static extern bool GenerateConsoleCtrlEvent(ConsoleCtrlEvent sigevent, int dwProcessGroupId);

		static void Main(string[] args)
		{
			WFProcessorArguments procArgs = new WFProcessorArguments(args);
			if (procArgs.Usage)
			{
				WFProcessor.PrintUsage(procArgs);
				return;
			}
			KRSrcWorkflow.WFLogger.NLogger.Info(procArgs.ToString());

			WFProcessor.Process(procArgs);
		}

		private static void Process(WFProcessorArguments procArgs)
		{
			// if no config file build xml string from arguments
			// <wfprocessor processor="assembly@PSTFileParser.dll:type@PSTFileParser" 
			//		src="assembly@PSTFileParser.dll:type@PSTFileParserData:host@10.3.2.54:queue@pstqueue:queuetype@rabbitmq" 
			//		target="host@10.3.2.54:queue@masterqueue:queuetype@rabbitmq" numthreads="1"/>
			string configxml = string.Empty;
			if (!string.IsNullOrEmpty(procArgs.ConfigXml))
				configxml = procArgs.ConfigXml;
			else if (!string.IsNullOrEmpty(procArgs.Configfile))
				configxml = System.IO.File.ReadAllText(procArgs.Configfile);
			else
			{
				configxml += @"<wfprocessors>";
				configxml += String.Format(@"  <wfprocessor processor=""assembly@{0}:type@{1}"" src=""assembly@{0}:type@{2}:host@{3}:queue@{4}:queuetype@{5}"" target=""host@{6}:queue@{7}:queuetype@{8}"" numthreads=""1"" ",
					procArgs.ProcessorAssembly,
					procArgs.ProcessorType,
					procArgs.ProcessorDataType,
					procArgs.InputQueueHost,
					procArgs.InputQueue,
					procArgs.InputQueueType,
					procArgs.OutputQueueHost,
					procArgs.OutputQueue,
					procArgs.OutputQueueType);
				configxml += @"</wfprocessors>";
			}

			List<ManualResetEvent> threadexitevents = new List<ManualResetEvent>();
			ManualResetEvent threadinterrupt = new ManualResetEvent(false);
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(configxml);
				foreach (XmlNode node in doc.SelectNodes("/wfprocessors/wfprocessor"))
				{
					if (false) //procArgs.Configfile != string.Empty)
					{
						ProcessStartInfo startInfo = new ProcessStartInfo("WFProcessor.exe");
						startInfo.Arguments = string.Format(@"-ASSEMBLYCACHE ""{0}"" -CONFIGXML ""<wfprocessors>{1}</wfprocessors>""", procArgs.AssemblyCache, node.OuterXml.Replace("\"", "\\\""));
						startInfo.UseShellExecute = false;
						startInfo.CreateNoWindow = false;
						startInfo.RedirectStandardInput = true;
						System.Diagnostics.Process.Start(startInfo);
						KRSrcWorkflow.WFLogger.NLogger.Info(startInfo.FileName + " " + startInfo.Arguments);
					}
					else
					{
						int numthreads = 1;

						KRSrcWorkflow.WFLogger.NLogger.Debug(node.OuterXml);

						XmlNode attrib = null;
						KRSrcWorkflow.Config.WFSrc wfsrc = null;
						KRSrcWorkflow.Config.WFTarget wftarget = null; // new KRSrcWorkflow.Config.WFTarget();
						KRSrcWorkflow.Config.WFProcessor wfprocessor = null; // new KRSrcWorkflow.Config.WFProcessor();

						// config processor
						attrib = node.Attributes.GetNamedItem("processor");
						if (attrib != null)
						{
							KRSrcWorkflow.Config.WFProcessorData wfprocessordata = new KRSrcWorkflow.Config.WFProcessorData(attrib.Value);
							AppDomain.CurrentDomain.AppendPrivatePath(procArgs.AssemblyCache + @"\" + wfprocessordata.AssemblyType);
							wfprocessor = new KRSrcWorkflow.Config.WFProcessor(wfprocessordata, procArgs.AssemblyCache); //string.Format(@"{0}\{1}", procArgs.AssemblyCache, wfprocessordata.AssemblyType));
						}

						// config WFTarget
						attrib = node.Attributes.GetNamedItem("target");
						if (attrib != null)
							wftarget = new KRSrcWorkflow.Config.WFTarget(attrib.Value);

						// config WFSrc
						attrib = node.Attributes.GetNamedItem("src");
						if (attrib != null)
							wfsrc = new KRSrcWorkflow.Config.WFSrc(attrib.Value, string.Format(@"{0}", procArgs.AssemblyCache)); //, wfprocessordata.AssemblyType));
						else
						{
							KRSrcWorkflow.WFLogger.NLogger.Error("No src node");
							continue;
						}

						// config numthreads
						attrib = node.Attributes.GetNamedItem("numthreads");
						if (attrib != null)
							numthreads = Convert.ToInt32(attrib.Value);

						if (wfsrc != null)
						{
							if (numthreads > 0)
							{
								//							Type genericthreadtype = typeof(WFThread<>).MakeGenericType(new[] { wfsrc.AssemblyType });
								Type genericthreadtype = typeof(WFWorkflowNode<>).MakeGenericType(new[] { wfsrc.AssemblyType });

								int threadid = 0;
								for (int i = 0; i < numthreads; i++)
								{
									//								object genericthreadobj = Activator.CreateInstance(genericthreadtype, new object[] { threadinterrupt, wfsrc.GenericQueue, wftarget.GenericQueue, threadid++ });
									object genericthreadobj = Activator.CreateInstance(genericthreadtype,
																																		new object[]
                                                                  {
																																		wfprocessor.AssemblyTypeInstance,
//																																		Activator.CreateInstance(wfprocessor.AssemblyType),
																																		wfsrc.MessageQueue,
                                                                    wftarget.MessageQueue, 
                                                                    threadinterrupt
//																																			 threadid++
                                                                   });
									//								Delegate d = Delegate.CreateDelegate(typeof(WaitCallback), genericthreadobj, "Run");
									//								ThreadPool.QueueUserWorkItem((WaitCallback)d);
									threadexitevents.Add(
											(ManualResetEvent)
											genericthreadtype.GetProperty("ThreadExitEvent").GetGetMethod().Invoke(genericthreadobj,
																																														 new object[0]));
								}
							}
						}
						else
							continue;
					}
				}
			}
			catch (Exception ex)
			{
				KRSrcWorkflow.WFLogger.NLogger.ErrorException(
						"USAGE: WFProcessor.exe -assembly {AssemblyDll:AssemblyType} -src {IPQueue:IPQueueName} [-target {IPQueue:IPQueueName}] [-numthreads {1,2,3...}] [-configfile {wfprocess.xml}]",
						ex);
				Environment.Exit(-1);
			}

			Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
																	{
																		KRSrcWorkflow.WFLogger.NLogger.Debug("Application CancelKeyPress received");
																		// if process is hosting WFProcesor threads then set thread interrupts 
																		// and wait until all threads complete
																		if (threadexitevents.Count > 0)
																		{
																			KRSrcWorkflow.WFLogger.NLogger.Debug("Setting thread interrupts");
																			threadinterrupt.Set();
																			KRSrcWorkflow.WFLogger.NLogger.Debug("Waiting on thread events");
																			WaitHandle.WaitAll(threadexitevents.ToArray());
																			KRSrcWorkflow.WFLogger.NLogger.Debug("Received all thread events");
																		}
																		else // kill all WFProcessor.exe's but yourself
//																			(new ManagementObjectSearcher(string.Format(@"select ProcessId from Win32_Process where Name = 'WFProcessor.exe' and ProcessId != {0}", System.Diagnostics.Process.GetCurrentProcess().Id))).Get().Cast<ManagementObject>().ToList().ForEach(x => System.Diagnostics.Process.GetProcessById(Convert.ToInt32(x["ProcessId"])).Kill());
																			(new ManagementObjectSearcher(string.Format(@"select ProcessId, SessionId from Win32_Process where Name = 'WFProcessor.exe' and ProcessId != {0}", System.Diagnostics.Process.GetCurrentProcess().Id))).Get().Cast<ManagementObject>().ToList().ForEach(x => GenerateConsoleCtrlEvent(ConsoleCtrlEvent.CTRL_C, Convert.ToInt32(x["ProcessId"])));
																		KRSrcWorkflow.WFLogger.NLogger.Debug("Exiting app");
																		Environment.Exit(0);
																	};

			Thread.CurrentThread.Join();
		}

		private static void CreateInstallFromArgs(string[] args, int index)
		{
			string serviceName = args[index + 1];
			string serviceDesc = args.Length > (index + 2) ? args[index + 2] : serviceName;
			string description = args.Length > (index + 3) ? args[index + 3] : serviceName;
			string[] configArgs = Enumerable.Range(0, index).Select(i => args[i]).ToArray();

			Processor.Install(serviceName, serviceDesc, description, configArgs);
		}

		private static void WaitIndefinitely()
		{
			Thread.CurrentThread.Join();
		}

		private static void PrintUsage(WFProcessorArguments procArgs)
		{
			KRSrcWorkflow.WFLogger.NLogger.Debug(string.Format(@"-assembly {AssemblyDll:AssemblyType} -src {IPQueue:IPQueueName} [-target {IPQueue:IPQueueName}] [-numthreads {1,2,3...}] [-configfile {wfprocess.cfg}]
As a system service:
	[regular config] -install ""ServiceName"" [""Service Description""] [""Actual Descrption""]
	-remove ""ServiceName""
{0}", procArgs.ToString()));
//			KRSrcWorkflow.WFLogger.NLogger.Debug(procArgs.ToString());
		}
	}
}
