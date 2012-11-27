using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KRSrcWorkflow.Interfaces
{
	public interface ISerialize<T>
	{
		T Deserialize(string data);
		string Serialize(T t);
	}
}
