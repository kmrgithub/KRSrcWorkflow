using System;
using System.Linq;

using SimpleTree;
using KRSrcWorkflow;

namespace WFManager
{
	public class TrackingDataTree : SimpleTree<TrackingData>
	{
		private void _OuputTrackingTree(SimpleTreeNode<TrackingData> tree, int offset)
		{
			Console.WriteLine(string.Format("{0}{1}-{2}", ((new string(' ', offset)).ToString(System.Globalization.CultureInfo.CurrentCulture)), tree.Value.Guid, tree.Value.Filename));
			WFLogger.NLogger.Trace("{0}{1}-{2}", ((new string(' ', offset)).ToString(System.Globalization.CultureInfo.CurrentCulture)), tree.Value.Guid, tree.Value.Filename);
			offset += 2;
			foreach (var children in tree.Children)
				_OuputTrackingTree(children, offset);
		}

		public void OuputTrackingTree()
		{
			_OuputTrackingTree(this, 0);
		}

		public SimpleTreeNode<TrackingData> Find(Guid guid)
		{
			return base.Find(new TrackingData(guid));
		}

		public SimpleTreeNode<TrackingData> Find(string filename)
		{
			return base.Find(new TrackingData(Guid.Empty, filename));
		}

		public SimpleTreeNode<TrackingData> ProcessItem(Guid trackingid, string filename, Guid parenttrackingid, WFState state, bool isprocesingcompleted)
		{
			WFLogger.NLogger.Info("Guid={0}  Filename={1}  ParentGuid={2}  IsProcessingComplete={3}", trackingid, filename, parenttrackingid, isprocesingcompleted);
			SimpleTreeNode<TrackingData> parenttreenode = this.Find(parenttrackingid);
			SimpleTreeNode<TrackingData> srctreenode = null;

			// now go find node in tree by filename
			srctreenode = this.Find(new TrackingData(Guid.Empty, filename));

			// if node by filename not found then create new tree whose root is trackingid
			if (srctreenode == null)
			{
				srctreenode = parenttreenode.Children.Add(new TrackingData(trackingid, filename, parenttreenode.Value.WFClient));

				// notify client of new entry
				if (srctreenode.Value.WFClient != null)
				{
					srctreenode.Value.WFClient.Processing(srctreenode.Value.Guid, srctreenode.Value.Filename, (uint)srctreenode.Depth, srctreenode.Parent != null ? srctreenode.Parent.Value.Guid : srctreenode.Value.Guid);
					WFLogger.NLogger.Info("Processing: Guid={0}  Filename={1}  Depth={2}  ParentGuid={3}", srctreenode.Value.Guid, srctreenode.Value.Filename, (uint)srctreenode.Depth, srctreenode.Parent != null ? srctreenode.Parent.Value.Guid : srctreenode.Value.Guid);
				}
			}
			if (isprocesingcompleted && false)
			{
				// send notification processing is completed to client
				if (srctreenode.Value.WFClient != null)
				{
					srctreenode.Value.WFClient.Completed(srctreenode.Value.Guid, srctreenode.Value.Filename, (uint)srctreenode.Depth, srctreenode.Parent != null ? srctreenode.Parent.Value.Guid : srctreenode.Value.Guid);
					WFLogger.NLogger.Info("Completed: Guid={0}  Filename={1}  Depth={2}  ParentGuid={3}", srctreenode.Value.Guid, srctreenode.Value.Filename, (uint)srctreenode.Depth, srctreenode.Parent != null ? srctreenode.Parent.Value.Guid : srctreenode.Value.Guid);
				}

				// get and check if node has parent
				SimpleTreeNode<TrackingData> parentnode = srctreenode.Parent;
				if (parentnode != null)
				{
					// delete node from parent
					parentnode.Children.Remove(srctreenode);
					do
					{
						if (parentnode == parentnode.Root)
							break;

						// if parent node has no children then notify client of completion
						// and remove parentnode from parent
						if (parentnode.Children.Count == 0)
						{
							// notify clients of completion
							if (parentnode.Value.WFClient != null)
							{
								parentnode.Value.WFClient.CompletedEx(parentnode.Value.Guid, state);
								WFLogger.NLogger.Info("CompletedEx: Guid={0}  State={1}", parentnode.Value.Guid, state);
								//								parentnode.Value.WFClient.Completed(parentnode.Value.Guid, parentnode.Value.Filename, (uint)parentnode.Depth, parentnode.Parent != null ? parentnode.Parent.Value.Guid : parentnode.Value.Guid);
								//								WFLogger.NLogger.Info("Completed: Guid={0}  Filename={1}  Depth={2}  ParentGuid={3}", parentnode.Value.Guid, parentnode.Value.Filename, (uint)parentnode.Depth, parentnode.Parent != null ? parentnode.Parent.Value.Guid : parentnode.Value.Guid);
							}
							// remove parentnode from parent
							if (parentnode.Parent != null)
								parentnode.Parent.Children.Remove(parentnode);
						}
						parentnode = parentnode.Parent;
					} while (parentnode != null);

					if ((parentnode != null) && (parentnode == parentnode.Root) && (parentnode.Children.Count == 0))
					{
						// notify clients of completion
						if (parentnode.Value.WFClient != null)
						{
							parentnode.Value.WFClient.Completed(parentnode.Value.Guid, parentnode.Value.Filename, (uint)parentnode.Depth, parentnode.Value.Guid);
							WFLogger.NLogger.Info("Completed: Guid={0}  Filename={1}  Depth={2}  ParentGuid={3}", parentnode.Value.Guid, parentnode.Value.Filename, (uint)parentnode.Depth, parentnode.Value.Guid);
						}
					}
				}
			}

			return srctreenode;
		}
	}
}
