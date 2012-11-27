using System;
using System.Collections.Generic;

using KRSrcWorkflow.Interfaces;

namespace KRSrcWorkflow.MessageQueueImplementations
{
	public class WFMessageQueue_Queue<T> : KRSrcWorkflow.Abstracts.WFMessageQueue<T> where T : KRSrcWorkflow.Abstracts.ProcessorData // Queue<T>, IWFMessageQueue<T>, IWFMessageQueue
	{
#if true
		private Queue<byte[]> _queue = new Queue<byte[]>();
		public override void Dequeue(ref byte[] t)
		{
			t = _queue.Dequeue();
		}

		public override void Enqueue(byte[] t)
		{
			_queue.Enqueue(t);
		}

		public override int Count
		{
			get
			{
				return _queue.Count;
			}
		}

		public override string Path
		{
			get
			{
				return string.Empty;
			}
		}
#else
		private static System.Collections.Generic.Dictionary<string, Object> _lockobjdictionary = new System.Collections.Generic.Dictionary<string, object>();
		private static string _path = string.Empty;
//		private static readonly Object _lockobj = new object();

		public WFMessageQueue_Queue()
			: base()
		{
			if(this.Path == string.Empty)
				this.Path = this.GetType().FullName;

			if (!_lockobjdictionary.ContainsKey(this.Path))
				_lockobjdictionary.Add(this.Path, new object());
		}

		public void Dequeue(ref T t)
		{
			t = default(T);
			if (base.Count > 0)
			{
				System.Threading.Monitor.Enter(_lockobjdictionary[this.Path]);
				try
				{
					t = base.Dequeue();
				}
				catch (Exception ex)
				{
					WFLogger.NLogger.ErrorException(string.Empty, ex);
					t = default(T);
				}
				finally
				{
					System.Threading.Monitor.Exit(_lockobjdictionary[this.Path]);
				}
			}
		}

		public new T Dequeue()
		{
			T t = default(T);
			this.Dequeue(ref t);
			return t;
//			base.Count > 0 ? base.Dequeue() : default(T);
		}

		public new void Enqueue(T obj)
		{
			System.Threading.Monitor.Enter(_lockobjdictionary[this.Path]);
			try
			{
				base.Enqueue(obj);
			}
			catch (Exception ex)
			{
				WFLogger.NLogger.ErrorException(string.Empty, ex);
			}
			finally
			{
				System.Threading.Monitor.Exit(_lockobjdictionary[this.Path]);
			}
		}

		#region IWFMessageQueue Members

		public void Dequeue(ref object t)
		{
			T obj = (T)t;
			this.Dequeue(ref obj);
		}

		public void Enqueue(object obj)
		{
			this.Enqueue((T)obj);
		}

#if false
		public new int Count
		{
			get
			{
				int cnt = 0;

				System.Threading.Monitor.Enter(_lockobj);
				try
				{
					cnt = base.Count;
				}
				catch (Exception ex)
				{
					WFLogger.NLogger.ErrorException(string.Empty, ex);
				}
				finally
				{
					System.Threading.Monitor.Exit(_lockobj);
				}
				return cnt;
			}
		}
#endif

		public string Path
		{
			private set
			{
				_path = value;
			}

			get
			{
				return _path;
			}
		}

		#endregion
#endif
	}
}
