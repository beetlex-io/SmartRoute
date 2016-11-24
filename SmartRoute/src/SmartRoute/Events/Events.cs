using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute.Events
{
    public delegate void EventNodeConnecterReceive(INodeConnection connecter, object message);

    public delegate void EventSubscriberRegisted(INode node, ISubscriber subscriber);

    public delegate void EventServiceRegisted(ISubscriber subscriber, RemoteService service);
}
