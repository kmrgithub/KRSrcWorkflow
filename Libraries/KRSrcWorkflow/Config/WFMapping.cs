using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using KRSrcWorkflow.Abstracts;

namespace KRSrcWorkflow.Config
{
	class GenericCastClass
	{
		public static object Cast(string o, Type t)
		{
#if true
			TypeCode tc = Type.GetTypeCode(t);
			switch (tc)
			{
				case TypeCode.Boolean:
					return o.ToLower() == "true" || o.ToLower() == "false" ? (object)Convert.ToBoolean(o) : null;
				case TypeCode.Byte:
					return Convert.ToByte(o);
				case TypeCode.Char:
					return Convert.ToChar(o);
				case TypeCode.DateTime:
					return Convert.ToDateTime(o);
				case TypeCode.Decimal:
					return Convert.ToDecimal(o);
				case TypeCode.Double:
					return Convert.ToDouble(o);
				case TypeCode.Int16:
					return Convert.ToInt16(o);
				case TypeCode.Int32:
					return Convert.ToInt32(o);
				case TypeCode.Int64:
					return Convert.ToInt64(o);
				case TypeCode.SByte:
					return Convert.ToSByte(o);
				case TypeCode.Single:
					return Convert.ToSingle(o);
				case TypeCode.String:
					return Convert.ToString(o);
				case TypeCode.UInt16:
					return Convert.ToUInt16(o);
				case TypeCode.UInt32:
					return Convert.ToUInt32(o);
				case TypeCode.UInt64:
					return Convert.ToUInt64(o);
			}
#else
				if (t == typeof(Boolean))
					return o.ToLower() == "true" || o.ToLower() == "false" ? (object)Convert.ToBoolean(o) : null;
				else if (t == typeof(Int16))
					return Convert.ToInt16(o);
				else if (t == typeof(Int32))
					return Convert.ToInt32(o);
				else if (t == typeof(Int64))
					return Convert.ToInt64(o);
				else if (t == typeof(UInt16))
					return Convert.ToUInt16(o);
				else if (t == typeof(UInt32))
					return Convert.ToUInt32(o);
				else if (t == typeof(UInt64))
					return Convert.ToUInt64(o);
				else if (t == typeof(DateTime))
					return Convert.ToDateTime(o);
				else if (t == typeof(Decimal))
					return Convert.ToDecimal(o);
				else if (t == typeof(Double))
					return Convert.ToDouble(o);
				else if (t == typeof(String))
					return Convert.ToString(o);
				else
				{
					TypeCode tc = Type.GetTypeCode(t);
				}
#endif
			return o;
		}


		public static T Cast<T>(object o)
		{
			return (T)o;
		}
	}

	// <mapping src="[property||value@]SrcPropertyOrValue" target="TargetProperty"/>
	public class WFMapping
	{
		public WFMappingSrc Src { get; set; }
		public WFMappingTarget Target { get; set; }

		public WFMapping(string src, Type srctype, string target, Type targettype)
		{
			try
			{
				this.Src = new WFMappingSrc(src, srctype);
				this.Target = new WFMappingTarget(target, targettype);
			}
			catch (Exception ex)
			{
				throw new Exception("Invalid arguments", ex);
			}
		}

		public WFMapping(XmlNode node, Type srctype, Type targettype)
		{
			XmlNode src = node.Attributes.GetNamedItem("src");
			if ((src == null) || (src.Value == string.Empty))
				throw new Exception("No src attribute in node");

			XmlNode target = node.Attributes.GetNamedItem("target");
			if ((target == null) || (target.Value == string.Empty))
				throw new Exception("No target attribute in node");

			try
			{
				this.Src = new WFMappingSrc(src.Value, srctype);
			}
			catch (Exception ex)
			{
				throw new Exception("Create WFMappingSrc failed", ex);
			}

			try
			{
				this.Target = new WFMappingTarget(target.Value, targettype);
			}
			catch (Exception ex)
			{
				throw new Exception("Create WFMappingTarget failed", ex);
			}
		}

		public override string ToString()
		{
			return string.Format("Src={0}  Target={1}", this.Src.ToString(), this.Target.ToString());
		}

		public object GetSrcValue(ProcessorData processor)
		{
			return this.Src.GetValue(processor);
		}

		public void SetTargetValue(ProcessorData processor, object value)
		{
			if ((this.Src.TargetType == WFMappingTarget.TargetTypes.Value) ||
					((this.Src.TargetType == WFMappingTarget.TargetTypes.Property) &&
					 (this.Src.Property.ReturnType == this.Target.Property.GetParameters()[0].ParameterType)))
			{
				try
				{
					if (this.Src.TargetType == WFMappingTarget.TargetTypes.Value)
						value = GenericCastClass.Cast((string)value, this.Target.Property.GetParameters()[0].ParameterType);
					this.Target.SetProperty(processor, value);
				}
				catch (Exception ex)
				{
					WFLogger.NLogger.ErrorException("Cast failed", ex);
				}
			}
		}
	}
}
