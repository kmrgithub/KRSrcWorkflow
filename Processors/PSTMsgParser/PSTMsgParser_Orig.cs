using System;
using System.Collections.Generic;
using System.IO;

using pstsdk.definition.util.primitives;
using pstsdk.layer.pst;
using pstsdk.definition.pst;

using SFWorkflow;
using SFWorkflow.Abstracts;

namespace PSTMsgParser
{
	[Serializable]
	public class PSTMsgParser : WFProcessor
	{
		[Flags]
		public enum SaveAsType { None = 0, Eml = 1, Html = 2, Text = 4, Xml = 8, Msg = 16, Rtf = 32, All = Eml | Html | Text | Msg | Rtf };

		[System.Xml.Serialization.XmlIgnore]
		protected List<NodeID> ProcessedMsgs { get; set; }

		public string PSTFile { get; set; }
//		public string ExportDirectory { get; set; }
//		private NodeID EntryID { get; set; }
		public SaveAsType SaveAsTypes { get; set; }
		public bool SaveAttachments { get; set; }
		public bool SaveEmbeddedMsgs { get; set; }
		public string ParentMsg { get; set; }
		public string FolderPath { get; set; }
		public bool Pst2MsgCompatible { get; set; }

		public PSTMsgParser(string pstfile, NodeID entryid, string exportdir)
		{
			this.SaveAsTypes = SaveAsType.Msg; // | SaveAsType.Xml | SaveAsType.Html;
			this.SaveAttachments = false;
			this.ProcessedMsgs = new List<NodeID>();
			this.PSTFile = pstfile;
			this.ExportDirectory = exportdir;
			this.SaveEmbeddedMsgs = false;
			this.ParentMsg = "0";
			this.FolderPath = string.Empty;
			this.Pst2MsgCompatible = false;
		}

		public PSTMsgParser() : this(string.Empty, 0, string.Empty)
		{
		}

		private void HandleMessage(Message rdomail, string exportdir)
		{
			if (this.ProcessedMsgs.Contains(rdomail.Node))
				return;

			this.ProcessedMsgs.Add(rdomail.Node);

			string msgdir = exportdir;
			if (this.SaveAsTypes != SaveAsType.None || this.SaveAttachments == true)
				msgdir = msgdir + @"\" + rdomail.Node.ToString();

			if (!Directory.Exists(msgdir))
				Directory.CreateDirectory(msgdir);
			if (Directory.Exists(msgdir))
			{
				if ((this.SaveAsTypes & SaveAsType.Xml) == SaveAsType.Xml)
					this.OutputFiles.Add(rdomail.Write(this.Pst2MsgCompatible ? exportdir + @"\XML" : msgdir, Message.SaveAsMessageType.Xml, true), "Xml");
				if ((this.SaveAsTypes & SaveAsType.Eml) == SaveAsType.Eml)
					this.OutputFiles.Add(rdomail.Write(msgdir, Message.SaveAsMessageType.Eml, true));
				if ((this.SaveAsTypes & SaveAsType.Msg) == SaveAsType.Msg)
					this.OutputFiles.Add(rdomail.Write(this.Pst2MsgCompatible ? exportdir + @"\MSG" : msgdir, Message.SaveAsMessageType.Msg, true), "Msg");
				if ((this.SaveAsTypes & SaveAsType.Html) == SaveAsType.Html)
					this.OutputFiles.Add(rdomail.Write(msgdir, Message.SaveAsMessageType.Html, true), "Html");
				if (rdomail.HasBody && ((this.SaveAsTypes & SaveAsType.Text) == SaveAsType.Text))
					this.OutputFiles.Add(rdomail.Write(msgdir, Message.SaveAsMessageType.Text, true), "Text");
				if (rdomail.HasRtfBody && ((this.SaveAsTypes & SaveAsType.Rtf) == SaveAsType.Rtf))
					this.OutputFiles.Add(rdomail.Write(msgdir, Message.SaveAsMessageType.Rtf, true));
				foreach (pstsdk.definition.pst.message.IAttachment rdoattachment in rdomail.Attachments)
				{
					if (rdoattachment.IsMessage)
					{
						Message attachmsg = null;
						try
						{
							attachmsg = (Message)rdoattachment.OpenAsMessage();
							attachmsg.Pst = rdomail.Pst;
						}
						catch (Exception ex)
						{
							WFLogger.NLogger.ErrorException(string.Format("PSTFile={0}  NodeID={1}", this.PSTFile, this.FileToProcess), ex);
						}
						finally
						{
							if (attachmsg != null)
							{
								if (this.SaveEmbeddedMsgs == true && attachmsg.Node == Convert.ToUInt32(this.FileToProcess))
								{
									SaveAsType origsaveastype = this.SaveAsTypes;
									this.SaveAsTypes = SaveAsType.Msg | SaveAsType.Xml | SaveAsType.Html;
									HandleMessage(attachmsg, exportdir);
									this.SaveAsTypes = origsaveastype;
								}
								else
									this.OutputFiles.Add(attachmsg.Node.Value.ToString(), "EmbeddedMsg");
							}
						}
					}
					else if (this.SaveAttachments)
					{
						string filedir = string.Format("{0}\\{1}", msgdir, WFUtilities.GetNextDirectoryNumber(msgdir));
						if (!Directory.Exists(filedir))
							Directory.CreateDirectory(filedir);
						if (Directory.Exists(filedir))
						{
							string filename = filedir + @"\" + rdoattachment.Filename;
							using (var bw = new BinaryWriter(File.OpenWrite(filename)))
							{
								bw.Write(rdoattachment.Bytes);
								this.OutputFiles.Add(filename, "Attachment");
							}
						}
					}
					rdoattachment.Dispose();
				}
			}
		}

		public override WFState Run()
		{
			WFState retval = new WFState();

			IPst rdopststore = null;
			try
			{
				retval.Value = WFState.WFStateFail;
				WFLogger.NLogger.Info("PSTFile={0}  NodeID={1}", this.PSTFile, this.FileToProcess);
		
				rdopststore = new Pst(this.PSTFile);

				Message rdomail = null;
				try
				{
					NodeID msgid = Convert.ToUInt32(this.FileToProcess);
					if (this.ParentMsg != "0")
						msgid = Convert.ToUInt32(this.ParentMsg);

					rdomail = (Message)rdopststore.OpenMessage(msgid); //this.EntryID);
					rdomail.Pst = (Pst)rdopststore;
					rdomail.FolderPath = this.FolderPath;
					WFLogger.NLogger.Info("EntryID={0}", rdomail.EntryID.ToString());
				}
				catch (Exception ex)
				{
					WFLogger.NLogger.ErrorException("ERROR: ProcessQueueObjectImplementation", ex);
				}
				if (rdomail != null)
				{
					try
					{
						HandleMessage(rdomail, this.ExportDirectory);
						this.ExportDirectory = this.ExportDirectory + @"\" + this.FileToProcess;
						retval.Value = WFState.WFStateSuccess;
					}
					catch (Exception ex)
					{
						WFLogger.NLogger.ErrorException("ERROR: HandleMessage", ex);
					}
					rdomail.Dispose();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
			finally
			{
				if (rdopststore != null)
					rdopststore.Dispose();
			}

			return retval;
		}
	}
}
