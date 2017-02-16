using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtoBuf;
namespace SmartRoute.Protocols
{
    [ProtoContract]
    public class SyncServiceInfo
    {

        public SyncServiceInfo()
        {
            Items = new List<ServiceSubscriberItem>();
        }
        [ProtoMember(1)]
        public List<ServiceSubscriberItem> Items { get; set; }

        [ProtoMember(2)]
        public string RemoteNode { get; set; }

    }
    [ProtoContract]
    public class ServiceSubscriberItem
    {
        [ProtoMember(1)]
        public string Service { get; set; }
        [ProtoMember(2)]
        public string Name { get; set; }
    }
}
