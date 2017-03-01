# SmartRoute（服务即集群）
  SmartRoute是基于Dotnet Core设计的可运行在linux和windows下的服务通讯组件，其设计理念是去中心化和零配置即可实现服务通讯集群。SmartRoute是通过消息订阅的机制实现服务与服务之间的通讯，它可以让广播网段内所有服务器上的应用自动构建通讯集群； 而通讯集群完全是SmartRoute自动构建并不需要进行任何配置或安装中间服务。通过这种全新的通讯开发方式可以让开发者更轻松和简单地构建基于服务的集群通讯应用。
  ![image](https://github.com/IKende/SmartRoute/blob/master/smartroute.jpg)
##扩展服务
[分布式业务号生成服务:https://github.com/IKende/SmartRoute.BNR](https://github.com/IKende/SmartRoute.BNR)  
[分布式锁服务https  ://github.com/IKende/SmartRoute.DLocks](https://github.com/IKende/SmartRoute.DLocks)
##提供集群节点监控功能 2017-2-16　
   通过任意一个节点获取整个集群节点的状态信息,主要包括:网络IO,网络流量,消息处理量和当前每秒相应并发量的信息.
``` c#
node.GetClusterInfo()
```
##提供远程接口调用功能 2017-2-14　
    注册接口服务
``` c#
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
```
    调用接口服务
``` c#
			INode node = NodeFactory.Default;
			node.Loger.Type = LogType.ALL;
			node.AddLogHandler(new ConsoleLogHandler(LogType.ALL));
			node.Open();
			SwitchSubscriber rmiserver = new SwitchSubscriber(node);
			mUserService = new UserService(rmiserver);
      DateTime result = mUserService.Register("henry" + i, "hrenyfan@msn.com");
```
##提供订阅负载 2016－11－24
　　　通过SwitchSubscriber实现多节点订阅负载处理
``` c#
 IList<Employee> item = mSwitchSubscriber.PublishToServicee<IList<Employee>>("henry", Employee.GetEmployee());
```
##提供同步返回消息支持 2016-11-10
   由于很多场景需要支持同步返回消息处理，因此加入些功能的支持
``` c#
  Employee result = ken.Publish<Employee>("henry", Employee.GetEmployee());
```
  
  
##创建订阅
``` c#
    public class Program
    {
        static long mCount;
        public static void Main(string[] args)
        {
            INode node = NodeFactory.Default;
            node.Open();
            EventSubscriber henry = node.Register<EventSubscriber>("henry");
            henry.Register<User>(OnUser);
            henry.Register<Employee>(OnEmployees);
            Console.Read();
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
```

##订阅,发现和推送消息
``` c#
    public class Program
    {
        static long mCount;
        public static void Main(string[] args)
        {
            INode node = NodeFactory.Default;
            node.Open();
            EventSubscriber ken = node.Register<EventSubscriber>("ken");
            ken.Register<User>(OnUser);
            ken.Register<Employee>(OnEmployees);
            node.SubscriberRegisted = (n, s) =>
            {
                if (s.Name == "henry")
                {
                    ken.Publish("henry", Employee.GetEmployee());
                }
            };
            while (true)
            {
                Console.WriteLine(mCount);
                System.Threading.Thread.Sleep(1000);
            }
            Console.Read();
        }
        private static void OnEmployees(Message msg, Employee emp)
        {
            System.Threading.Interlocked.Increment(ref mCount);
            msg.Reply(Employee.GetEmployee());
        }
        private static void OnUser(Message msg, User user)
        {
            msg.Reply(new User { Name = "key" });
            System.Threading.Interlocked.Increment(ref mCount);
        }
    }
```
