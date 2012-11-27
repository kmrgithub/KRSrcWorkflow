using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;

using dtSearch.Engine;

namespace docuity.releaseToAnalytics.bll
{
	class ErrorDataObject
	{
		private String _errorcode;
		private String _errortext;
		private String _level;

		public ErrorDataObject(String ec, String et, String le)
		{
			_errorcode = ec;
			_errortext = et;
			_level = le;
		}

		public string errorcode
		{
			get { return _errorcode; }
			set { _errorcode = value; }
		}

		public string errortext
		{
			get { return _errortext; }
			set { _errortext = value; }
		}

		public string level
		{
			get { return _level; }
			set { _level = value; }
		}
	}

	class ExtractTextAndMetadata
	{
		List<TypeId> imageTypes = new List<TypeId>();
		string _docid;
		string _sourceFileName;
		string _title;
		string _indexText;
		string _analysisText;
		List<ErrorDataObject> _errObjs;
		bool _errorFlag;

		public ExtractTextAndMetadata()
		{
			imageTypes.Add(TypeId.it_Binary);
			imageTypes.Add(TypeId.it_FilteredBinary);
			imageTypes.Add(TypeId.it_FilteredBinaryUnicode);
			imageTypes.Add(TypeId.it_FilteredBinaryUnicodeStream);
			imageTypes.Add(TypeId.it_Media);
			imageTypes.Add(TypeId.it_NonTextData);
			imageTypes.Add(TypeId.it_MP3);
			imageTypes.Add(TypeId.it_MP4);
			imageTypes.Add(TypeId.it_M4A);
			imageTypes.Add(TypeId.it_MPG);
			imageTypes.Add(TypeId.it_JPEG);
			imageTypes.Add(TypeId.it_TIFF);
			imageTypes.Add(TypeId.it_MDI);
			imageTypes.Add(TypeId.it_GIF);
			imageTypes.Add(TypeId.it_PNG);
			imageTypes.Add(TypeId.it_WAV);
			imageTypes.Add(TypeId.it_BMP);
			imageTypes.Add(TypeId.it_AVI);
		}

		public void setParams(string docID, string sourceFileName, string title, ref string indexText, ref string analysisText,
															 List<ErrorDataObject> errObjs)
		{
			_docid = docID;
			_sourceFileName = sourceFileName;
			_title = title;
			_indexText = indexText;
			_analysisText = analysisText;
			_errObjs = errObjs;
		}

		public void runExtractText()
		{
			extractText(_docid, _sourceFileName, _title, ref _indexText, ref _analysisText, _errObjs, out _errorFlag);
		}

		public void getReturnValues(ref string indexText, ref string analysisText,
															 List<ErrorDataObject> errObjs, out bool errorFlag)
		{
			indexText = _indexText;
			analysisText = _analysisText;
			errObjs = _errObjs;
			errorFlag = _errorFlag;
		}

