using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
    public class RemoteServiceSubscriber
    {

        public string Name { get; set; }

        public long ActiveTime { get; set; }

        public bool Available(long currentTime)
        {
            return (currentTime - ActiveTime) < 5000;
        }
        public string RemoteNode { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is RemoteServiceSubscriber)
                return this.Name == ((RemoteServiceSubscriber)obj).Name;
            return this.Name == obj.ToString();
        }
    }

    public class RemoteService
    {
        private System.Collections.Concurrent.ConcurrentDictionary<string, RemoteServiceSubscriber> mServiceSubscribers = new System.Collections.Concurrent.ConcurrentDictionary<string, RemoteServiceSubscriber>();

        private long mIndex = 0;

        private long mVersion = 0;

        private OnlineSegment mOnlines = new OnlineSegment();

        public string ServiceName { get; set; }




        public void Register(string name, INode node, string remoteNode)
        {
            RemoteServiceSubscriber item = null;
            if (mServiceSubscribers.TryGetValue(name, out item))
            {
                item.ActiveTime = node.GetRuntime();
                //node.Loger.Process(LogType.DEBUG, "update service {0} ActiveTime {1}", name, item.ActiveTime);
            }
            else
            {
                item = new RemoteServiceSubscriber();
                item.RemoteNode = remoteNode;
                item.Name = name;
                item.ActiveTime = node.GetRuntime();
                mServiceSubscribers[item.Name] = item;
                mVersion++;
                //node.Loger.Process(LogType.DEBUG, "regiteed remote service {0} ActiveTime {1}", name, item.ActiveTime);
            }

        }

        public RemoteServiceSubscriber[] GetOnlines()
        {
            if (mOnlines.Version != mVersion)
            {
                mOnlines.Version = mVersion;
                mOnlines.Values = mServiceSubscribers.Values.ToArray();
            }
            return mOnlines.Values;
        }

        public RemoteServiceSubscriber GetServiceSubscriber(INode node)
        {
            long runtime = node.GetRuntime();
            RemoteServiceSubscriber result = null;
            mIndex++;
            int count = 0;
            RemoteServiceSubscriber[] items = GetOnlines();
            while (count < items.Length)
            {
                RemoteServiceSubscriber item = items[(int)(mIndex % items.Length)];
                if (item.Available(runtime))
                {
                    result = item;
                    break;
                }
                count++;
                mIndex++;
            }
            return result;
        }

        class OnlineSegment
        {
            public OnlineSegment()
            {
                Version = -1;
                Values = new RemoteServiceSubscriber[0];
            }
            public long Version
            {
                get;
                set;
            }

            public RemoteServiceSubscriber[] Values
            {
                get;
                set;
            }

        }
    }

}
