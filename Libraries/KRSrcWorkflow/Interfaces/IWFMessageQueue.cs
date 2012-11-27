using System;

namespace KRSrcWorkflow.Interfaces
{
	public interface IWFMessageQueue
	{
		void Dequeue(ref object t);
		void Enqueue(object t);
		int Count { get; }
		string Path { get; }
	}

	public interface IWFMessageQueue<T> : IWFMessageQueue
	{
		void Dequeue(ref T t);
		void Enqueue(T t);
	}
}
