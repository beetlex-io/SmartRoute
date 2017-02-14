using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
	public interface ILogHandler
	{
		LogType Type
		{
			get;
			set;
		}

		void Process(LogType type, string message);

		void Process(LogType type, string formate, params object[] parameters);
		// bool Enabled(LogType type);
	}

	public enum LogType
	{
		NONE = 1,
		DEBUG = 2,
		INFO = 4,
		WARN = 8,
		ERROR = 16,
		FATAL = 32,
		BROAD_DEBUG = 64,
		BROAD_ERROR = 128,
		MESSAGE_DEBUG = 256,
		ALL = 1 | 2 | 4 | 8 | 16 | 32

	}

	class LogHandlerAdapter : ILogHandler
	{
		public LogHandlerAdapter()
		{
			Handlers = new List<ILogHandler>();

			Type = LogType.ALL;
		}

		public IList<ILogHandler> Handlers
		{
			get;
			set;
		}

		public void Process(LogType type, string message)
		{
			if (Enabled(type))
			{
				for (int i = 0; i < Handlers.Count; i++)
				{
					try
					{
						Handlers[i].Process(type, message);
					}
					catch
					{
					}
				}
			}

		}

		public void Process(LogType type, string formate, params object[] parameters)
		{
			if (Enabled(type))
				Process(type, string.Format(formate, parameters));
		}


		public LogType Type
		{
			get;
			set;
		}

		private bool Enabled(LogType type)
		{
			return (type & Type) > 0;
		}
	}

	public class ConsoleLogHandler : ILogHandler
	{



		private System.Collections.Concurrent.ConcurrentQueue<LogItem> mQueue = new System.Collections.Concurrent.ConcurrentQueue<LogItem>();

		class LogItem
		{
			public LogType Type
			{
				get;
				set;
			}
			public string Message
			{
				get;
				set;
			}

		}

		public ConsoleLogHandler()
		{
			Console.InputEncoding = System.Text.Encoding.UTF8;
			Console.OutputEncoding = System.Text.Encoding.UTF8;
			OnInit();
			Type = LogType.ALL;
		}

		private void OnInit()
		{
			System.Threading.ThreadPool.QueueUserWorkItem(o =>
			{
				while (true)
				{
					LogItem item = null;
					if (mQueue.TryDequeue(out item))
					{
						switch (item.Type)
						{
							case LogType.INFO:
								Console.ForegroundColor = ConsoleColor.Green;
								break;
							case LogType.DEBUG:
								Console.ForegroundColor = ConsoleColor.Yellow;
								break;
							case LogType.BROAD_DEBUG:
								Console.ForegroundColor = ConsoleColor.DarkYellow;
								break;
							case LogType.WARN:
								Console.ForegroundColor = ConsoleColor.DarkYellow;
								break;
							case LogType.ERROR:
								Console.ForegroundColor = ConsoleColor.Red;
								break;
							case LogType.FATAL:
								Console.ForegroundColor = ConsoleColor.Red;
								break;
							case LogType.BROAD_ERROR:
								Console.ForegroundColor = ConsoleColor.DarkRed;
								break;

							default:
								Console.ForegroundColor = ConsoleColor.White;
								break;
						}

						Console.WriteLine("{0}\t[{1}]\t{2}", item.Type, DateTime.Now, item.Message);
					}
					else
					{
						System.Threading.Thread.Sleep(20);
					}
				}
			});
		}

		public ConsoleLogHandler(LogType outputType)
		{
			Type = outputType;
			OnInit();
		}

		public void Process(LogType type, string message)
		{
			if (Enabled(type))
				mQueue.Enqueue(new LogItem { Type = type, Message = message });
		}

		public void Process(LogType type, string formate, params object[] parameters)
		{
			if (Enabled(type))
				Process(type, string.Format(formate, parameters));
		}


		public LogType Type
		{
			get;
			set;
		}

		private bool Enabled(LogType type)
		{
			return (Type & type) > 0;
		}
	}
}
