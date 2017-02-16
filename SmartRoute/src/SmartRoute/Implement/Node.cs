using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeetleX;
using BeetleX.EventArgs;
using SmartRoute.Events;
using SmartRoute.Protocols;
using System.Collections.Concurrent;

namespace SmartRoute
{
	class Node : INode, BeetleX.IServerHandler
	{

		public Node()
		{

			Cluster = "SMARTROUTE_DEFAULT";
			Port = 10280;
			ID = Guid.NewGuid().ToString("N");
			mLogHandler = new LogHandlerAdapter();
			TokenKey = "SMARTROUTE_DEFAULT";

		}

		private NodeResourceStatistics mIoStatistics = new NodeResourceStatistics();

		private object mLockStatistics = new object();

		private bool mIsLinux = false;

		private long mProcessingMessages;

		private long mProcessedMessages;

		private System.Threading.Timer mPingNodeTimer = null;

		private ISubscriberCenter mLocalSubscriberCenter = new SubscriberCenter();

		private ISubscriberCenter mRemoteSubscriberCenter = new SubscriberCenter();

		private IRemoteNodeCenter mRemoteNodeCenter = new RemoteNodeCenter();

		private Dictionary<string, object> mProperties = new Dictionary<string, object>();

		private System.Collections.Concurrent.ConcurrentDictionary<string, ISession> mRemoteNodeSessions = new System.Collections.Concurrent.ConcurrentDictionary<string, ISession>();

		private LogHandlerAdapter mLogHandler = new LogHandlerAdapter();

		private INodeBroadcastListen mBroadcastListen;

		private BeetleX.IServer mServer;

		public string Cluster
		{
			get;
			set;
		}

		public string Host
		{
			get;
			set;
		}

		public string ID
		{
			get;
			internal set;
		}

		public ILogHandler Loger
		{
			get
			{
				return mLogHandler;
			}
		}

		public int Port
		{
			get;
			set;
		}

		public string TokenKey
		{
			get;
			set;
		}

		public NodeStatus Status
		{
			get;
			private set;
		}

		public void AddLogHandler(ILogHandler item)
		{
			mLogHandler.Handlers.Add(item);
		}

		public void AddLogHandler<T>() where T : ILogHandler, new()
		{
			mLogHandler.Handlers.Add(new T());
		}

		public void Dispose()
		{
			if (mRemoteNodeCenter != null)
				mRemoteNodeCenter.Dispose();
			if (mServer != null)
				mServer.Dispose();
			mProperties.Clear();
			if (mPingNodeTimer != null)
				mPingNodeTimer.Dispose();
		}

		public void Open()
		{
			try
			{

#if DOTNET_CORE
				mIsLinux = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
#endif
				IPacket packet = new Protocols.Packet();
				NetConfig netconfig = new NetConfig();

				netconfig.Port = Port;
				netconfig.Host = Host;
				mServer = ServerFactory.CreateTcpServer(netconfig, this, packet);
				while (!mServer.Open())
				{
					Port++;
					netconfig.Port++;
					if (Port > 60000)
					{
						Status = NodeStatus.Error;
						Loger.Process(LogType.ERROR, "node create server error!");
						return;
					}
				}
				mBroadcastListen = new NodeBroadcastListen(this.Cluster, Host, Port, ID, TokenKey);
				mBroadcastListen.Loger = Loger;
				mBroadcastListen.Discover = OnDiscoverNode;
				mBroadcastListen.Open();
				mPingNodeTimer = new System.Threading.Timer(OnPingRemoteNode, null, 1000, 1000);
				Loger.Process(LogType.INFO, "[{0}] Node Start {1}@{2}", ID, Host, Port);
				Status = NodeStatus.Start;
			}
			catch (Exception e_)
			{
				Status = NodeStatus.Error;
				Loger.Process(LogType.ERROR, "node start error {0}!", e_.Message);
			}

		}

		private void ProcessingMessage()
		{

			System.Threading.Interlocked.Increment(ref mProcessingMessages);
		}

		private void ProcessedMessage()
		{
			System.Threading.Interlocked.Decrement(ref mProcessingMessages);
			System.Threading.Interlocked.Increment(ref mProcessedMessages);

		}

