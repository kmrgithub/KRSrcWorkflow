using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KRSrcWorkflow.Interfaces;

using Newtonsoft.Json;

namespace KRSrcWorkflow.SerializationImplementations
{
	class JSONSerialization<T> : ISerialize<T>
	{
		public string Serialize(T t)
		{
			return JsonConvert.SerializeObject(t);
		}

		public T Deserialize(string json)
		{
			return JsonConvert.DeserializeObject<T>(json);
		}
	}
}
