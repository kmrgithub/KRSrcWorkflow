using System;
using System.Linq;

using KRSrcWorkflow;
using KRSrcWorkflow.Abstracts;

namespace ZipFileParser
{
	public class ZipFileParser : Processor<ZipFileParserData>
	{
		public override void Process(ZipFileParserData data)
		{
			data.WFState.Value = WFState.WFStateFail;

			Ionic.Zip.ZipFile zf = new Ionic.Zip.ZipFile(data.DocumentToProcess);
			string filedir = System.IO.Directory.GetParent(data.DocumentToProcess).FullName + "\\" + KRSrcWorkflow.WFUtilities.GetNextDirectoryNumber(System.IO.Directory.GetParent(data.DocumentToProcess).FullName).ToString(); // "\\expanded";
			foreach (Ionic.Zip.ZipEntry ze in zf.EntriesSorted.Where(x => !x.IsDirectory))
			{
				string zedir = ze.FileName.Replace("/", "\\");
				int idx = zedir.LastIndexOf('\\');
				string path = filedir;
				if (idx != -1)
				{
					path = filedir + "\\" + zedir.Substring(0, idx);
					zedir = zedir.Substring(idx + 1);
				}
				if (!System.IO.Directory.Exists(path))
					System.IO.Directory.CreateDirectory(path);
				if (System.IO.Directory.Exists(path))
				{
					using (System.IO.FileStream fss = new System.IO.FileStream(path + "\\" + zedir, System.IO.FileMode.Create, System.IO.FileAccess.Write))
					{
						ze.Extract(fss);
						data.OutputDocuments.Add(path + "\\" + zedir);
					}
				}
			}
			data.WFState.Value = WFState.WFStateSuccess;
		}
	}
}