		private void OnPingRemoteNode(object state)
		{
			ICollection<INodeConnection> onlines = mRemoteNodeCenter.GetOnlines();
			foreach (INodeConnection item in onlines)
			{
				if (item.Pings >= 10)
				{
					INodeConnection result = mRemoteNodeCenter.UnRegister(item.RemoteNodeID);
					mBroadcastListen.Remove(item.RemoteNodeID);
					item.Dispose();
					Loger.Process(LogType.INFO, "remove node [{0}]", item.RemoteNodeID);
				}
				else
				{
					item.Ping();
					ISession session;
					if (this.mRemoteNodeSessions.TryGetValue(item.RemoteNodeID, out session))
					{
						item.SyncSubscriber();
					}
				}
			}
		}

		private void OnDiscoverNode(object sender, NodeBroadcastListen.DiscoverEventArgs e)
		{
			Loger.Process(LogType.INFO, "discovery node [{0}] from {1}@{2}", e.NodeID, e.Address, e.Port);
			INodeConnection connecter = mRemoteNodeCenter.Register(this, e.NodeID, e.Address, e.Port); // new SmartRoute.NodeConnecter(this, e.NodeID, e.Address, e.Port);
			connecter.Receive = OnRemoteNodeReceive;
			connecter.Connect();
		}

		public void Connecting(IServer server, ConnectingEventArgs e)
		{
			Loger.Process(LogType.INFO, "connect from {0}", e.Socket.RemoteEndPoint);

		}

		public void Connected(IServer server, ConnectedEventArgs e)
		{
			Loger.Process(LogType.INFO, "session connected from {0}@{1}", e.Session.RemoteEndPoint, e.Session.ID);
		}

		void IServerHandler.Log(IServer server, ServerLogEventArgs e)
		{
			this.Loger.Process(LogType.INFO, e.Message);
		}

		public void Error(IServer server, ServerErrorEventArgs e)
		{
			if (e.Session == null)
			{
				Loger.Process(LogType.ERROR, "server error {0}@{1}", e.Message, e.Error.Message);
			}
			else
			{
				Loger.Process(LogType.ERROR, "session {2}@{3} error {0}@{1}", e.Message, e.Error.Message, e.Session.RemoteEndPoint, e.Session.ID);
			}
		}

		public void SessionReceive(IServer server, SessionReceiveEventArgs e)
		{


		}

		private void ProcessMessage(IServer server, ISession session, object message)
		{
			if (message is Ping)
			{
				Pong poing = new Pong();
				poing.NodeID = ID;
				poing.Properties = GetResourceStatistics();
				server.Send(poing, session);
			}
			else if (message is BroadSubscriber)
			{
				BroadSubscriber bs = (BroadSubscriber)message;
				RegisterRemoteSubscribers(bs.NodeID, bs.Name);
				Loger.Process(LogType.DEBUG, "registered remote subscriber [{0}]", bs.Name);
			}
			else if (message is GetSubscribers)
			{
				GetSubscribersResponse response = new GetSubscribersResponse();
				response.NodeID = ID;
				response.Subscribers = mLocalSubscriberCenter.GetAll();
				server.Send(response, session);
			}
			else if (message is BroadRemoveSubscriber)
			{
				BroadRemoveSubscriber remove = (BroadRemoveSubscriber)message;
				UnRegisterRemoteSubscribers(remove.Name);
			}
			else if (message is Message)
			{
				((Message)message).Track("node receive message");
				Publish((Message)message);
				((Message)message).EndTrack("node receive message completed!", this);
			}
			else
			{

			}
		}

		private ISession GetNodeSession(string nodeid)
		{
			ISession result = null;
			mRemoteNodeSessions.TryGetValue(nodeid, out result);
			return result;
		}

