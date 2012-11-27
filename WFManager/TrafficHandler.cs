using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.CompilerServices;
using System.Reflection;

using KRSrcWorkflow;
using KRSrcWorkflow.Abstracts;
using KRSrcWorkflow.Interfaces.Wcf;
using KRSrcWorkflow.Interfaces;
using KRSrcWorkflow.Config;
using SimpleTree;
using WFState = KRSrcWorkflow.Config.WFState;
using WFProcessor = KRSrcWorkflow.Abstracts.WFProcessor;
using ProcessorData = KRSrcWorkflow.Abstracts.ProcessorData;

namespace WFManager
{
	[Serializable]
	class WFProcessType : ProcessorData
	{
	}

	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	internal class TrafficHandler : Processor<ProcessorData>, IWFManager
	{
		private delegate void ProcessingAsync(ProcessorData queueobject);
		public Dictionary<string, WFProcessorStates> TypeToTrafficTypeData { get; set; }
		private TrackingDataTreeList TrackingTree { get; set; }
		private Dictionary<Guid, IWFClientProcessing> Subscribers { get; set; }

		#region implementations for IWFManager
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Discovery()
		{
			WFLogger.NLogger.Info("Channel={0}", OperationContext.Current.Channel.RemoteAddress.Uri.ToString());
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public Guid Execute(ProcessorData procdata)
		{
			procdata.OutputDocuments.Add(procdata.DocumentToProcess, procdata.WFState.Value);

			TrackingDataTree newtree = new TrackingDataTree();
			newtree.Value = new TrackingData(procdata.TrackingId, procdata.DocumentToProcess, OperationContext.Current.GetCallbackChannel<IWFClient>());
			this.TrackingTree.Add(newtree);

			if (string.IsNullOrEmpty(procdata.ExportDirectory))
			{
				if (string.IsNullOrEmpty(procdata.DocumentToProcess))
					procdata.ExportDirectory = System.Environment.CurrentDirectory;
				else
					procdata.ExportDirectory = System.IO.Path.GetDirectoryName(procdata.DocumentToProcess);
			}

			ProcessingAsync cd = new ProcessingAsync(this.Process);
			cd.BeginInvoke(procdata, cd.EndInvoke, null);

			return procdata.TrackingId;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public Guid Process(string filename, string exportdir)
		{
			WFLogger.NLogger.Info("Filename={0}  ExportDir={1}", filename, exportdir);

			// user did not indicate export directory
			// extract directory name from filename
			if (exportdir == string.Empty)
				exportdir = System.IO.Path.GetDirectoryName(filename);

			WFProcessType wfprocesstype = new WFProcessType { DocumentToProcess = filename, ExportDirectory = exportdir };
			wfprocesstype.OutputDocuments.Add(filename, "Process");

			TrackingDataTree newtree = new TrackingDataTree();
			newtree.Value = new TrackingData(wfprocesstype.TrackingId, filename, OperationContext.Current.GetCallbackChannel<IWFClient>());
			this.TrackingTree.Add(newtree);

			ProcessingAsync cd = new ProcessingAsync(this.Process);
			cd.BeginInvoke(wfprocesstype, cd.EndInvoke, null);

			return wfprocesstype.TrackingId;
		}

		// service method to allow client to subscribe to file processing completion events
		[MethodImpl(MethodImplOptions.Synchronized)]
		public bool Subscribe(Guid guid)
		{
			WFLogger.NLogger.Info("Guid={0}", guid.ToString());
			bool retval = true;
			try
			{
				Subscribers.Add(guid, OperationContext.Current.GetCallbackChannel<IWFClient>());
			}
			catch (Exception)
			{
				retval = false;
			}

			return retval;
		}
		#endregion

		#region constructors
		public TrafficHandler()
		{
			this.Subscribers = new Dictionary<Guid, IWFClientProcessing>();
			this.TypeToTrafficTypeData = new Dictionary<string, WFProcessorStates>();
			this.TrackingTree = new TrackingDataTreeList();
		}
		#endregion

		[MethodImpl(MethodImplOptions.Synchronized)]
		public override void Process(ProcessorData queueobject)
		{
			if (queueobject == default(ProcessorData))
				return;

			string TypeName = queueobject.TypeName;
			string TypeFullName = queueobject.TypeFullName;

			this.TrackingTree.OuputTrackingTreeList();

			KRSrcWorkflow.WFState resultstate = queueobject.WFState;
			WFLogger.NLogger.Info("Type={0}  State={1}  Filename={2}", TypeName, resultstate.Value, queueobject.DocumentToProcess);

			KeyValuePair<string, WFProcessorStates> kvp;
			if (!(kvp = this.TypeToTrafficTypeData.FirstOrDefault(x => TypeFullName.Contains(x.Key))).Equals(default(KeyValuePair<string, WFProcessorStates>)))
			{
				ProcessorData srcobj = queueobject; // (WFProcessor)typeof(WFProcessingResult).GetMethod("GetProcessedObject").MakeGenericMethod(new[] { kvp.Key }).Invoke(queueobject, new object[0]);
				if (srcobj != null)
				{
//					WFLogger.NLogger.Info("Type={0}  TrackingId={1}  ParentTrackingId={2}", queueobject.ProcessedObjectType, srcobj.TrackingId, srcobj.ParentTrackingId);
					if (kvp.Value != null)
					{
						// create the first list by using a specific "template" type.
						var completeoutputfiles = new[] { new { TrackingId = Guid.Empty, Filename = string.Empty, ParentTrackingId = Guid.Empty, WFState = new KRSrcWorkflow.WFState(string.Empty), IsComplete = true } }.ToList();
						completeoutputfiles.Clear();
						if (queueobject.OutputDocuments.Count == 0)
							completeoutputfiles.Add(new { TrackingId = srcobj.TrackingId, Filename = srcobj.DocumentToProcess, ParentTrackingId = srcobj.TrackingId, WFState = new KRSrcWorkflow.WFState(srcobj.WFState.Value), IsComplete = true });
						else
						{
							foreach (WFKeyValuePair<string, KRSrcWorkflow.WFState> pair in queueobject.OutputDocuments)
							{
								bool iscompleteoutputfiles = !kvp.Value.Any(x => (new System.Text.RegularExpressions.Regex((Char.IsLetter(x.State.Value[0]) ? "^(" : string.Empty) + x.State.Value + (Char.IsLetter(x.State.Value[0]) ? ")$" : string.Empty))).IsMatch(pair.Value.Value) && x.Target != null);
//								bool iscompleteoutputfiles = !kvp.Value.Any(x => pair.Value.Value.StartsWith(x.State.Value.TrimEnd('*')) && x.Target != null);
//								bool iscompleteoutputfiles = !kvp.Value.Any(x => x.State.Value == pair.Value.Value && x.Target != null);
								if (iscompleteoutputfiles)
									completeoutputfiles.Add(new { TrackingId = (srcobj.DocumentToProcess == pair.Key ? srcobj.TrackingId : Guid.NewGuid()), Filename = pair.Key, ParentTrackingId = (srcobj.DocumentToProcess == pair.Key ? srcobj.ParentTrackingId : srcobj.TrackingId), WFState = new KRSrcWorkflow.WFState(string.Empty), IsComplete = true });
								else
								{
									foreach (WFState wfstate in kvp.Value.Where(x => (new System.Text.RegularExpressions.Regex((Char.IsLetter(x.State.Value[0]) ? "^(" : string.Empty) + x.State.Value + (Char.IsLetter(x.State.Value[0]) ? ")$" : string.Empty))).IsMatch(pair.Value.Value) && (x.Target != null)).ToList())
//									foreach (WFState wfstate in kvp.Value.Where(x => pair.Value.Value.StartsWith(x.State.Value.TrimEnd('*')) && (x.Target != null)))
//									foreach (WFState wfstate in kvp.Value.Where(x => (x.State.Value == pair.Value.Value) && (x.Target != null)))
									{
										if (wfstate != null)
										{
											// create instance of target object
											ProcessorData targetobj = new ProcessorData(); // (ProcessorData)wfstate.Target.AssemblyTypeInstance;
											targetobj.TrackingId = srcobj.TrackingId;
											targetobj.ExportDirectory = srcobj.ExportDirectory;
											targetobj.DocumentToProcess = pair.Key;
//												ProcessorData.CreateInstance(wfstate.Target.AssemblyType, pair.Key, srcobj.TrackingId, srcobj.ExportDirectory);
											if (targetobj != null)
											{
												targetobj.State = queueobject.State;

												// if the output file is the same as what we just processed then set
												// target tracking id's to src
												// this case usually indicates the processor was informative in nature e.g. WFFileType
												if (targetobj.DocumentToProcess == srcobj.DocumentToProcess)
												{
													targetobj.TrackingId = srcobj.TrackingId;
													targetobj.ParentTrackingId = srcobj.ParentTrackingId;
												}

												// loop over mappings and set values and properties
												foreach (WFMapping mapping in wfstate.Mappings)
												{
													object value = mapping.GetSrcValue(srcobj);
													if (value != null)
														mapping.SetTargetValue(targetobj, value);
												}
												completeoutputfiles.Add(new { TrackingId = targetobj.TrackingId, Filename = targetobj.DocumentToProcess, ParentTrackingId = srcobj.TrackingId, WFState = new KRSrcWorkflow.WFState(string.Empty), IsComplete = false });

												WFLogger.NLogger.Info("Enqueue:  Type={0}  Filename={1}  TrackingId={2}  ParentTrackingId={3}", targetobj.GetType().FullName, targetobj.DocumentToProcess, targetobj.TrackingId, targetobj.ParentTrackingId);
												wfstate.Target.MessageQueue.Enqueue(targetobj);
											}
											else
												WFLogger.NLogger.Error(string.Format("Could not create instance of {0}."), wfstate.Target.AssemblyType.Name);
										}
									}
								}
							}
							foreach (var completeoutputfile in completeoutputfiles)
								this.TrackingTree.ProcessItem(completeoutputfile.TrackingId, completeoutputfile.Filename, completeoutputfile.ParentTrackingId, completeoutputfile.WFState, completeoutputfile.IsComplete);
						}
					}
				}
				else
					WFLogger.NLogger.Error("GetProcessedObject failed for type: ", TypeFullName);
			}
			else
				WFLogger.NLogger.Error("{0} not found in TypeToQueue", TypeFullName);
		}
	}
}
