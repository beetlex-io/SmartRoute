using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtoBuf;
namespace SmartRoute.Protocols
{
    [ProtoContract]
    public class GetSubscribers
    {
        [ProtoMember(1)]
        public string NodeID { get; set; }
    }
}
