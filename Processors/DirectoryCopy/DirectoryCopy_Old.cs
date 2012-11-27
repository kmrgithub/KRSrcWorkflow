using System;
using System.IO;

using SFWorkflow;
using SFWorkflow.Abstracts;

namespace DirectoryCopy
{
	[Serializable]
	public class DirectoryCopy : WFProcessor
	{
		public DirectoryCopy()
		{
		}

		public DirectoryCopy(string srcdir, string targetdir)
		{
			this.FileToProcess = srcdir;
			this.ExportDirectory = targetdir;
		}

		public override WFState Run()
		{
			WFState retval = new WFState();
			try
			{
				retval.Value = WFState.WFStateFail;
				if (Directory.Exists(this.FileToProcess))
				{
					if(!Directory.Exists(this.ExportDirectory))
						Directory.CreateDirectory(this.ExportDirectory);
					if (Directory.Exists(this.ExportDirectory))
					{
						new Microsoft.VisualBasic.Devices.Computer().FileSystem.CopyDirectory(this.FileToProcess, this.ExportDirectory); 
						retval.Value = WFState.WFStateSuccess;
					}
				}
			}
			catch (Exception)
			{
			}

			return retval;
		}
	}
}
