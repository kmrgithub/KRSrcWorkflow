using System;

using dtSearch.Engine;

using KRSrcWorkflow;
using KRSrcWorkflow.Abstracts;

namespace TextExtractor
{
	public class TextExtractor : Processor<TextExtractorData>
	{
		public override void Process(TextExtractorData data)
		{
			data.WFState.Value = WFState.WFStateFail;
			try
			{
				Options dtOptions = new Options
				                    	{
				                    		FieldFlags = FieldFlags.dtsoFfOfficeSkipHiddenContent,
				                    		BinaryFiles = BinaryFilesSettings.dtsoIndexSkipBinary
				                    	};
				dtOptions.Save();

				FileConverter fileConverter = new FileConverter
				                              	{
				                              		InputFile = data.DocumentToProcess,
																					OutputFile = data.DocumentToProcess + ".dts",
				                              		OutputFormat = OutputFormats.it_ContentAsXml,
				                              		Flags = ConvertFlags.dtsConvertInlineContainer
				                              	};
				fileConverter.Execute();
				data.OutputDocuments.Add(data.DocumentToProcess + ".dts");
				data.WFState.Value = WFState.WFStateSuccess;

				JobErrorInfo errorInfo = fileConverter.Errors;
				if (errorInfo != null && errorInfo.Count > 0)
				{
					for (int i = 0; i < errorInfo.Count; i++)
						KRSrcWorkflow.WFLogger.NLogger.Error(string.Format("DTSearch Error: ErrorCode={0}  ErrorMessage={1}", errorInfo.Message(i), errorInfo.Code(i)));
				}
			}
			catch (Exception ex)
			{
				KRSrcWorkflow.WFLogger.NLogger.ErrorException("ERROR: TextExtractor.Run", ex);
			}
		}
	}
}
