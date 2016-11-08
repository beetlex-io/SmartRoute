# SmartRoute（服务即集群）
  SmartRoute的设计理念是去中心化和零配置，通过消息订阅的机制实现服务与服务之间的通讯。SmartRoute可以让广播网段内所有服务器上的应用自动构建通讯集群，
  而这通讯的集群是构建并不则要进行任何配置或安装中间服务。通过这种全新的服务通讯应用可以让开发者变重更轻松和简单地构建服务通讯。
  
##　订阅
``` c#
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

##　订阅,发现和推送消息
``` c#
    public class Program
    {
        static long mCount;
        public static void Main(string[] args)
        {
            INode node = NodeFactory.Default;
            node.Loger.Type = LogType.ALL;
            node.AddLogHandler(new SmartRoute.ConsoleLogHandler(LogType.ALL));
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
