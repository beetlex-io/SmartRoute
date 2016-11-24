using SmartRoute.NodeTest.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute.NodeService
{
    public class Program
    {
        static long mCount;
        static long mLastCount;
        public static void Main(string[] args)
        {
            INode node = NodeFactory.Default;
            node.Loger.Type = LogType.ALL;
            node.AddLogHandler(new SmartRoute.ConsoleLogHandler(LogType.ALL));
            node.Open();
            SwitchSubscriber switchSubscriber = new SwitchSubscriber(node);
            EventSubscriber henry = switchSubscriber.GetService("henry");
            henry.Register<User>(OnUser);
            henry.Register<Employee>(OnEmployees);
            while (true)
            {
                Console.WriteLine("{0}/秒|{1}", mCount - mLastCount, mCount);
                mLastCount = mCount;
                System.Threading.Thread.Sleep(1000);
            }
            Console.Read();
        }
        private static void OnEmployees(Message msg, Employee emp)
        {
            System.Threading.Interlocked.Increment(ref mCount);
            msg.Reply(Employee.GetEmployees(2));
        }
        private static void OnUser(Message msg, User user)
        {
            System.Threading.Interlocked.Increment(ref mCount);
            msg.Reply(new User { Name = "henry" });
        }
    }
}
