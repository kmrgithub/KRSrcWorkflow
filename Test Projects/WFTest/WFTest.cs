using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Messaging;
using System.ServiceModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Drawing;

using KRSrcWorkflow;
using SimpleTree;

namespace PSTFileDriver
{
	class CallbackClass : KRSrcWorkflow.Interfaces.Wcf.IWFClientProcessing
	{
		public Dictionary<string, int> ExtensionToImageIndex = new Dictionary<string,int>();

		public class TrackingDataTree : SimpleTree<TrackingData>
		{
			public SimpleTreeNode<TrackingData> Find(Guid guid, string filename)
			{
				return base.Find(new TrackingData(guid, filename));
			}

			public SimpleTreeNode<TrackingData> Find(string filename)
			{
				return base.Find(new TrackingData(Guid.Empty, filename));
			}

			public SimpleTreeNode<TrackingData> Find(Guid guid)
			{
				return base.Find(new TrackingData(guid, string.Empty));
			}
		}

		public class TrackingTreeList : List<TrackingDataTree>
		{
		}

		public class TrackingData
		{
			public bool Notified { get; set; }
			public Guid Guid { get; set; }
			public string Filename { get; set; }

			public TrackingData(Guid guid) :
				this(guid, string.Empty, false)
			{
			}

			public TrackingData(Guid guid, string filename) :
				this(guid, filename, false)
			{
			}

			public TrackingData(Guid guid, string filename, bool notified)
			{
				this.Guid = guid;
				this.Filename = filename;
				this.Notified = notified;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			public override bool Equals(Object obj)
			{
				TrackingData t = (TrackingData)obj;
				return t.Filename == this.Filename || t.Guid == this.Guid;
			}
		}

		public CallbackClass() : this(null)
		{
		}

		public CallbackClass(System.Windows.Forms.TreeView treeview)
		{
			this.TreeView = treeview;
			this.TrackingTree = new TrackingTreeList();

//			System.Collections.Hashtable FileTypeToIcon = RegisteredFileType.GetFileTypeAndIcon();
//			foreach (string extension in FileTypeToIcon.Keys)
//			{
//				string fileAndParam = (string)FileTypeToIcon[extension];
//				if (!string.IsNullOrEmpty(fileAndParam))
//				{
//					Icon icon = RegisteredFileType.ExtractIconFromFile(fileAndParam, false);
//					if (icon != null)
//					{
//						if (this.TreeView.ImageList == null)
//							this.TreeView.ImageList = new ImageList();
//						this.TreeView.ImageList.Images.Add(icon);
//						ExtensionToImageIndex.Add(extension, this.TreeView.ImageList.Images.Count - 1);
//					}
//				}
//			}
		}

		private System.Windows.Forms.TreeView TreeView { get; set; }
		private static Object _lockobj = new object();
		public TrackingTreeList TrackingTree { get; set; }

		private void OutputTrackingTree(SimpleTreeNode<TrackingData> tree, int offset)
		{
			Console.WriteLine((new string(' ', offset)).ToString(CultureInfo.CurrentCulture) + (tree.Value.Notified == true ? "+" : "-") + tree.Value.Guid);

			if (tree.Parent == null && tree.Value.Notified == true)
				return;

			offset++;
			foreach (SimpleTreeNode<TrackingData> children in tree.Children)
				OutputTrackingTree(children, offset);
		}

		public void Processing(Guid guid, string filename, uint depth, Guid parentguid)
		{
			WFLogger.NLogger.Info("Processing: Guid={0}  Filename={1}  Depth={2}  ParentGuid={3}", guid, filename, depth, parentguid);

			SimpleTreeNode<TrackingData> srctreenode = null;
			SimpleTreeNode<TrackingData> srctree =
				this.TrackingTree.FirstOrDefault(b => (srctreenode = b.Find(parentguid)) != null);

			// could not find parent (parentguid) in tracking tree
			// so need to create new tree
			if (srctreenode == null)
			{
					srctreenode = new TrackingDataTree();
					srctreenode.Value = new TrackingData(guid, filename);
					this.TrackingTree.Add((TrackingDataTree)srctreenode);
			}
			else
				srctreenode = srctreenode.Children.Add(new TrackingData(guid, filename));

			if (this.TreeView != null)
			{
				string extension = string.Empty;
				if (System.IO.Directory.Exists(filename))
					extension = "Folder";
				if (string.IsNullOrEmpty(extension))
				{
					extension = System.IO.Path.GetExtension(filename);
					if (string.IsNullOrEmpty(extension))
						extension = ".txt";
				}
				if (!string.IsNullOrEmpty(extension) && !this.ExtensionToImageIndex.ContainsKey(extension))
				{
					try
					{
						Icon icon = Icon.ExtractAssociatedIcon(filename);
						if (icon != null)
						{
							this.TreeView.ImageList.Images.Add(icon);
							ExtensionToImageIndex.Add(extension, this.TreeView.ImageList.Images.Count - 1);
						}
					}
					catch (Exception ex)
					{
                        WFLogger.NLogger.ErrorException("ERROR: GetProperty failed!", ex);
                    }
				}
				//						ExtensionToImageIndex.Add(extension, this.TreeView.ImageList.Images.Count - 1);

				TreeNode[] nodes = this.TreeView.Nodes.Find(guid.ToString(), true);
				if (!nodes.Any())
				{
					nodes = this.TreeView.Nodes.Find(parentguid.ToString(), true);
					if (!nodes.Any())
					{
						this.TreeView.Invoke((MethodInvoker)delegate()
																									{
																										TreeNode tn = this.TreeView.Nodes.Add(filename); //guid.ToString());
																										tn.Name = guid.ToString();
																										tn.Tag = srctreenode.Value;
																										tn.ImageIndex = 0;
																										if (!string.IsNullOrEmpty(extension) && ExtensionToImageIndex.ContainsKey(extension))
																										{
																											tn.ImageIndex = ExtensionToImageIndex[extension];
																											tn.SelectedImageIndex = ExtensionToImageIndex[extension];
																										}
																									});
					}
					else
						this.TreeView.Invoke((MethodInvoker)delegate()
																									{
																										TreeNode tn = nodes[0].Nodes.Add(filename); //guid.ToString());
																										tn.Name = guid.ToString();
																										tn.Tag = srctreenode.Value;
																										tn.ImageIndex = 0;
																										if (!string.IsNullOrEmpty(extension) && ExtensionToImageIndex.ContainsKey(extension))
																										{
																											tn.ImageIndex = ExtensionToImageIndex[extension];
																											tn.SelectedImageIndex = ExtensionToImageIndex[extension];
																										}
//																										this.TreeView.ExpandAll();
																									});
				}
			}
		}