		// called to extract text from a file
		// passed - sourceFileName - full path of file to extract text from
		// returns -
		// indexText - text that will be used for Lucene indexing, includes metadata
		// analysisText - text that will be used for clustering and LSA
		// errorFlag - false if an unrecoverable error occurred, else true
		// errorText - text of error if one occurred
		public void extractText(string docID, string sourceFileName, string title, ref string indexText, ref string analysisText,
															 List<ErrorDataObject> errObjs, out bool errorFlag)
		{
			Options dtOptions;
			FileConverter fileConverter;
			StringBuilder outStringIndex = new StringBuilder();
			StringBuilder outStringAnalysis = new StringBuilder();

			errorFlag = true;
			indexText = "";
			analysisText = "";

			try
			{
				// construct temporary file name for xml file output by dtSearch
				string targetFileNameDTSearch = @"C:\temp\_DTSearch.txt";
				File.Delete(targetFileNameDTSearch);

				dtOptions = new Options();
				dtOptions.FieldFlags = FieldFlags.dtsoFfOfficeSkipHiddenContent;
				dtOptions.BinaryFiles = BinaryFilesSettings.dtsoIndexSkipBinary;
				dtOptions.Save();

				fileConverter = new FileConverter();
				fileConverter.InputFile = sourceFileName;
				fileConverter.OutputFile = targetFileNameDTSearch;
				fileConverter.OutputFormat = OutputFormats.it_ContentAsXml;
				fileConverter.Flags = ConvertFlags.dtsConvertInlineContainer;

				fileConverter.Execute();

				//check for image file type
				TypeId deType = fileConverter.DetectedTypeId;
				if (imageTypes.Contains(deType))
				{
					errObjs.Add(new ErrorDataObject("1002", "Image File Type: " + deType.ToString(), "Warning"));
				}

				// return if there is a dtSearch error other than file corrupt (10) or file encrypted (17)
				JobErrorInfo errorInfo = fileConverter.Errors;
				bool fatalError = false;
				bool fileMissingOrNoText = false;
				int dtErCode = 0;
				if (errorInfo != null && errorInfo.Count > 0)
				{
					for (int i = 0; i < errorInfo.Count; i++)
					{
						dtErCode = errorInfo.Code(i);
						string errorCode = "";
						if (dtErCode != 9 && dtErCode != 10 && dtErCode != 17 && dtErCode != 207 && dtErCode != 16 && dtErCode != 21)
						{
							errObjs.Add(new ErrorDataObject("1005", "Text extraction Error occurred during processing of the document. " + errorInfo.Message(i), "Error"));
							fatalError = true;
						}
						else
						{
							string errText = "";
							if (dtErCode == 10)			// dtsErFileCorrupt
							{
								errorCode = "1013";
								errText = "Document is corrupted.";
							}
							if (dtErCode == 17)			// dtsErFileEncrypted 
							{
								errorCode = "1007";
								errText = "A component of the document is encrypted.";
							}
							if (dtErCode == 207)		// dtsErContainerItemEncrypted, internal error code
							{
								errorCode = "1014";
								errText = "The document is encrypted.";
								string text = errorInfo.Message(i);
								if (text != null)
								{
									int index = text.IndexOf("->");
									if (index >= 0)
									{
										errText = "A component of the document is encrypted. " + text.Substring(index);
									}
								}
							}
							if (dtErCode == 9)			// dtsErAccFile
							{
								errorCode = "1010";
								errText = "The system cannot access the file specified.";
							}
							if (dtErCode == 16)			// dtsErFileNotFound
							{
								errorCode = "1011";
								errText = "Document file does not exist.";
							}
							if (dtErCode == 21)			// dtsErFileEmpty
							{
								errorCode = "1012";
								errText = "Document file is empty";
							}

							if (dtErCode == 9 || dtErCode == 10 || dtErCode == 207 || dtErCode == 16 || dtErCode == 21)
								fileMissingOrNoText = true;	// file missing, no text, corrupt or encrypted

							if (errText == "")
								errText = errorInfo.Message(i);
							errObjs.Add(new ErrorDataObject(errorCode, "Text extraction error: " + errText, "Warning"));
						}
					}
				}

				if (fatalError)
				{
					errorFlag = false;
					return;
				}
				else
				{
					if (fileMissingOrNoText)
						return;

					if (dtErCode == 17)
					{
						FileInfo fi = new FileInfo(targetFileNameDTSearch);
						if (fi.Length == 0)
						{
							errObjs.Clear();	// remove error "1007"
							errObjs.Add(new ErrorDataObject("1014", "Text extraction error: document is encrypted.", "Warning"));
							return;
						}
					}

					//load the dtSearch XML output file into an XML document
					XmlDocument xmlDoc = new XmlDocument();
					try
					{
						xmlDoc.Load(targetFileNameDTSearch);
					}
					catch
					{
						//try cleaning up the metadata tags and loading again
						cleanMetadataTags(targetFileNameDTSearch);
						xmlDoc.Load(targetFileNameDTSearch);
					}

					//start with the document node
					XmlNode docNode = xmlDoc.DocumentElement;

					//initialize the output strings
					outStringIndex.Length = 0;
					outStringIndex.AppendLine("DocID: " + docID);
					if (title != null && title.Length > 0)
						outStringIndex.AppendLine("Filename: " + title);
					outStringAnalysis.Length = 0;

					//start outputting with the document node
					outputNode(docNode, outStringIndex, outStringAnalysis, errObjs);

					indexText = outStringIndex.ToString();
					analysisText = outStringAnalysis.ToString();

					//signal error if no analysis text
					if (analysisText.Length == 0)
						errObjs.Add(new ErrorDataObject("1003", "No text to analyze", "Warning"));

					return;
				}
			}
			catch (Exception ex)
			{
				errObjs.Add(new ErrorDataObject("1001", "Text extraction Error occurred during processing of the document. " + ex.Message, "Error"));
				errorFlag = false;
				return;
			}
		}

