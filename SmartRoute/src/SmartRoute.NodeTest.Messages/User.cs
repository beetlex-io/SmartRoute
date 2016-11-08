using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtoBuf;
namespace SmartRoute.NodeTest.Messages
{
    [ProtoContract]
    public class User
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string Address { get; set; }
        [ProtoMember(3)]
        public string City { get; set; }
        [ProtoMember(4)]
        public string Password { get; set; }
        [ProtoMember(5)]
        public string EMail { get; set; }
    }
}
