using System;
using System.Linq;

using SFWorkflow;
using SFWorkflow.Abstracts;

namespace StructuredStorageFileParser
{
	internal class Ole10Native : IDisposable
	{
		private readonly int totalSize; // Total stream size including this field
		private readonly short flags1; // Some flags. Mostly, 02 00
		private readonly string label; // ASCIIZ
		private readonly string fileName; // ASCIIZ
		private readonly short flags2; // Also some flags. Mostly, 00 00
		private readonly byte[] unknown1; // Unknown
		private readonly byte[] unknown2; // Unknown. Mostly, 00 00 00
		private readonly string command; // ASCIIZ
		private readonly int nativeDataSize; // It's actual data size. We need this.
		private readonly byte[] nativeData;  // Actual data.
		private readonly short unknown3;

		public byte[] NativeData
		{
			get 
			{
				return nativeData;
			}
		}

		public int TotalSize
		{
			get
			{
				return totalSize;
			}
		}

		public string Label
		{
			get
			{
				return label;
			}
		}

		public string FileName
		{
			get
			{
				return fileName;
			}
		}

		public Ole10Native(System.IO.Stream inputStream)
		{
			System.IO.BinaryReader reader = new System.IO.BinaryReader(inputStream);
			totalSize = reader.ReadInt32();
			if (totalSize < 4)
			{
				throw new Exception(
						String.Format("Invalid total data size: {0}.", totalSize));
			}
			flags1 = reader.ReadInt16();
			label = ReadString(reader);
			fileName = ReadString(reader);
			flags2 = reader.ReadInt16();
			byte unknown1Len = reader.ReadByte();
			unknown1 = reader.ReadBytes(unknown1Len);
			unknown2 = reader.ReadBytes(3);
			command = ReadString(reader);
			nativeDataSize = reader.ReadInt32();
			if (nativeDataSize > totalSize || nativeDataSize < 0)
			{
				throw new Exception(
						String.Format("Invalid native data size: {0}.", nativeDataSize));
			}
			nativeData = reader.ReadBytes(nativeDataSize);
			unknown3 = unknown1.Length > 0 ? reader.ReadInt16() : (short)0;
		}

		private static string ReadString(System.IO.BinaryReader reader)
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder();
			byte nextByte = 0;
			while ((nextByte = reader.ReadByte()) != 0)
			{
				builder.Append((char)nextByte);
			}
			return builder.ToString();
		}

