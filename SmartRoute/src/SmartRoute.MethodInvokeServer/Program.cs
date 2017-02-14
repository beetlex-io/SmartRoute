using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute.MethodInvokeServer
{
	public class Program : IUserService
	{
		public static void Main(string[] args)
		{
			INode node = SmartRoute.NodeFactory.Default;
			node.Loger.Type = LogType.ALL;
			node.AddLogHandler(new SmartRoute.ConsoleLogHandler(LogType.ALL));
			node.Open();
			SwitchSubscriber rmiserver = new SwitchSubscriber(node);
			rmiserver.Register<IUserService>(new Program());
			System.Threading.Thread.Sleep(-1);
		}

		public void ChangePWD(string name, string oldpwd, string newpwd)
		{
			Console.WriteLine("ChangePWD {0}/{1}/{2}", name, oldpwd, newpwd);
		}

		public DateTime Register(string name, string email)
		{
			Console.WriteLine("register {0}/{1}", name, email);
			return DateTime.Now;
		}
	}
}
