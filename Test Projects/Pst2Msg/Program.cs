using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using NLog;

namespace Pst2Eml
{
	class Program
	{
		private class MessageData
		{
			public uint NumAttachments { get; set; }
			public string MessageEntryId { get; set; }
			public pstsdk.definition.util.primitives.NodeID NodeId { get; set; }
			public MessageData()
				: this(string.Empty, 0, 0)
			{
			}

			public MessageData(string entryid, pstsdk.definition.util.primitives.NodeID nodeid, uint numattachments)
			{
				this.NumAttachments = numattachments;
				this.MessageEntryId = entryid;
				this.NodeId = nodeid;
			}
		}

		private class FolderData
		{
			public string FolderPath { get; set; }
			public string FolderEntryId { get; set; }
			public pstsdk.definition.util.primitives.NodeID NodeId { get; set; }
			public uint NumMessages { get; set; }
			public uint NumAttachments { get; set; }
			public List<MessageData> MessageData { get; private set; }

			public FolderData()
				: this(string.Empty)
			{
			}

			public FolderData(string currpath)
				: this(currpath, null)
			{
			}

			public FolderData(string currpath, pstsdk.definition.pst.folder.IFolder folder)
				: this(currpath, folder, 0, 0)
			{
			}

			public FolderData(string currpath, pstsdk.definition.pst.folder.IFolder folder, uint nummessages, uint numattachments)
			{
				this.MessageData = new List<MessageData>();
				this.FolderPath = currpath == string.Empty ? "Root Container" : currpath;
				if (folder != null)
				{
					this.FolderEntryId = folder.EntryID.ToString();
					this.NodeId = folder.Node;
				}
				this.NumAttachments = numattachments;
				this.NumMessages = nummessages;
			}
		}

		private static void GetFolderData(pstsdk.definition.pst.folder.IFolder rootfolder, string currpath, bool filtered, bool includemessages, ref List<FolderData> folders)
		{
			if (rootfolder.Name != string.Empty)
				currpath += (currpath != string.Empty ? "\\" : string.Empty) + rootfolder.Name;

			if (rootfolder.Name == string.Empty)
				currpath = "Root Container";

			FolderData folderdata = null;
			if ((folderdata = folders.FirstOrDefault(x => (x.FolderPath == currpath) || (x.FolderEntryId == rootfolder.EntryID.ToString()))) != null)
			{
				folderdata.NodeId = rootfolder.Node;
				if(folderdata.FolderEntryId == string.Empty)
					folderdata.FolderEntryId = rootfolder.EntryID.ToString();
				if (folderdata.FolderPath == string.Empty)
					folderdata.FolderPath = currpath;
			}
			else if (filtered == false)
				folders.Add(folderdata = new FolderData(currpath, rootfolder, includemessages ? (uint)rootfolder.MessageCount : 0, includemessages ? (uint)rootfolder.Messages.Count(x => x.AttachmentCount > 0) : 0));

			if (folderdata != null && includemessages == true)
			{
				List<MessageData> messagedatalist = folderdata.MessageData;
				foreach (pstsdk.definition.pst.message.IMessage msg in rootfolder.Messages)
				{
					MessageData messagedata = new MessageData(msg.EntryID.ToString(), msg.Node, (uint)msg.AttachmentCount);
					messagedatalist.Add(messagedata);
					msg.Dispose();
				}
			}

			if (rootfolder.Name == string.Empty)
				currpath = string.Empty;

			foreach (pstsdk.definition.pst.folder.IFolder folder in rootfolder.SubFolders)
			{
				GetFolderData(folder, currpath, filtered, includemessages, ref folders);
				folder.Dispose();
			}
		}

