using System;
using System.ServiceModel;

namespace KRSrcWorkflow.Interfaces.Wcf
{
	[ServiceContract(Name = "IWFClientProcessing")]
	public interface IWFClientProcessing
	{
		[OperationContract(IsOneWay = true)]
		void Processing(Guid guid, string filename, uint depth, Guid parentguid);

		[OperationContract(IsOneWay = true)]
		void Completed(Guid guid, string filename, uint depth, Guid parentguid);

		[OperationContract(IsOneWay = true)]
		void CompletedEx(Guid guid, WFState state);
	}

	[ServiceContract(Name = "IWFClientAdmin")]
	public interface IWFClientAdmin
	{
		[OperationContract(IsOneWay = true)]
		void Interrupt();
	}

	[ServiceContract(Name = "IWFClient")]
	public interface IWFClient : IWFClientAdmin, IWFClientProcessing
	{
	}
}