		public void SessionPacketDecodeCompleted(IServer server, PacketDecodeCompletedEventArgs e)
		{
			if (e.Session.Authentication != AuthenticationType.security && e.Message is Authentication)
			{
				Authentication authentication = (Authentication)e.Message;
				if (!authentication.Token.VerifyMd5Hash(authentication.NodeID + TokenKey))
				{
					Loger.Process(LogType.ERROR, "[{0}] node from {1} authentication error", authentication.NodeID, e.Session.RemoteEndPoint);
					e.Session.Dispose();
				}
				else
				{
					e.Session.Authentication = AuthenticationType.security;
					Loger.Process(LogType.INFO, "[{0}] node from {1} authentication success", authentication.NodeID, e.Session.RemoteEndPoint);
					mRemoteNodeSessions[authentication.NodeID] = e.Session;
					e.Session["NODEID"] = authentication.NodeID;
					INodeConnection connection = mRemoteNodeCenter.Get(authentication.NodeID);
					if (connection != null)
						connection.SyncSubscriber();

				}
				return;
			}
			if (e.Session.Authentication != AuthenticationType.security)
			{
				Loger.Process(LogType.ERROR, "[{0}] session authentication error", e.Session.RemoteEndPoint);
				e.Session.Dispose();
			}
			else
			{
				ProcessMessage(server, e.Session, e.Message);
			}

		}

		private void OnRemoteNodeReceive(INodeConnection connecter, object message)
		{
			if (message is GetSubscribersResponse)
			{
				GetSubscribersResponse response = (GetSubscribersResponse)message;
				RegisterRemoteSubscribers(response.NodeID, response.Subscribers);
				Loger.Process(LogType.INFO, "sync remote subscribers from [{0}]", response.NodeID);

			}
			else if (message is Message)
			{
				((Message)message).Track("node receive message");
				Publish((Message)message);
				((Message)message).EndTrack("node receive message completed!", this);
			}
		}

		public void Disconnect(IServer server, SessionEventArgs e)
		{
			Loger.Process(LogType.INFO, "session {0}@{1} disconnected", e.Session.RemoteEndPoint, e.Session.ID);
			string nodeid = (string)e.Session["NODEID"];
			if (nodeid != null)
			{
				ISession result;
				mRemoteNodeSessions.TryRemove(nodeid, out result);
			}
		}

		public void SessionDetection(IServer server, SessionDetectionEventArgs e)
		{

		}

		private void UnRegisterRemoteSubscribers(string name)
		{
			mRemoteSubscriberCenter.UnRegister(name);
			Loger.Process(LogType.DEBUG, "unregistered remote subscriber [{0}]", name);
		}

		private void RegisterRemoteSubscribers(string remoteNode, params string[] names)
		{
			foreach (string name in names)
			{
				if (mIsLinux)
				{
					ISession session = GetNodeSession(remoteNode);
					ISubscriber subscriber = null;
					if (session != null)
					{
						subscriber = new SessionSubscriber(this, name, session);
						mRemoteSubscriberCenter.Register(name, subscriber);
						Loger.Process(LogType.DEBUG, "remote {0} registed to SessionSubscriber", name);
						OnSubscriberRegisted(subscriber);

					}
					else
					{
						INodeConnection connection = mRemoteNodeCenter.Get(remoteNode);
						if (connection != null)
						{
							subscriber = new RemoteNodeSubscriber(name, this, connection);
							mRemoteSubscriberCenter.Register(name, subscriber);
							Loger.Process(LogType.DEBUG, "remote {0} registed to RemoteNodeSubscriber", name);
							OnSubscriberRegisted(subscriber);
						}
					}
				}
				else
				{
					ISubscriber subscriber = null;
					INodeConnection connection = mRemoteNodeCenter.Get(remoteNode);
					if (connection != null)
					{
						subscriber = new RemoteNodeSubscriber(name, this, connection);
						mRemoteSubscriberCenter.Register(name, subscriber);
						Loger.Process(LogType.DEBUG, "remote {0} registed to RemoteNodeSubscriber", name);
						OnSubscriberRegisted(subscriber);
					}
				}
			}
		}

		public void Register(string name, ISubscriber subscriber)
		{
			subscriber.Node = this;
			mLocalSubscriberCenter.Register(name, subscriber);
			BroadSubscriber bs = new BroadSubscriber();
			bs.NodeID = this.ID;
			bs.Name = name;

			mRemoteNodeCenter.Send(bs);
			OnSubscriberRegisted(subscriber);
			Loger.Process(LogType.INFO, "registered local subscriber [{0}]", name);
		}

