using System;
using System.Runtime.InteropServices;
using System.Text;

namespace KRSrcWorkflow
{
	/// <summary>
	/// It's just like a System.Collections.Generic.KeyValuePair,
	/// but the XmlSerializer will serialize the
	/// Key and Value properties!
	/// </summary>
	[Serializable, StructLayout(LayoutKind.Sequential)]
	public struct WFKeyValuePair<TKey, TValue> where TKey : class where TValue : class
	{
		private TKey key;
		private TValue value;
		public WFKeyValuePair(TKey key,
		TValue value)
		{
			this.key = key;
			this.value = value;
		}
		public override string ToString()
		{
			StringBuilder builder1 = new StringBuilder();
			builder1.Append('[');
			if (this.Key != null)
			{
				builder1.Append(this.Key.ToString());
			}
			builder1.Append(", ");
			if (this.Value != null)
			{
				builder1.Append(this.Value.ToString());
			}
			builder1.Append(']');
			return builder1.ToString();
		}
		/// <summary>
		/// Gets the Value in the Key/Value Pair
		/// </summary>
		public TValue Value
		{
			get
			{
				return this.value;
			}
			set
			{
				this.value = value;
			}
		}
		/// <summary>
		/// Gets the Key in the Key/Value pair
		/// </summary>
		public TKey Key
		{
			get
			{
				return this.key;
			}
			set
			{
				this.key = value;
			}
		}
	}
}