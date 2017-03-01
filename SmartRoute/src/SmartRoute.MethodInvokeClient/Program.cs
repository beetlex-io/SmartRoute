using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute.MethodInvokeClient
{
    public class Program
    {
        private static IUserService mUserService;

        public static void Main(string[] args)
        {

            INode node = NodeFactory.Default;
            node.Loger.Type = LogType.ALL;
            node.AddLogHandler(new ConsoleLogHandler(LogType.ALL));
            node.Open();
            mUserService = new UserService(node);
            node.DefaultSwitchSubscriber.ServiceRegisted = (s, m) =>
            {
                if (m.ServiceName == "IUserService")
                {
                    Console.WriteLine("find interface {0}", m.ServiceName);
                    System.Threading.ThreadPool.QueueUserWorkItem(Test_Register);
                    System.Threading.ThreadPool.QueueUserWorkItem(Test_ChangePWD);
                }
            };
            System.Threading.Thread.Sleep(-1);
        }

        static void Test_ChangePWD(object state)
        {
            int i = 0;
            System.Threading.Thread.Sleep(5000);
            while (true)
            {
                try
                {

                    i++;
                    mUserService.ChangePWD("henry" + i, "123", "123456");

                }
                catch (Exception e_)
                {
                    Console.WriteLine(e_.Message);
                }
            }
        }

        static void Test_Register(object state)
        {
            int i = 0;
            System.Threading.Thread.Sleep(5000);
            while (true)
            {
                try
                {

                    i++;
                    DateTime result = mUserService.Register("henry" + i, "hrenyfan@msn.com");

                }
                catch (Exception e_)
                {
                    Console.WriteLine(e_.Message);
                }
            }

        }

    }

    public class UserService : IUserService
    {
        public UserService(INode context)
        {
            this.Node = context;
        }

        public INode Node { get; set; }

        public void ChangePWD(string name, string oldpwd, string newpwd)
        {
            Node.MethodInvoke("IUserService", "ChangePWD", name, oldpwd, newpwd);
        }

        public DateTime Register(string name, string email)
        {
            return Node.MethodInvoke<DateTime>("IUserService", "Register", name, email);
        }
    }

}
