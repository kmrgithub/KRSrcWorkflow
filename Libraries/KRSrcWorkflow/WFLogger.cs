using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace KRSrcWorkflow
{
	public abstract class WFLogger
	{
		private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
		public static void Initialize()
		{
			_logger = NLog.LogManager.GetCurrentClassLogger();
		}

		public static NLog.Logger NLogger
		{
			get
			{
				return _logger;
			}
		}
		private class LoggerData
		{
			internal static bool _logging = true;
			public static void TurnOffLogging()
			{
				_logging = false;
			}
			public static void TurnOnLogging()
			{
				_logging = false;
			}
			public int TickCount { get; set; }
			public string Message { get; set; }
			public string CallingMethod { get; set; }
			public LoggerData(string message, int tickcount, string callingmethod)
			{
				this.TickCount = tickcount;
				this.Message = message;
				this.CallingMethod = callingmethod;
			}
		}

		private static Dictionary<int, Stack<LoggerData>> _threadtotickcountqueue = new Dictionary<int, Stack<LoggerData>>();
		private static void EndLogAll(bool endlogall)
		{
			if (LoggerData._logging.Equals(true))
			{
				int managedthreadid = Thread.CurrentThread.ManagedThreadId;
				if (_threadtotickcountqueue.ContainsKey(managedthreadid))
				{
					if (((Stack<LoggerData>)_threadtotickcountqueue[managedthreadid]).Count > 0)
					{
						List<LoggerData> loggerdatalist = null;
						if (endlogall.Equals(true))
							loggerdatalist = ((Stack<LoggerData>)_threadtotickcountqueue[managedthreadid]).ToList();
						else
						{
							loggerdatalist = new List<LoggerData>();
							loggerdatalist.Add((LoggerData)((Stack<LoggerData>)_threadtotickcountqueue[managedthreadid]).Pop());
						}
						foreach (LoggerData loggerdata in loggerdatalist)
						{
							_logger.Debug(loggerdata.CallingMethod + "(" + (System.Environment.TickCount - loggerdata.TickCount).ToString() + ")  ");
							_logger.Debug(loggerdata.Message);
						}
					}
				}
			}
		}
		public static void BeginLog(string message)
		{
			if (LoggerData._logging.Equals(true))
			{
				string callingmethod = string.Empty;
				int managedthreadid = Thread.CurrentThread.ManagedThreadId;

				StackTrace stackTrace = new StackTrace();
				if (stackTrace != null)
				{
					StackFrame[] stackFrames = stackTrace.GetFrames();
					if (stackFrames != null && (stackFrames.Count() >= 1))
					{
						callingmethod = ((System.Type)stackFrames[1].GetMethod().ReflectedType).Name + "." + stackFrames[1].GetMethod().Name;
//						System.Diagnostics.Debug.WriteLine(callingmethod);
					}
				}

				LoggerData loggerdata = new LoggerData(message, System.Environment.TickCount, callingmethod);
				if (_threadtotickcountqueue.ContainsKey(managedthreadid))
					((Stack<LoggerData>)_threadtotickcountqueue[managedthreadid]).Push(loggerdata);
				else
				{
					Stack<LoggerData> queue = new Stack<LoggerData>();
					queue.Push(loggerdata);
					_threadtotickcountqueue[managedthreadid] = queue;
				}
			}
		}

		public static void EndLog()
		{
			EndLogAll(false);
		}

		public static void EndLogAll()
		{
			EndLogAll(true);
		}
	}
}
