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
			SwitchSubscriber rmiserver = new SwitchSubscriber(node);
			mUserService = new UserService(rmiserver);
			rmiserver.ServiceRegisted = (s, m) =>
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
			while (true)
			{
				System.Threading.Thread.Sleep(1000);
				i++;
				 mUserService.ChangePWD("henry" + i, "123","123456");
				 mUserService.ChangePWD("henry" + i, null, null);
			}
		}

		static void Test_Register(object state)
		{
			int i = 0;
			while (true)
			{
				System.Threading.Thread.Sleep(1000);
				i++;
				DateTime result = mUserService.Register("henry" + i, "hrenyfan@msn.com");
				Console.WriteLine(result);
			}

		}

	}

	public class UserService : IUserService
	{
		public UserService(SwitchSubscriber context)
		{
			this.Context = context;
		}

		public SwitchSubscriber Context { get; set; }

		public void ChangePWD(string name, string oldpwd, string newpwd)
		{
			Context.MethodInvoke("IUserService", "ChangePWD", name, oldpwd, newpwd);
		}

		public DateTime Register(string name, string email)
		{
			return Context.MethodInvoke<DateTime>("IUserService", "Register", name, email);
		}
	}

}