		public void UnRegister(string name)
		{
			ISubscriber subscriber = mLocalSubscriberCenter.UnRegister(name);
			if (subscriber != null)
			{
				BroadRemoveSubscriber remove = new BroadRemoveSubscriber();
				remove.NodeID = this.ID;
				remove.Name = name;
				mRemoteNodeCenter.Send(remove);
				Loger.Process(LogType.INFO, "unregistered loal subscriber [{0}]", name);
			}
		}

		private void OnPulish(Message message)
		{
			message.Track("node publish message");
			if (!SubscriberCenter.IsSubscribers(message.Consumers) && message.Mode == ReceiveMode.Eq)
			{
				message.Track("match consumers start");
				ISubscriber subs = mLocalSubscriberCenter.Find(message.Consumers);
				message.Track("match consumers completed");
				if (subs != null)
				{
					ProcessingMessage();
					message.Track("publish to " + message.Consumers + subs.GetType());
					subs.Process(this, message);
					message.Track("publish to " + message.Consumers + " completed!");
					ProcessedMessage();
					return;
				}
				subs = mRemoteSubscriberCenter.Find(message.Consumers);
				if (subs != null)
				{
					ProcessingMessage();
					message.Track("publish to " + message.Consumers + subs.GetType());
					subs.Process(this, message);
					message.Track("publish to " + message.Consumers + " completed!");
					ProcessedMessage();
					return;
				}
				string error = string.Format("[{0}] subscriber not fount!", message.Consumers);
				message.ProcessError(new SRException(error));
				Loger.Process(LogType.ERROR, error);
			}
			else
			{
				IList<ISubscriber> local, remote;
				message.Track("match consumers start");
				local = mLocalSubscriberCenter.Find(message);
				remote = mRemoteSubscriberCenter.Find(message);
				message.Track("match consumers completed");
				if (local.Count == 0 && remote.Count == 0)
				{
					string error = string.Format("[{0}] subscriber not fount!", message.Consumers);
					message.ProcessError(new SRException(error));
					Loger.Process(LogType.ERROR, error);
					return;
				}
				foreach (ISubscriber item in local)
				{
					if (message.Pulisher == item.Name)
						continue;
					ProcessingMessage();
					Message sendMsg = message.Copy();
					sendMsg.Consumers = item.Name;

					sendMsg.Mode = ReceiveMode.Eq;
					message.Track("publish to " + sendMsg.Consumers + item.GetType());
					item.Process(this, sendMsg);
					ProcessedMessage();
					message.Track("publish to " + sendMsg.Consumers + " completed!");
				}
				foreach (ISubscriber item in remote)
				{
					ProcessingMessage();
					Message sendMsg = message.Copy();
					sendMsg.Consumers = item.Name;
					sendMsg.Mode = ReceiveMode.Eq;
					message.Track("publish to " + sendMsg.Consumers + item.GetType());
					item.Process(this, sendMsg);
					message.Track("publish to " + sendMsg.Consumers + " completed!");
					ProcessedMessage();
				}
			}

		}

		private ConcurrentDictionary<long, PublishResult> mAsyncResults = new ConcurrentDictionary<long, PublishResult>();

		[ThreadStatic]
		private static PublishResult mPublishResult;

		public object PublishSync(Message message, int millisecondsTimeout = 10000)
		{
			if (mPublishResult == null)
				mPublishResult = new SmartRoute.PublishResult();
			mPublishResult.Reset();
			try
			{
				mAsyncResults[message.ID] = mPublishResult;
				message.AsyncResult = mPublishResult;
				OnPulish(message);
				bool timeout = !mPublishResult.Wait(millisecondsTimeout);
				if (timeout)
				{
					throw new SRException("publish message timeout!");
				}
				else
				{
					if (mPublishResult.Error != null)
						throw mPublishResult.Error;
					if (mPublishResult.Result is Protocols.Error)
						throw new SRException(((Protocols.Error)mPublishResult.Result).Message);

				}

			}
			finally
			{
				PublishResult result;
				mAsyncResults.TryRemove(message.ID, out result);
			}
			return mPublishResult.Result;
		}

