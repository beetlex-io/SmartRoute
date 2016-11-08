using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute.Protocols
{
    [ProtoContract]
    public class Authentication
    {
        [ProtoMember(1)]
        public string NodeID { get; set; }
        [ProtoMember(2)]
        public string Token { get; set; }
    }
}
