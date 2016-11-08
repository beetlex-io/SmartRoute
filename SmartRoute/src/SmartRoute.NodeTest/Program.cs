using SmartRoute.NodeTest.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute.NodeTest
{
    public class Program
    {
        static long mCount;
        public static void Main(string[] args)
        {
            INode node = NodeFactory.Default;
            node.Loger.Type = LogType.ALL;
            node.AddLogHandler(new SmartRoute.ConsoleLogHandler(LogType.ALL));
            node.Open();
            EventSubscriber henry = node.Register<EventSubscriber>("henry");

            henry.Register<User>(OnUser);
            henry.Register<Employee>(OnEmployees);

            node.SubscriberRegisted = (n, s) =>
            {
                if (s.Name == "ken")
                {
                    // System.Threading.ThreadPool.QueueUserWorkItem(ToKen, henry);
                }
            };

            while (true)
            {
                //System.Text.StringBuilder sb = new System.Text.StringBuilder();
                //foreach (ISubscriber item in node.GetLocalSubscriber())
                //    sb.Append(item.ToString() + ',');
                //foreach (ISubscriber item in node.GetRemoteSubscriber())
                //    sb.Append(item.ToString() + ',');
                //node.Loger.Process(LogType.INFO, sb.ToString());
                // Console.WriteLine(mCount);
                System.Threading.Thread.Sleep(1000);
            }
            Console.Read();
        }
        private static void ToKen(object state)
        {
            EventSubscriber henry = (EventSubscriber)state;
            while (true)
            {
                henry.Publish("ken", Employee.GetEmployee());
                System.Threading.Thread.Sleep(1);
            }
        }
        private static void OnEmployees(Message msg, Employee emp)
        {
            System.Threading.Interlocked.Increment(ref mCount);
            msg.Reply(Employee.GetEmployee());
        }
        private static void OnUser(Message msg, User user)
        {
            System.Threading.Interlocked.Increment(ref mCount);
            msg.Reply(new User { Name = "henry" });
        }
    }
}
