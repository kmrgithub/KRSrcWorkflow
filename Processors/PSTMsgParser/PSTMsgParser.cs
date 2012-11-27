using System;
using System.Collections.Generic;
using System.IO;

using pstsdk.definition.util.primitives;
using pstsdk.layer.pst;
using pstsdk.definition.pst;

using KRSrcWorkflow;
using KRSrcWorkflow.Abstracts;

using SaveAsType = PSTMsgParser.PSTMsgParserData.SaveAsType;

namespace PSTMsgParser
{
	[Serializable]
	public class PSTMsgParser : Processor<PSTMsgParserData>
	{
		private List<NodeID> ProcessedMsgs { get; set; }
		private string ParentMsg { get; set; }
		private SaveAsType SaveAsTypes { get; set; }
		private bool SaveAttachments { get; set; }
		private bool SaveEmbeddedMsgs { get; set; }
		private bool Pst2MsgCompatible { get; set; }
		private WFFileList OutputFiles { get; set; }
		private string PSTFile { get; set; }
		private string FileToProcess { get; set; }

		public PSTMsgParser()
		{
			this.ProcessedMsgs = new List<NodeID>();
			this.ParentMsg = "0";
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

		public override void Process(PSTMsgParserData data)
		{
			this.PSTFile = data.PSTFile; // GetProperty<string>("PSTFile", "PSTFileParserData");
			this.PSTFile = data.PSTFile; // GetProperty<string>("PSTFile", "PSTFileParserData");
			this.ProcessedMsgs.Clear();
			this.ParentMsg = "0";
			this.SaveAsTypes = data.SaveAsTypes;
			this.SaveAttachments = data.SaveAttachments;
			this.SaveEmbeddedMsgs = data.SaveEmbeddedMsgs;
			this.Pst2MsgCompatible = data.Pst2MsgCompatible;
			this.OutputFiles = data.OutputDocuments;
			this.FileToProcess = data.DocumentToProcess;

			data.WFState.Value = "Fail";

			IPst rdopststore = null;
			try
			{
				WFLogger.NLogger.Info("PSTFile={0}  NodeID={1}", PSTFile, this.FileToProcess);
		
				rdopststore = new Pst(PSTFile);

				Message rdomail = null;
				try
				{
					NodeID msgid = Convert.ToUInt32(this.FileToProcess);
					if (this.ParentMsg != "0")
						msgid = Convert.ToUInt32(this.ParentMsg);

					rdomail = (Message)rdopststore.OpenMessage(msgid); //this.EntryID);
					rdomail.Pst = (Pst)rdopststore;
					rdomail.FolderPath = data.FolderPath;
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
						HandleMessage(rdomail, data.ExportDirectory);
						data.ExportDirectory = data.ExportDirectory + @"\" + this.FileToProcess;
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
			data.WFState.Value = "Succcess";
		}
	}
}
