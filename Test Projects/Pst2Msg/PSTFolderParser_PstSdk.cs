#if USE_PSTSDK
using System;
using System.IO;

using pstsdk.layer.pst;
using pstsdk.definition.pst;
using pstsdk.definition.pst.message;
using pstsdk.definition.util.primitives;

namespace Pst2Eml
{
	partial class PSTFolderParser
	{
		protected override PSTProcessingResult ProcessQueueObjectImplementation()
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

			PSTProcessingResult result = new PSTProcessingResult() { IsSuccessful = true, Filename = string.Empty};
			result.SetProcessingObject<PSTFolderParser>(this);
			return result;
		}
	}
}
#endif