		public T PublishSync<T>(Message message, int millisecondsTimeout = 10000)
		{

			return (T)PublishSync(message, millisecondsTimeout);
		}

		public void Publish(Message message)
		{
			PublishResult result;
			if (mAsyncResults.TryGetValue(message.ID, out result))
			{
				result.Completed(message.Data, null);
			}
			else
			{
				OnPulish(message);
			}
		}

		public T Register<T>(string name) where T : ISubscriber, new()
		{
			T item = new T();
			item.Name = name;
			item.Node = this;
			Register(name, item);
			return item;
		}

		public ICollection<ISubscriber> GetLocalSubscriber()
		{
			return mLocalSubscriberCenter.GetAllSubscriber();
		}

		public ICollection<ISubscriber> GetRemoteSubscriber()
		{
			return mRemoteSubscriberCenter.GetAllSubscriber();
		}

		private static INode mDefault;

		public static INode Default
		{
			get
			{
				if (mDefault == null)
					mDefault = new Node();
				return mDefault;
			}
		}

		private void OnSubscriberRegisted(ISubscriber e)
		{
			if (SubscriberRegisted != null)
				SubscriberRegisted(this, e);
		}

		public long GetRuntime()
		{
			return mServer.GetRunTime();
		}

		public Dictionary<string, double> GetResourceStatistics()
		{
			lock (mLockStatistics)
			{
				long serveTime = mServer.GetRunTime();
				if (serveTime - mIoStatistics.LastTime >= 1000)
				{
					NodeResourceStatistics nstatis = new NodeResourceStatistics();
					nstatis.NetworkReceiveBytes = mServer.ReceivBytes;
					nstatis.NetworkReceiveIO = mServer.ReceiveQuantity;
					nstatis.NetworkSendBytes = mServer.SendBytes;
					nstatis.NetworkSendIO = mServer.SendQuantity;
					mRemoteNodeCenter.IOStatistics(nstatis);

					nstatis.NetworkReceiveBytesPersecond = nstatis.NetworkReceiveBytes - mIoStatistics.NetworkReceiveBytes;
					nstatis.NetworkReceiveIOPersecond = nstatis.NetworkReceiveIO - mIoStatistics.NetworkReceiveIO;
					nstatis.NetworkSendBytesPersecond = nstatis.NetworkSendBytes - mIoStatistics.NetworkSendBytes;
					nstatis.NetworkSendIOPersecond = nstatis.NetworkSendIO - mIoStatistics.NetworkSendIO;


					if (GetResourceStatisticsEvent != null)
					{
						GetResourceStatisticsEvent(nstatis, this);
					}
					nstatis.LastTime = serveTime;
					mIoStatistics = nstatis;
				}
				mIoStatistics.ProcessedMessages = mProcessedMessages;
				mIoStatistics.ProcessingMessages = mProcessingMessages;
				return mIoStatistics.ToProperties();
			}
		}

		public ClusterInfo GetClusterInfo()
		{
			ClusterInfo result = new SmartRoute.ClusterInfo(this.Cluster);
			NodeInfo node = new SmartRoute.NodeInfo(ID, true);
			node.Statistics = new NodeResourceStatistics();
			node.Statistics.FromProperties(GetResourceStatistics());
			result.Nodes.Add(node);
			foreach (INodeConnection item in mRemoteNodeCenter.GetOnlines())
			{
				node = new SmartRoute.NodeInfo(item.RemoteNodeID, false);
				node.Statistics = new SmartRoute.NodeResourceStatistics();
				node.Statistics.FromProperties(item.ResourceStatistics);
				result.Nodes.Add(node);
			}
			return result;
		}

		public EventSubscriberRegisted SubscriberRegisted
		{
			get;
			set;
		}

		public Action<NodeResourceStatistics, INode> GetResourceStatisticsEvent
		{
			get; set;
		}

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
	}
}