		public void Dispose()
		{
			// Do nothing.
		}
	}

	[Serializable]
	public class StructuredStorageFileParser : WFProcessor
	{
		public bool ExpandFully { get; set; }
		public uint Depth { get; set; }

		private void Write(OpenMcdf.CFStorage tostorage, OpenMcdf.CFStorage fromstorage)
		{
			foreach (OpenMcdf.CFItem childitem in fromstorage.EnumChildren)
			{
				if (childitem is OpenMcdf.CFStream)
				{
					OpenMcdf.CFStream olechildstream = tostorage.AddStream(childitem.Name);
					olechildstream.SetData(((OpenMcdf.CFStream)childitem).GetData());
				}
				else if (childitem is OpenMcdf.CFStorage)
				{
					OpenMcdf.CFStorage olechildstorage = tostorage.AddStorage(childitem.Name);
					Write(olechildstorage, (OpenMcdf.CFStorage)childitem);
				}
			}
		}

		public StructuredStorageFileParser()
			: base()
		{
			this.ExpandFully = false;
			this.Depth = 1; // UInt32.MaxValue;
		}

		private void WriteStorage(OpenMcdf.CFStorage cfstorage, string directory, uint depth, ref System.Collections.Generic.List<string> outputfiles)
		{
			SFWorkflow.WFFileType.FileType type = SFWorkflow.WFFileType.GetOleFileType(cfstorage);
			if ((cfstorage.Name != "Root Entry") || ((cfstorage.Name == "Root Entry") && (type == SFWorkflow.WFFileType.FileType.OlePackage || type == SFWorkflow.WFFileType.FileType.OleContents)))
			{
				string filename = string.Empty;
				if (type == SFWorkflow.WFFileType.FileType.OlePackage)
				{
					Ole10Native olenative = new Ole10Native(new System.IO.MemoryStream(cfstorage.GetStream(Convert.ToChar((byte)1).ToString() + "Ole10Native").GetData()));
					filename = olenative.FileName;
					int idx = olenative.FileName.LastIndexOf("\\");
					if (idx != -1)
						filename = olenative.FileName.Substring(idx + 1);
					filename = String.Format("{0}\\{1}", directory, filename);
					using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write))
					{
						fs.Write(olenative.NativeData, 0, olenative.NativeData.Length);
					}
				}
				else if (type == SFWorkflow.WFFileType.FileType.OleContents)
				{
					filename = String.Format("{0}\\{1}", directory, "contents");
					using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write))
					{
						byte [] data = cfstorage.GetStream("CONTENTS").GetData();
						fs.Write(data, 0, data.Length);
					}
				}
				else
				{
					OpenMcdf.CompoundFile tmp = new OpenMcdf.CompoundFile();
					Write(tmp.RootStorage, cfstorage);
					filename = String.Format("{0}\\{1}", directory, cfstorage.Name);
					tmp.Save(filename);
					tmp.Close();
				}
				outputfiles.Add(filename);
			}

			if (depth == 0)
				return;
			depth--;

			if (type == SFWorkflow.WFFileType.FileType.OleWord || type == WFFileType.FileType.OlePublisher)
			{
				try
				{
					OpenMcdf.CFStorage objectpool = cfstorage.GetStorage(type == SFWorkflow.WFFileType.FileType.OleWord ? "ObjectPool" : "Objects");
					if (objectpool != null)
						cfstorage = objectpool;
				}
				catch (Exception)
				{
				}
			}

			if (cfstorage != null)
			{
				foreach (OpenMcdf.CFItem item in cfstorage.EnumChildren.Where(x => x.IsStorage))
				{
					string filedir = string.Format("{0}\\{1}", directory, SFWorkflow.WFUtilities.GetNextDirectoryNumber(directory));
					if (!System.IO.Directory.Exists(filedir))
						System.IO.Directory.CreateDirectory(filedir);
					if (System.IO.Directory.Exists(filedir))
						WriteStorage((OpenMcdf.CFStorage)item, filedir, depth, ref outputfiles);
				}
			}
		}

		private void ParseFile(string filename, uint depth, ref System.Collections.Generic.List<string> outputfiles)
		{
			SFWorkflow.WFFileType.FileType type = SFWorkflow.WFFileType.GetFileType(filename);
			if (type == SFWorkflow.WFFileType.FileType.OlePowerPoint)
			{
				uint fileidx = 0;

				System.Collections.Generic.List<string> pptfiles = new System.Collections.Generic.List<string>();
				DIaLOGIKa.b2xtranslator.StructuredStorage.Reader.StructuredStorageReader ssr = new DIaLOGIKa.b2xtranslator.StructuredStorage.Reader.StructuredStorageReader(filename);
				DIaLOGIKa.b2xtranslator.PptFileFormat.PowerpointDocument ppt = new DIaLOGIKa.b2xtranslator.PptFileFormat.PowerpointDocument(ssr);
				foreach (uint persistId in ppt.PersistObjectDirectory.Keys)
				{
					UInt32 offset = ppt.PersistObjectDirectory[persistId];
					ppt.PowerpointDocumentStream.Seek(offset, System.IO.SeekOrigin.Begin);
					DIaLOGIKa.b2xtranslator.PptFileFormat.ExOleObjStgAtom obj = DIaLOGIKa.b2xtranslator.OfficeDrawing.Record.ReadRecord(ppt.PowerpointDocumentStream) as DIaLOGIKa.b2xtranslator.PptFileFormat.ExOleObjStgAtom;
					if (obj != null)
					{
						string filedir = string.Format("{0}\\{1}", System.IO.Directory.GetParent(filename).FullName, SFWorkflow.WFUtilities.GetNextDirectoryNumber(System.IO.Directory.GetParent(filename).FullName));
						if (!System.IO.Directory.Exists(filedir))
							System.IO.Directory.CreateDirectory(filedir);
						if (System.IO.Directory.Exists(filedir))
						{
							byte[] data = obj.DecompressData();
							System.IO.MemoryStream ms = new System.IO.MemoryStream(data);

							SFWorkflow.WFFileType.FileType oletype = SFWorkflow.WFFileType.GetOleFileType(data);
							if (oletype == WFFileType.FileType.OlePackage || oletype == WFFileType.FileType.OleContents)
							{
								using (OpenMcdf.CompoundFile cf = new OpenMcdf.CompoundFile(ms))
								{
									WriteStorage(cf.RootStorage, filedir, depth, ref pptfiles);
								}
							}
							else
							{
								string filenm = String.Format("{0}\\pptembed{1}", filedir, fileidx);
								using (System.IO.FileStream fs = new System.IO.FileStream(filenm, System.IO.FileMode.Create, System.IO.FileAccess.Write))
								{
									byte[] buffer = new byte[1024];
									int len;
									while ((len = ms.Read(buffer, 0, buffer.Length)) > 0)
									{
										fs.Write(buffer, 0, len);
									}
									pptfiles.Add(filenm);
								}
								fileidx++;
								ms.Close();
								ms.Dispose();
							}
						}
					}
				}
#if false
				foreach (DIaLOGIKa.b2xtranslator.PptFileFormat.ExOleEmbedContainer ole in ppt.OleObjects.Values)
				{
					string filedir = string.Format("{0}\\{1}", System.IO.Directory.GetParent(filename).FullName, diridx++);
					if (!System.IO.Directory.Exists(filedir))
						System.IO.Directory.CreateDirectory(filedir);
					if (System.IO.Directory.Exists(filedir))
					{
						string filenm = String.Format("{0}\\pptembed{1}", filedir, ole.SiblingIdx);
						try
						{
							System.IO.FileStream fss = new System.IO.FileStream(filenm, System.IO.FileMode.Create, System.IO.FileAccess.Write);
							byte[] data = ole.stgAtom.DecompressData();
							fss.Write(data, 0, data.Length);
							fss.Flush();
							fss.Close();
							fss.Dispose();

							pptfiles.Add(filenm);
						}
						catch (Exception ex)
						{
						}
					}
				}
#endif
				foreach (DIaLOGIKa.b2xtranslator.OfficeDrawing.Record record in ppt.PicturesContainer._pictures.Values) //.Where(x => x.TypeCode == 0xF01C || x.TypeCode == 0xF01D || x.TypeCode == 0xF01E || x.TypeCode == 0xF01F || x.TypeCode == 0xF029 || x.TypeCode == 0xF02A))
				{
					string filedir = string.Format("{0}\\{1}", System.IO.Directory.GetParent(filename).FullName, "PPTPictures"); //, SFWorkflow.WFUtilities.GetNextDirectoryNumber(System.IO.Directory.GetParent(filename).FullName));
					if (!System.IO.Directory.Exists(filedir))
						System.IO.Directory.CreateDirectory(filedir);
					if (System.IO.Directory.Exists(filedir))
					{
						string extension = string.Empty;
						int skip = 0;
						switch (record.TypeCode)
						{
							case 0xF01A:
								extension = ".emf";
								break;

							case 0xF01B:
								extension = ".wmf";
								break;

							case 0xF01C:
								extension = ".pict";
								break;

							case 0xF01D:
							case 0xF02A:
								extension = ".jpg";
								skip = 17;
								break;

							case 0xF01E:
								extension = ".png";
								skip = 17;
								break;

							case 0xF01F:
								extension = ".dib";
								break;

							case 0xF029:
								extension = ".tiff";
								break;
						}
						string filenm = String.Format("{0}\\pptembed{1}{2}", filedir, fileidx++, extension);
						using(System.IO.FileStream fs = new System.IO.FileStream(filenm, System.IO.FileMode.Create, System.IO.FileAccess.Write))
						{
							// need to skip 17 byte header in raw data stream
							byte[] data = ((extension == ".emf" || extension == ".wmf")) ? ((DIaLOGIKa.b2xtranslator.OfficeDrawing.MetafilePictBlip)record).Decrompress() : record.RawData.Skip(skip).ToArray();
							fs.Write(data, 0, data.Length);
							fs.Flush();
							fs.Close();
							fs.Dispose();

							pptfiles.Add(filenm);
						}
					}
				}
				ssr.Close();
				ssr.Dispose();
				outputfiles.AddRange(pptfiles);
				depth--;
				if (depth > 0)
				{
					foreach(string fn in pptfiles)
						ParseFile(fn, depth, ref outputfiles);
				}
			}
			else
			{
				using (OpenMcdf.CompoundFile cf = new OpenMcdf.CompoundFile(filename))
				{
					WriteStorage(cf.RootStorage, System.IO.Directory.GetParent(filename).FullName, depth, ref outputfiles);
				}
			}
		}

		public override WFState Run()
		{
			WFState retval = new WFState(WFState.WFStateFail);

			System.Collections.Generic.List<string> outputfiles = new System.Collections.Generic.List<string>();
			ParseFile(this.FileToProcess, this.Depth, ref outputfiles);
			foreach (string outputfile in outputfiles)
			{
				string filenm = outputfile;
				SFWorkflow.WFFileType.FileType type = SFWorkflow.WFFileType.GetFileType(outputfile);
				if (type != SFWorkflow.WFFileType.FileType.Unknown)
				{
					string newfilenm = filenm;
					string extension = SFWorkflow.WFFileType.GetFileTypeExtension(type);
					if (System.IO.Path.GetExtension(filenm) != extension)
						newfilenm = String.Format("{0}{1}", filenm, extension);
					if (newfilenm != filenm)
					{
						if (System.IO.File.Exists(newfilenm))
							System.IO.File.Delete(newfilenm);
						System.IO.File.Move(filenm, newfilenm);
						filenm = newfilenm;
					}
				}
				this.OutputFiles.Add(filenm, "Process");
			}
			retval.Value = WFState.WFStateSuccess;

			return retval;
		}
	}
}
