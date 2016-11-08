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
        }

        private int mPingCount = 0;

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
                return mClient.Connected && mPingCount < 5;
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

        public int PingCount
        {
            get
            {
                return mPingCount;
            }
        }

        public void Ping()
        {
            System.Threading.Interlocked.Increment(ref mPingCount);
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
                        System.Threading.Interlocked.Exchange(ref mPingCount, 0);
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
                System.Threading.Interlocked.Decrement(ref mPingCount);
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
