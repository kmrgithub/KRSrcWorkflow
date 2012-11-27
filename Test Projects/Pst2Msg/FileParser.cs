using System;
using System.Linq;

using dtSearch.Engine;

namespace Pst2Eml
{
	[Serializable]
	public class FileParser : InputOutputThreadObjectHandler<PSTProcessingResult>
	{
		private static readonly byte [] ZipHdr = new byte[] { 0x50, 0x4B};
		private static readonly byte[] CabHdr = new byte[] { 0x4D, 0x53, 0x43, 0x46 };
		private static readonly byte[] RarHdr = new byte[] { 0x52, 0x61, 0x72, 0x21 };
		private static readonly byte[] GZipHdr = new byte[] { 0x15, 0x8B, 0x08 };
		private static readonly byte[] OleHdr = new byte[] { 0xD0, 0xCF, 0x11, 0xE0 };
		private static readonly byte[] PstHdr = new byte[] { 0x21, 0x42, 0x44, 0x4E };
		public enum FileParserType { Unknown, Zip, Cab, Rar, GZip, Ole, Pst };

		static FileParserType FileType(string filename)
		{
			byte[] buffer = new byte[4];
			using (System.IO.FileStream fileStream = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
			{
				fileStream.Read(buffer, 0, buffer.Length);
			}

			if (buffer.Take(ZipHdr.Length).SequenceEqual(ZipHdr))
				return FileParserType.Zip;
			else if (buffer.Take(CabHdr.Length).SequenceEqual(CabHdr))
				return FileParserType.Cab;
			else if (buffer.Take(RarHdr.Length).SequenceEqual(RarHdr))
				return FileParserType.Rar;
			else if (buffer.Take(GZipHdr.Length).SequenceEqual(GZipHdr))
				return FileParserType.GZip;
			else if (buffer.Take(OleHdr.Length).SequenceEqual(OleHdr))
				return FileParserType.Ole;
			else if (buffer.Take(PstHdr.Length).SequenceEqual(PstHdr))
				return FileParserType.Pst;

			return FileParserType.Unknown;
		}

		public string FileToProcess { get; set;}
		public string ExportDirectory { get; set; }
		public FileParserType Type { get; set; }

		public FileParser()
			: this(string.Empty, FileParserType.Unknown, string.Empty)
		{
		}

		public FileParser(string infile, FileParserType type, string exportdir)
		{
			this.FileToProcess = infile;
			this.ExportDirectory = exportdir;
			this.Type = FileParser.FileType(this.FileToProcess);
		}

		public PSTProcessingResult ProcessQueueObject()
		{
			bool issuccessful = false;

			switch(this.Type)
			{
				case FileParserType.Ole:
					using (OpenMcdf.CompoundFile cf = new OpenMcdf.CompoundFile(this.FileToProcess))
					{
					}
					break;
			}

			PSTProcessingResult result = new PSTProcessingResult() { IsSuccessful = issuccessful, Filename = this.FileToProcess + ".xml" };
			result.SetProcessingObject<FileParser>(this);
			return result;
		}
	}
}
