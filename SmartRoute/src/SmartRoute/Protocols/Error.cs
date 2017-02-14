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
		[ProtoMember(2)]
		public string StackTrace { get; set; }

		[ProtoMember(3)]
		public int ErrorCode { get; set; }
	}
	[ProtoContract]
	public class Success
	{
		[ProtoMember(1)]
		public int StatusCode { get; set; }

	}
}
