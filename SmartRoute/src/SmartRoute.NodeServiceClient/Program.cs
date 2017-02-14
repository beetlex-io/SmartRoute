using SmartRoute.NodeTest.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute.NodeServiceClient
{
	public class Program
	{
		static long mCount;
		static long mLastCount;
		static SwitchSubscriber mSwitchSubscriber;
		public static void Main(string[] args)
		{
			INode node = NodeFactory.Default;
			node.Loger.Type = LogType.ALL;
			node.AddLogHandler(new SmartRoute.ConsoleLogHandler(LogType.ALL));
			node.Open();
			mSwitchSubscriber = new SwitchSubscriber(node);
			mSwitchSubscriber.ServiceRegisted = (o, e) =>
			{
				if (e.ServiceName == "henry")
				{

				}
			};
			System.Threading.ThreadPool.QueueUserWorkItem(Test);
			while (true)
			{
				Console.WriteLine("{0}/秒|{1}", mCount - mLastCount, mCount);
				mLastCount = mCount;
				System.Threading.Thread.Sleep(1000);
			}
			Console.Read();
		}

		private static void Test(object state)
		{
			System.Threading.Thread.Sleep(10000);
			while (true)
			{
				try
				{
					var item = mSwitchSubscriber.PublishToService<IList<Employee>>("henry", Employee.GetEmployee());
					System.Threading.Interlocked.Increment(ref mCount);
				}
				catch (Exception e_)
				{
					NodeFactory.Default.Loger.Process(LogType.ERROR, e_.Message);
				}
			}
		}

	}
}
