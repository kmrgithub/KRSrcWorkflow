using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel;
using System.Runtime.InteropServices;
using System.IO;

using KRSrcWorkflow.CustomAttributes;

namespace PSTFileDriver
{
	public partial class WFTestForm : Form
	{
		private CallbackClass CallbackClass { get; set; }
		KRSrcWorkflow.WFManagerProxy Proxy { get; set; }
		string IPAddress { get; set; }
		uint Port { get; set; }

		private WFTestForm() : this(string.Empty, 0)
		{
		}

		public WFTestForm(string ipaddress, uint port)
		{
			InitializeComponent();
			this.treeView1.AfterSelect += new TreeViewEventHandler(treeView1_AfterSelect);
			this.treeView1.DrawMode = TreeViewDrawMode.OwnerDrawText;
			this.treeView1.DrawNode += new DrawTreeNodeEventHandler(treeView1_DrawNode);
			this.IPAddress = ipaddress;
			this.Port = port;
			this.Proxy = null;

			if (this.IPAddress != string.Empty)
			{
				this.CallbackClass = new CallbackClass(this.treeView1);
				this.Proxy = new KRSrcWorkflow.WFManagerProxy(this.IPAddress, 8000, CallbackClass);
			}
		}

		private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
		{
			Color nodeColor = Color.Red;
			if (e.Node.Tag != null && ((CallbackClass.TrackingData)e.Node.Tag).Notified)
				nodeColor = Color.Green;

			TextRenderer.DrawText(e.Graphics,
														e.Node.Text,
														e.Node.NodeFont,
														e.Bounds,
														nodeColor,
														Color.Empty,
														TextFormatFlags.VerticalCenter);
		}

		void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
		{
			TreeNode tn = ((TreeView)sender).SelectedNode;
			if (tn != null && tn.Tag != null)
			{
				string filename = ((CallbackClass.TrackingData)tn.Tag).Filename;
				if (File.Exists(filename))
					this.webBrowser1.Navigate(@"file" + @":///" + filename.Replace("\\", "/")); //htmlfile);
				//					this.richTextBox1.LoadFile(filename, RichTextBoxStreamType.PlainText);
				else
					this.richTextBox1.Text = string.Empty;
			}
		}

		[System.Runtime.Serialization.DataContract]
		public class MyClass : KRSrcWorkflow.Abstracts.ProcessorData
		{
			[KRSrcWorkflowAttribute]
			public string Property1 { get; set; }

			[KRSrcWorkflowAttribute]
			public string Property2 { get; set; }
		}

		private void button1_Click(object sender, EventArgs e)
		{
			this.CallbackClass.ExtensionToImageIndex.Clear();
			this.treeView1.Nodes.Clear();
			if (this.treeView1.ImageList != null)
				this.treeView1.ImageList.Images.Clear();
			else
				this.treeView1.ImageList = new ImageList();
			System.Windows.Forms.PictureBox pb = new PictureBox();
			this.treeView1.ImageList.Images.Add(pb.ErrorImage);
			this.CallbackClass.ExtensionToImageIndex["Error"] = this.treeView1.ImageList.Images.Count - 1;

			//this.treeView1.ImageList.Images.Add(Image.FromFile(@"C:\Program Files\Microsoft Visual Studio 9.0\Common7\VS2008ImageLibrary\1033\Objects\ico_format\Office & Dev\Folder.ico", true));
			this.CallbackClass.ExtensionToImageIndex["Folder"] = this.treeView1.ImageList.Images.Count - 1;

			this.openFileDialog1.ShowDialog(this);
			this.textBox1.Text = this.openFileDialog1.FileName;
#if true
			string fname = PSTFileDriver.PathingX.GetUNCPath(this.textBox1.Text);
//			Guid guid = Guid.NewGuid();
			KRSrcWorkflow.Abstracts.ProcessorData procdata = new MyClass() { DocumentToProcess = fname, WFState = new KRSrcWorkflow.WFState("Process"), Property1 = "Hello World", Property2 = "Goodbye World" };
			Guid guid = this.Proxy.Execute(procdata);
//			Guid guid = this.Proxy.Process(fname, string.Empty);
//			WFFileType.WFFileType wffiletype = new WFFileType.WFFileType { FileToProcess = fname, ExportDirectory = System.IO.Path.GetDirectoryName(fname) };
//			KRSrcWorkflow.WFProcessingResult result = new KRSrcWorkflow.WFProcessingResult { State = new KRSrcWorkflow.WFState(("Process")) };
//			result.ProcessedObject = wffiletype;
//			result.OutputFiles().Add(fname, "Process");
//			result.Filename = wffiletype.FileToProcess;
//			KRSrcWorkflow.MessageQueueImplementations.WFMessageQueue_MessageQueue<KRSrcWorkflow.WFProcessingResult> queue = new KRSrcWorkflow.MessageQueueImplementations.WFMessageQueue_MessageQueue<KRSrcWorkflow.WFProcessingResult>("169.254.25.129", "masterqueue");
//			queue.Enqueue(result);

			TreeNode tn = this.treeView1.Nodes.Add(fname); //guid.ToString());
			tn.Name = guid.ToString();
			tn.Tag = new CallbackClass.TrackingData(guid, fname);
			string extension = System.IO.Path.GetExtension(fname);
			Icon icon = Icon.ExtractAssociatedIcon(fname);
			if (icon != null)
			{
				this.treeView1.ImageList.Images.Add(icon);
				this.CallbackClass.ExtensionToImageIndex.Add(extension, this.treeView1.ImageList.Images.Count - 1);
				tn.ImageIndex = this.treeView1.ImageList.Images.Count - 1;
				tn.SelectedImageIndex = this.treeView1.ImageList.Images.Count - 1;
			}
//			string extension = System.IO.Path.GetExtension(fname);
//			if (!string.IsNullOrEmpty(extension))
//			{
//				if (!string.IsNullOrEmpty(extension) && this.CallbackClass.ExtensionToImageIndex.ContainsKey(extension))
//					tn.ImageIndex = this.CallbackClass.ExtensionToImageIndex[extension];
//			}

			CallbackClass.TrackingDataTree newtree = new CallbackClass.TrackingDataTree();
			this.CallbackClass.TrackingTree.Add(newtree);
			newtree.Value = (CallbackClass.TrackingData)tn.Tag;
#else
			for (uint j = 0; j <= 8; j++)
			{
				string dir = string.Format(@"C:\Temp\KRTest{0}", j);
				string fname = PSTFileDriver.PathingX.GetUNCPath(string.Format(@"{0}\KRTest.xls", dir));
				Guid guid = this.Proxy.Process(fname, string.Empty);
				TreeNode tn = this.treeView1.Nodes.Add(fname); //guid.ToString());
				tn.Name = guid.ToString();
				tn.Tag = new CallbackClass.TrackingData(guid, fname);

				CallbackClass.TrackingDataTree newtree = new CallbackClass.TrackingDataTree();
				this.CallbackClass.TrackingTree.Add(newtree);
				newtree.Value = (CallbackClass.TrackingData)tn.Tag;
			}
#endif
		}

