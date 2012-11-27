namespace KRSrcWorkflow.Interfaces
{
	public interface IMessageQueue
	{
		void Dequeue(ref object t);
		void Enqueue(object t);
		int Count { get; }
		string Path { get; }
	}

	public interface IMessageQueue<T> : IMessageQueue
	{
		void Dequeue(ref T t);
		void Enqueue(T t);
	}
}
