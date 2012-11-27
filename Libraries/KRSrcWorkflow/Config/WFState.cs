using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace KRSrcWorkflow.Config
{
	// <wfstate value="Success" target="assembly@PstMsgParser.dll:type@PstMsgParser:queue@msgqueue">
	public class WFState
	{
		public KRSrcWorkflow.WFState State { get; set; }
		public WFTarget Target { get; set; }
		public List<WFMapping> Mappings { get; set; }

		public WFState(string val)
		{
			this.State = new KRSrcWorkflow.WFState(val);
			this.Target = null;
			this.Mappings = new List<WFMapping>();
		}

		public WFState(XmlNode node, WFSrc wfsrc) : this(node, wfsrc, null)
		{
		}

		public WFState(XmlNode node, WFSrc wfsrc, WFTarget wftarget)
			: this(node, wfsrc, wftarget, string.Empty)
		{
		}

		public WFState(XmlNode node, WFSrc wfsrc, WFTarget wftarget, string assemblycache)
		{
			this.State = new KRSrcWorkflow.WFState(string.Empty);
			this.Target = null;
			this.Mappings = new List<WFMapping>();

			if (node == null)
				throw new Exception("Input node is null");

			XmlNode value = node.Attributes.GetNamedItem("value");
			if((value == null) || (value.Value == string.Empty))
				throw new Exception("No value attribute in node");
			this.State.Value = value.Value;

			XmlNode target = node.Attributes.GetNamedItem("target");
			if ((wftarget == null) && ((value == null) || (value.Value == string.Empty)))
				throw new Exception("No target attribute in node");

			if (target != null)
			{
				try
				{
					wftarget = new WFTarget(target.Value, assemblycache);
				}
				catch (Exception ex)
				{
					throw new Exception("Create WFTarget failed.", ex);
				}
				this.Target = wftarget;
			}
			foreach (XmlNode mappingnode in node.SelectNodes("mapping"))
				this.Mappings.Add(new WFMapping(mappingnode, wfsrc.AssemblyType, wftarget.AssemblyType));
		}

		public override string ToString()
		{
			string retval = string.Format("State={0}  Target={1}", this.State, this.Target);
			foreach (WFMapping mapping in this.Mappings)
				retval += string.Format("\n{0}", mapping.ToString());
			return retval;
		}
	}
}
