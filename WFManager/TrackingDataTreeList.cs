using System;
using System.Collections.Generic;
using System.Linq;

using KRSrcWorkflow;
using SimpleTree;

namespace WFManager
{
	public class TrackingDataTreeList : List<TrackingDataTree>
	{
		public void OuputTrackingTreeList()
		{
			foreach (TrackingDataTree tdt in this)
				tdt.OuputTrackingTree();
		}

		public TrackingDataTree Find(Guid guid)
		{
			return this.FirstOrDefault(x => x.Find(guid) != null);
		}

		public SimpleTreeNode<TrackingData> Find(Guid guid, string filename)
		{
			TrackingDataTree tdt = this.Find(guid);
			return tdt != null ? tdt.Find(filename) : null;
		}

		public SimpleTreeNode<TrackingData> ProcessItem(Guid trackingid, string filename, Guid parenttrackingid, bool isprocesingcompleted)
		{
			return ProcessItem(trackingid, filename, parenttrackingid, new WFState(string.Empty), isprocesingcompleted);
		}

		public SimpleTreeNode<TrackingData> ProcessItem(Guid trackingid, string filename, Guid parenttrackingid, WFState state, bool isprocesingcompleted)
		{
			WFLogger.NLogger.Info("Guid={0}  Filename={1}  ParentGuid={2}  IsProcessingComplete={3}", trackingid, filename, parenttrackingid, isprocesingcompleted);
			SimpleTreeNode<TrackingData> parenttreenode = null;
			SimpleTreeNode<TrackingData> parenttree = this.FirstOrDefault(x => (parenttreenode = x.Find(parenttrackingid)) != null);
			SimpleTreeNode<TrackingData> srctreenode = null;

			// srctree == null then create tree for filename
			if (parenttree == null)
			{
				TrackingDataTree newtree = new TrackingDataTree();
				newtree.Value = new TrackingData(trackingid, filename);
				this.Add(newtree);
				srctreenode = newtree;
				WFLogger.NLogger.Info("CreateTree: Guid={0}  Filename={1}  Depth={2}  ParentGuid={3}", srctreenode.Value.Guid, srctreenode.Value.Filename, (uint)srctreenode.Depth, srctreenode.Parent != null ? srctreenode.Parent.Value.Guid : srctreenode.Value.Guid);
			}
			else // srctree is found so is srctreenode
			{
				// now go find node in tree by filename
				srctreenode = parenttree.Find(new TrackingData(trackingid, string.Empty));
//				if(srctreenode == null)
//					srctreenode = parenttreenode.Find(new TrackingData(Guid.Empty, filename));

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
			}

			if (isprocesingcompleted && srctreenode.Children.Count == 0)
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
					WFLogger.NLogger.Info("RemoveNode: Guid={0}  Filename={1}  Depth={2}  ParentGuid={3}", srctreenode.Value.Guid, srctreenode.Value.Filename, (uint)srctreenode.Depth, srctreenode.Parent != null ? srctreenode.Parent.Value.Guid : srctreenode.Value.Guid);
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
								parentnode.Value.WFClient.Completed(parentnode.Value.Guid, parentnode.Value.Filename, (uint)parentnode.Depth, parentnode.Parent != null ? parentnode.Parent.Value.Guid : parentnode.Value.Guid);
								WFLogger.NLogger.Info("Completed: Guid={0}  Filename={1}  Depth={2}  ParentGuid={3}", parentnode.Value.Guid, parentnode.Value.Filename, (uint)parentnode.Depth, parentnode.Parent != null ? parentnode.Parent.Value.Guid : parentnode.Value.Guid);
							}
							// remove parentnode from parent
							if (parentnode.Parent != null)
							{
								WFLogger.NLogger.Info("RemoveNode: Guid={0}  Filename={1}  Depth={2}  ParentGuid={3}", parentnode.Value.Guid, parentnode.Value.Filename, (uint)parentnode.Depth, parentnode.Parent != null ? parentnode.Parent.Value.Guid : parentnode.Value.Guid);
								parentnode.Parent.Children.Remove(parentnode);
							}
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

						// remove from tracking tree
						WFLogger.NLogger.Info("RemoveTree: Guid={0}  Filename={1}  Depth={2}  ParentGuid={3}", parentnode.Value.Guid, parentnode.Value.Filename, (uint)parentnode.Depth, parentnode.Parent != null ? parentnode.Parent.Value.Guid : parentnode.Value.Guid);
						this.Remove((TrackingDataTree)parentnode);
					}
				}
			}

			return srctreenode;
		}

		public SimpleTreeNode<TrackingData> ProcessItemOld(Guid trackingid, string filename, bool isprocesingcompleted)
		{
			SimpleTreeNode<TrackingData> srctreenode = null;
			SimpleTreeNode<TrackingData> srctree =
				this.FirstOrDefault(
					x => (srctreenode = x.Find(new TrackingData(trackingid, string.Empty))) != null);

			// determine if we need to add srctrackingid to TrackingTree
			if (srctreenode == null)
			{
				if (!isprocesingcompleted)
				{
					TrackingDataTree newtree = new TrackingDataTree();
					newtree.Value = new TrackingData(trackingid, filename);
					this.Add(newtree);
					return newtree;
				}
			}
			else if (isprocesingcompleted)
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
								parentnode.Value.WFClient.Completed(parentnode.Value.Guid, parentnode.Value.Filename, (uint)parentnode.Depth, parentnode.Parent != null ? parentnode.Parent.Value.Guid : parentnode.Value.Guid);
								WFLogger.NLogger.Info("Completed: Guid={0}  Filename={1}  Depth={2}  ParentGuid={3}", parentnode.Value.Guid, parentnode.Value.Filename, (uint)parentnode.Depth, parentnode.Parent != null ? parentnode.Parent.Value.Guid : parentnode.Value.Guid);
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

						// remove from tracking tree
						this.Remove((TrackingDataTree)parentnode);
					}
				}
			}

			return srctreenode;
		}
	}
}
