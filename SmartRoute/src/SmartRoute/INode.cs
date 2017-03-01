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

		T PublishSync<T>(Message message, int millisecondsTimeout = 10000);

		object PublishSync(Message message, int millisecondsTimeout = 10000);

		ICollection<ISubscriber> GetLocalSubscriber();

		ICollection<ISubscriber> GetRemoteSubscriber();

		event EventSubscriberRegisted SubscriberRegisted;

		Dictionary<string, double> GetResourceStatistics();

		Action<NodeResourceStatistics, INode> GetResourceStatisticsEvent { get; set; }

		ClusterInfo GetClusterInfo();

		EventSubscriber DefaultEventSubscriber { get; }

		SwitchSubscriber DefaultSwitchSubscriber { get; }

		void RegisterService<T>(object service);

		Object MethodInvoke(string name, string method, params object[] parameters);

		T MethodInvoke<T>(string name, string method, params object[] parameters);

		object PublishToService(string service, object data);

		T PublishToService<T>(string service, object data);
	}
}
