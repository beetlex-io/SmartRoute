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

        IList<ISubscriber> Find(string name, ReceiveMode mode);

        IList<ISubscriber> Find(Message message);

        ISubscriber Find(string name);

        string[] GetAll();

        ICollection<ISubscriber> GetAllSubscriber();
    }
}
