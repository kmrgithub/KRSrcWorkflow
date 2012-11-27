namespace KRSrcWorkflow.Interfaces
{
	public interface IWFObjectProcessor
	{
		void Process();
	}

	public interface IWFObjectProcessor<T>
	{
		T Process();
	}
}
