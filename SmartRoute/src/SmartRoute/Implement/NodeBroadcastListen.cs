using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace SmartRoute
{
    class NodeBroadcastListen : INodeBroadcastListen
    {



        public NodeBroadcastListen(string cluster, string host, int port, string nodeid, string tokenKey)
        {
            NodeID = nodeid;
            ListenIPAddress = host;
            Port = port;
            this.Cluster = cluster;
            mTokenKey = tokenKey;

        }
        [ProtoBuf.ProtoContract]
        public class BrodcastMessage
        {

            public BrodcastMessage()
            {
                MessageType = "SMART_ROUTE";
            }
            [ProtoMember(1)]
            public string MessageType
            {
                get;
                set;
            }
            [ProtoMember(2)]
            public string Cluster
            {
                get;
                set;
            }
            [ProtoMember(3)]
            public string IP
            {
                get;
                set;
            }
            [ProtoMember(4)]
            public string Port
            {
                get;
                set;
            }
            [ProtoMember(5)]
            public string Token
            {
                get;
                set;
            }
            [ProtoMember(6)]
            public string NodeID
            {
                get;
                set;
            }

            public override string ToString()
            {
                return string.Format("MessageType={0},Cluster={1},IP={2},Port={3},Token={4},NodeID={5}", MessageType, Cluster, IP, Port, Token, NodeID);
            }
        }

        public ILogHandler Loger
        {
            get;
            set;
        }

        private System.Collections.Concurrent.ConcurrentDictionary<string, DiscoverEventArgs> mDiscoverTable = new System.Collections.Concurrent.ConcurrentDictionary<string, DiscoverEventArgs>();

        public EventHandler<DiscoverEventArgs> Discover
        {
            get;
            set;
        }

        private List<BroadcastUdpClient> mBroadcastClients = new List<BroadcastUdpClient>();

        private string mTokenKey = null;

        private string mBroadcastMessage = null;

        private int mPort = 21000;

        public string NodeID { get; set; }

        private System.Threading.Timer mTimer;

        private IPEndPoint mEndPoint;

        private System.Net.Sockets.SocketAsyncEventArgs mReceiveSAEA = new SocketAsyncEventArgs();

        private Socket mSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        private Socket mReveiveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        public string ListenIPAddress { get; set; }

        public int Port { get; set; }

        public string Cluster
        {
            get;
            set;
        }

        public async void Open()
        {

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    if (ni.SupportsMulticast)
                    {
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                try
                                {
                                    mBroadcastClients.Add(new BroadcastUdpClient(new IPEndPoint(ip.Address, 0)));
                                    Loger.Process(LogType.BROAD_DEBUG, "broadcast send bind {0}", ip.Address.ToString());
                                }
                                catch (Exception e_)
                                {
                                    Loger.Process(LogType.BROAD_ERROR, "broadcast send bind {0} error ", ip.Address.ToString(), e_.Message);
                                }
                            }
                        }
                    }
                }
            }

            BroadcastSend();

            BroadcastReceive();

            Loger.Process(LogType.BROAD_DEBUG, "{0} node discover start", NodeID);
        }

        private void OnReceive()
        {
            try
            {
                if (!mReveiveSocket.ReceiveFromAsync(mReceiveSAEA))
                {
                    OnReceiveCompleted(this, mReceiveSAEA);
                }
            }
            catch (Exception e_)
            {
                Loger.Process(LogType.BROAD_ERROR, "broadcast reveive error {0}", e_.Message);
            }
        }

        private void OnDiscorver(DiscoverEventArgs e)
        {
            if (Discover != null)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        Discover(this, e);
                    }
                    catch (Exception e_)
                    {
                        Loger.Process(LogType.BROAD_ERROR, "broadcast reveive discover error {0}", e_.Message);
                    }

                });
            }
        }

        private void OnReceiveCompleted(object sender, System.Net.Sockets.SocketAsyncEventArgs e)
        {
            try
            {
                System.IO.MemoryStream stream = new System.IO.MemoryStream(e.Buffer, 0, e.BytesTransferred);
                BrodcastMessage msg = stream.Deserialize<BrodcastMessage>(e.BytesTransferred);
                if (msg.MessageType == "SMART_ROUTE")
                {

                    if (!msg.Token.VerifyMd5Hash(msg.NodeID + mTokenKey))
                    {
                        return;
                    }

                    if (msg.NodeID != NodeID && msg.Cluster == this.Cluster)
                    {
                        Loger.Process(LogType.BROAD_DEBUG, "broadcast reveive {0}", msg);
                        if (string.IsNullOrEmpty(msg.IP))
                            msg.IP = ((IPEndPoint)e.RemoteEndPoint).Address.ToString();
                        try
                        {

                            if (!mDiscoverTable.ContainsKey(msg.NodeID))
                            {
                                DiscoverEventArgs de = new DiscoverEventArgs();
                                de.Address = msg.IP;
                                de.Port = int.Parse(msg.Port);
                                de.NodeID = msg.NodeID;
                                mDiscoverTable[msg.NodeID] = de;
                                OnDiscorver(de);
                            }

                        }
                        catch (Exception e__)
                        {
                            Loger.Process(LogType.BROAD_ERROR, "node ping error {0}", e__.Message);
                        }

                    }
                }
            }
            catch (Exception e_)
            {
                Loger.Process(LogType.BROAD_ERROR, "broadcast reveive error {0}", e_.Message);
            }
            finally
            {
                OnReceive();
            }
        }

        private void BroadcastReceive()
        {
            IPEndPoint iep = null;
            EndPoint ep = null;
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    iep = new IPEndPoint(IPAddress.Any, mPort + i);
                    ep = (EndPoint)iep;
                    mReveiveSocket.Bind(iep);
                    Loger.Process(LogType.BROAD_DEBUG, "broadcast socket bind {0}", iep);
                    EndPoint remotePoint = new IPEndPoint(IPAddress.Any, mPort + i);
                    byte[] buffer = new byte[1024 * 8];
                    mReceiveSAEA.SetBuffer(buffer, 0, buffer.Length);
                    mReceiveSAEA.Completed += OnReceiveCompleted;
                    mReceiveSAEA.RemoteEndPoint = remotePoint;
                    OnReceive();
                    return;
                }
                catch (Exception e_)
                {

                    Loger.Process(LogType.BROAD_ERROR, "broadcast socket bind error {0}", e_.Message);
                }
            }

            Loger.Process(LogType.ERROR, "broadcast socket bind error!");

        }

        private void BroadcastSend()
        {
            BrodcastMessage msg = new BrodcastMessage();
            msg.Cluster = Cluster;
            msg.IP = ListenIPAddress;
            msg.NodeID = NodeID;
            msg.Port = Port.ToString();
            if (!string.IsNullOrEmpty(mTokenKey))
                msg.Token = (NodeID + mTokenKey).GetMd5Hash();
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            msg.Serialize(stream);
            byte[] data = stream.ToArray();
            mTimer = new System.Threading.Timer(o =>
            {
                try
                {

                    for (int i = 0; i < 20; i++)
                    {
                        mEndPoint = new IPEndPoint(IPAddress.Broadcast, mPort + i);
                        foreach (BroadcastUdpClient client in mBroadcastClients)
                        {
                            client.SendNodeInfo(data, mEndPoint);
                        }

                    }

                    Loger.Process(LogType.BROAD_DEBUG, "broadcast send {0}", msg);
                }
                catch (Exception e_)
                {

                    Loger.Process(LogType.BROAD_ERROR, "broadcast send note info error {0}", e_.Message);
                }
            }, data, 100, 2000);
        }

        public void Remove(string node)
        {
            DiscoverEventArgs result;
            mDiscoverTable.TryRemove(node, out result);
        }

        internal class DiscoverEventArgs : EventArgs
        {
            public string Address { get; set; }
            public int Port { get; set; }
            public string NodeID { get; set; }
        }

        internal class BroadcastUdpClient : UdpClient
        {
            public BroadcastUdpClient()
                : base()
            {

                Socket s = this.Client;

                s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
            }

            public BroadcastUdpClient(IPEndPoint ipLocalEndPoint)
                : base(ipLocalEndPoint)
            {

                Socket s = this.Client;

                s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
            }
            public void SendNodeInfo(byte[] data, IPEndPoint point)
            {
                this.SendAsync(data, data.Length, point);
            }

        }

    }
}
