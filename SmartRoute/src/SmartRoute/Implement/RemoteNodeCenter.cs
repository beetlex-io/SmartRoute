using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
	class RemoteNodeCenter : IRemoteNodeCenter
	{

		private System.Collections.Concurrent.ConcurrentDictionary<string, INodeConnection> mRemoteNodes = new System.Collections.Concurrent.ConcurrentDictionary<string, INodeConnection>();

		private Dictionary<string, NodeResourceStatistics> mNetworkStatistics = new Dictionary<string, SmartRoute.NodeResourceStatistics>();

		private long mVersion = 0;

		private RemoteNodeSegment mSegment = new RemoteNodeSegment();

		class RemoteNodeSegment
		{
			public RemoteNodeSegment()
			{
				Version = -1;
			}
			public long Version
			{
				get; set;
			}
			public ICollection<INodeConnection> Values
			{
				get; set;
			}

		}

		public ICollection<INodeConnection> GetOnlines()
		{
			if (mSegment.Version != mVersion)
			{
				mSegment.Values = mRemoteNodes.Values;
				mSegment.Version = mVersion;
			}
			return mSegment.Values;
		}

		public void Dispose()
		{
			mRemoteNodes.Clear();
		}

		public INodeConnection Get(string remoteNode)
		{
			INodeConnection result;
			mRemoteNodes.TryGetValue(remoteNode, out result);
			return result;
		}

		public INodeConnection Register(INode node, string remotNode, string host, int port)
		{
			NodeConnecter connecter = new SmartRoute.NodeConnecter(node, remotNode, host, port);
			mRemoteNodes[remotNode] = connecter;
			mVersion++;
			return connecter;
		}

		public void Send(object message)
		{
			ICollection<INodeConnection> items = GetOnlines();
			if (items.Count > 0)
				foreach (INodeConnection conn in GetOnlines())
				{
					conn.Send(message);
				}
		}

		public INodeConnection UnRegister(string remoteNode)
		{
			INodeConnection result;
			mRemoteNodes.TryRemove(remoteNode, out result);
			mVersion++;
			return result;
		}

		public void IOStatistics(NodeResourceStatistics statis)
		{
			lock (mNetworkStatistics)
			{
				NodeResourceStatistics value;
				foreach (INodeConnection conn in GetOnlines())
				{
					if (!mNetworkStatistics.TryGetValue(conn.RemoteNodeID, out value))
					{
						value = new SmartRoute.NodeResourceStatistics();
						mNetworkStatistics[conn.RemoteNodeID] = value;
					}
					value.NetworkReceiveBytes = conn.Client.ReceivBytes;
					value.NetworkReceiveIO = conn.Client.ReceiveQuantity;
					value.NetworkSendBytes = conn.Client.SendBytes;
					value.NetworkSendIO = conn.Client.SendQuantity;
				}

				foreach (NodeResourceStatistics item in mNetworkStatistics.Values)
				{
					statis.NetworkSendIO += item.NetworkSendIO;
					statis.NetworkSendBytes += item.NetworkSendBytes;
					statis.NetworkReceiveIO += item.NetworkReceiveIO;
					statis.NetworkReceiveBytes += item.NetworkReceiveBytes;
				}
			}
		}
	}
}
