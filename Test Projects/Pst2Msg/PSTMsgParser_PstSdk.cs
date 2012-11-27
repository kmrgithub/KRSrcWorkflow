#if USE_PSTSDK
using System;
using System.IO;

using pstsdk.layer.pst;
using pstsdk.definition.pst;
using pstsdk.definition.pst.message;
using pstsdk.definition.util.primitives;

namespace Pst2Eml
{
	partial class PSTMsgParser
	{
		private void HandleMessage(Message rdomail, string exportdir) //, List<InputOutputThreadObjectHandler<PSTMsgParser>> outputobjects, InputOutputMessageQueue<PSTMsgParser> inqueue, InputOutputMessageQueue<PSTMsgParser> outqueue)
		{
			if (this.ProcessedMsgs.Contains(rdomail.Node))
				return;

			this.ProcessedMsgs.Add(rdomail.Node);

			string entryid = rdomail.Node.ToString();
			string msgdir = exportdir; // +@"\" + entryid;
			if (!Directory.Exists(msgdir))
				Directory.CreateDirectory(msgdir);
			if (Directory.Exists(msgdir))
			{
				string attachmentsdir = msgdir + @"\Children";
				if (this.SaveAttachments && rdomail.AttachmentCount > 0)
					Directory.CreateDirectory(attachmentsdir);

				if ((this.SaveAsTypes & SaveAsType.Eml) == SaveAsType.Eml)
					this.OutputFiles.Add(rdomail.Write(msgdir, Message.SaveAsMessageType.Eml, true));
				if ((this.SaveAsTypes & SaveAsType.Msg) == SaveAsType.Msg)
					this.OutputFiles.Add(rdomail.Write(msgdir + @"MSG\", Message.SaveAsMessageType.Msg, true));
				if ((this.SaveAsTypes & SaveAsType.Html) == SaveAsType.Html)
					this.OutputFiles.Add(rdomail.Write(msgdir, Message.SaveAsMessageType.Html, true));
				if (rdomail.HasBody && ((this.SaveAsTypes & SaveAsType.Text) == SaveAsType.Text))
					this.OutputFiles.Add(rdomail.Write(msgdir, Message.SaveAsMessageType.Text, true));
				if (rdomail.HasRtfBody && ((this.SaveAsTypes & SaveAsType.Rtf) == SaveAsType.Rtf))
					this.OutputFiles.Add(rdomail.Write(msgdir, Message.SaveAsMessageType.Rtf, true));
				if ((this.SaveAsTypes & SaveAsType.Xml) == SaveAsType.Xml)
					this.OutputFiles.Add(rdomail.Write(msgdir + @"XML\", Message.SaveAsMessageType.Xml, true));
				foreach (pstsdk.definition.pst.message.IAttachment rdoattachment in rdomail.Attachments)
				{
//					if (rdoattachment.IsMessage)
//						HandleMessage((Message)rdoattachment.OpenAsMessage(), this.ExportDirectory + @"\" + entryid); //, outputobjects, inqueue, outqueue);
//					else
//					{
						if (this.SaveAttachments)
						{
							string filename = attachmentsdir + @"\" + rdoattachment.Filename;
							using (var bw = new BinaryWriter(File.OpenWrite(filename)))
							{
								bw.Write(rdoattachment.Bytes);
								this.OutputFiles.Add(filename);
							}
						}
//					}
					rdoattachment.Dispose();
				}
			}
		}

		protected override PSTProcessingResult ProcessQueueObjectImplementation() //List<InputOutputThreadObjectHandler<PSTMsgParser>> outputobjects, InputOutputMessageQueue<PSTMsgParser> inqueue, InputOutputMessageQueue<PSTMsgParser> outqueue)
		{
			IPst rdopststore = null;
			try
			{
				Logger.NLogger.Info("PSTFile: {0}  EntryID: {1}", this.PSTFile, this.EntryID);
#if USE_PSTSTORE
				rdopststore = this.PstStore;
#else
				rdopststore = new Pst(this.PSTFile);
#endif
				Message rdomail = null;
				try
				{
					rdomail = (Message)rdopststore.OpenMessage(this.EntryID);
					rdomail.Pst = (Pst)rdopststore;
					Logger.NLogger.Info("EntryID: {0}", rdomail.EntryID.ToString());
				}
				catch (Exception ex)
				{
					Logger.NLogger.ErrorException("ERROR: ProcessQueueObjectImplementation", ex);
				}
				if (rdomail != null)
				{
					try
					{
						HandleMessage(rdomail, this.ExportDirectory); //, outputobjects, inqueue, outqueue);
					}
					catch (Exception ex)
					{
						Logger.NLogger.ErrorException("ERROR: HandleMessage", ex);
					}
					rdomail.Dispose();
					rdomail = null;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
			finally
			{
#if !USE_PSTSTORE
				if (rdopststore != null)
				{
					rdopststore.Dispose();
					rdopststore = null;
				}
#endif
			}

			foreach (string file in this.OutputFiles)
				Console.WriteLine(file);

			PSTProcessingResult result = new PSTProcessingResult() { IsSuccessful = true, Filename = string.Empty};
			result.SetProcessingObject<PSTMsgParser>(this);
			return result;
		}
	}
}
#endif
