using System;

using dtSearch.Engine;

namespace Pst2Eml
{
	[Serializable]
	public class TextExtractor : InputOutputThreadObjectHandler<PSTProcessingResult>
	{
		public string FileToProcess { get; set;}
		public string ExportDirectory { get; set; }

		public TextExtractor()
		{
		}

		public TextExtractor(string infile, string exportdir)
		{
			this.FileToProcess = infile;
			this.ExportDirectory = exportdir;
		}

		public PSTProcessingResult ProcessQueueObject()
		{
			int start = System.Environment.TickCount;

			try
			{
				Options dtOptions = new Options();
				dtOptions.FieldFlags = FieldFlags.dtsoFfOfficeSkipHiddenContent;
				dtOptions.BinaryFiles = BinaryFilesSettings.dtsoIndexSkipBinary;
				dtOptions.Save();

				FileConverter fileConverter = new FileConverter();
				fileConverter.InputFile = this.FileToProcess;
				fileConverter.OutputFile = this.FileToProcess + ".xml";
				fileConverter.OutputFormat = OutputFormats.it_ContentAsXml;
				fileConverter.Flags = ConvertFlags.dtsConvertInlineContainer;

				fileConverter.Execute();

				JobErrorInfo errorInfo = fileConverter.Errors;
				if (errorInfo != null && errorInfo.Count > 0)
				{
					for (int i = 0; i < errorInfo.Count; i++)
						Console.WriteLine("DTSearch Error: " + errorInfo.Code(i));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			PSTProcessingResult result = new PSTProcessingResult() { IsSuccessful = true, Filename = this.FileToProcess + ".xml" };
			result.SetProcessingObject<TextExtractor>(this);
			return result;
		}
	}
}