        public void CompletedEx(Guid guid, WFState state)
        {
        }

        public void Completed(Guid guid, string filename, uint depth, Guid parentguid)
		{
			WFLogger.NLogger.Info("Completed: Guid={0}  Filename={1}  Depth={2}  ParentGuid={3}", guid, filename, depth, parentguid);

			SimpleTreeNode<TrackingData> srctreenode = null;
			SimpleTreeNode<TrackingData> srctree =
				this.TrackingTree.FirstOrDefault(b => (srctreenode = b.Find(guid)) != null);

			// could not find (guid,filename) in tracking tree
			// try parentguid
			if (srctreenode == null)
			{
				srctree = this.TrackingTree.FirstOrDefault(b => (srctreenode = b.Find(parentguid)) != null);
				srctreenode = srctreenode.Children.Add(new TrackingData(guid, filename));
			}
			if (srctreenode != null)
			{
				srctreenode.Value.Notified = true;
				TreeNode [] tn = this.TreeView.Nodes.Find(guid.ToString(), true);
				if (tn != null && tn.Count() > 0)
					tn[0].Text = filename; // guid.ToString();
			}
		}
	}

	class WFTest
	{
		public static KRSrcWorkflow.Interfaces.Wcf.IWFManagerProcessing Proxy = null; // { get; set; }

//		public Program()
//		{
//			EndpointAddress ep = new EndpointAddress(new Uri("http://localhost:8000/WFManagerWCF/WFManagerWCF"), EndpointIdentity.CreateDnsIdentity("localhost"));
//			PSTFileDriver.Program.Proxy = DuplexChannelFactory<KRSrcWorkflow.Interfaces.Wcf.IWFManagerProcessing>.CreateChannel(new InstanceContext(this), new WSDualHttpBinding(), ep);
//		}

		[STAThread]
		static void Main(string[] args)
		{
			string host = string.Empty;
			string queuename = string.Empty;

			WFLogger.NLogger.Info("Running: {0}", Environment.CommandLine);

			for (int i = 0; i < args.Length; i++)
			{
				switch (args[i].ToUpper())
				{
					case "-HOST":
						host = args[i + 1];
						break;

					case "-Q":
						queuename = args[i + 1];
						break;
				}
			}

			string ipaddress = string.Empty;
			if (host == string.Empty)
				host = System.Net.Dns.GetHostName();
			if (KRSrcWorkflow.WFUtilities.SetHostAndIPAddress(host, ref ipaddress) == false)
				return;

			// delete all processing directories
			for (uint j = 0; j <= 8; j++)
			{
				string dir = string.Format(@"C:\Temp\KRTest{0}", j);
				uint nextdirnum = KRSrcWorkflow.WFUtilities.GetNextDirectoryNumber(dir);
				for (uint i = 0; i < nextdirnum; i++)
				{
					if (System.IO.Directory.Exists(string.Format(@"{0}\{1}", dir, i)))
						System.IO.Directory.Delete(string.Format(@"{0}\{1}", dir, i), true);
				}
			}

			// purge all of the queues
			foreach (string queuenm in new string[] { "ftqueue", "masterqueue", "msgqueue", "ocrqueue", "pdfqueue", "pstqueue", "ssqueue", "tequeue", "zipqueue", "msgattachqueue" })
			{
				try
				{
					(new MessageQueue(String.Format("FormatName:Direct=TCP:{0}\\Private$\\{1}", ipaddress, queuenm))).Purge();
				}
				catch (Exception)
				{
				}
			}

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			WFTestForm pstfiledriverfrm = new WFTestForm(ipaddress, 8000);
			Application.Run(pstfiledriverfrm);
		}
	}
}
