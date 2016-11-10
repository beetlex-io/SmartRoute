using SmartRoute.NodeTest.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute.SyncPublishTest
{
    class Program
    {

        static int mCount = 0;
        static int mLastCount = 0;
        static void Main(string[] args)
        {
            INode node = NodeFactory.Default;
            node.Loger.Type = LogType.ALL;
            node.AddLogHandler(new ConsoleLogHandler(LogType.ALL));
            node.Open();
            EventSubscriber ken = node.Register<EventSubscriber>("ken");
            node.SubscriberRegisted = (n, s) =>
            {
                if (s.Name == "henry")
                {

                    System.Threading.ThreadPool.QueueUserWorkItem(o =>
                    {
                        while (true)
                        {
                            Employee result = ken.Publish<Employee>("henry", Employee.GetEmployee());
                            if (result == null)
                                throw new Exception("error");
                            System.Threading.Interlocked.Increment(ref mCount);
                        }
                    });
                }
            };
            while (true)
            {
                Console.WriteLine("{0}/秒|{1}", mCount - mLastCount, mCount);
                mLastCount = mCount;
                System.Threading.Thread.Sleep(1000);
            }
            Console.Read();

        }

    }
}
