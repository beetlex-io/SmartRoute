using BeetleX.Clients;
using SmartRoute.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
	public interface INodeConnection : IDisposable
	{
		IClient Client
		{ get; }

		object this[string key] { get; set; }

		void Connect();

		INode Node { get; }

		string Host { get; set; }

		int Port { get; set; }

		string RemoteNodeID { get; set; }

		bool Available { get; }

		void Send(object data);

		void Ping();

		int Pings { get; }

		EventNodeConnecterReceive Receive { get; set; }

		void SyncSubscriber();

		Dictionary<string, double> ResourceStatistics { get; }

	}
}
