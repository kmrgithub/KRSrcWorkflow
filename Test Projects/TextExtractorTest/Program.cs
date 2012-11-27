using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections;
using System.Text.RegularExpressions;

//using TextExtractor;
//using dtSearch;

namespace TextExtractorTest
{
	class Program
	{
		private class ErrorDataObject
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

		static void Main(string[] args)
		{
			string indexText = string.Empty;
			string analysisText = string.Empty;
			bool errorFlag = false;
			List<docuity.releaseToAnalytics.bll.ErrorDataObject> errObjs = new List<docuity.releaseToAnalytics.bll.ErrorDataObject>(); 
			docuity.releaseToAnalytics.bll.ExtractTextAndMetadata etm = new docuity.releaseToAnalytics.bll.ExtractTextAndMetadata();
			etm.setParams("10", @"C:\temp\PM5752\PHYS000000007.msg", "Hello World", ref indexText, ref analysisText, errObjs);
			etm.runExtractText();
			etm.getReturnValues(ref indexText, ref analysisText, errObjs, out errorFlag);
		}
	}
}