		private static void HandleFolder(pstsdk.definition.pst.folder.IFolder rootfolder, System.Xml.XmlDocument doc, System.Xml.XmlNode node, string currpath, bool unicode)
		{
			uint messagecnt = (uint)rootfolder.MessageCount;
			uint attachcnt = (uint)rootfolder.Messages.Sum(x => x.AttachmentCount);

			System.Xml.XmlElement folderinfo = doc.CreateElement("folderinfo");
			System.Xml.XmlNode folderinfonode = node.AppendChild(folderinfo);
			System.Xml.XmlElement numofmsg = doc.CreateElement("numOfMsg");
			System.Xml.XmlNode numofmsgnode = folderinfonode.AppendChild(numofmsg);
			numofmsgnode.InnerText = messagecnt.ToString();
			System.Xml.XmlElement numofattachments = doc.CreateElement("numOfAttachments");
			System.Xml.XmlNode numofattachmentsnode = folderinfonode.AppendChild(numofattachments);
			numofattachmentsnode.InnerText = attachcnt.ToString();
			System.Xml.XmlElement foldername = doc.CreateElement("foldername");
			System.Xml.XmlNode foldernamenode = folderinfonode.AppendChild(foldername);
			System.Xml.XmlAttribute foldernameattrib = doc.CreateAttribute("type");
			foldernameattrib.InnerText = unicode == true ? "PT_UNICODE" : "PT_ASCII";
			foldernamenode.Attributes.Append(foldernameattrib);
//			if (currpath == "Root Container")
//				foldernamenode.InnerText = String.Format("cb: {0} lpb: {1}", 0, string.Empty);
//			else
//			{
				byte[] bytes = null;
				if (unicode == true)
					bytes = System.Text.UnicodeEncoding.Unicode.GetBytes(currpath);
				else
					bytes = System.Text.UnicodeEncoding.ASCII.GetBytes(currpath);
				foldernamenode.InnerText = String.Format("cb: {0} lpb: {1}", bytes.Length, bytes.Select(x => String.Format("{0:X2}", x)).Aggregate((i, j) => i + j));
//			}
			System.Xml.XmlElement folderid = doc.CreateElement("folderid");
			System.Xml.XmlNode folderidnode = folderinfonode.AppendChild(folderid);
			folderidnode.InnerText = rootfolder.EntryID.ToString();
		}

