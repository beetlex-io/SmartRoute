using SmartRoute.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
    public interface INode : IDisposable
    {

        object this[string key] { get; set; }

        ILogHandler Loger { get; }

        long GetRuntime();

        string ID { get; }

        string Cluster { get; set; }

        string Host { get; set; }

        string TokenKey { get; set; }

        int Port { get; set; }

        void Open();

        void AddLogHandler<T>() where T : ILogHandler, new();

        void AddLogHandler(ILogHandler item);

        NodeStatus Status { get; }

        T Register<T>(string name) where T : ISubscriber, new();

        void Register(string name, ISubscriber subscriber);

        void UnRegister(string name);

        void Publish(Message message);

        T Publish<T>(Message message, int millisecondsTimeout = 10000);

        ICollection<ISubscriber> GetLocalSubscriber();

        ICollection<ISubscriber> GetRemoteSubscriber();

        EventSubscriberRegisted SubscriberRegisted { get; set; }

    }
}
