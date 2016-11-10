using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute.Protocols
{
    [ProtoContract]
    public class Error
    {
        [ProtoMember(1)]
        public string Message { get; set; }
    }
}
