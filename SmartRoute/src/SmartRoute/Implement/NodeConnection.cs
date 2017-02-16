using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeetleX.Clients;
using SmartRoute.Events;

namespace SmartRoute
{
	class NodeConnecter : INodeConnection
	{

		public NodeConnecter(INode node, string remoteNode, string host, int port)
		{
			Node = node;
			RemoteNodeID = remoteNode;
			Host = host;
			Port = port;
			mClient = BeetleX.ServerFactory.CreateTcpClient(Host, Port);
			mClient.Packet = new Protocols.ClientPacket();
			mClient.ConnectedServer = OnConnected;
			mClient.ClientError = OnError;
			mClient.Packet.Completed = OnReceive;
			ResourceStatistics = new Dictionary<string, double>();
		}

		private int mPings = 0;

		private int mConnectStatus = 0;

		private bool mIsSyncSubscriber = false;

		private IClient mClient;

		private Dictionary<string, Object> mProperties = new Dictionary<string, object>();

		public object this[string key]
		{
			get
			{
				object result = null;
				mProperties.TryGetValue(key, out result);
				return result;
			}

			set
			{
				mProperties[key] = value;
			}
		}

		public bool Available
		{
			get
			{
				return mClient.Connected && mPings < 5;
			}
		}

		public IClient Client
		{
			get
			{
				return mClient;
			}
		}

		public string Host
		{
			get; set;
		}

		public INode Node
		{
			get;
			internal set;
		}

		public string RemoteNodeID
		{
			get; set;
		}

		public int Port
		{
			get; set;
		}

		public EventNodeConnecterReceive Receive
		{
			get; set;
		}

		public int Pings
		{
			get
			{
				return mPings;
			}
		}

		public Dictionary<string, double> ResourceStatistics
		{
			get;
			private set;
		}

		public void Ping()
		{
			System.Threading.Interlocked.Increment(ref mPings);
			if (Available)
			{
				Protocols.Ping ping = new Protocols.Ping();

				mClient.Send(ping);

			}
			else
			{
				ConnectToServer();
			}

		}

		public void Send(object data)
		{
			if (!Available)
				throw new SRException("{0} node connecter not a available!", RemoteNodeID);
			mClient.Send(data);
		}

		public void Connect()
		{

			ConnectToServer();
		}

		private void ConnectToServer()
		{
			if (System.Threading.Interlocked.CompareExchange(ref mConnectStatus, 1, 0) == 0)
			{
				System.Threading.ThreadPool.QueueUserWorkItem(o =>
				{

					if (mClient.Connect())
						System.Threading.Interlocked.Exchange(ref mPings, 0);
					System.Threading.Interlocked.Exchange(ref mConnectStatus, 0);
				});
			}
		}

		private void OnConnected(IClient c)
		{
			mIsSyncSubscriber = false;
			Protocols.Authentication auth = new Protocols.Authentication();
			auth.NodeID = Node.ID;
			auth.Token = (auth.NodeID + Node.TokenKey).GetMd5Hash();
			c.Send(auth);

		}

		public void SyncSubscriber()
		{
			if (!mIsSyncSubscriber)
			{
				Protocols.GetSubscribers sync = new Protocols.GetSubscribers();
				sync.NodeID = Node.ID;
				mClient.Send(sync);
				mIsSyncSubscriber = true;
			}
		}

		private void OnError(IClient c, Exception e, string message)
		{
			Node.Loger.Process(LogType.ERROR, "{0} node network error {1}", RemoteNodeID, message);
		}

		private void OnReceive(IClient c, object data)
		{
			if (data is Protocols.Pong)
			{
				Protocols.Pong pong = (Protocols.Pong)data;
				System.Threading.Interlocked.Decrement(ref mPings);
				ResourceStatistics = pong.Properties;

			}
			else
			{
				if (Receive != null)
					Receive(this, data);
			}
		}

		public void Dispose()
		{
			this.Client.DisConnect();
			mProperties.Clear();
			Node = null;
		}
	}
}