		[MTAThread]
		static void Main(string[] args)
		{
			int msgprocessthreads = 4;
			string pstfile = string.Empty;
			string pathtopstfiles = string.Empty;
			string pathtoemlfiles = string.Empty;
			string pathtoeidfile = string.Empty;
			string pathtofolderfile = string.Empty;
			string host = string.Empty;
			PSTMsgParser.PSTMsgParserData.SaveAsType saveas = PSTMsgParser.PSTMsgParserData.SaveAsType.Msg;
			string tracefile = string.Empty;
			uint offset = 0;
			uint count = Int32.MaxValue;
			bool isclient = false;
			bool isserver = true;
			bool docount = false;
			bool unicode = false;
			List<string> entryids = new List<string>();
			List<FolderData> folderdata = new List<FolderData>();
			string queuetype = ".netqueue";

			Logger.NLogger.Info("Running: {0}", Environment.CommandLine);

			for (int i = 0; i < args.Length; i++)
			{
				switch (args[i].ToUpper())
				{
					case "-OFFSET":
						offset = Convert.ToUInt32(args[i + 1]);
						break;

					case "-COUNT":
						count = Convert.ToUInt32(args[i + 1]);
						break;

					case "-HOST":
						host = args[i + 1];
						break;

					case "-CLIENT":
						isclient = true;
						break;

					case "-SERVER":
						isserver = true;
						break;

					case "-MSGPROCESSTHREADS":
						msgprocessthreads = Convert.ToInt32(args[i + 1]);
						break;

					case "-INPUTDIR":
						pathtopstfiles = args[i + 1];
						pathtopstfiles = pathtopstfiles + (pathtopstfiles.EndsWith("\\") ? string.Empty : "\\");
						break;

					case "-OUTPUTDIR":
						pathtoemlfiles = args[i + 1];
						pathtoemlfiles = pathtoemlfiles + (pathtoemlfiles.EndsWith("\\") ? string.Empty : "\\");
						break;

					case "-QUEUETYPE":
						queuetype = args[i + 1];
						break;
				}
			}

			// added this code to support old Pst2Msg command line parameters
			List<string> pst2msgparameters = new List<string>() { "-E", "-F", "-M", "-O", "-R", "-S", "-T", "-U"};
			List<string> pst2msgargs = new List<string>();
			string pst2msgargument = string.Empty;
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].Length >= 2 && pst2msgparameters.Contains(args[i].ToUpper().Substring(0, 2)))
				{
					if (pst2msgargument != string.Empty)
					{
						pst2msgargs.Add(pst2msgargument);
						pst2msgargument = args[i];
					}
					else
						pst2msgargument = args[i];
				}
				else
					pst2msgargument += (" " + args[i]);
			}
			if (pst2msgargument != string.Empty)
				pst2msgargs.Add(pst2msgargument);
			for (int i = 0; i < pst2msgargs.Count; i++)
			{
				if (pst2msgargs[i].Length > 2)
				{
					switch (pst2msgargs[i].ToUpper().Substring(0, 2))
					{
						case "-E":
							if (pst2msgargs[i].Substring(0, 4).ToUpper() == "-EID")
								pathtoeidfile = pst2msgargs[i].Substring(4);
							break;

						case "-F":
							pstfile = pst2msgargs[i].Substring(2);
							break;

						case "-M":
							if (pst2msgargs[i].Substring(2).ToUpper() == "MSG")
								saveas = PSTMsgParser.PSTMsgParserData.SaveAsType.Msg;
							else if (pst2msgargs[i].Substring(2).ToUpper() == "META")
								saveas = PSTMsgParser.PSTMsgParserData.SaveAsType.Xml;
							else if (pst2msgargs[i].Substring(2).ToUpper() == "ALL")
								saveas = PSTMsgParser.PSTMsgParserData.SaveAsType.Xml | PSTMsgParser.PSTMsgParserData.SaveAsType.Msg;//						| PSTMsgParser.SaveAsType.Text;
							else if (pst2msgargs[i].Substring(2).ToUpper() == "COUNT")
								docount = true;
							break;

						case "-O":
							if (pst2msgargs[i].ToUpper() != "-OFFSET" && pst2msgargs[i].ToUpper() != "-OUTPUTDIR")
								pathtoemlfiles = pst2msgargs[i].Substring(2);
							break;

						case "-R":
							if (pst2msgargs[i].ToUpper().Substring(0, 3) != "-RT")
							{
								pathtoemlfiles = pst2msgargs[i].Substring(2);
								pathtoemlfiles = pathtoemlfiles + (pathtoemlfiles.EndsWith("\\") ? string.Empty : "\\");
							}
							break;

						case "-S":
							if (pst2msgargs[i].ToUpper() != "-SERVER")
								pathtofolderfile = pst2msgargs[i].Substring(2);
							break;

						case "-T":
							tracefile = pst2msgargs[i].Substring(2);
							break;

						case "-U":
							unicode = true;
							break;
					}
				}
			}

			if (docount)
			{
				offset = 0;
				count = Int32.MaxValue;
				msgprocessthreads = 0;
				pathtoeidfile = string.Empty;
				pathtofolderfile = string.Empty;
			}

			if (pathtoeidfile != string.Empty)
			{
				using (System.IO.StreamReader sr = new System.IO.StreamReader(pathtoeidfile))
				{
					String line = string.Empty;
					while ((line = sr.ReadLine()) != null)
					{
						entryids.Add(line.Split(new char[]{'\t'})[1]);
					}
				}
			}

			if (pathtofolderfile != string.Empty)
			{
				if (System.IO.File.Exists(pathtofolderfile))
				{
					using (System.IO.StreamReader sr = new System.IO.StreamReader(pathtofolderfile, true))
					{
						String line = string.Empty;
						while ((line = sr.ReadLine()) != null)
						{
							folderdata.Add(new FolderData(line));
						}
					}
				}
			}

			if (msgprocessthreads > 0)
				isclient = true;

			KRSrcWorkflow.Interfaces.IWFMessageQueue<PSTMsgParser.PSTMsgParser> msgqueue = null;
			if (queuetype == "rabbbitmq" || queuetype == "msmq")
			{
				if (host == string.Empty)
					KRSrcWorkflow.WFUtilities.SetHostAndIPAddress(System.Net.Dns.GetHostName(), ref host);
//				if (queuetype == "rabbbitmq")
//					msgqueue = new KRSrcWorkflow.MessageQueueImplementations.WFMessageQueue_RabbitMQ<PSTMsgParser.PSTMsgParser>(host, 5672, "msgqueue", KRSrcWorkflow.Abstracts.WFMessageQueueType.Publisher);
//				else if (queuetype == "msmq")
//					msgqueue = new KRSrcWorkflow.MessageQueueImplementations.WFMessageQueue_MessageQueue<PSTMsgParser.PSTMsgParser>(host, "msgqueue");
			}
