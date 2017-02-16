using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute.Protocols
{
	[ProtoContract]
	public class Pong
	{
		[ProtoMember(1)]
		public int Status { get; set; }

		[ProtoMember(2)]
		public Dictionary<string, Double> Properties { get; set; }

		[ProtoMember(3)]
		public string NodeID { get; set; }
	}
}
