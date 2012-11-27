using System;
using System.ServiceModel;

using KRSrcWorkflow.Abstracts;

namespace KRSrcWorkflow.Interfaces.Wcf
{
	[ServiceContract(Name = "IWFManagerProcessing", CallbackContract = typeof(IWFClientProcessing))]
	public interface IWFManagerProcessing
	{
		[OperationContract]
		bool Subscribe(Guid guid);

		[OperationContract]
		Guid Process(string filename, string exportdir);

		[OperationContract]
		Guid Execute(ProcessorData procdata);
	}

	[ServiceContract(Name = "IWFManagerAdmin", CallbackContract = typeof(IWFClientAdmin))]
	public interface IWFManagerAdmin
	{
		[OperationContract]
		void Discovery();
	}

	[ServiceContract(Name = "IWFManager", CallbackContract = typeof(IWFClient))]
	public interface IWFManager : IWFManagerProcessing, IWFManagerAdmin
	{
	}
}
