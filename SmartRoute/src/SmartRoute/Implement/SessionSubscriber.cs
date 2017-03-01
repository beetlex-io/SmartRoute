using BeetleX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
	class SessionSubscriber : ISubscriber
	{
		public SessionSubscriber(INode node, string name, ISession session)
		{
			Node = node;
			Name = name;
			mSession = session;
		}

		private ISession mSession;

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
			if (!mSession.IsDisposed)
			{
				mSession.Server.Send(message, mSession);
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
			return string.Format("{0}@{1}", Name, Node.ID);
		}
	}
}
