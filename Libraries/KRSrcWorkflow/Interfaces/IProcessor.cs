namespace KRSrcWorkflow.Interfaces
{
	public interface IProcessor<T, TU>
	{
		TU ProcessEx(T t);
	}
}