		// outputNode is passed an xml document or section node and outputs its contents to outString.
		// outputNode first handles all child Filename nodes, then all child Fields nodes, then all child #text nodes,
		// then calls itself recursively to handle all document and section nodes.
		public void outputNode(XmlNode xNode, StringBuilder outStringIndex, StringBuilder outStringAnalysis,
													 List<ErrorDataObject> errObjs)
		{
			// if this is a "DocFile" node, preceed it with a line of dashes.
			XmlAttributeCollection ac = xNode.Attributes;
			string nType = string.Empty;
			if (ac != null && ac.Count > 0)
			{
				XmlNode xmltype = ac.GetNamedItem("type");
				if(xmltype != null)
					nType = xmltype.Value;
			}
//			else
//				nType = "";
			if (nType.StartsWith("DocFile") || nType.StartsWith("Attachment") || nType.StartsWith("Extracted.ZIP"))
			{
				if (nType.StartsWith("DocFile"))
					nType = nType.Remove(0, 8);
				if (nType.StartsWith("Extracted."))
					nType = "Extracted from Zip";

				outStringIndex.AppendLine("------------------------------------" + nType + "--------------------------------------");
			}
			// check for FileCorruptError type
			if (nType.Equals("FileCorruptError"))
				errObjs.Add(new ErrorDataObject("1006", "Document component is corrupt", "Warning"));
			// check for FileEncryptedError type
			if (nType.Equals("FileEncryptedError"))
				errObjs.Add(new ErrorDataObject("1007", "Document component is encrypted", "Warning"));

			// check to see if current node has an attribute called CONTENTS
			string nameValue = "";
			bool contentsAttribute = CheckForContents(xNode, ref nameValue);

			XmlNode iNode;
			//get all field nodes first
			XmlNodeList oNodeList = xNode.ChildNodes;
			int totalNodeCount = oNodeList.Count;
			int filenameNodeCount = 0;
			for (int x = 0; x < totalNodeCount; x++)
			{
				iNode = oNodeList.Item(x);
				if (iNode.Name == "Filename")
				{
					processFilenameNode(iNode, outStringIndex, outStringAnalysis);
					filenameNodeCount++;
				}
			}
			int fieldsNodeCount = 0;
			for (int x = 0; x < totalNodeCount; x++)
			{
				iNode = oNodeList.Item(x);
				if (iNode.Name == "Fields")
				{
					processFieldsNode(iNode, outStringIndex, outStringAnalysis, errObjs);
					fieldsNodeCount++;
				}
			}
			int textNodeCount = 0;
			for (int x = 0; x < totalNodeCount; x++)
			{
				iNode = oNodeList.Item(x);
				if (iNode.Name == "#text")
				{
					processTextNode(iNode, contentsAttribute, nameValue, outStringIndex, outStringAnalysis, errObjs);
					textNodeCount++;
				}
			}
			int documentNodeCount = 0;
			for (int x = 0; x < totalNodeCount; x++)
			{
				iNode = oNodeList.Item(x);
				if (iNode.Name == "document")
				{
					outputNode(iNode, outStringIndex, outStringAnalysis, errObjs);
					documentNodeCount++;
				}
			}
			int sectionNodeCount = 0;
			for (int x = 0; x < totalNodeCount; x++)
			{
				iNode = oNodeList.Item(x);
				if (iNode.Name == "Section")
				{
					outputNode(iNode, outStringIndex, outStringAnalysis, errObjs);
					sectionNodeCount++;
				}
			}

			// if this is a document node that has no section nodes or text nodes,
			// or this is a section node without any text nodes, flag component w/o
			// text issue.
			if ((xNode.Name.Equals("document") && textNodeCount == 0 && sectionNodeCount == 0) ||
					(xNode.Name.Equals("Section") && textNodeCount == 0 && sectionNodeCount == 0))
				errObjs.Add(new ErrorDataObject("1009", "Document component has no text", "Warning"));

			if (filenameNodeCount + fieldsNodeCount + textNodeCount + documentNodeCount + sectionNodeCount != totalNodeCount)
			{
				// process other type nodes.
				string otherNodes = "";
				for (int x = 0; x < totalNodeCount; x++)
				{
					iNode = oNodeList.Item(x);
					if (iNode.Name != "Filename" && iNode.Name != "Fields" && iNode.Name != "#text" &&
							iNode.Name != "document" && iNode.Name != "Section")
					{
						processMiscNode(iNode, outStringIndex, outStringAnalysis);
						otherNodes = otherNodes + iNode.Name + "; ";
					}
				}

				string strMismatch = "Unexpected node type encountered: " + otherNodes + " Total Count = " + totalNodeCount +
														 " Text Count = " + textNodeCount +
														 " Filename Count = " + filenameNodeCount +
														 " Document Count = " + documentNodeCount +
														 " Fields Count = " + fieldsNodeCount +
														 " Section Count = " + sectionNodeCount;
				errObjs.Add(new ErrorDataObject("1004", strMismatch, "Warning"));
			}
		}

