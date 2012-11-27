using System;
using System.Collections.Generic;

using SFWorkflow.Abstracts;

using iTextSharp.text.pdf;

namespace PdfFileParser
{
	[Serializable]
	public class PdfFileParser : WFProcessor
	{
		public PdfFileParser()
			: base()
		{
		}

		public override SFWorkflow.WFState Run()
		{
			SFWorkflow.WFState retval = new SFWorkflow.WFState();

			Dictionary<String, byte[]> files = new Dictionary<String, byte[]>();

			PdfReader reader = new PdfReader(this.FileToProcess);
			PdfDictionary root = reader.Catalog;
			PdfDictionary names = root.GetAsDict(PdfName.NAMES); // may be null
			if (names != null)
			{
				PdfArray embeddedFiles = names.GetAsArray(PdfName.EMBEDDEDFILES); //may be null
				if (embeddedFiles != null)
				{
					//			int len = embeddedFiles.Length;
					//			for (int i = 0; i < len; i += 2)
					//			{
					//				PdfName name = embeddedFiles[i].; // should always be present
					//				PdfDictionary fileSpec = embeddedFiles.getAsDict(i + 1); // ditto
					//				PRStream stream = (PRStream)fileSpec.getAsStream(PdfName.EF);
					//				if (stream != null)
					//				{
					//					files.put(PdfName.decodeName(name.toString()), stream.getBytes());
					//				}
					//			}
				}
			}

			retval.Value = SFWorkflow.WFState.WFStateSuccess;

			return retval;
		}
	}
}
