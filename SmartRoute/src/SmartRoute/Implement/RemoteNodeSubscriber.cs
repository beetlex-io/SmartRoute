using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
    class RemoteNodeSubscriber : ISubscriber
    {
        public RemoteNodeSubscriber(string name, INode node, INodeConnection remoteConnection)
        {
            Node = node;
            Name = name;
            mConnection = remoteConnection;
        }

        private INodeConnection mConnection;

        public string Name
        {
            get; set;
        }

        public INode Node
        {
            get; set;
        }

        public void Dispose()
        {


        }

        public void Process(INode node, Message message)
        {
            message.Subscriber = this;
            if (mConnection.Available)
            {
                message.Track("reply message send");
                mConnection.Send(message);
                message.EndTrack("reply message send completed");
            }
            else
            {
                string error = string.Format("{0} network not available", Name);
                message.ProcessError(new SRException(error));
            }

        }

        public void Publish(Message message)
        {
            Node.Publish(message);
        }

        public override string ToString()
        {
            return string.Format("{0}@{1}", Name, mConnection.RemoteNodeID);
        }


    }
}
