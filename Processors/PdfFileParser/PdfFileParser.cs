using System;
using System.Collections.Generic;
using System.IO;

using KRSrcWorkflow;
using KRSrcWorkflow.Abstracts;

using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace PdfFileParser
{
	public class PdfFileParser : Processor<PdfFileParserData>
	{
		private class MyImageRenderListener : IRenderListener
		{
			public void RenderText(TextRenderInfo renderInfo) { }
			public void BeginTextBlock() { }
			public void EndTextBlock() { }

			public List<byte[]> Images = new List<byte[]>();
			public List<string> ImageNames = new List<string>();
			public void RenderImage(ImageRenderInfo renderInfo)
			{
				PdfImageObject image = null; // renderInfo.GetImage();
				try
				{
					image = renderInfo.GetImage();
					if (image == null) return;

					ImageNames.Add(string.Format(
						"Image{0}.{1}", renderInfo.GetRef().Number, image.GetFileType()
					));
					using (MemoryStream ms = new MemoryStream(image.GetImageAsBytes()))
					{
						Images.Add(ms.ToArray());
					}
				}
				catch (Exception ie)
				{
					/*
					 * pass-through; image type not supported by iText[Sharp]; e.g. jbig2
					*/
                    WFLogger.NLogger.ErrorException("ERROR: RenderImage failed!", ie);
				}
			}
		}

		public override void Process(PdfFileParserData data)
		{
			data.WFState.Value = WFState.WFStateFail;

			Dictionary<String, byte[]> files = new Dictionary<String, byte[]>();

			PdfReader reader = new PdfReader(data.DocumentToProcess);
			PdfReaderContentParser parser = new PdfReaderContentParser(reader);
			MyImageRenderListener listener = new MyImageRenderListener();
			for (int i = 1; i <= reader.NumberOfPages; i++)
				parser.ProcessContent(i, listener);
			for (int i = 0; i < listener.Images.Count; ++i)
			{
				string filedir = string.Format("{0}\\{1}", Path.GetDirectoryName(data.DocumentToProcess), WFUtilities.GetNextDirectoryNumber(Path.GetDirectoryName(data.DocumentToProcess)));
				if (!Directory.Exists(filedir))
					Directory.CreateDirectory(filedir);
				if (Directory.Exists(filedir))
				{
					using (FileStream fs = new FileStream(string.Format("{0}\\{1}", filedir, listener.ImageNames[i]), FileMode.Create, FileAccess.Write))
					{
						fs.Write(listener.Images[i], 0, listener.Images[i].Length);
					}
					data.OutputDocuments.Add(string.Format("{0}\\{1}", filedir, listener.ImageNames[i]));
				}
			}

			data.WFState.Value = KRSrcWorkflow.WFState.WFStateSuccess;
		}
	}
}
