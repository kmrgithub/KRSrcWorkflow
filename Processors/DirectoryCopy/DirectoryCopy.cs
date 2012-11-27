using System;
using System.IO;

using KRSrcWorkflow;
using KRSrcWorkflow.Abstracts;

namespace DirectoryCopy
{
	public class DirectoryCopy : Processor<DirectoryCopyData>
	{
		public override void Process(DirectoryCopyData data)
		{
			data.WFState.Value = WFState.WFStateFail;
			try
			{
				if (Directory.Exists(data.DocumentToProcess))
				{
					if(!Directory.Exists(data.ExportDirectory))
						Directory.CreateDirectory(data.ExportDirectory);
					if (Directory.Exists(data.ExportDirectory))
					{
						new Microsoft.VisualBasic.Devices.Computer().FileSystem.CopyDirectory(data.DocumentToProcess, data.ExportDirectory);
						data.WFState.Value = WFState.WFStateSuccess;
					}
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
