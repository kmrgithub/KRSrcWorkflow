using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Runtime.CompilerServices;
using System.Reflection;

using KRSrcWorkflow;
using KRSrcWorkflow.Abstracts;
using KRSrcWorkflow.Interfaces.Wcf;
using KRSrcWorkflow.Interfaces;
using KRSrcWorkflow.Config;
using SimpleTree;
using WFState = KRSrcWorkflow.Config.WFState;

namespace WFManager
{
	public class WFManager
	{
		static void Usage()
		{
			WFLogger.NLogger.Error("USAGE: WFManager.exe -configfile {wfmanager.cfg}");
			WFLogger.NLogger.Error("       " + Environment.CommandLine);
			Environment.Exit(-1);
		}

		static void Main(string[] args)
		{
			string configfile = string.Empty;
			string assemblycache = string.Empty;

			WFLogger.NLogger.Info("Running: {0}", Environment.CommandLine);

			for (int i = 0; i < args.Length; i++)
			{
				switch (args[i].ToUpper())
				{
					case "-CONFIGFILE":
						configfile = args[i + 1];
						break;

					case "-ASSEMBLYCACHE":
						assemblycache = args[i + 1];
						break;
				}
			}
			if (string.IsNullOrEmpty(assemblycache))
				assemblycache = System.IO.Directory.GetCurrentDirectory();

			// if no config file build xml string from arguments
			//<wfprocessor assembly="PstFileParser.dll:PstFileParser" src="169.254.25.129:pstqueue" target="169.254.25.129:masterqueue" numthreads="1"/>;
			string configxml = string.Empty;
			if (configfile == string.Empty)
				Usage();
			else
			{
			    configxml = System.IO.File.ReadAllText(configfile);
			}

			List<ServiceHost> servicehosts = new List<ServiceHost>();
			List<ManualResetEvent> threadexitevents = new List<ManualResetEvent>();
			ManualResetEvent threadinterrupt = new ManualResetEvent(false);
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(configxml);
				XmlNodeList workflownodelist = doc.SelectNodes("/workflows/workflow");
				if (workflownodelist != null)
				{
					foreach (XmlNode workflownode in workflownodelist)
					{
						WFSrc workflowsrc = null;
						WFLogger.NLogger.Debug(workflownode.OuterXml);

						XmlAttributeCollection workflowattributes = workflownode.Attributes;
						if (workflowattributes != null)
						{
							XmlNode attrib = workflowattributes.GetNamedItem("src");
							if (attrib != null)
							{
								try
								{
									workflowsrc = new WFSrc(attrib.Value);
								}
								catch (Exception ex)
								{
									WFLogger.NLogger.ErrorException("ERROR: WFSrc constructor failed!", ex);
								}
							}
						}

						if (workflowsrc == null)
							continue;

						TrafficHandler th = new TrafficHandler();
						Type genericthreadtype = typeof(WFWorkflowNode<>).MakeGenericType(new[] { workflowsrc.AssemblyType });

						object genericthreadobj = Activator.CreateInstance(genericthreadtype,
																															new object[]
                                                                  {
																																		th,
																																		workflowsrc.MessageQueue,
                                                                    null, 
                                                                    threadinterrupt
//																																			 threadid++
                                                                   });
						//								Delegate d = Delegate.CreateDelegate(typeof(WaitCallback), genericthreadobj, "Run");
						//								ThreadPool.QueueUserWorkItem((WaitCallback)d);
						threadexitevents.Add(
								(ManualResetEvent)
								genericthreadtype.GetProperty("ThreadExitEvent").GetGetMethod().Invoke(genericthreadobj,
																																											 new object[0]));

//						TrafficHandler th = new TrafficHandler(threadinterrupt,
//						                                       (IWFMessageQueue<ProcessorData>)
//						                                       workflowsrc.MessageQueue, 0);
//						threadexitevents.Add(th.ThreadExitEvent);

						XmlNodeList wfprocessornodelist = workflownode.SelectNodes("wfprocessors/wfprocessor");
						if (wfprocessornodelist != null)
						{
							foreach (XmlNode node in wfprocessornodelist)
							{
								WFTarget wftarget = null;
								WFSrc wfsrc = null;
								WFSrcData wfsrcdata = null;
								XmlAttributeCollection nodeattributes = node.Attributes;

								if (nodeattributes != null)
								{
									XmlNode srcattrib = nodeattributes.GetNamedItem("src");
									if (srcattrib != null && srcattrib.Value != string.Empty)
									{
										try
										{
											wfsrcdata = new WFSrcData(srcattrib.Value);
											wfsrc = new WFSrc(srcattrib.Value, wfsrcdata.AssemblyType == "ProcessorData" ? string.Empty : assemblycache);
										}
										catch (Exception ex)
										{
											WFLogger.NLogger.ErrorException("ERROR: WFSrc constructor failed!", ex);
											continue;
										}
									}

									XmlNode targetattrib = nodeattributes.GetNamedItem("target");
									if (targetattrib != null && targetattrib.Value != string.Empty)
									{
										try
										{
											wftarget = new WFTarget(targetattrib.Value, assemblycache);
										}
										catch (Exception ex)
										{
											WFLogger.NLogger.ErrorException("ERROR: WFTarget constructor failed!", ex);
										}
									}
								}

								if (wfsrc == null)
									continue;

								WFProcessorStates wfprocessorstates = new WFProcessorStates();
								if (th.TypeToTrafficTypeData.Count(x => x.Key == wfsrcdata.AssemblyType) == 0)
								{
//									th.TypeToTrafficTypeData.Remove((ProcessorData)wfsrc.AssemblyTypeInstance);
									th.TypeToTrafficTypeData.Add(wfsrcdata.AssemblyType, wfprocessorstates);
								}
								// make sure TypeToTrafficTypeData has a default entry for wftarget
								if (wftarget != null)
								{
									if (th.TypeToTrafficTypeData.Count(x => x.Key == wfsrcdata.AssemblyType) == 0)
										th.TypeToTrafficTypeData.Add(wfsrcdata.AssemblyType, null);

									// modify default Success target with WFTarget
									WFState wfstate = wfprocessorstates.Find(x => x.State.Value == "Success");
									if (wfstate != null)
										wfstate.Target = wftarget;
								}

								XmlNodeList wfstatenodelist = node.SelectNodes("wfstates/wfstate");
								if(wfstatenodelist == null)
									continue;

								foreach (XmlNode statenode in wfstatenodelist)
								{
									WFState wfstate = new WFState(statenode, wfsrc, wftarget, assemblycache);
									wfprocessorstates.Add(wfstate);

									// make sure TypeToTrafficTypeData has a default entry for wftarget
									if (wfstate.Target == null) 
										continue;

									if (th.TypeToTrafficTypeData.Count(x => x.Key == wfsrcdata.AssemblyType) == 0)
										th.TypeToTrafficTypeData.Add(wfsrcdata.AssemblyType, new WFProcessorStates());
								}
								KeyValuePair<string, WFProcessorStates> kvp = th.TypeToTrafficTypeData.FirstOrDefault(x => x.Key == wfsrcdata.AssemblyType);
								if (!kvp.Equals(default(KeyValuePair<string, WFProcessorStates>)))
									th.TypeToTrafficTypeData[kvp.Key] = wfprocessorstates;
							}
						}
//						ThreadPool.QueueUserWorkItem(th.Run);

						try
						{
							ServiceHost servicehost = new ServiceHost(th, new Uri(string.Format("http://{0}:8000/WFManagerWCF", System.Net.Dns.GetHostName())));
							servicehost.AddServiceEndpoint(typeof (IWFManager), new WSDualHttpBinding(), "WFManagerWCF");

							ServiceThrottlingBehavior throttling = new ServiceThrottlingBehavior { MaxConcurrentCalls = 1000, MaxConcurrentSessions = 1000};
							servicehost.Description.Behaviors.Add(throttling);

							servicehost.Open();
							servicehosts.Add(servicehost);
						}
						catch (Exception ex)
						{
							WFLogger.NLogger.ErrorException("ERROR: ServiceHost creation failed", ex);
						}
					}
				}
			}
			catch (Exception ex)
			{
				WFLogger.NLogger.ErrorException("ERROR!", ex);
			}

			Console.CancelKeyPress += delegate
			{
				WFLogger.NLogger.Debug("Application CancelKeyPress received");
				WFLogger.NLogger.Debug("Setting thread interrupts");
				threadinterrupt.Set();
				WFLogger.NLogger.Debug("Waiting on thread events");
				WaitHandle.WaitAll(threadexitevents.ToArray());
				WFLogger.NLogger.Debug("Received all thread events");
				WFLogger.NLogger.Debug("Exiting app");
				foreach (ServiceHost servicehost in servicehosts)
					servicehost.Close();

				Environment.Exit(0);
			};

			Thread.CurrentThread.Join();
		}
	}
}