        private void archiveButton_Click(object sender, EventArgs e)
        {

        }
	}
	public static class PathingX
	{
		//
		//Dll import declarations
		//
		[DllImport("mpr.dll")]
		private static extern int WNetGetUniversalName(string lpLocalPath, int dwInfoLevel, ref UNIVERSAL_NAME_INFO lpBuffer, ref int lpBufferSize);

		[DllImport("mpr", CharSet = CharSet.Auto)]
		private static extern int WNetGetUniversalName(string lpLocalPath, int dwInfoLevel, IntPtr lpBuffer, ref int lpBufferSize);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct UNIVERSAL_NAME_INFO
		{
			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpUniversalName;
		}

		//
		//Constants
		//
		private const int NO_ERROR = 0;
		private const int ERROR_MORE_DATA = 234;
		private const int ERROR_NOT_CONNECTED = 2250;
		private const int UNIVERSAL_NAME_INFO_LEVEL = 1;

		//
		//GetUNCPath function
		//
		public static string GetUNCPath(string mappedDrive)
		{
			string uncpath = mappedDrive;
			UNIVERSAL_NAME_INFO rni = new UNIVERSAL_NAME_INFO();
			int bufferSize = Marshal.SizeOf(rni);

			int nRet = WNetGetUniversalName(mappedDrive, UNIVERSAL_NAME_INFO_LEVEL, ref rni, ref bufferSize);
			if (ERROR_MORE_DATA == nRet)
			{
				IntPtr pBuffer = default(IntPtr);
				try
				{
					pBuffer = Marshal.AllocHGlobal(bufferSize);
					nRet = WNetGetUniversalName(mappedDrive, UNIVERSAL_NAME_INFO_LEVEL, pBuffer, ref bufferSize);
					if (NO_ERROR == nRet)
					{
						rni = (UNIVERSAL_NAME_INFO)Marshal.PtrToStructure(pBuffer, typeof(UNIVERSAL_NAME_INFO));
						uncpath = rni.lpUniversalName;
					}
					else
						uncpath = mappedDrive;
				}
				finally
				{
					if (pBuffer != default(IntPtr))
						Marshal.FreeHGlobal(pBuffer);
				}
			}

			System.Uri uri = new Uri(uncpath);
			if (uri.IsUnc)
			{
				System.Net.IPAddress[] ipaddresses = System.Net.Dns.GetHostAddresses(uri.DnsSafeHost);
				if (ipaddresses != null && ipaddresses.Count() > 0)
					uri = new Uri(string.Format(@"\\{0}\{1}", ipaddresses[0], uri.AbsolutePath));
			}

			return uri.LocalPath;
		}
	}
}
