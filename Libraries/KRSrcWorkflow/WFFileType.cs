using System;
using System.Globalization;
using System.Linq;

namespace KRSrcWorkflow
{
	public class WFFileType
	{
		private static readonly byte[] ZipHdr = new byte[] { 0x50, 0x4B };
		private static readonly byte[] CabHdr = new byte[] { 0x4D, 0x53, 0x43, 0x46 };
		private static readonly byte[] RarHdr = new byte[] { 0x52, 0x61, 0x72, 0x21 };
		private static readonly byte[] GZipHdr = new byte[] { 0x15, 0x8B, 0x08 };
		private static readonly byte[] OleHdr = new byte[] { 0xD0, 0xCF, 0x11, 0xE0 };
		private static readonly byte[] PstHdr = new byte[] { 0x21, 0x42, 0x44, 0x4E };
		private static readonly byte[] PdfHdr = new byte[] { 0x25, 0x50, 0x44, 0x46 };
		private static readonly byte[] WmfHdr = new byte[] { 0x01, 0x00, 0x09, 0x00, 0x00, 0x03 };
		private static readonly byte[] PngHdr = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
		private static readonly byte[] Tiff1Hdr = new byte[] { 0x49, 0x20, 0x49 };
		private static readonly byte[] Tiff2Hdr = new byte[] { 0x49, 0x49, 0x2A, 0x00 };
		private static readonly byte[] Tiff3Hdr = new byte[] { 0x4D, 0x4D, 0x00, 0x2A };
		private static readonly byte[] Tiff4Hdr = new byte[] { 0x4D, 0x4D, 0x00, 0x2B };
		private static readonly byte[] ExeDll = new byte[] { 0x4D, 0x5A };
		private static readonly byte[] Jpeg = new byte[] { 0xFF, 0xD8, 0xFF };
		private static readonly byte[] Gif = new byte[] { 0x47, 0x49, 0x46 };
		private static readonly System.Collections.Generic.Dictionary<FileType, string> FileTypeExtensionMap = new System.Collections.Generic.Dictionary<FileType, string>() { 
			{FileType.Unknown, string.Empty},
			{FileType.Cab, "cab"},
			{FileType.GZip, "gz"},
			{FileType.Ole, string.Empty},
			{FileType.OleContents, string.Empty},
			{FileType.OleExcel, "xls"},
			{FileType.OleMsg, "msg"},
			{FileType.OlePackage, string.Empty},
			{FileType.OlePowerPoint, "ppt"},
			{FileType.OleVisio, "vsd"},
			{FileType.OleWord, "doc"},
			{FileType.Pdf, "pdf"},
			{FileType.Pst, "pst"},
			{FileType.Rar, "rar"},
			{FileType.Zip, "zip"},
			{FileType.OleProject, "mpp"},
			{FileType.OlePublisher, "pub"},
			{FileType.OleMsi, "msi"},
			{FileType.Wmf, "wmf"},
			{FileType.OleMsp, "msp"},
			{FileType.OleMst, "mst"},
			{FileType.Png, "png"},
			{FileType.Tiff, "tif"},
			{FileType.Jpeg, "jpg"},
			{FileType.Gif, "gif"}
		};

		public enum FileType { Unknown, Zip, Cab, Rar, GZip, Ole, Pst, Pdf, OleExcel, OleWord, OlePowerPoint, OleVisio, OleMsg, OlePackage, OleContents, OleProject, OlePublisher, OleMsi, OleMsp, OleMst, Wmf, Png, Tiff, ExeDll, Jpeg, Gif };
		public static string GetFileTypeExtension(FileType filetype)
		{
			string extension = string.Empty;

			if (FileTypeExtensionMap.ContainsKey(filetype))
				extension = FileTypeExtensionMap[filetype];

			return extension == string.Empty ? string.Empty : "." + extension;
		}

