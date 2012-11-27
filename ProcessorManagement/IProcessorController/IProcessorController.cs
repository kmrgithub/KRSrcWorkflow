using System.Collections.Generic;
using System.ServiceModel;

using ProcessorManagement.Data;

namespace ProcessorManagement.Interfaces
{
	[ServiceContract(Name = "IProcessorController")]
	public interface IProcessorController
	{
		[OperationContract]
		bool StartProcessor();

		[OperationContract]
		bool StopProcessor(string pid);

		[OperationContract]
		ProcessorDataList ListProcessors();
	}
}