		public void processFilenameNode(XmlNode xNode, StringBuilder outStringIndex, StringBuilder outStringAnalysis)
		{
			string fileName = xNode.InnerText;

			//dont write anything out. We will write out the file_name field of the metadata.

			//outStringIndex.AppendLine("Filename: " + Regex.Replace(fileName, @"\s", ""));
			//outStringIndex.AppendLine();
		}

		public void processFieldsNode(XmlNode xNode, StringBuilder outStringIndex, StringBuilder outStringAnalysis,
																	List<ErrorDataObject> errObjs)
		{
			XmlNodeList oNodeList = xNode.ChildNodes;
			int totalNodeCount = oNodeList.Count;
			for (int x = 0; x < totalNodeCount; x++)
			{
				XmlNode iNode = oNodeList.Item(x);
				if (iNode.Name.Equals("#text"))
					continue;
				//string fieldLine = "SA_" + iNode.Name + ": " + (iNode.InnerText.Trim());
				string fieldLine = iNode.Name + ": " + (iNode.InnerText.Trim());
				outStringIndex.AppendLine(fieldLine);
				if (iNode.Name.Equals("Subject"))
					outStringAnalysis.AppendLine(iNode.InnerText.Trim());
				if (iNode.Name.Equals("Image_Format"))
					errObjs.Add(new ErrorDataObject("1008", "Document component is image file", "Warning"));
			}
			outStringIndex.AppendLine();
		}

		// processes text node.
		// passed - xNode - the text node to be processed,
		//          contentsAttribute - true if the parent section node has a CONTENTS attribute, else false.
		//          nameValue - value of the name attribute of the parent node
		public void processTextNode(XmlNode xNode, bool contentsAttribute, string nameValue, StringBuilder outStringIndex, StringBuilder outStringAnalysis,
																List<ErrorDataObject> errObjs)
		{
			string textLine = xNode.Value;
			// dont output dtSearch "Filename:" line in section by itself
			if (textLine.Length <= 100)
			{
				string stripLine = Regex.Replace(textLine, @"\s", "");
				if (stripLine.Equals("Filename:"))
					return;
				//note that the replace removes blanks from inside the tested for strings.
				if (stripLine.Contains("[PNGImage]") || stripLine.Contains("[MPEGFile]"))
					errObjs.Add(new ErrorDataObject("1008", "Document component is image file", "Warning"));
			}
			// remove extra "CONTENTS" string that dtSearch prepends to certain output
			if (contentsAttribute && textLine.StartsWith("CONTENTS"))
				textLine = textLine.Substring(8);
			// remove Internet Header section
			textLine = RemoveInternetHeader(textLine);

			//if the beginning of the text is the same as the value of the name attribute of the parent node,
			//and there is a -> in it, it is a pointer from a container file to an extracted file.  The container 
			//file name has our physical path so take out the container file path.
			if (nameValue.Length > 0 && nameValue.Contains("->") && textLine.StartsWith(nameValue))
			{
				string textLine2 = (string)textLine.Clone();
				int ptrOffset = textLine2.IndexOf("->");
				textLine2 = textLine2.Substring(ptrOffset + 2);
				outStringIndex.AppendLine(textLine2);
			}
			else
				outStringIndex.AppendLine(textLine);

			//if the beginning of the text is the same as the value of the name attribute of the parent node,
			//it is metadata so dont output it to the analysis text
			if (nameValue.Length > 0 && textLine.StartsWith(nameValue))
				textLine = textLine.Substring(nameValue.Length);
			outStringAnalysis.AppendLine(textLine);
		}

