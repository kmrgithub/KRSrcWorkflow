using System;

using SimpleTree;

namespace WFManager
{
	public class TrackingDataTreeNode : SimpleTreeNode<TrackingData>
	{
		public SimpleTreeNode<TrackingData> Find(Guid guid)
		{
			return base.Find(new TrackingData(guid));
		}

		public SimpleTreeNode<TrackingData> Find(string filename)
		{
			return base.Find(new TrackingData(Guid.Empty, filename));
		}
	}
}
