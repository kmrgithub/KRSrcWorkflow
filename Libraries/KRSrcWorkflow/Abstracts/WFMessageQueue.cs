using System;
using System.Threading;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
using Microsoft.Practices.Unity.Configuration;

using KRSrcWorkflow.Interfaces;
using KRSrcWorkflow.CompressionImplementations;
using KRSrcWorkflow.SerializationImplementations;

namespace KRSrcWorkflow.Abstracts
{
	[Flags]
	public enum WFMessageQueueType { None, Publisher, Consumer }

	public abstract class WFMessageQueue<T> : IWFMessageQueue<T>, IWFMessageQueue where T : ProcessorData
	{
		protected static readonly TimeSpan _timespan = new TimeSpan(0, 0, 0, 1, 0);

		private static readonly Object _lockobj = new object();

		private IUnityContainer _unitycontainer = null;

		public ICompression CompressionLib { get; set; }

		public ISerialize<T> SerializeLib { get; set; }

		protected WFMessageQueue(ICompression compressionlib, ISerialize<T> serializelib)
		{
			this.CompressionLib = compressionlib;
			this.SerializeLib = serializelib;

			if (this.CompressionLib == null)
				this.CompressionLib = new DefaultCompression();

			if (this.SerializeLib == null)
				this.SerializeLib = new JSONSerialization<T>();

			_unitycontainer = new UnityContainer()
				.AddNewExtension<Interception>();
			_unitycontainer.Configure<Interception>()
				.SetInterceptorFor<T>(new TransparentProxyInterceptor())
				.AddPolicy("PropertyInterceptionPolicy")
				.AddMatchingRule(new PropertyMatchingRule("*", PropertyMatchingOption.GetOrSet));
		}

		protected WFMessageQueue(ISerialize<T> serializelib)
			: this(null, serializelib)
		{
		}

		protected WFMessageQueue(ICompression compressionlib)
			: this(compressionlib, null)
		{
		}

		protected WFMessageQueue()
			: this(null, null)
		{
		}

		private int CountLock()
		{
			Monitor.Enter(_lockobj);
			int cnt = this.Count;
			if (cnt == 0)
				Monitor.Exit(_lockobj);
			return cnt;
		}

		#region IWFMessageQueue<T> Members
		public void Dequeue(ref T t)
		{
			t = default(T);
			if (this.CountLock() > 0)
			{
//				System.Threading.Monitor.Enter(_lockobj);
				try
				{
					WFLogger.NLogger.Trace("Dequeue: {0}", this.Path);
					byte[] bytes = new byte[0];
					Dequeue(ref bytes);
					if (bytes != null && bytes.Length > 0)
					{
						string json = this.CompressionLib.Decompress(bytes); // WFUtilities.Decompress(bytes);
						t = this.SerializeLib.Deserialize(json); // Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
						t.DequeueTime = DateTime.UtcNow;
						t = _unitycontainer.RegisterInstance(t).Resolve<T>();
					}
				}
				catch (Exception ex)
				{
					WFLogger.NLogger.ErrorException(string.Empty, ex);
					t = default(T);
				}
				finally
				{
					Monitor.Exit(_lockobj);
				}
			}
		}

		public void Enqueue(T t)
		{
			Monitor.Enter(_lockobj);
			try
			{
				WFLogger.NLogger.Trace("Enqueue: {0}", this.Path);
				t.EnqueueTime = DateTime.UtcNow;
				string json = this.SerializeLib.Serialize(t); // Newtonsoft.Json.JsonConvert.SerializeObject(t);
				byte[] bytes = this.CompressionLib.Compress(json); // KRSrcWorkflow.WFUtilities.CompressString(json);
				Enqueue(bytes);
			}
			catch (Exception ex)
			{
				WFLogger.NLogger.ErrorException(string.Empty, ex);
			}
			finally
			{
				Monitor.Exit(_lockobj);
			}
		}
		#endregion

		#region IWFMessageQueue Members
		public void Dequeue(ref object obj)
		{
			T t = (T)obj;
			this.Dequeue(ref t);
		}

		public void Enqueue(object t)
		{
			this.Enqueue((T)t);
		}
		#endregion

		#region abstract methods
		abstract public void Enqueue(byte[] t);
		abstract public void Dequeue(ref byte[] t);
		abstract public int Count { get; }
		abstract public string Path { get; }
		#endregion
	}
}
