using System;
using System.Linq;

using SFWorkflow;
using SFWorkflow.Abstracts;

namespace ZipFileParser
{
	[Serializable]
	public class ZipFileParser : WFProcessor
	{
		public override WFState Run()
		{
			WFState retval = new WFState(WFState.WFStateFail);

			Ionic.Zip.ZipFile zf = new Ionic.Zip.ZipFile(this.FileToProcess);
			string filedir = System.IO.Directory.GetParent(this.FileToProcess).FullName + "\\" + SFWorkflow.WFUtilities.GetNextDirectoryNumber(System.IO.Directory.GetParent(this.FileToProcess).FullName).ToString(); // "\\expanded";
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
						this.OutputFiles.Add(path + "\\" + zedir);
					}
				}
			}
			retval.Value = WFState.WFStateSuccess;

			return retval;
		}
	}
}
