using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
    interface IRemoteNodeCenter : IDisposable
    {
        INodeConnection Register(INode node, string remotNode, string host, int port);

        INodeConnection UnRegister(string remoteNode);

        INodeConnection Get(string remoteNode);

        void Send(object message);

        ICollection<INodeConnection> GetOnlines();


    }
}