		//passed an xml node
		//returns - true - element contains attribute where Name = "name" and Value = "CONTENTS"
		//          false - otherwise
		//returns value of the Name attribute in nameValue
		public bool CheckForContents(XmlNode xNode, ref string nv)
		{
			XmlAttributeCollection ac = xNode.Attributes;
			IEnumerator ie = ac.GetEnumerator();
			nv = "";
			while (ie.MoveNext())
			{
				XmlAttribute xa = (XmlAttribute)ie.Current;
				if (xa.Name.Equals("name"))
				{
					nv = xa.Value;
					if (nv.Equals("CONTENTS"))
						return true;
				}
			}
			return false;
		}

		// passed a string. removes Internet Header.  It assumes that the header is the last text in the string,
		// that a header will contain the string "MIME-Version:",
		// and that the header begins after the first "CRLFCRLF" combination preceding "MIME-Version:"
		public string RemoveInternetHeader(string str)
		{
			int mimeIndex = str.IndexOf("MIME-");
			if (mimeIndex == -1)
				return str;
			int startOfHeader = str.LastIndexOf("\n\n", mimeIndex);
			if (startOfHeader == -1)
				startOfHeader = str.LastIndexOf("\r\n\r\n", mimeIndex);
			if (startOfHeader == -1)
				return str;
			else
				return str.Substring(0, startOfHeader);
		}

		private void cleanMetadataTags(string filePath)
		{
			StreamReader srDTSearchr = new StreamReader(filePath);
			string dtFileContents = srDTSearchr.ReadToEnd();
			srDTSearchr.Close();
			srDTSearchr.Dispose();
			MatchCollection mc = Regex.Matches(dtFileContents, "</?[^<>]+>");
			foreach (Match m in mc)
			{
				string sTag = m.Value;
				int sLen = m.Length;
				if (sTag.StartsWith("<Section") || sTag.StartsWith("<Fields"))
					continue;
				string sTagClean = Regex.Replace(sTag, "[^\\da-zA-Z_<>/\\.]+", "");
				sTagClean = Regex.Replace(sTagClean, @"(?<tag></?)\d+", "${tag}");
				sTagClean = Regex.Replace(sTagClean, @"(?<tag></?)\.+", "${tag}");
				if (sTagClean.Equals("<>"))
					sTagClean = "<x>";
				if (sTagClean.Equals("</>"))
					sTagClean = "</x>";
				// remove all forward slashes except for one right after the opening left bracket.
				if (sTagClean.LastIndexOf('/') > 1)
				{
					string cS = "";
					for (int ii = 0; ii < sTagClean.Length; ii++)
					{
						string ch = sTagClean.Substring(ii, 1);
						if (ii == 1 || !ch.Equals("/"))
							cS = cS + ch;
					}
					sTagClean = cS;
				}

				if (!sTag.Equals(sTagClean))
				{
					//pad sTagClean with blanks so it is the same length as sTag
					sTagClean = sTagClean.PadRight(sLen, ' ');
					int st = m.Index;
					dtFileContents = dtFileContents.Substring(0, st) + sTagClean + dtFileContents.Substring(st + sLen);
				}
			}

			StreamWriter srDTSearchw = new StreamWriter(filePath);
			srDTSearchw.Write(dtFileContents);
			srDTSearchw.Close();
			srDTSearchw.Dispose();
		}

		// processes unknown node type.  Appends the nodes innertext to index and analysis text.
		// passed - xNode - the  node to be processed,
		public void processMiscNode(XmlNode xNode, StringBuilder outStringIndex, StringBuilder outStringAnalysis)
		{
			string textLine = xNode.InnerText;
			outStringIndex.AppendLine(textLine);
			outStringAnalysis.AppendLine(textLine);
		}
	}
}
