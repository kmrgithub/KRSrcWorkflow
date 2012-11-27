using System.Collections.Generic;
using System.ServiceModel;

using ConfigurationServer.Data;

namespace ConfigurationServer.Interfaces
{
	[ServiceContract(Name = "IConfigurationServer")]
	public interface IConfigurationServer
	{
		[OperationContract]
		LocationDataDictionary GetLocations();
	}
}