//			else
//				msgqueue = new KRSrcWorkflow.MessageQueueImplementations.WFMessageQueue_Queue<PSTMsgParser.PSTMsgParser>();

			List<ManualResetEvent> msgthreadevents = new List<ManualResetEvent>();

			ManualResetEvent msgthreadinterrupt = null;
			if (isclient)
			{
				int threadid = 0;
				KRSrcWorkflow.WFGenericThread<PSTMsgParser.PSTMsgParser> iothread = null;
				if (msgprocessthreads > 0)
				{
					msgthreadinterrupt = new ManualResetEvent(false);
					for (int i = 0; i < msgprocessthreads; i++)
					{
//						ThreadPool.QueueUserWorkItem((iothread = new KRSrcWorkflow.WFThread<PSTMsgParser.PSTMsgParserData>(msgthreadinterrupt, msgqueue, null, threadid++)).Run);
						msgthreadevents.Add(iothread.ThreadExitEvent);
					}
				}
			}

			if (isserver)
			{
				string[] pstfiles = null;

				if (pathtopstfiles != string.Empty)
					pstfiles = System.IO.Directory.GetFiles(pathtopstfiles, "*.pst", System.IO.SearchOption.TopDirectoryOnly);
				else if (pstfile != string.Empty)
					pstfiles = new string[1] { pstfile };
				if (pstfiles.Length != 0)
				{
					foreach (string pfile in pstfiles)
					{
						bool append = false;

						Logger.NLogger.Info("Processing: {0}", pfile);
						string exportdir = pathtoemlfiles;
						if(pathtopstfiles != string.Empty)
							exportdir = pathtoemlfiles + pfile.Substring((pfile.LastIndexOf("\\") == -1 ? 0 : pfile.LastIndexOf("\\")) + 1, pfile.Length - 5 - (pfile.LastIndexOf("\\") == -1 ? 0 : pfile.LastIndexOf("\\")));
						Logger.NLogger.Info("Export Directory: {0}", exportdir);

						if (docount)
						{
							if (System.IO.File.Exists(exportdir + "\\AllEntryID.txt"))
								System.IO.File.Delete(exportdir + "\\AllEntryID.txt");

							if (System.IO.File.Exists(exportdir + "\\FolderInfo.xml"))
								System.IO.File.Delete(exportdir + "\\FolderInfo.xml");
						}

						if (!System.IO.Directory.Exists(exportdir + @"MSG\"))
							System.IO.Directory.CreateDirectory(exportdir + @"MSG\");

						if (!System.IO.Directory.Exists(exportdir + @"XML\"))
							System.IO.Directory.CreateDirectory(exportdir + @"XML\");

						Logger.NLogger.Info("Logon to PST store: {0}", pfile);
						pstsdk.definition.pst.IPst rdopststore = null;
						try
						{
							rdopststore = new pstsdk.layer.pst.Pst(pfile);
						}
						catch (Exception ex)
						{
							Logger.NLogger.ErrorException("Pst constructor failed for " + pfile, ex);
						}
						if (rdopststore != null)
						{
							Logger.NLogger.Info("Successfully logged on to PST store: {0}", pfile);
							GetFolderData(rdopststore.OpenRootFolder(), string.Empty, folderdata.Count > 0 ? true : false, docount == true, ref folderdata);
							uint totmessages = (uint)folderdata.Sum(x => x.NumMessages);
							uint totattachments = (uint)folderdata.Sum(x => x.NumAttachments);

							System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
							System.Xml.XmlNode foldersnode = null;
							if (docount == true)
							{
								doc = new System.Xml.XmlDocument();// Create the XML Declaration, and append it to XML document
								System.Xml.XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "windows-1252", null);
								doc.AppendChild(dec);
								// create message element
								System.Xml.XmlElement folders = doc.CreateElement("Folders");
								foldersnode = doc.AppendChild(folders);
							}
//							List<pstsdk.definition.util.primitives.NodeID> foldernodeids = docount == true ? folderdata.Where(x => x.NodeId != 0).Select(x => x.NodeId).ToList() : rdopststore.Folders.Where(x => x.Node != 0).Select(x => x.Node).ToList();
//							foreach (pstsdk.definition.util.primitives.NodeID foldernodeid in foldernodeids)
							foreach (FolderData fd in folderdata)
							{
								pstsdk.definition.util.primitives.NodeID foldernodeid = fd.NodeId;

								pstsdk.definition.pst.folder.IFolder folder = null;
								try
								{
									folder = rdopststore.OpenFolder(foldernodeid);
									if (docount == true)
										HandleFolder(folder, doc, foldersnode, fd.FolderPath, unicode);
									uint idx = 0;
									foreach (pstsdk.definition.pst.message.IMessage msg in folder.Messages)
									{
										if (docount == true)
										{
											using (System.IO.StreamWriter outfile = new System.IO.StreamWriter(exportdir + "\\AllEntryID.txt", append))
											{
												outfile.WriteLine(folder.EntryID.ToString() + "\t" + msg.EntryID.ToString());
											}
											append = true;
										}
										else
										{
											if (idx >= offset)
											{
//												if ((entryids.Count == 0) || entryids.Contains(msg.EntryID.ToString()))
//													msgqueue.Enqueue(new PSTMsgParser.PSTMsgParser(pfile, msg.Node, exportdir) { SaveAsTypes = PSTMsgParser.PSTMsgParser.SaveAsType.Msg | PSTMsgParser.PSTMsgParser.SaveAsType.Xml, SaveAttachments = false, FileToProcess = msg.Node.Value.ToString(), FolderPath = fd.FolderPath, ExportDirectory = exportdir, Pst2MsgCompatible = true });
											}
											idx++;
											if (count != UInt32.MaxValue)
											{
												if (idx == (offset + count))
													break;
											}
										}
										msg.Dispose();
									}
								}
								catch (Exception ex)
								{
									Logger.NLogger.ErrorException("OpenFolder failed!" + " NodeId=" + foldernodeid + " FolderPath=" + folderdata.FirstOrDefault(x => x.NodeId == foldernodeid).FolderPath, ex);
								}
								finally
								{
									if (folder != null)
										folder.Dispose();
								}
							}
							if (docount == true)
							{
								System.Xml.XmlElement numoffolders = doc.CreateElement("numOfFolders");
								System.Xml.XmlNode numoffoldersnode = foldersnode.AppendChild(numoffolders);
								numoffoldersnode.InnerText = rdopststore.Folders.Count().ToString();

								System.Xml.XmlElement numofmsgs = doc.CreateElement("numOfMsgs");
								System.Xml.XmlNode numofmsgsnode = foldersnode.AppendChild(numofmsgs);
								numofmsgsnode.InnerText = totmessages.ToString();

								System.Xml.XmlElement numoftotattachments = doc.CreateElement("numOfAttachments");
								System.Xml.XmlNode numoftotattachmentsnode = foldersnode.AppendChild(numoftotattachments);
								numoftotattachmentsnode.InnerText = totattachments.ToString();

								System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(exportdir + "\\FolderInfo.xml", System.Text.Encoding.GetEncoding("windows-1252"));
								writer.Formatting = System.Xml.Formatting.Indented;
								doc.Save(writer);
								writer.Close();
							}
							rdopststore.Dispose();
						}
					}
				}
				else
					Console.WriteLine("ERROR: No pst files found in directory " + pathtopstfiles);
				if (msgprocessthreads > 0)
				{
					do
					{
						Thread.Sleep(100);
					} while (msgqueue.Count > 0);
					System.Diagnostics.Debug.WriteLine("Setting msgthreadinterrupt");
					msgthreadinterrupt.Set();
					WaitHandle.WaitAll(msgthreadevents.ToArray());
					System.Diagnostics.Debug.WriteLine("All message threads exited");
					Thread.Sleep(5000);
				}
			}
			else
				Thread.Sleep(Int32.MaxValue);
		}
	}
}