		public static FileType GetFileType(string filename)
		{
			byte[] buffer = new byte[8]{0, 0, 0, 0, 0, 0, 0, 0};

			if (System.IO.File.Exists(filename))
			{
				using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
				{
					fs.Read(buffer, 0, buffer.Length);

					if (buffer.Take(ZipHdr.Length).SequenceEqual(ZipHdr))
						return FileType.Zip;
					else if (buffer.Take(CabHdr.Length).SequenceEqual(CabHdr))
						return FileType.Cab;
					else if (buffer.Take(RarHdr.Length).SequenceEqual(RarHdr))
						return FileType.Rar;
					else if (buffer.Take(GZipHdr.Length).SequenceEqual(GZipHdr))
						return FileType.GZip;
					else if (buffer.Take(OleHdr.Length).SequenceEqual(OleHdr))
					{
						fs.Seek(0, System.IO.SeekOrigin.Begin);
						return GetOleFileType(fs);
					}
					else if (buffer.Take(PstHdr.Length).SequenceEqual(PstHdr))
						return FileType.Pst;
					else if (buffer.Take(PdfHdr.Length).SequenceEqual(PdfHdr))
						return FileType.Pdf;
					else if (buffer.Take(WmfHdr.Length).SequenceEqual(WmfHdr))
						return FileType.Wmf;
					else if (buffer.Take(PngHdr.Length).SequenceEqual(PngHdr))
						return FileType.Png;
					else if (buffer.Take(Tiff1Hdr.Length).SequenceEqual(Tiff1Hdr))
						return FileType.Tiff;
					else if (buffer.Take(Tiff2Hdr.Length).SequenceEqual(Tiff2Hdr))
						return FileType.Tiff;
					else if (buffer.Take(Tiff3Hdr.Length).SequenceEqual(Tiff3Hdr))
						return FileType.Tiff;
					else if (buffer.Take(Tiff4Hdr.Length).SequenceEqual(Tiff4Hdr))
						return FileType.Tiff;
					else if (buffer.Take(ExeDll.Length).SequenceEqual(ExeDll))
						return FileType.ExeDll;
					else if (buffer.Take(Jpeg.Length).SequenceEqual(Jpeg))
						return FileType.Jpeg;
					else if (buffer.Take(Gif.Length).SequenceEqual(Gif))
						return FileType.Gif;
				}
			}

			return FileType.Unknown;
		}

		public static FileType GetOleFileType(OpenMcdf.CFStorage cfstorage)
		{
			switch(cfstorage.CLSID.ToString())
			{
				case "000C1084-0000-0000-C000-000000000046":
					return FileType.OleMsi;
				case "000C1086-0000-0000-C000-000000000046":
					return FileType.OleMsp;
				case "000C1082-0000-0000-C000-000000000046":
					return FileType.OleMst;
				case "00020810-0000-0000-c000-000000000046":
					return FileType.OleExcel;
				case "64818d10-4f9b-11cf-86ea-00aa00b929e8":
					return FileType.OlePowerPoint;
				case "00020906-0000-0000-c000-000000000046":
					return FileType.OleWord;
				case "00021a14-0000-0000-c000-000000000046":
					return FileType.OleVisio;
				case "74b78f3a-c8c8-11d1-be11-00c04fb6faf1":
					return FileType.OleProject;
				case "00021201-0000-0000-00c0-000000000046":
					return FileType.OlePublisher;
				case "00020d0b-0000-0000-c000-000000000046":
					return FileType.OleMsg;
			}

			foreach (OpenMcdf.CFItem item in cfstorage.EnumChildren.Where(x => x.IsStream))
			{
				if (item.Name == Convert.ToChar((byte)1).ToString(CultureInfo.InvariantCulture) + "Ole10Native")
					return FileType.OlePackage;
				if (item.Name == "CONTENTS")
					return FileType.OleContents;
				if (item.Name == "WordDocument")
					return FileType.OleWord;
				else if (item.Name == "Workbook")
					return FileType.OleExcel;
				else if (item.Name == "VisioDocument")
					return FileType.OleVisio;
				else if (item.Name == "PowerPoint Document")
					return FileType.OlePowerPoint;
			}

			return FileType.Ole;
		}

		public static FileType GetOleFileType(byte [] data)
		{
			FileType type = FileType.Unknown;
			System.IO.MemoryStream stream = new System.IO.MemoryStream(data);
			using (OpenMcdf.CompoundFile cf = new OpenMcdf.CompoundFile(stream))
			{
				type = GetOleFileType(cf.RootStorage);
			}

			return type;
		}

		public static FileType GetOleFileType(System.IO.Stream stream)
		{
			FileType type = FileType.Unknown;
			using (OpenMcdf.CompoundFile cf = new OpenMcdf.CompoundFile(stream))
			{
				type = GetOleFileType(cf.RootStorage);
			}

			return type;
		}
	}
}
