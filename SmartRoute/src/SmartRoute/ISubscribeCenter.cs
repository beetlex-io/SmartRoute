using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartRoute
{
	public interface ISubscriberCenter : IDisposable
	{

		INode Node { get; set; }

		void Register(string name, ISubscriber subscriber);

		ISubscriber UnRegister(string name);

		void Find(string name, ReceiveMode mode, IList<ISubscriber> items);

		void Find(Message message, IList<ISubscriber> items);

		ISubscriber Find(string name);

		string[] GetAll();

		ICollection<ISubscriber> GetAllSubscriber();

		long Version { get; }

	}
}
