using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using KRSrcWorkflow.Abstracts;

namespace KRSrcWorkflow.Config
{
	public enum WFMappingTargetTypes { WFSrc, WFTarget};
	public class WFMappingTarget : IDisposable
	{
		public enum TargetTypes { Unknown, Value, Property };
		public TargetTypes TargetType { get; private set; }
		public MethodInfo Property { get; set; }
		public object Value { get; set; }

		public WFMappingTarget(string target, Type targettype) : this(target, targettype, WFMappingTargetTypes.WFTarget)
		{
		}

		public WFMappingTarget(string target, Type targettype, WFMappingTargetTypes mappingtargettype)
		{
			if (target == null || target == string.Empty || targettype == null)
				throw new Exception("Invalid arguments");

			string property = string.Empty;
			this.TargetType = TargetTypes.Unknown;
			this.Property = default(MethodInfo);
			this.Value = null;

			string[] splitdata = target.Split(new char[] { '@' });
			if (splitdata.Length == 1)
			{
				property = splitdata[0];
				this.TargetType = TargetTypes.Property;
			}
			else
			{
				property = splitdata[1];
				switch (splitdata[0].ToUpper())
				{
					case "VALUE":
						this.TargetType = TargetTypes.Value;
						break;
					case "PROPERTY":
						this.TargetType = TargetTypes.Property;
						break;
				}
			}

			if (this.TargetType == TargetTypes.Unknown)
				throw new Exception("Invalid arguments");

			if (this.TargetType == TargetTypes.Property)
			{
				try
				{
					if(mappingtargettype == WFMappingTargetTypes.WFTarget)
						this.Property = targettype.GetProperty(property).GetSetMethod();
					else
						this.Property = targettype.GetProperty(property).GetGetMethod();
				}
				catch (Exception ex)
				{
					throw new Exception("GetProperty or SetProperty failed arguments", ex);
				}
			}
			else
				this.Value = property;

		}

		public void Dispose()
		{
		}

		public override string ToString()
		{
			return string.Format("TargetType={0}  " + (this.TargetType == TargetTypes.Property ? "Property={1}" : "Value={1}"), this.TargetType.ToString(), (this.TargetType == TargetTypes.Property ? this.Property.Name : this.Value.ToString()));
		}

		public object GetValue(ProcessorData processor)
		{
			object value = null;
			if (this.TargetType == WFMappingTarget.TargetTypes.Property)
			{
				try
				{
					value = this.Property.Invoke(processor, new object[0]);
				}
				catch (Exception ex)
				{
					WFLogger.NLogger.ErrorException(String.Format("Invoke failed for property: {0} on type: {1}", this.Property.Name, processor.GetType().FullName), ex);
				}
			}
			else if (this.TargetType == WFMappingTarget.TargetTypes.Value)
				value = this.Value;

			return value;
		}

		public void SetProperty(ProcessorData processor, object value)
		{
			try
			{
				this.Property.Invoke(processor, new[] { value });
			}
			catch (Exception ex)
			{
				WFLogger.NLogger.ErrorException("Invoke failed", ex);
			}
		}
	}
}
