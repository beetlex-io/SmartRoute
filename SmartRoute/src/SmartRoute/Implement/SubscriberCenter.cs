using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
    class SubscriberCenter : ISubscriberCenter
    {
        private System.Collections.Concurrent.ConcurrentDictionary<string, ISubscriber> mSubscribers = new System.Collections.Concurrent.ConcurrentDictionary<string, ISubscriber>();

        private long mVersion = 0;

        private SubscriberSegment mSegment = new SubscriberSegment();

        public INode Node
        {
            get; set;
        }

        public void Dispose()
        {
            mSubscribers.Clear();
            Node = null;
        }

        public ISubscriber Find(string name)
        {
            ISubscriber result;
            mSubscribers.TryGetValue(name, out result);
            return result;
        }

        class SubscriberSegment
        {
            public long Version
            {
                get; set;
            }
            public string[] Keys
            {
                get; set;
            }

        }

        public string[] GetAll()
        {
            if (mSegment.Version != mVersion)
            {
                mSegment.Keys = mSubscribers.Keys.ToArray();
                mSegment.Version = mVersion;
            }
            return mSegment.Keys;
        }

        public IList<ISubscriber> Find(string name, ReceiveMode mode)
        {
            List<ISubscriber> result = new List<ISubscriber>();
            ICollection<String> onlines = GetAll();
            string[] subscribers = GetSubscribers(name);
            switch (mode)
            {
                case ReceiveMode.All:
                    foreach (string item in onlines)
                        GetSubscriberToList(item, result);
                    break;
                case ReceiveMode.Eq:
                    if (subscribers.Length == 1)
                    {
                        GetSubscriberToList(subscribers[0], result);
                    }
                    else
                    {
                        foreach (string item in onlines)
                            if (subscribers.Contains(item))
                                GetSubscriberToList(item, result);
                    }
                    break;
                case ReceiveMode.NotEq:
                    foreach (string item in onlines)
                        if (!subscribers.Contains(item))
                            GetSubscriberToList(item, result);
                    break;
                case ReceiveMode.Regex:
                    foreach (string item in onlines)
                    {
                        foreach (string pattern in subscribers)
                            if (System.Text.RegularExpressions.Regex.IsMatch(item, pattern))
                            {
                                GetSubscriberToList(item, result);
                                break;
                            }
                    }
                    break;
            }
            return result;
        }

        private void GetSubscriberToList(string name, IList<ISubscriber> items)
        {
            ISubscriber sub = Find(name);
            if (sub != null)
                items.Add(sub);
        }

        public static string[] GetSubscribers(string name)
        {
            return name.Split(';');
        }

        public static bool IsSubscribers(string name)
        {
            return name.IndexOf(';') > 0;
        }

        public void Register(string name, ISubscriber subscriber)
        {
            ISubscriber result;
            if (mSubscribers.TryGetValue(name, out result))
            {
                result.Dispose();
            }
            mSubscribers[name] = subscriber;
            mVersion++;

        }

        public ISubscriber UnRegister(string name)
        {
            ISubscriber result;
            if (mSubscribers.TryRemove(name, out result))
            {
                mVersion++;
                result.Dispose();
                return result;
            }
            return null;

        }

        public IList<ISubscriber> Find(Message message)
        {
            return Find(message.Consumers, message.Mode);
        }

        public ICollection<ISubscriber> GetAllSubscriber()
        {
            return mSubscribers.Values;
        }
    }
}
