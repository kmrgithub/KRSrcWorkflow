using System;
using System.IO;

using OpenMcdf;
using SFWorkflow.Abstracts;
using SFWorkflow;

namespace PSTMsgAttachmentParser
{
	[Serializable]
	public class PSTMsgAttachmentParser : WFProcessor
	{
		private const string nameOfAttachStorage_Format = "__attach_version1.0_#{0}";
		private const string nameOfSubStorageStream_Format = "__substg1.0_{0}{1}";
		private const string nameOfEmbeddedMessageStream = "__substg1.0_3701000D";

		private string MakeSubStorageStreamName(int propID, int propType)
		{
			return string.Format(nameOfSubStorageStream_Format, ((int)propID).ToString("X4"), ((int)propType).ToString("X4"));
		}

		private string MakeAttachStorageName(int attachmentIncrement)
		{
			return string.Format(nameOfAttachStorage_Format, attachmentIncrement.ToString("X8"));
		}

		private CFStorage GetStorage(CFStorage cfstorage, string storagename)
		{
			CFStorage storage = null;

			try
			{
				storage = cfstorage.GetStorage(storagename);
			}
			catch (Exception)
			{
			}

			return storage;
		}

		private CFStream GetStream(CFStorage cfstorage, string streamname)
		{
			CFStream stream = null;

			try
			{
				stream = cfstorage.GetStream(streamname);
			}
			catch (Exception)
			{
			}

			return stream;
		}

		public PSTMsgAttachmentParser()
		{
		}

		public override WFState Run()
		{
			WFState retval = new WFState() { Value = "Success" };

			OpenMcdf.CompoundFile cf = null;
			try
			{
				cf = new CompoundFile(this.FileToProcess);
				bool attachfound = false;
				int attachinc = 0;
				do
				{
					CFStorage cfstorage = this.GetStorage(cf.RootStorage, MakeAttachStorageName(attachinc));
					if (cfstorage != null)
					{
						// check if attachment is embedded message - if so do not process
						if (this.GetStorage(cfstorage, nameOfEmbeddedMessageStream) == null)
						{
							string filename = string.Format("attachment{0}", attachinc);

							// first get filename
							CFStream cfstream = this.GetStream(cfstorage, MakeSubStorageStreamName(0x3001, 0x001F));
							if (cfstream != null)
								filename = System.Text.UnicodeEncoding.Unicode.GetString(cfstream.GetData());
							// second get filename
							cfstream = this.GetStream(cfstorage, MakeSubStorageStreamName(0x3701, 0x0102));
							if (cfstream != null)
							{
								string filedir = string.Format("{0}\\{1}", this.ExportDirectory, WFUtilities.GetNextDirectoryNumber(this.ExportDirectory));
								if (!Directory.Exists(filedir))
									Directory.CreateDirectory(filedir);
								if (Directory.Exists(filedir))
								{
									using (var bw = new BinaryWriter(File.OpenWrite(string.Format("{0}\\{1}", filedir, filename))))
									{
										bw.Write(cfstream.GetData());
										this.OutputFiles.Add(string.Format("{0}\\{1}", filedir, filename), "Success");
									}
								}
							}
						}
						attachfound = true;
					}
					else
						attachfound = false;
					attachinc++;
				} while(attachfound);
			}
			catch (Exception)
			{
				retval.Value = "Fail";
			}
			finally
			{
				if(cf != null)
					cf.Close();
			}

			return retval;
		}
	}
}
