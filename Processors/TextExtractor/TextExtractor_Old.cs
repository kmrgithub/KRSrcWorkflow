using System;

using dtSearch.Engine;

using SFWorkflow;
using SFWorkflow.Abstracts;

namespace TextExtractor
{
	[Serializable]
	public class TextExtractor : WFProcessor
	{
		public TextExtractor()
		{
		}

		public TextExtractor(string infile, string exportdir)
		{
			this.FileToProcess = infile;
			this.ExportDirectory = exportdir;
		}

		public override WFState Run()
		{
			WFState retval = new WFState();
			try
			{
				retval.Value = WFState.WFStateFail;

				Options dtOptions = new Options
				                    	{
				                    		FieldFlags = FieldFlags.dtsoFfOfficeSkipHiddenContent,
				                    		BinaryFiles = BinaryFilesSettings.dtsoIndexSkipBinary
				                    	};
				dtOptions.Save();

				FileConverter fileConverter = new FileConverter
				                              	{
				                              		InputFile = this.FileToProcess,
				                              		OutputFile = this.FileToProcess + ".dts",
				                              		OutputFormat = OutputFormats.it_ContentAsXml,
				                              		Flags = ConvertFlags.dtsConvertInlineContainer
				                              	};
				fileConverter.Execute();
				this.OutputFiles.Add(this.FileToProcess + ".dts");
				retval.Value = WFState.WFStateSuccess;

				JobErrorInfo errorInfo = fileConverter.Errors;
				if (errorInfo != null && errorInfo.Count > 0)
				{
					for (int i = 0; i < errorInfo.Count; i++)
						SFWorkflow.WFLogger.NLogger.Error(string.Format("DTSearch Error: ErrorCode={0}  ErrorMessage={1}", errorInfo.Message(i), errorInfo.Code(i)));
				}
			}
			catch (Exception ex)
			{
				SFWorkflow.WFLogger.NLogger.ErrorException("ERROR: TextExtractor.Run", ex);
			}

			return retval;
		}
	}
}
