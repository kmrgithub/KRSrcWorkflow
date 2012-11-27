using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel;

using KRSrcWorkflow;
using KRSrcWorkflow.Abstracts;

namespace ExeDllParser
{
	public class ExeDllParser : Processor<ExeDllParserData>
	{
		public class StringLoader : IDisposable
		{
			[DllImport("kernel32", CharSet = CharSet.Auto)]
			static extern IntPtr LoadLibrary(string filename);
			[DllImport("kernel32", CharSet = CharSet.Auto)]
			static extern bool FreeLibrary(IntPtr lib);
			[DllImport("user32", CharSet = CharSet.Auto)]
			static extern int LoadString(IntPtr hInstance, int uID, out IntPtr buffer, int BufferMax);

			IntPtr libPtr;

			public StringLoader(string filename)
			{
				libPtr = LoadLibrary(filename);
				if (libPtr == IntPtr.Zero)
					throw new Win32Exception();
			}
			public string Load(int id)
			{
				IntPtr resource;
				int length = LoadString(libPtr, id, out resource, 0);
				if (length == 0)
					return null;
				else
					return Marshal.PtrToStringAuto(resource, length);
			}
			public void Dispose()
			{
				FreeLibrary(libPtr);
				GC.SuppressFinalize(this);
			}
			~StringLoader()
			{
				Dispose();
			}
		}

		public override void Process(ExeDllParserData data)
		{
			WFState retval = new WFState();
			try
			{
				retval.Value = WFState.WFStateFail;

				StringLoader sl = new StringLoader(data.DocumentToProcess);
//				string output = sl.Load(strId);
				sl.Dispose(); 
//				Assembly assembly = Assembly.LoadFile(this.FileToProcess);
//				string[] resources = assembly.GetManifestResourceNames();
			}
			catch (Exception)
			{
			}
		}
	}
}
